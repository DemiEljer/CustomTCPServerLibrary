using BinarySerializerLibrary;
using BinarySerializerLibrary.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Frames
{
    public class BaseFrameSequence
    {
        /// <summary>
        /// Код подписи протокола
        /// </summary>
        public const ulong ProtocolSignatureCode = 0x0320031609122501;
        /// <summary>
        /// Код последовательности пакетов
        /// </summary>
        [BinaryTypeUInt(64, BinarySerializerLibrary.Enums.BinaryNullableTypeEnum.Nullable)]
        public ulong? ProtocolSignature { get; set; } = ProtocolSignatureCode;
        /// <summary>
        /// Последовательность кадров
        /// </summary>
        [BinaryTypeObject(BinarySerializerLibrary.Enums.BinaryArgumentTypeEnum.Array)]
        public BaseFrame[]? Frames { get; set; } = Array.Empty<BaseFrame>();
        /// <summary>
        /// Код последовательности пакетов
        /// </summary>
        [BinaryTypeUInt(64, BinarySerializerLibrary.Enums.BinaryNullableTypeEnum.Nullable)]
        public ulong? PackageCode { get; set; } = null;
        /// <summary>
        /// Преобразование в вектор данных
        /// </summary>
        /// <param name="frame"></param>
        public static implicit operator byte[](BaseFrameSequence frames)
        {
            return BinarySerializer.Serialize(frames);
        }
    }
}
