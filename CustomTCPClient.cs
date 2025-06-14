using CustomTCPServerLibrary.Base;
using CustomTCPServerLibrary.BaseEntities;
using CustomTCPServerLibrary.Frames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary
{
    public class CustomTCPClient : BaseTCPClient
    {
        /// <summary>
        /// Время на стороне сервера
        /// </summary>
        public long ServerTime { get; private set; }
        /// <summary>
        /// Атомарный объект доступа к клиенту
        /// </summary>
        protected AtomicObject<NetEndPoint> _ServerEndpoint { get; } = new();
        /// <summary>
        /// Оконечная точка сервера
        /// </summary>
        public NetEndPoint? ServerEndpoint { get => _ServerEndpoint.Value; private set => _ServerEndpoint.Value = value; }
        /// <summary>
        /// Событие прием сообщения
        /// </summary>
        public event Action<CustomTCPClient, byte[]>? ReceiveDataEvent;
        /// <summary>
        /// Событие отправки сообщения
        /// </summary>
        public event Action<CustomTCPClient, byte[]>? TransmitDataEvent;
        /// <summary>
        /// Флаг автоматического подключения в случае разрыва соединения
        /// </summary>
        public bool AutoConnection { get; set; } = false;

        public CustomTCPClient(NetEndPoint? endPoint = null) : base(endPoint)
        {
            InternalReceiveDataEvent += (_, baseFrame) =>
            {
                var frame = FramesFabric.GetFrameBody(baseFrame);

                if (frame is DataFrame)
                {
                    var _frame = (DataFrame)frame;

                    if (_frame is not null)
                    {
                        if (_frame.Data is not null)
                        {
                            ReceiveDataEvent?.Invoke(this, _frame.Data);
                        }

                        _FinalStateMachine.PingMessageHasBeenReceived();
                    }
                }
                else if (frame is PingServerToClientFrame)
                {
                    var _frame = (PingServerToClientFrame)frame;

                    if (_frame is not null)
                    {
                        ServerTime = _frame.ServerTime;
                        TimeShift = GetCurrentTime() - ServerTime;

                        Timings.ReceivingTimeout = _frame.ReceivingTimeout;
                        Timings.SendingTimeout = _frame.SendingTimeout;
                        Timings.PingInterval = _frame.PingInterval;
                        Timings.PingTimeout = _frame.PingTimeout;
                        // Параметры меняются местами, из-за зеркальности буферов
                        DataConfigs.ReceiveDataBufferSize = _frame.TransmitDataBufferSize;
                        DataConfigs.TransmitDataBufferSize = _frame.ReceiveDataBufferSize;
                        DataConfigs.BufferIncreaseFactor = _frame.BufferIncreaseFactor;
                        // Обработка состояний конечных автоматов
                        _FinalStateMachine.PingMessageHasBeenReceived();
                        _TransmittingFinalStateMachine.CheckIfAnotherPointReceiveBufferHasIncreased(_frame.ReceiveDataBufferSize);
                    }
                }
            };

            InternalTransmitDataEvent += (_, baseFrame) =>
            {
                if (TransmitDataEvent is null)
                {
                    return;
                }

                var frame = FramesFabric.GetFrameBody(baseFrame);

                if (frame is DataFrame)
                {
                    var _frame = (DataFrame)frame;

                    if (_frame?.Data is not null)
                    {
                        TransmitDataEvent?.Invoke(this, _frame.Data);
                    }
                }
            };

            HasStoppedEvent += (_) =>
            {
                if (AutoConnection
                    && (ConnectionStatus == Enums.ClientConnectionStatusEnum.ConnectionFlagHasBeenResetted
                        || ConnectionStatus == Enums.ClientConnectionStatusEnum.ConnectionError
                        || ConnectionStatus == Enums.ClientConnectionStatusEnum.PingTimeoutHasBeenElapsed)
                    )
                {
                    Start();
                }
            };
        }

        public void SetServerEndPoint(NetEndPoint endPoint)
        {
            _HandleIfNotActive(() =>
            {
                ServerEndpoint = endPoint;
            });
        }

        protected override bool _Start()
        {
            _ConnectionController.ResetConnectionStatus();
            // Проверка, что клиент был создан и есть конечная точка подключения
            if (ServerEndpoint is null)
            {
                return false;
            }

            var connectionInitStatus = _ConnectionController.Connect(_GetClient(EndPoint), ServerEndpoint);

            if (connectionInitStatus)
            {
                base._Start();
            }

            return connectionInitStatus;
        }

        protected override BaseFrame _GetPingFrame()
        {
            return FramesFabric.CreatePingClientToServerFrame
            (
                GetCurrentTime()
                , DataConfigs.ReceiveDataBufferSize
                , DataConfigs.TransmitDataBufferSize
            );
        }
        /// <summary>
        /// Полное терминирование работы клиента с выключением автоматического подключения
        /// </summary>
        public void Terminate()
        {
            AutoConnection = false;
            Stop();
        }

    }
}
