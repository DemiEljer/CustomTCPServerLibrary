using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Enums
{
    public enum ClientConnectionStatusEnum
    {
        /// <summary>
        /// Ручное выключение клиента
        /// </summary>
        IsNotConnected = 0,
        /// <summary>
        /// Ручное выключение клиента
        /// </summary>
        IsConnected = 1,
        /// <summary>
        /// Ручное выключение клиента
        /// </summary>
        ManualDisconnected = 2,
        /// <summary>
        /// Ошибка устаноки соединения
        /// </summary>
        ConnectionError = 3,
        /// <summary>
        /// Сброшен внутренний флаг установки соединения
        /// </summary>
        ConnectionFlagHasBeenResetted = 4,
        /// <summary>
        /// Превышено время ожидания сообщения Ping
        /// </summary>
        PingTimeoutHasBeenElapsed = 5,
        /// <summary>
        /// Провалена валидация протокола
        /// </summary>
        ProtocolValidationHasFailed = 6,
        /// <summary>
        /// Ошибка запуска клиента
        /// </summary>
        StartingException = 7,
        /// <summary>
        /// Ошибка внутренней логики клиента
        /// </summary>
        InThreadException = 8,
    }
}
