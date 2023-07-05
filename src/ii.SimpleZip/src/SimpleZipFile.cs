using ii.SimpleZip.CRC;
using ii.SimpleZip.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ii.SimpleZip.Extensions;

namespace ii.SimpleZip
{
    public class SimpleZipFile
    {
        public void Create(string inputDirectory, string outputFile, int fileCount = 55000)
        {
            if (!inputDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                inputDirectory += Path.DirectorySeparatorChar;
            }

            var crc = new CRC32();
            var entries = new List<ZipEntry>(fileCount);

            using (var finalStream = new MemoryStream())
            using (var bw = new BinaryWriter(finalStream))
            {
                // Loop through all the files and folders, doing two things:
                // a) Create a "Local File Header", add it to the output stream, and add the file/folder
                // b) Make a note of a few things that we'll need to use later
                foreach (var f in Directory.EnumerateFileSystemEntries(inputDirectory, "*", SearchOption.AllDirectories).OrderBy(o => o))
                {
                    if (File.Exists(f))
                    {
                        var entryPosition = bw.BaseStream.Position;
                        var filename = f.Replace(inputDirectory, "").Replace(@"\", "/");

                        bw.Write((byte)'P');
                        bw.Write((byte)'K');
                        bw.Write((byte)3);
                        bw.Write((byte)4);

                        // Version
                        bw.Write((byte)10);
                        bw.Write((byte)0);

                        // General flag
                        bw.Write((byte)0);
                        bw.Write((byte)0);

                        // Compression method
                        bw.Write((byte)0);
                        bw.Write((byte)0);

                        // file modification time, file modification date
                        var fi = new FileInfo(f);
                        var dt = fi.LastAccessTime.ToDOSDateTime();
                        bw.Write(dt);

                        using (var m = new FileStream(f, FileMode.Open))
                        {
                            // CRC
                            var crcValue = crc.GetCrc32(m);
                            bw.Write(crcValue);

                            // Compressed size
                            bw.Write(Convert.ToInt32(fi.Length));

                            // Uncompressed size
                            bw.Write(Convert.ToInt32(fi.Length));

                            // Filename length
                            bw.Write(filename.Length);

                            // Filename
                            bw.Write(filename.ToCharArray());

                            m.Position = 0;
                            m.CopyTo(bw.BaseStream);

                            var ze = new ZipEntry
                            {
                                LastAccess = dt,
                                CRC = crcValue,
                                CompressedSize = Convert.ToInt32(fi.Length),
                                UncompressedSize = Convert.ToInt32(fi.Length),
                                Filename = filename,
                                Offset = entryPosition,
                                Modified = fi.LastWriteTime,
                                Created = fi.CreationTime,
                                Accessed = fi.LastAccessTime
                            };

                            entries.Add(ze);
                        }
                    }
                    else
                    {
                        var entryPosition = bw.BaseStream.Position;
                        var filename = f.Replace(inputDirectory, "").Replace(@"\", "/");
                        if (!filename.EndsWith("/"))
                        {
                            filename += "/";
                        }

                        bw.Write((byte)'P');
                        bw.Write((byte)'K');
                        bw.Write((byte)3);
                        bw.Write((byte)4);

                        // Version
                        bw.Write((byte)20);
                        bw.Write((byte)0);

                        // General flag
                        bw.Write((byte)0);
                        bw.Write((byte)0);

                        // Compression method
                        bw.Write((byte)0);
                        bw.Write((byte)0);

                        // File modification date/time
                        var dt2 = File.GetLastAccessTime(f);
                        var dt3 = dt2.ToDOSDateTime();
                        bw.Write(dt3);

                        // CRC
                        bw.Write(0);

                        // Compressed size
                        bw.Write(0);

                        // Uncompressed size
                        bw.Write(0);

                        // Filename length
                        bw.Write(filename.Length);

                        // Filename
                        bw.Write(filename.ToCharArray());

                        var ze = new ZipEntry
                        {
                            LastAccess = dt3,
                            Filename = filename,
                            Offset = entryPosition,
                            IsDirectory = true
                        };

                        var di = new FileInfo(f);
                        ze.Modified = di.LastWriteTime;
                        ze.Created = di.CreationTime;
                        ze.Accessed = di.LastAccessTime;

                        entries.Add(ze);
                    }
                }

                var startOfFileHeaders = bw.BaseStream.Position;

                // Create a "Central Directory File Header" (for each file)                
                foreach (var entry in entries)
                {
                    bw.Write((byte)'P');
                    bw.Write((byte)'K');
                    bw.Write((byte)1);
                    bw.Write((byte)2);

                    // Version made by
                    bw.Write((byte)63);
                    bw.Write((byte)0);

                    // Version required
                    bw.Write((byte)10);
                    bw.Write((byte)0);

                    // General flag
                    bw.Write((short)0);

                    // Compression method
                    bw.Write((short)0);

                    // Last access
                    bw.Write(entry.LastAccess);

                    // CRC
                    bw.Write(entry.CRC);

                    // Compressed Size
                    bw.Write(Convert.ToInt32(entry.CompressedSize));

                    // Uncompressed Size
                    bw.Write(Convert.ToInt32(entry.UncompressedSize));

                    // Filename length
                    bw.Write((short)entry.Filename.Length);

                    // Extra field length
                    bw.Write((short)36);

                    // File comment length
                    bw.Write((short)0);

                    // Disk number where file starts
                    bw.Write((short)0);

                    // Internal file attributes
                    if (entry.IsDirectory)
                    {
                        bw.Write((short)Constants.Directory);
                    }
                    else
                    {
                        bw.Write((short)0);
                    }

                    // External file attributes
                    if (entry.IsDirectory)
                    {
                        bw.Write(Constants.Directory);
                    }
                    else
                    {
                        bw.Write(Constants.Archive);
                    }

                    // Relative offset of local file header
                    bw.Write(Convert.ToInt32(entry.Offset));

                    // Filename
                    bw.Write(entry.Filename.ToCharArray());

                    bw.Write((short)10);
                    bw.Write((short)32);
                    bw.Write(0);
                    bw.Write((short)1);
                    bw.Write((short)24);

                    bw.Write(entry.Modified.ToFileTime());
                    bw.Write(entry.Created.ToFileTime());
                    bw.Write(entry.Accessed.ToFileTime());
                }

                var startOfFileCentralDirectoryRecord = bw.BaseStream.Position;

                // Create the "End of Central Directory Record"
                bw.Write((byte)'P');
                bw.Write((byte)'K');
                bw.Write((byte)5);
                bw.Write((byte)6);
                bw.Write((short)0); // Disk number
                bw.Write((short)0); // Disk where central directory starts
                bw.Write((short)entries.Count); // Number of central directory records on this disk
                bw.Write((short)entries.Count); // Number of central directory records in total
                bw.Write(Convert.ToInt32(startOfFileCentralDirectoryRecord - startOfFileHeaders));
                bw.Write(Convert.ToInt32(startOfFileHeaders));
                bw.Write((short)0);

                using (var fileStream = File.Create(outputFile))
                {
                    bw.BaseStream.Seek(0, SeekOrigin.Begin);
                    bw.BaseStream.CopyTo(fileStream);
                }
                bw.Close();
            }
        }
    }
}