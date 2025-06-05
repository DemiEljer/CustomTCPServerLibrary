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

        public CustomTCPClient(NetEndPoint? endPoint = null) : base(endPoint)
        {
            InternalReceiveDataEvent += (_, data) =>
            {
                var frame = FramesFabric.ParseFrame(data);

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
                        PingTime = GetCurrentTime() - ServerTime;

                        Timings.ReceivingTimeout = _frame.ReceivingTimeout;
                        Timings.SendingTimeout = _frame.SendingTimeout;
                        Timings.PingInterval = _frame.PingInterval;
                        Timings.PingTimeout = _frame.PingTimeout;

                        _FinalStateMachine.PingMessageHasBeenReceived();
                    }
                }
            };

            InternalTransmitDataEvent += (_, data) =>
            {
                if (TransmitDataEvent is null)
                {
                    return;
                }

                var frame = FramesFabric.ParseFrame(data);

                if (frame is DataFrame)
                {
                    var _frame = (DataFrame)frame;

                    if (_frame?.Data is not null)
                    {
                        TransmitDataEvent?.Invoke(this, _frame.Data);
                    }
                }
            };

            _FinalStateMachine.CallPingMessageSending += () =>
            {
                InternalAddTransmittingData(FramesFabric.CreatePingClientToServerFrame
                (
                    GetCurrentTime()
                ));
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
            // Создание клиента
            _SetClient(_GetClient(EndPoint));
            // Проверка, что клиент был создан и есть конечная точка подключения
            if (Client is null
                || ServerEndpoint is null)
            {
                return false;
            }

            base._Start();

            Client.Connect(ServerEndpoint);

            return true;
        }
    }
}
