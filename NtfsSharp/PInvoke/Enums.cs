using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NtfsSharp.PInvoke
{
    public static class Enums
    {
        [Flags]
        public enum IndexEntryFlags : byte
        {
            HasSubNode = 1 << 0,
            IsLastEntry = 1 << 1
        }
    }
}
