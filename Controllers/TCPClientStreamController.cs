using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Controllers
{
    public abstract class TCPClientStreamController : IDisposable
    {
        protected NetworkStream? _Stream { get; set; }
        /// <summary>
        /// Объект блокировки
        /// </summary>
        private object _LockObject { get; } = new();
        /// <summary>
        /// Флаг, что обработчик потока занят
        /// </summary>
        public bool IsBusy { get; protected set; }

        public NetworkStream? Stream
        {
            get 
            { 
                lock (_LockObject)
                {
                    return _Stream;
                }
            }
            set 
            {
                lock (_LockObject)
                {
                    _Stream = value;
                }
            }
        }
        /// <summary>
        /// Потокобезопасная обработка потока
        /// </summary>
        protected void _SafetyHandleStream(Action<NetworkStream>? handler)
        {
            lock (_LockObject)
            {
                if (_Stream is not null)
                {
                    handler?.Invoke(_Stream);
                }
            }
        }

        public abstract void Dispose();
    }
}
