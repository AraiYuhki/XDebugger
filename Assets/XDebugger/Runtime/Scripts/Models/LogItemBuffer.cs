using System;
using Xeon.Common.FlyweightScrollView.Model;
using Xeon.XDebugger.Console;

namespace Xeon.XDebugger.Model
{
    public class LogItemBuffer : CircularBuffer<LogItemData>
    {      
        public LogItemBuffer(int capacity) : base(capacity, Array.Empty<LogItemData>())
        {
        }

        public LogItemBuffer(int capacity, bool isFill) : base(capacity, isFill)
        {
        }
    }
}