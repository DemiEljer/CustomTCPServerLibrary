using CustomTCPServerLibrary.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.FinalStateMachines
{
    public class ClientFinalStateMachine
    {
        /// <summary>
        /// Интервал времени отправки сообщения Ping
        /// </summary>
        public int PingInterval { get; set; } = 1000;
        /// <summary>
        /// Максимальное время ожидания получения сообщения Ping
        /// </summary>
        public int PingTimeout { get; set; } = 5000;
        /// <summary>
        /// Метка времени отправки 
        /// </summary>
        private TimeMarkObject _PingMessageSendingTimeMark { get; } = new(0);
        /// <summary>
        /// Метка времени отправки 
        /// </summary>
        private TimeMarkObject _PingMessageReceivingTimeMark { get; } = new();
        /// <summary>
        /// Флаг, что протокол валидный
        /// </summary>
        private bool? _IsProtocolValid { get; set; }
        /// <summary>
        /// Запрос отправки сообщения Ping
        /// </summary>
        public event Action? CallPingMessageSending;
        /// <summary>
        /// Запрос отправки сообщения Ping
        /// </summary>
        public event Action? CallPingWaitingTimeElapsed;
        /// <summary>
        /// Запрос отправки сообщения Ping
        /// </summary>
        public event Action? CallingProtocolIsNotValid;
        /// <summary>
        /// Подтверждение, что протокол валиден
        /// </summary>
        public bool IsProtocolValid => _IsProtocolValid == true;
        /// <summary>
        /// Событие верификации протокола
        /// </summary>
        public event Action? ProtocolHasBeenVerifiedEvent;
        /// <summary>
        /// 
        /// </summary>
        public void PingMessageHasBeenReceived()
        {
            _PingMessageReceivingTimeMark.Update();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            _PingMessageReceivingTimeMark.Update();
            _IsProtocolValid = null;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Invoke()
        {
            // Вызов обработчика отправки сообщения Ping
            if (_PingMessageSendingTimeMark.HasIntervalElapsed(PingInterval))
            {
                InvokePingMessageTransmitting();
            }
            // Вызов обработчика истечения времени ожидания Ping
            if (_PingMessageReceivingTimeMark.HasIntervalElapsed(PingTimeout))
            {
                CallPingWaitingTimeElapsed?.Invoke();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="isValid"></param>
        public void SetReceivingFramesValidFlag(bool isValid)
        {
            if (_IsProtocolValid is null)
            {
                _IsProtocolValid = isValid;

                if (_IsProtocolValid == false)
                {
                    CallingProtocolIsNotValid?.Invoke();
                }
                else
                {
                    ProtocolHasBeenVerifiedEvent?.Invoke();
                }

            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void InvokePingMessageTransmitting()
        {
            _PingMessageSendingTimeMark.Update();

            CallPingMessageSending?.Invoke();
        }
    }
}
