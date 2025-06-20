using BinarySerializerLibrary.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Frames
{
    public class DataFrame
    {
        /// <summary>
        /// Данные кадра
        /// </summary>
        [BinaryTypeUInt(8, BinarySerializerLibrary.Enums.BinaryArgumentTypeEnum.Array)]
        public byte[]? Data { get; set; }
        /// <summary>
        /// Создать кадр
        /// </summary>
        public static DataFrame CreateFrame(byte[]? data)
        {
            return new DataFrame()
            {
                Data = data
            };
        }
    }
}
