using Serilog;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static TSRE4.Pak.Ts4PakStr;

namespace TSRE4.Pak
{
    public class PCK2
    {
        public Header Head;
        public List<FileEntry> Entries;
        public Ts4PakStr PakStrings;
        public BinaryReader[] Chunks;
        private Dictionary<string, (FileEntry entry, EntryInfo meta)> FileLookup = new Dictionary<string, (FileEntry, EntryInfo)>();

        public void Load(string path)
        {
            var data = File.ReadAllBytes(path);
            var reader = new SpanReader();
            ReadHead(data, reader);

            Log.Information("Laoded PAK: {magic} Num Files: {numFiles:N0}, Num Chunks: {numChunks}", Head.MagicStr, Head.NumFiles, Head.NumChunks);

            Entries = new List<FileEntry>((int)Head.NumFiles);
            for (int i = 0; i < Head.NumFiles; i++)
            {
                var entry = ReadFileEntry(data, reader);
                Entries.Add(entry);
            }

            PakStrings = new Ts4PakStr();
            PakStrings.Load($"{path}.str", Head.NumFiles);

            Chunks = new BinaryReader[Head.NumChunks];
            for (int i = 0; i < Head.NumChunks; i++)
            {
                var chunkPath = $"{path}.{i:D2}";
                var chunk     = new BinaryReader(File.OpenRead(chunkPath));
                Chunks[i]     = chunk;
            }

            for (int i = 0; i < Head.NumFiles; i++)
            {
                var fileEntry = Entries[i];
                var fileInfo  = PakStrings.Entries[i];
                var fileName = fileInfo.Name;
                FileLookup.Add(fileName, (fileEntry, fileInfo));
            }
        }

        private void ReadHead(ReadOnlySpan<byte> data, SpanReader reader)
        {
            Head.MAGIC     = reader.ReadUint(data);
            Head.NumFiles  = reader.ReadUint(data);
            Head.UNK1      = reader.ReadUint(data);
            Head.UNK2      = reader.ReadByte(data);
            Head.NumChunks = reader.ReadByte(data);
            Head.UNK3      = reader.ReadUShort(data);
        }

        private FileEntry ReadFileEntry(ReadOnlySpan<byte> data, SpanReader reader)
        {
            var entry      = new FileEntry();
            entry.Size     = reader.ReadUint(data);
            entry.UNK1     = reader.ReadUint(data);
            entry.Offset   = reader.ReadUint(data);
            entry.UNK3     = reader.ReadUShort(data);
            entry.UNK4     = reader.ReadByte(data);
            entry.ChunkIdx = reader.ReadByte(data);

            return entry;
        }

        public void ExtractFile(string outDir, string file)
        {
            try
            {
                if (FileLookup.TryGetValue(file, out var entry))
                {
                    Log.Information("Extracting {file} ({entry:N0}) chunk: {chunk}", file, entry.entry.Size, entry.entry.ChunkIdx);

                    var fullOutPath = Path.Combine(outDir, file);
                    Directory.CreateDirectory(Path.GetDirectoryName(fullOutPath));

                    var chunk = Chunks[entry.entry.ChunkIdx];
                    chunk.BaseStream.Seek(entry.entry.Offset, SeekOrigin.Begin);
                    var data = chunk.ReadBytes((int)entry.entry.Size);
                    File.WriteAllBytesAsync(fullOutPath, data);
                }
                else
                {
                    Log.Error("Couldn't extract file {file}", file);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Couldn't extract file {file} {ex}", file);
            }
        }

        public void ExtractAllFiles(string outDir)
        {
            var groupedByChunk = FileLookup.Values.GroupBy(x => x.entry.ChunkIdx);
            var extractingTasks = new List<Task>();
            foreach (var item in groupedByChunk)
            {
                var task = Task.Factory.StartNew(() =>
                {
                    var ordered = item.OrderBy(x => x.entry.Offset);
                    foreach (var fileInfo in ordered)
                    {
                        ExtractFile(outDir, fileInfo.meta.Name);
                    }
                });

                extractingTasks.Add(task);
            }

            Task.WaitAll(extractingTasks.ToArray());
        }

        public struct Header
        {
            public uint MAGIC;
            public uint NumFiles;
            public uint UNK1;
            public byte UNK2;
            public byte NumChunks;
            public ushort UNK3;

            public string MagicStr => Encoding.ASCII.GetString(BitConverter.GetBytes(MAGIC).Reverse().ToArray()); // >,>
        }

        public struct FileEntry
        {
            public uint Size;
            public uint UNK1;
            public uint Offset;
            public ushort UNK3;
            public byte UNK4;
            public byte ChunkIdx;
        }
    }
}
