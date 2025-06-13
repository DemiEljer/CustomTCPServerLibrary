using BinarySerializerLibrary.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Frames
{
    public class PingClientToServerFrame
    {
        /// <summary>
        /// Текущее время сервера
        /// </summary>
        [BinaryTypeInt(64)]
        public long ClientTime { get; set; }
        /// <summary>
        /// Размер буфера на отправку
        /// </summary>
        [BinaryTypeInt(32)]
        public int TransmitDataBufferSize { get; set; }
        /// <summary>
        /// Создать кадр
        /// </summary>
        public static PingClientToServerFrame CreateFrame(long clientTime, int transmitDataBufferSize)
        {
            return new PingClientToServerFrame()
            {
                ClientTime = clientTime
                , TransmitDataBufferSize = transmitDataBufferSize
            };
        }
    }
}
