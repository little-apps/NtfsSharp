using System.Runtime.InteropServices;

namespace NtfsSharp.Tests.Driver
{
    public abstract class Byteable
    {
        /// <summary>
        /// Converts a structure into it's bytes
        /// </summary>
        /// <typeparam name="T">Structure type</typeparam>
        /// <param name="structure">Structure to convert</param>
        /// <param name="size">Number of bytes to be return. If zero, the <seealso cref="Marshal"/>.SizeOf of the structure is used. (default: 0)</param>
        /// <returns>Byte array containing structure</returns>
        /// <remarks>If the size is bigger than the actual structure, any extra bytes will be 0x00.</remarks>
        protected static byte[] StructureToBytes<T>(T structure, uint size = 0) where T : struct
        {
            if (size == 0)
                size = (uint)Marshal.SizeOf(structure);

            var arr = new byte[size];
            var ptr = Marshal.AllocHGlobal((int)size);

            Marshal.StructureToPtr(structure, ptr, true);

            Marshal.Copy(ptr, arr, 0, (int)size);

            Marshal.FreeHGlobal(ptr);

            return arr;
        }
    }
}
