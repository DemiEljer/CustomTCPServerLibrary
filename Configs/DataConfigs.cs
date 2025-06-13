using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Configs
{
    public class DataConfigs
    {
        private int _ReceiveDataBufferSize = 65000;
        /// <summary>
        /// Ограничение времени на отправку сообщения
        /// </summary>
        public int ReceiveDataBufferSize
        {
            get => _ReceiveDataBufferSize;
            set
            {
                if (_IsDecreasedLock)
                {
                    value = Math.Max(value, _ReceiveDataBufferSize);
                }

                bool invokeEventUpdateEvent = false;
                if (_ReceiveDataBufferSize != value)
                {
                    invokeEventUpdateEvent = true;
                }

                _ReceiveDataBufferSize = value;

                if (invokeEventUpdateEvent)
                {
                    _InvokeUpdateEvent();
                }
            }
        }

        private int _TransmitDataBufferSize = 65000;
        /// <summary>
        /// Ограничение времени на прием сообщения
        /// </summary>
        public int TransmitDataBufferSize
        {
            get => _TransmitDataBufferSize;
            set
            {
                if (_IsDecreasedLock)
                {
                    value = Math.Max(value, _TransmitDataBufferSize);
                }

                bool invokeEventUpdateEvent = false;
                if (_TransmitDataBufferSize != value)
                {
                    invokeEventUpdateEvent = true;
                }

                _TransmitDataBufferSize = value;

                if (invokeEventUpdateEvent)
                {
                    _InvokeUpdateEvent();
                }
            }
        }

        private bool _IsDecreasedLock { get; set; } = false;

        public event Action<DataConfigs>? UpdateEvent;

        private void _InvokeUpdateEvent()
        {
            UpdateEvent?.Invoke(this);
        }

        public void Update(DataConfigs? dataConfigs)
        {
            if (dataConfigs != null)
            {
                ReceiveDataBufferSize = dataConfigs.ReceiveDataBufferSize;
                TransmitDataBufferSize = dataConfigs.TransmitDataBufferSize;
            }
        }

        public void LockDecrease(bool lockFlag)
        {
            _IsDecreasedLock = lockFlag;
        }
    }
}
