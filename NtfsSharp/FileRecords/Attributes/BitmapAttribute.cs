﻿using NtfsSharp.FileRecords.Attributes.Base;
using System.Collections;

namespace NtfsSharp.FileRecords.Attributes
{
    public class BitmapAttribute : AttributeBodyBase
    {
        /// <summary>
        /// In an index, the bit field shows which index entries are in use. Each bit represents one VCN of the index allocation. 
        /// In the $MFT, the bit field shows which FILE records are in use. 
        /// </summary>
        public readonly BitArray Bitmap;

        public BitmapAttribute(AttributeHeader header) : base(header)
        {
            Bitmap = new BitArray(Body);
        }
    }
}
