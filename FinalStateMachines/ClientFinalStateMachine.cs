using CustomTCPServerLibrary.Base;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Запрос отправки сообщения Ping
        /// </summary>
        public event Action? CallPingMessageSending;
        /// <summary>
        /// Запрос отправки сообщения Ping
        /// </summary>
        public event Action? CallPingWaitingTimeElapsed;

        public void PingMessageHasBeenReceived()
        {
            _PingMessageReceivingTimeMark.Update();
        }

        public void Start()
        {
            _PingMessageReceivingTimeMark.Update();
        }

        public void Invoke()
        {
            // Вызов обработчика отправки сообщения Ping
            if (_PingMessageSendingTimeMark.HasIntervalElapsed(PingInterval))
            {
                _PingMessageSendingTimeMark.Update();

                CallPingMessageSending?.Invoke();
            }
            // Вызов обработчика истечения времени ожидания Ping
            if (_PingMessageReceivingTimeMark.HasIntervalElapsed(PingTimeout))
            {
                CallPingWaitingTimeElapsed?.Invoke();
            }
        }
    }
}
