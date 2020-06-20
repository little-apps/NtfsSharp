using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace NtfsSharp.Helpers
{
    public static class MarshalHelper
    {
        public enum Endianness
        {
            BigEndian,
            LittleEndian
        }

        /// <summary>
        /// Adjusts the endianness of data for marshalling
        /// </summary>
        /// <param name="type">Structure that will contain <paramref name="data"/></param>
        /// <param name="data">Data to be adjusted to be little/big endian and that'll be marshalled to <paramref name="type"/></param>
        /// <param name="endianness">The endian the <paramref name="data"/> should be</param>
        /// <param name="startOffset">Starting offseting in <paramref name="data"/> (default is 0)</param>
        /// <remarks>Modified from https://stackoverflow.com/a/15020402/533242. </remarks>
        private static void MaybeAdjustEndianness(Type type, byte[] data, Endianness endianness, int startOffset = 0)
        {
            if (BitConverter.IsLittleEndian == (endianness == Endianness.LittleEndian))
            {
                // nothing to change => return
                return;
            }

            foreach (var field in type.GetFields())
            {
                var fieldType = field.FieldType;

                if (field.IsStatic)
                    // don't process static fields
                    continue;

                if (fieldType == typeof(string)) 
                    // don't swap bytes for strings
                    continue;

                var offset = Marshal.OffsetOf(type, field.Name).ToInt32();

                // handle enums
                if (fieldType.IsEnum)
                    fieldType = Enum.GetUnderlyingType(fieldType);

                // check for sub-fields to recurse if necessary
                var subFields = fieldType.GetFields().Where(subField => subField.IsStatic == false).ToArray();

                var effectiveOffset = startOffset + offset;

                if (subFields.Length == 0)
                {
                    var sizeConst = 0;

                    // Check if UnmanagedType attribute exists and is set to either UnmanagedType.ByValArray or UnmanagedType.ByValTStr and get SizeConst value
                    foreach (var customAttr in field.CustomAttributes)
                    {
                        if (customAttr.NamedArguments == null ||
                            !customAttr.ConstructorArguments.Any(constructorArgs =>
                                constructorArgs.ArgumentType == typeof(UnmanagedType) &&
                                ((int)constructorArgs.Value == (int)UnmanagedType.ByValArray ||
                                 (int)constructorArgs.Value == (int)UnmanagedType.ByValTStr)))
                            continue;

                        // Get value of SizeConst
                        sizeConst =
                            (int)customAttr.NamedArguments
                                .First(namedArg => namedArg.MemberName == nameof(MarshalAsAttribute.SizeConst))
                                .TypedValue.Value;
                    }

                    Array.Reverse(data, effectiveOffset, sizeConst > 0 ? sizeConst : Marshal.SizeOf(fieldType));
                }
                else
                {
                    // recurse
                    MaybeAdjustEndianness(fieldType, data, endianness, effectiveOffset);
                }
            }
        }

        /// <summary>
        /// Translates raw bytes into a structure and adjusts the endianness (if needed).
        /// </summary>
        /// <typeparam name="T">Structure type to translate to.</typeparam>
        /// <param name="rawData">Raw bytes to translate.</param>
        /// <param name="endianness">Endinness to use before translating to structure.</param>
        /// <returns>Structure with type <typeparamref name="T"/>.</returns>
        public static T ToStructure<T>(this byte[] rawData, Endianness endianness) where T : struct
        {
            MaybeAdjustEndianness(typeof(T), rawData, endianness);

            var handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);

            try
            {
                var rawDataPtr = handle.AddrOfPinnedObject();
                return (T) Marshal.PtrToStructure(rawDataPtr, typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }

        /// <summary>
        /// Translates raw bytes into a structure.
        /// </summary>
        /// <typeparam name="T">Structure type to translate to.</typeparam>
        /// <param name="bytes">Raw bytes to translate.</param>
        /// <param name="offset">Offset in bytes to start at (default is 0)</param>
        /// <returns>Structure with type <typeparamref name="T"/>.</returns>
        public static T ToStructure<T>(this byte[] bytes, uint offset = 0)
        {
            var bytesPtr = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            var ret = Marshal.PtrToStructure<T>(IntPtr.Add(bytesPtr.AddrOfPinnedObject(), (int) offset));

            bytesPtr.Free();

            return ret;
        }

        /// <summary>
        /// Extracts bytes from offset to offset + length.
        /// </summary>
        /// <param name="bytes">Bytes to extract from.</param>
        /// <param name="offset">Starting offset of bytes.</param>
        /// <param name="length">Length of bytes from offset.</param>
        /// <returns>Extracted bytes.</returns>
        public static byte[] GetBytesAtOffset(this byte[] bytes, uint offset, uint length)
        {
            var newBytes = new byte[length];

            Array.Copy(bytes, (int) offset, newBytes, 0, newBytes.Length);

            return newBytes;
        }
    }
}
