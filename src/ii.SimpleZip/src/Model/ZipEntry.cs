using System;

namespace ii.SimpleZip.Model
{
    public class ZipEntry
    {
        public string Filename { get; set; }
        public int LastAccess { get; set; }
        public uint CRC { get; set; }
        public int CompressionType { get; set; }
        public short VersionCreated { get; set; }
        public short VersionRequired { get; set; }
        public short Flags { get; set; }
        public int CompressedSize { get; set; }
        public int UncompressedSize { get; set; }
        public long Offset { get; set; }
        public bool IsDirectory { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Created { get; set; }
        public DateTime Accessed { get; set; }
    }
}