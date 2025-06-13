using BinarySerializerLibrary.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Configs
{
    public class TimingConfigs
    {
        private int _SendingTimeout = 5000;
        /// <summary>
        /// Ограничение времени на отправку сообщения
        /// </summary>
        public int SendingTimeout
        { 
            get => _SendingTimeout; 
            set
            {
                bool invokeEventUpdateEvent = false;
                if (_SendingTimeout != value)
                {
                    invokeEventUpdateEvent = true;
                }

                _SendingTimeout = value;

                if (invokeEventUpdateEvent)
                {
                    _InvokeUpdateEvent();
                }
            }
        }

        private int _ReceivingTimeout = 5000;
        /// <summary>
        /// Ограничение времени на прием сообщения
        /// </summary>
        public int ReceivingTimeout
        {
            get => _ReceivingTimeout;
            set
            {
                bool invokeEventUpdateEvent = false;
                if (_ReceivingTimeout != value)
                {
                    invokeEventUpdateEvent = true;
                }

                _ReceivingTimeout = value;

                if (invokeEventUpdateEvent)
                {
                    _InvokeUpdateEvent();
                }
            }
        }

        private int _PingInterval = 1000;
        /// <summary>
        /// Интервал времени отправки сообщения Ping
        /// </summary>
        public int PingInterval
        {
            get => _PingInterval;
            set
            {
                bool invokeEventUpdateEvent = false;
                if (_PingInterval != value)
                {
                    invokeEventUpdateEvent = true;
                }

                _PingInterval = value;

                if (invokeEventUpdateEvent)
                {
                    _InvokeUpdateEvent();
                }
            }
        }

        private int _PingTimeout = 5000;
        /// <summary>
        /// Максимальное время ожидания получения сообщения Ping
        /// </summary>
        public int PingTimeout
        {
            get => _PingTimeout;
            set
            {
                bool invokeEventUpdateEvent = false;
                if (_PingTimeout != value)
                {
                    invokeEventUpdateEvent = true;
                }

                _PingTimeout = value;

                if (invokeEventUpdateEvent)
                {
                    _InvokeUpdateEvent();
                }
            }
        }

        public event Action<TimingConfigs>? UpdateEvent;

        private void _InvokeUpdateEvent()
        {
            UpdateEvent?.Invoke(this);
        }

        public void Update(TimingConfigs? timings)
        {
            if (timings != null)
            {
                ReceivingTimeout = timings.ReceivingTimeout;
                SendingTimeout = timings.SendingTimeout;
                PingInterval = timings.PingInterval;
                PingTimeout = timings.PingTimeout;
            }
        }
    }
}
