using System;

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
