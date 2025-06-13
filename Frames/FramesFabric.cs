using BinarySerializerLibrary;
using BinarySerializerLibrary.Base;
using CustomTCPServerLibrary.Base;
using CustomTCPServerLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CustomTCPServerLibrary.Frames
{
    public static class FramesFabric
    {
        public static BaseFrame CreateDataFrame(byte[]? data)
        {
            var baseFrame = BaseFrame.CreateFrame(FrameCodeEnum.DataFrame, FrameReqResType.Request);

            baseFrame.Body.Data = DataFrame.CreateFrame(data);

            return baseFrame;
        }

        public static BaseFrame CreatePingClientToServerFrame(long clientTime, int transmitDataBufferSize)
        {
            var baseFrame = BaseFrame.CreateFrame(FrameCodeEnum.PingClientToServer, FrameReqResType.Request);

            baseFrame.Body.PingClientToServer = PingClientToServerFrame.CreateFrame(clientTime, transmitDataBufferSize);

            return baseFrame;
        }

        public static BaseFrame CreatePingServerToClientFrame(
            int sendingTimeout
            , int receivingTimeout
            , long serverTime
            , int pingInterval
            , int pingTimeout
            , int receiveDataBufferSize
            , int transmitDataBufferSize)
        {
            var baseFrame = BaseFrame.CreateFrame(FrameCodeEnum.PingServerToClient, FrameReqResType.Request);

            baseFrame.Body.PingServerToClient = PingServerToClientFrame.CreateFrame(
                sendingTimeout
                , receivingTimeout
                , serverTime
                , pingInterval
                , pingTimeout
                , receiveDataBufferSize
                , transmitDataBufferSize);

            return baseFrame;
        }
        /// <summary>
        /// ������ ���������� ������������������� ������
        /// </summary>
        private static SafeIndexer _FrameSequenceCodeIndexer { get; } = new();

        public static BaseFrameSequence CreateFrameSequence(BaseFrame[] frames)
        {
            return new BaseFrameSequence()
            {
                PackageCode = _FrameSequenceCodeIndexer.GetNextIndex()
                , Frames = frames
            };
        }

        public static object? ParseFrame(byte[] data)
        {
            var baseFrame = BinarySerializer.Deserialize<BaseFrame>(data);

            if (baseFrame != null && baseFrame.Body != null)
            {
                switch ((FrameCodeEnum)baseFrame.FrameCode)
                {
                    case FrameCodeEnum.DataFrame: return baseFrame.Body.Data;
                    case FrameCodeEnum.PingServerToClient: return baseFrame.Body.PingServerToClient;
                    case FrameCodeEnum.PingClientToServer: return baseFrame.Body.PingClientToServer;
                    default: return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static object? GetFrameBody(BaseFrame frame)
        {
            switch ((FrameCodeEnum)frame.FrameCode)
            {
                case FrameCodeEnum.DataFrame: return frame.Body.Data;
                case FrameCodeEnum.PingServerToClient: return frame.Body.PingServerToClient;
                case FrameCodeEnum.PingClientToServer: return frame.Body.PingClientToServer;
                default: return null;
            }
        }

        public static void ParseFrame(byte[] data, Action<object>? frameHandler)
        {
            var frame = ParseFrame(data);

            if (frame != null)
            {
                frameHandler?.Invoke(frame);
            }
        }

        public static BaseFrameSequence?[] ParseFrameSequence(byte[] data, out bool areFramesValid)
        {
            bool _areFramesValid = false;

            IEnumerable<BaseFrameSequence?> _getBaseFrameSequences(byte[] data)
            {
                BinaryArrayReader reader = new BinaryArrayReader(data);

                // �������������� ��������, ����� ��������� ������������������� ��������� � ������ (�������� �� ���� ���)
                while (!reader.IsEndOfArray)
                {
                    // �������� ������������ ��� ��������� ����������� �������������������
                    reader.MakeAlignment(BinarySerializerLibrary.Enums.AlignmentTypeEnum.ByteAlignment);

                    var frameSequence = BinarySerializer.Deserialize<BaseFrameSequence>(reader);

                    if (frameSequence is not null
                        && frameSequence.ProtocolSignature == BaseFrameSequence.ProtocolSignatureCode
                        && frameSequence.Frames is not null
                        && frameSequence.PackageCode is not null)
                    {
                        _areFramesValid = true;

                        yield return frameSequence;
                    }
                    else
                    {
                        yield return null;
                    }
                }
            }

            var resultArray = _getBaseFrameSequences(data).ToArray();

            areFramesValid = _areFramesValid;

            return resultArray;
        }

        public static IEnumerable<BaseFrame> GetBaseFramesFromSequences(IEnumerable<BaseFrameSequence?> frameSequences)
        {
            foreach (var frameSequence in frameSequences)
            {
                if (frameSequence != null && frameSequence.Frames != null)
                {
                    foreach (var frame in frameSequence.Frames)
                    {
                        yield return frame;
                    }
                }
            }
        }
    }
}
