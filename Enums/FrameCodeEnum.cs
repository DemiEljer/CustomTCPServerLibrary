using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Enums
{
    public enum FrameCodeEnum
    {
        None = 0,
        PingServerToClient = 1,
        PingClientToServer = 2,
        DataFrame = 128
    }
}
