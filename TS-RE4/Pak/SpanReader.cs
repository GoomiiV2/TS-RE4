using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TSRE4.Pak
{
    public class SpanReader
    {
        public int Offset;
        public bool IsBigEndian = true;

        public uint ReadUint(ReadOnlySpan<byte> data)
        {
            uint val = 0;
            if (IsBigEndian)
                val = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(Offset));
            else
                val = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(Offset));

            Offset += 4;
            return val;
        }

        public ushort ReadUShort(ReadOnlySpan<byte> data)
        {
            ushort val = 0;
            if (IsBigEndian)
                val = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(Offset));
            else
                val = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(Offset));

            Offset += 2;
            return val;
        }

        public byte ReadByte(ReadOnlySpan<byte> data)
        {
            byte val = data[Offset];
            Offset += 1;
            return val;
        }
    }
}
