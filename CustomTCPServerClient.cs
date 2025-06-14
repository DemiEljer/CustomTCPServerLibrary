using CustomTCPServerLibrary.Base;
using CustomTCPServerLibrary.BaseEntities;
using CustomTCPServerLibrary.Frames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary
{
    public class CustomTCPServerClient : BaseTCPClient
    {
        /// <summary>
        /// Время клиента
        /// </summary>
        public long ClientTime { get; private set; }
        /// <summary>
        /// Событие прием сообщения
        /// </summary>
        public event Action<CustomTCPServerClient, byte[]>? ReceiveDataEvent;
        /// <summary>
        /// Событие отправки сообщения
        /// </summary>
        public event Action<CustomTCPServerClient, byte[]>? TransmitDataEvent;

        internal CustomTCPServerClient(TcpClient client) : base((NetEndPoint)client.Client.RemoteEndPoint, client)
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
                else if (frame is PingClientToServerFrame)
                {
                    var _frame = (PingClientToServerFrame)frame;

                    if (_frame is not null)
                    {
                        ClientTime = _frame.ClientTime;
                        TimeShift = GetCurrentTime() - ClientTime;
                        // Параметры меняются местами, из-за зеркальности буферов
                        DataConfigs.ReceiveDataBufferSize = _frame.TransmitDataBufferSize;
                        DataConfigs.TransmitDataBufferSize = _frame.ReceiveDataBufferSize;

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
        }

        protected override bool _Start()
        {
            _ConnectionController.ResetConnectionStatus();

            base._Start();

            return true;
        }

        protected override BaseFrame _GetPingFrame()
        {
            return FramesFabric.CreatePingServerToClientFrame
            (
                Timings.SendingTimeout
                , Timings.ReceivingTimeout
                , GetCurrentTime()
                , Timings.PingInterval
                , Timings.PingTimeout
                , DataConfigs.ReceiveDataBufferSize
                , DataConfigs.TransmitDataBufferSize
                , DataConfigs.BufferIncreaseFactor
            );
        }
    }
}
