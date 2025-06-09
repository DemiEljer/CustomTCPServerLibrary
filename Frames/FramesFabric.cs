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
            var baseFrame = BaseFrame.CreateFrame(FrameCodeEnum.DataFrame, FrameReqResType.Request);

            baseFrame.Body.Data = DataFrame.CreateFrame(data);

            return baseFrame;
        }

        public static BaseFrame CreatePingClientToServerFrame(long clientTime)
        {
            var baseFrame = BaseFrame.CreateFrame(FrameCodeEnum.PingClientToServer, FrameReqResType.Request);

            baseFrame.Body.PingClientToServer = PingClientToServerFrame.CreateFrame(clientTime);

            return baseFrame;
        }

        public static BaseFrame CreatePingServerToClientFrame(
            int sendingTimeout
            , int receivingTimeout
            , long serverTime
            , int pingInterval
            , int pingTimeout)
        {
            var baseFrame = BaseFrame.CreateFrame(FrameCodeEnum.PingServerToClient, FrameReqResType.Request);

            baseFrame.Body.PingServerToClient = PingServerToClientFrame.CreateFrame(
                sendingTimeout
                , receivingTimeout
                , serverTime
                , pingInterval
                , pingTimeout);

            return baseFrame;
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
