using BinarySerializerLibrary.Attributes;
using CustomTCPServerLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Frames
{
    public class PingServerToClientFrame
    {
        /// <summary>
        /// Таймаут отправки сообщений
        /// </summary>
        [BinaryTypeInt(32)]
        public int SendingTimeout { get; set; }
        /// <summary>
        /// Таймаут приема сообщений
        /// </summary>
        [BinaryTypeInt(32)]
        public int ReceivingTimeout { get; set; }
        /// <summary>
        /// Интервал времени отправки сообщения Ping
        /// </summary>
        [BinaryTypeInt(32)]
        public int PingInterval { get; set; }
        /// <summary>
        /// Максимальное время ожидания получения сообщения Ping
        /// </summary>
        [BinaryTypeInt(32)]
        public int PingTimeout { get; set; }
        /// <summary>
        /// Размер буфера на прием
        /// </summary>
        [BinaryTypeInt(32)]
        public int ReceiveDataBufferSize { get; set; }
        /// <summary>
        /// Размер буфера на отправку
        /// </summary>
        [BinaryTypeInt(32)]
        public int TransmitDataBufferSize { get; set; }
        /// <summary>
        /// Множитель увеличения буфера в случае нехватки места
        /// </summary>
        [BinaryTypeInt(32)]
        public int BufferIncreaseFactor = 1;
        /// <summary>
        /// Текущее время сервера
        /// </summary>
        [BinaryTypeInt(64)]
        public long ServerTime { get; set; }
        /// <summary>
        /// Создать кадр
        /// </summary>
        public static PingServerToClientFrame CreateFrame
        (
            int sendingTimeout
            , int receivingTimeout
            , long serverTime
            , int pingInterval
            , int pingTimeout
            , int receiveDataBufferSize
            , int transmitDataBufferSize
            , int bufferIncreaseFactor
        )
        {
            return new PingServerToClientFrame()
            {
                SendingTimeout = sendingTimeout
                , ReceivingTimeout = receivingTimeout
                , ServerTime = serverTime
                , PingInterval = pingInterval
                , PingTimeout = pingTimeout
                , ReceiveDataBufferSize = receiveDataBufferSize
                , TransmitDataBufferSize = transmitDataBufferSize
                , BufferIncreaseFactor = bufferIncreaseFactor
            };
        }
    }
}
