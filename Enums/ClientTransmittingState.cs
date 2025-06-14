using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Enums
{
    public enum ClientTransmittingState
    {
        /// <summary>
        /// Обычная отправка
        /// </summary>
        Ordinary = 0,
        /// <summary>
        /// Увеличение размера буфера данных
        /// </summary>
        BufferIncreasing = 1,
        /// <summary>
        /// Отправка отложенных данных
        /// </summary>
        DeferredDataTransmit = 2
    }
}
