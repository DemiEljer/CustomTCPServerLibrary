using CustomTCPServerLibrary.Frames;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Controllers
{
    public class TCPStreamWriterController : TCPClientStreamController
    {
        /// <summary>
        /// Делегат получения следующего пакета данных для отправки
        /// </summary>
        private Func<(byte[]? data, BaseFrame[] frames)>? _GetNextDataDelegate { get; set; }
        /// <summary>
        /// Текущие отправляемые данные
        /// </summary>
        private (byte[]? data, BaseFrame[] frames) _CurrentTransmittingData { get; set; }
        /// <summary>
        /// Объект блокировки потоков
        /// </summary>
        private object _LockObject { get; } = new();
        /// <summary>
        /// Событие отправки данных
        /// </summary>
        public event Action<(byte[]? data, BaseFrame[] frames)>? DataHasBeenTransmittedEvent;
        /// <summary>
        /// Инициализация делегата получения следующего пакета данных 
        /// </summary>
        /// <param name="nextDataGetter"></param>
        public void InitNextDataGetter(Func<(byte[]? data, BaseFrame[] frames)>? nextDataGetter)
        {
            _GetNextDataDelegate = nextDataGetter;
        }
        /// <summary>
        /// Получить следующий пакет данных
        /// </summary>
        /// <returns></returns>
        private (byte[]? data, BaseFrame[] frames) _GetNextData() => _GetNextDataDelegate is null ? (null, Array.Empty<BaseFrame>()) : _GetNextDataDelegate();
        /// <summary>
        /// Попытаться отправить пакет данных
        /// </summary>
        /// <returns></returns>
        public void TryTransmit()
        {
            lock (_LockObject)
            {
                if (!IsBusy)
                {
                    _SafetyHandleStream(_TryTransmit);
                }
            }
        }
        /// <summary>
        /// Внутренний обработчик попытки записи
        /// </summary>
        /// <param name="stream"></param>
        private void _TryTransmit(NetworkStream stream)
        {
            _CurrentTransmittingData = _GetNextData();

            if (_CurrentTransmittingData.data is not null)
            {
                IsBusy = true;

                stream.BeginWrite(_CurrentTransmittingData.data, 0, _CurrentTransmittingData.data.Length, _WriteThreadAsyncCallback, null);
            }
        }
        /// <summary>
        /// Обратный вызов завершения процесса записи
        /// </summary>
        /// <param name="arg"></param>
        private void _WriteThreadAsyncCallback(IAsyncResult arg)
        {
            lock (_LockObject)
            {
                IsBusy = false;
            }

            DataHasBeenTransmittedEvent?.Invoke(_CurrentTransmittingData);

            TryTransmit();
        }
        /// <summary>
        /// Удаление объекта
        /// </summary>
        public override void Dispose()
        {
            _GetNextDataDelegate = null;
            DataHasBeenTransmittedEvent = null;
            _CurrentTransmittingData = (null, Array.Empty<BaseFrame>());
        }
    }
}
