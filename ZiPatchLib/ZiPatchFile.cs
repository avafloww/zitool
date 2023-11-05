using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZiPatchLib.Util;
using ZiPatchLib.Chunk;
using ZiPatchLib.Chunk.SqpkCommand;
using ZiPatchLib.Inspection;

namespace ZiPatchLib
{
    public class ZiPatchFile : IDisposable
    {
        private static readonly uint[] zipatchMagic =
        {
            0x50495A91, 0x48435441, 0x0A1A0A0D
        };

        private readonly Stream _stream;
        private readonly long _headPosition;
        public FileHeaderChunk Header { get; }

        /// <summary>
        ///     Instantiates a ZiPatchFile from a Stream
        /// </summary>
        /// <param name="stream">Stream to a ZiPatch</param>
        public ZiPatchFile(Stream stream)
        {
            _stream = stream;

            var reader = new BinaryReader(stream);
            if (zipatchMagic.Any(magic => magic != reader.ReadUInt32()))
            {
                throw new ZiPatchException();
            }

            _headPosition = _stream.Position;

            // find the file header chunk
            foreach (var chunk in GetChunks())
            {
                if (chunk is FileHeaderChunk fhdr)
                {
                    Header = fhdr;
                    break;
                }
            }

            if (Header == null)
            {
                throw new ZiPatchException("Could not find FHDR chunk");
            }

            // rewind back to the original position
            _stream.Seek(_headPosition, SeekOrigin.Begin);
        }

        /// <summary>
        /// Instantiates a ZiPatchFile from a file path
        /// </summary>
        /// <param name="filepath">Path to patch file</param>
        public static ZiPatchFile FromFileName(string filepath)
        {
            var stream = SqexFileStream.WaitForStream(filepath, FileMode.Open);
            return new ZiPatchFile(stream);
        }

        public ZiPatchChangeSet CalculateChangedFiles(ZiPatchConfig config)
    {
        var startPos = _stream.Position;
        _stream.Seek(_headPosition, SeekOrigin.Begin);

        try
        {
            var added = new HashSet<string>();
            var deleted = new HashSet<string>();
            var modified = new HashSet<string>();

            foreach (var chunk in GetChunks())
            {
                switch (chunk)
                {
                    case AddDirectoryChunk adir:
                        added.Add(adir.DirName);
                        break;
                    case DeleteDirectoryChunk deld:
                        deleted.Add(deld.DirName);
                        break;
                    case SqpkHeader sqpkH:
                        modified.Add(sqpkH.TargetFile.GetFileName(config.Platform));
                        break;
                    case SqpkFile sqpkF:
                        switch (sqpkF.Operation)
                        {
                            case SqpkFile.OperationKind.AddFile:
                            default:
                                if (sqpkF.FileOffset == 0)
                                {
                                    added.Add(sqpkF.TargetFile.RelativePath);
                                }
                                else
                                {
                                    modified.Add(sqpkF.TargetFile.RelativePath);
                                }

                                break;
                            case SqpkFile.OperationKind.DeleteFile:
                                deleted.Add(sqpkF.TargetFile.RelativePath);
                                break;
                            case SqpkFile.OperationKind.RemoveAll:
                                var expansion = sqpkF.ExpansionId > 0 ? $"ex{sqpkF.ExpansionId}" : "ffxiv";
                                deleted.Add($"sqpack/{expansion}/");
                                deleted.Add($"movie/{expansion}/");

                                break;
                            case SqpkFile.OperationKind.MakeDirTree:
                                added.Add(sqpkF.TargetFile.RelativePath);
                                break;
                        }

                        break;
                    case SqpkAddData sqpkA:
                        modified.Add(sqpkA.TargetFile.GetFileName(config.Platform));
                        break;
                    case SqpkDeleteData sqpkD:
                        modified.Add(sqpkD.TargetFile.GetFileName(config.Platform));
                        break;
                    case SqpkExpandData sqpkE:
                        modified.Add(sqpkE.TargetFile.GetFileName(config.Platform));
                        break;
                }
            }

            added.RemoveWhere(s => modified.Contains(s));

            return new ZiPatchChangeSet
            {
                Added = added,
                Deleted = deleted,
                Modified = modified
            };
        }
        finally
        {
            _stream.Seek(startPos, SeekOrigin.Begin);
        }
    }

    public ZiPatchCommandCounts CalculateActualCounts()
    {
        var startPos = _stream.Position;
        _stream.Seek(_headPosition, SeekOrigin.Begin);

        try
        {
            // Calculate actual counts
            uint total = 0, adir = 0, deld = 0, sqpkH = 0, sqpkF = 0, sqpkA = 0, sqpkD = 0, sqpkE = 0;
            foreach (var chunk in GetChunks())
            {
                total++;

                switch (chunk)
                {
                    case AddDirectoryChunk:
                        adir++;
                        break;
                    case DeleteDirectoryChunk:
                        deld++;
                        break;
                    case SqpkHeader:
                        sqpkH++;
                        break;
                    case SqpkFile:
                        sqpkF++;
                        break;
                    case SqpkAddData:
                        sqpkA++;
                        break;
                    case SqpkDeleteData:
                        sqpkD++;
                        break;
                    case SqpkExpandData:
                        sqpkE++;
                        break;
                }
            }

            return new ZiPatchCommandCounts
            {
                AddDirectories = adir,
                DeleteDirectories = deld,
                SqpkHeaderCommands = sqpkH,
                SqpkFileCommands = sqpkF,
                SqpkExpandCommands = sqpkE,
                SqpkAddCommands = sqpkA,
                SqpkDeleteCommands = sqpkD,
                TotalCommands = total
            };
        }
        finally
        {
            _stream.Seek(startPos, SeekOrigin.Begin);
        }
    }

        public IEnumerable<ZiPatchChunk> GetChunks()
        {
            ZiPatchChunk chunk;
            do
            {
                chunk = ZiPatchChunk.GetChunk(_stream);

                yield return chunk;
            } while (chunk.ChunkType != EndOfFileChunk.Type);
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}