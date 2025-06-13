using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Base
{
    public class SafeIndexer
    {
        private ulong _NextIndex { get; set; } = 1;

        private object _LockObject { get; } = new object();

        public ulong GetNextIndex()
        {
            lock (_LockObject)
            {
                ulong nextIndex = _NextIndex;
                _NextIndex++;
                if (_NextIndex == 0)
                {
                    _NextIndex = 1;
                }
                return nextIndex;
            }
        }
    }
}
