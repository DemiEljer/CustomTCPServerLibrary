using CustomTCPServerLibrary.Frames;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CustomTCPServerLibrary.Controllers
{
    public class TCPClientStreamReaderController : TCPClientStreamController
    {
        /// <summary>
        /// Делегат получения размера читаемых данных
        /// </summary>
        private Func<int>? _GetDataLengthDelegate { get; set; }
        /// <summary>
        /// Событие отправки данных
        /// </summary>
        public event Action<byte[]>? DataHasBeenReceivedEvent;
        /// <summary>
        /// Объект блокировки потоков
        /// </summary>
        private object _LockObject { get; } = new();
        /// <summary>
        /// Полученные данные
        /// </summary>
        private byte[]? _ReceivedData { get; set; } = null;
        /// <summary>
        /// Инициализация делегата получения размера читаемых данных
        /// </summary>
        /// <param name="nextDataGetter"></param>
        public void InitDataLengthGetter(Func<int>? dataLengthGetter)
        {
            _GetDataLengthDelegate = dataLengthGetter;
        }
        /// <summary>
        /// Получить следующий пакет данных
        /// </summary>
        /// <returns></returns>
        private int _GetDataLength() => _GetDataLengthDelegate is null ? 0 : _GetDataLengthDelegate();
        /// <summary>
        /// Попытаться принять данные
        /// </summary>
        public void TryReceive()
        {
            lock (_LockObject)
            {
                if (!IsBusy)
                {
                    _SafetyHandleStream(_TryReceive);
                }
            }
        }
        /// <summary>
        /// Внутренний обработчик попытки чтения
        /// </summary>
        /// <param name="stream"></param>
        private void _TryReceive(Stream stream)
        {
            int dataLength = _GetDataLength();

            if (dataLength > 0)
            {
                IsBusy = true;

                _ReceivedData = new byte[dataLength];

                stream.BeginRead(_ReceivedData, 0, _ReceivedData.Length, _ReadThreadAsyncCallback, null);
            }
        }
        /// <summary>
        /// Обратный вызов завершения процесса чтения
        /// </summary>
        /// <param name="arg"></param>
        private void _ReadThreadAsyncCallback(IAsyncResult arg)
        {
            lock (_LockObject)
            {
                IsBusy = false;
            }

            if (_ReceivedData is not null)
            {
                DataHasBeenReceivedEvent?.Invoke(_ReceivedData);
            }

            TryReceive();
        }
        /// <summary>
        /// Удаление объекта
        /// </summary>
        public override void Dispose()
        {
            _GetDataLengthDelegate = null;
            DataHasBeenReceivedEvent = null;
            _ReceivedData = null;
        }
    }
}
