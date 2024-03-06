using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TSRE4.Pak
{
    public class Ts4PakStr
    {
        public List<EntryInfo> Entries = new List<EntryInfo>();

        public void Load(string path, uint numFiles)
        {
            var data = File.ReadAllBytes(path).AsSpan();
            var reader = new SpanReader();

            for (int i = 0; i < numFiles; i++)
            {
                var entry = ReadEntry(data, reader);
                Entries.Add(entry);
            }

            var remaining = data.Slice(reader.Offset);
            for (int i = 0; i < numFiles; i++)
            {
                var entry         = Entries[i];
                var strChunkStart = remaining.Slice((int)entry.StringOffset);
                var strLen        = GetStringLen(strChunkStart);
                var str           = Encoding.ASCII.GetString(strChunkStart.Slice(0, strLen));
                entry.Name        = str;
                Entries[i]        = entry;
            }
        }

        public int GetStringLen(Span<byte> span)
        {
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] == 0)
                    return i;
            }

            return -1;
        }

        private EntryInfo ReadEntry(ReadOnlySpan<byte> data, SpanReader reader)
        {
            var entry  = new EntryInfo();
            entry.Unk1 = reader.ReadUint(data);
            entry.Unk2 = reader.ReadUint(data);
            entry.Unk3 = reader.ReadUint(data);
            entry.Unk4 = reader.ReadUint(data);

            entry.Unk5         = reader.ReadUint(data);
            entry.Unk6         = reader.ReadUint(data);
            entry.StringOffset = reader.ReadUint(data);
            entry.Unk8         = reader.ReadUint(data);

            return entry;
        }

        public struct EntryInfo
        {
            public const int SIZE = 32;

            public uint Unk1;
            public uint Unk2;
            public uint Unk3;
            public uint Unk4;

            public uint Unk5;
            public uint Unk6;
            public uint StringOffset;
            public uint Unk8;

            public string Name;
        }
    }
}
