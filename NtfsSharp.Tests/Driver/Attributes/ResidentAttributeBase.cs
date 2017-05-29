using System;
using System.Runtime.InteropServices;

namespace NtfsSharp.Tests.Driver.Attributes
{
    public abstract class ResidentAttributeBase : DummyAttributeBase
    {
        protected uint BytesUsed
        {
            get { return (uint) (Marshal.SizeOf(Header) + Marshal.SizeOf(Resident)); }
        }

        protected ResidentAttribute Resident;

        protected ResidentAttributeBase()
        {
            Header.NonResident = false;
        }

        public override byte[] GetData()
        {
            var body = GetBody();
            
            // The AttributeLength represents the number of bytes in the body (not including NTFS_ATTRIBUTE_HEADER or ResidentAttribute)
            Resident.AttributeLength = (uint) body.Length;
            // AttributeOffset is the offset from the offset from the beginning (index 0 of NTFS_ATTRIBUTE_HEADER)
            Resident.AttributeOffset = (ushort) (HeaderLength + Marshal.SizeOf(Resident));
            
            // Get body header with resident attributes
            var residentData = GetResidentData();

            // Now we have length of body, get header bytes
            var header = GetHeaderBytes((uint) (residentData.Length + body.Length));

            // Merge header + residentData + body
            var bytes = new byte[header.Length + residentData.Length + body.Length];
            
            Array.Copy(header, 0, bytes, 0, header.Length);
            Array.Copy(residentData, 0, bytes, header.Length, residentData.Length);
            Array.Copy(body, 0, bytes, header.Length + residentData.Length, body.Length);

            return bytes;
        }

        private byte[] GetResidentData()
        {
            var size = Marshal.SizeOf(Resident);
            var bytes = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(Resident, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);

            Marshal.FreeHGlobal(ptr);

            return bytes;
        }

        protected abstract byte[] GetBody();

        public struct ResidentAttribute
        {
            public uint AttributeLength;
            public ushort AttributeOffset;
            public byte IndexedFlag;
            public byte Padding;
        }

        
    }
}
