using CustomTCPServerLibrary.Enums;
using CustomTCPServerLibrary.Frames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.FinalStateMachines
{
    /// <summary>
    /// Конечный автомат управления процессом отправки
    /// </summary>
    public class ClientTransmittingFinalStateMachine
    {
        /// <summary>
        /// Отсроченный к отправке данные
        /// </summary>
        private (byte[]? data, BaseFrame[] frames) _DeferredData { get; set; } = (null, Array.Empty<BaseFrame>());
        /// <summary>
        /// Конечный автомат управления отправкой данных
        /// </summary>
        public ClientTransmittingState State { get; private set; } = ClientTransmittingState.Ordinary;
        /// <summary>
        /// Пустая коллекция на отправку
        /// </summary>
        public (byte[]? data, BaseFrame[] frames) EmptyTransmittingData => (null, Array.Empty<BaseFrame>());
        /// <summary>
        /// Множитель увеличения буфера в случае нехватки места
        /// </summary>
        public int BufferIncreaseFactor { get; set; } = 1;
        /// <summary>
        /// Очистка отсроченных данных
        /// </summary>
        public void ClearDeferredData()
        {
            State = ClientTransmittingState.Ordinary;

            _DeferredData = EmptyTransmittingData;
        }
        /// <summary>
        /// Установка отсроченных данных
        /// </summary>
        /// <param name="data"></param>
        public void SetDeferredData((byte[]? data, BaseFrame[] frames) data)
        {
            _DeferredData = (data.data, data.frames);

            if (State == ClientTransmittingState.Ordinary
                && data.frames.Length > 0)
            {
                State = ClientTransmittingState.BufferIncreasing;
            }
        }
        /// <summary>
        /// Установка отсроченных данных
        /// </summary>
        /// <param name="data"></param>
        public (byte[]? data, BaseFrame[] frames) GetDeferredData()
        {
            return _DeferredData;
        }
        /// <summary>
        /// Получить целевое значение размера буфера на отправку
        /// </summary>
        /// <returns></returns>
        public int GetTargetTransmitBufferSize(int dataLength) => dataLength * BufferIncreaseFactor;
        /// <summary>
        /// Получить целевое значение размера буфера на отправку
        /// </summary>
        /// <returns></returns>
        public bool CheckIfBufferSizeSuitable(int bufferSize, int dataLength) => bufferSize >= GetTargetTransmitBufferSize(dataLength);
        /// <summary>
        /// Проверить, что на другой стороне был скорректирован размер буфера приема
        /// </summary>
        /// <param name="size"></param>
        public void CheckIfAnotherPointReceiveBufferHasIncreased(int size)
        {
            if (State == ClientTransmittingState.BufferIncreasing
                && _DeferredData.data != null
                && CheckIfBufferSizeSuitable(size, _DeferredData.data.Length))
            {
                State = ClientTransmittingState.DeferredDataTransmit;
            }
        }
        /// <summary>
        /// Сбросить состояние конечного автомата
        /// </summary>
        public void Reset()
        {
            ClearDeferredData();
        }
    }
}
