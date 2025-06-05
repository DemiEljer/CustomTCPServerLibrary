using BinarySerializerLibrary;
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
            return BaseFrame.CreateFrame(FrameCodeEnum.DataFrame, FrameReqResType.Request, data);
        }

        public static BaseFrame CreatePingClientToServerFrame(long clientTime)
        {
            byte[] data = BinarySerializer.Serialize(PingClientToServerFrame.CreateFrame(clientTime));

            return BaseFrame.CreateFrame(FrameCodeEnum.PingClientToServer, FrameReqResType.Request, data);
        }

        public static BaseFrame CreatePingServerToClientFrame(
            int sendingTimeout
            , int receivingTimeout
            , long serverTime
            , int pingInterval
            , int pingTimeout)
        {
            byte[] data = BinarySerializer.Serialize(PingServerToClientFrame.CreateFrame(
                sendingTimeout
                , receivingTimeout
                , serverTime
                , pingInterval
                , pingTimeout));

            return BaseFrame.CreateFrame(FrameCodeEnum.PingServerToClient, FrameReqResType.Request, data);
        }

        public static object? ParseFrame(byte[] data)
        {
            var baseFrame = BinarySerializer.Deserialize<BaseFrame>(data);

            if (baseFrame != null && baseFrame.Body != null)
            {
                switch ((FrameCodeEnum)baseFrame.FrameCode)
                {
                    case FrameCodeEnum.DataFrame: return DataFrame.CreateFrame(baseFrame.Body);
                    case FrameCodeEnum.PingServerToClient: return BinarySerializer.Deserialize<PingServerToClientFrame>(baseFrame.Body);
                    case FrameCodeEnum.PingClientToServer: return BinarySerializer.Deserialize<PingClientToServerFrame>(baseFrame.Body);
                    default: return null;
                }
            }
            else
            {
                return null;
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
    }
}
