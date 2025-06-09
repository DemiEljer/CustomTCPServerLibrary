using BinarySerializerLibrary;
using BinarySerializerLibrary.Attributes;
using CustomTCPServerLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Frames
{
    public class BaseFrame
    {
        /// <summary>
        /// Тип фрейма
        /// </summary>
        [BinaryTypeUInt(8)]
        public byte FrameCode { get; set; }
        /// <summary>
        /// Запрос или ответ
        /// </summary>
        [BinaryTypeUInt(1)]
        public byte FrameTypeField { get; set; }
        /// <summary>
        /// Запрос или ответ
        /// </summary>
        public FrameReqResType FrameType
        { 
            get => FrameTypeField == 0 ? FrameReqResType.Request : FrameReqResType.Response;
            set => FrameTypeField = (value == FrameReqResType.Request ? (byte)0 : (byte)1); }
        /// <summary>
        /// Тело запроса
        /// </summary>
        [BinaryTypeObject()]
        public BaseFrameBody Body { get; set; } = new BaseFrameBody();
        /// <summary>
        /// Преобразование в вектор данных
        /// </summary>
        /// <param name="frame"></param>
        public static implicit operator byte[](BaseFrame frame)
        {
            return BinarySerializer.Serialize(frame);
        }
        /// <summary>
        /// Создать кадр
        /// </summary>
        /// <param name="code"></param>
        /// <param name="type"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static BaseFrame CreateFrame(FrameCodeEnum code, FrameReqResType type)
        {
            return new BaseFrame()
            {
                FrameCode = (byte)code,
                FrameType = type,
            };
        }
    }
}
