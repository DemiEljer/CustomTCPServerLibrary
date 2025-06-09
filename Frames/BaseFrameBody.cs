using BinarySerializerLibrary.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Frames
{
    public class BaseFrameBody
    {
        /// <summary>
        /// Объект фрейма данных
        /// </summary>
        [BinaryTypeObject()]
        public DataFrame? Data { get; set; } = null;

        /// <summary>
        /// Объект фрейма пинга клиента со стороны сервера
        /// </summary>
        [BinaryTypeObject()]
        public PingServerToClientFrame? PingServerToClient { get; set; } = null;

        /// <summary>
        /// Объект фрейма пинга сервера со стороны клиента
        /// </summary>
        [BinaryTypeObject()]
        public PingClientToServerFrame? PingClientToServer { get; set; } = null;
    }
}
