// -------------------------------------------------------------------------------------------
// Instrumind ThinkComposer
//
// Copyright (C) 2011-2015 Néstor Marcel Sánchez Ahumada.
// http://thinkcomposer.codeplex.com
//
// This file is part of ThinkComposer, which is free software licensed under the GNU General Public License.
// It is provided without any warranty. You should find a copy of the license in the root directory of this software product.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : BytesHandling.cs
// Object : Instrumind.Common.BytesHandling (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.23 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Common functions and extensions for the handling of bytes and blocks of them.
    /// </summary>
    public static class BytesHandling
    {
        /// <summary>
        /// Predefined size of tiny block (256 bytes)
        /// </summary>
        public const int BLOCK_SIZE_TINY = 256;

        /// <summary>
        /// Predefined size of little block (1KB)
        /// </summary>
        public const int BLOCK_SIZE_LITTLE = 1024;

        /// <summary>
        /// Predefined size of medium block (8KB)
        /// </summary>
        public const int BLOCK_SIZE_MEDIUM = 8192;

        /// <summary>
        /// Predefined size of big block (64KB)
        /// </summary>
        public const int BLOCK_SIZE_BIG = 65536;

        /// <summary>
        /// Predefined size of huge block (1MB)
        /// </summary>
        public const int BLOCK_SIZE_HUGE = 1048576;

        /// <summary>
        /// Set of fixed length data types and their bytes count.
        /// </summary>
        public static readonly Dictionary<Type, int> FixedSizeSimpleTypes = new Dictionary<Type, int>
        {
            { typeof(byte), 1 },
            { typeof(DateTime), 8 },
            { typeof(bool), 1 },
            { typeof(char), 2 },
            { typeof(double), 8 },
            { typeof(float), 4 },
            { typeof(int), 4 },
            { typeof(long), 8 },
            { typeof(short), 2 },
            { typeof(uint), 4 },
            { typeof(ulong), 8 },
            { typeof(ushort), 2 }
        };

        //  /// Number of bytes used during the operations
        //  static private int BufferSize           = BLOCK_SIZE_LITTLE;

        /// Maximum number of bytes that can conform a limit mark for segmenting a block or stream.
        static private int LimitMarkMaxLength = 32;

        // -------------------------------------------------------------------------------------------
        /* /// <summary>
        /// Static constructor.
        /// </summary>
        public static BytesHandling()
        {
        } */

        // Only in case of using Strong-Named assemblies (which is discouraged if using binary deserialization)
        // public static readonly WeakToStrongNameUpgradeBinder BinderForWeakToStrongNamedAssembly = new WeakToStrongNameUpgradeBinder();

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the supplied Source fixed-length type as byte array or null if unknown.
        /// </summary>
        public static byte[] GetBytesFromFixedLengthTypeObject(object Source, bool FailIfUnknown = false)
        {
            byte[] Result = null;
            var DataType = Source.GetType();

            if (DataType == typeof(byte))
                Result = ((byte)Source).IntoArray();
            else
                if (DataType == typeof(bool))
                    Result = BitConverter.GetBytes((bool)Source);
                else
                    if (DataType == typeof(char))
                        Result = BitConverter.GetBytes((char)Source);
                    else
                        if (DataType == typeof(double))
                            Result = BitConverter.GetBytes((double)Source);
                        else
                            if (DataType == typeof(float))
                                Result = BitConverter.GetBytes((float)Source);
                            else
                                if (DataType == typeof(int))
                                    Result = BitConverter.GetBytes((int)Source);
                                else
                                    if (DataType == typeof(long))
                                        Result = BitConverter.GetBytes((long)Source);
                                    else
                                        if (DataType == typeof(short))
                                            Result = BitConverter.GetBytes((short)Source);
                                        else
                                            if (DataType == typeof(uint))
                                                Result = BitConverter.GetBytes((uint)Source);
                                            else
                                                if (DataType == typeof(ulong))
                                                    Result = BitConverter.GetBytes((ulong)Source);
                                                else
                                                    if (DataType == typeof(ushort))
                                                        Result = BitConverter.GetBytes((ushort)Source);
                                                    else
                                                        if (DataType == typeof(sbyte))
                                                            Result = BitConverter.GetBytes((short)Convert.ChangeType(Source, typeof(short)));

        if (Result == null)
            if (DataType == typeof(Guid))
                Result = ((Guid)Source).ToByteArray();
            else
                if (DataType == typeof(DateTime))
                    Result = BitConverter.GetBytes(((DateTime)Source).ToBinary());
                else
                    if (DataType == typeof(Point))
                    {
                        var Value = (Point)Source;
                        Result = FusionateByteArrays(BitConverter.GetBytes(Value.X), BitConverter.GetBytes(Value.Y));
                    }
                    else
                        if (DataType == typeof(Size))
                        {
                            var Value = (Size)Source;
                            Result = FusionateByteArrays(BitConverter.GetBytes(Value.Width), BitConverter.GetBytes(Value.Height));
                        }
                        else
                            if (DataType.IsEnum)
                            {
                                Result = GetBytesFromFixedLengthTypeObject(Convert.ChangeType(Source, Enum.GetUnderlyingType(DataType)));
                            }
                            else
                                if (!DataType.FullName.StartsWith("KeyValuePair"))
                                    Result = null;  // Therefore, will be treated as object with multiple fields
                                else
                                    if (FailIfUnknown)
                                        throw new UsageAnomaly("Unknown value type: " + DataType.FullName);

            return Result;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a byte array with the content of the source stream on it.
        /// </summary>
        public static byte[] ToBytes(this Stream Source)
        {
            byte[] Result = null;

            using (var MemTorrent = new MemoryStream())
            {
                Source.CopyTo(MemTorrent);
                Result = MemTorrent.ToArray();
            }

            return Result;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        ///   Searches a byte array inside another, returning the position whether it was found.
        ///   Returns -1 if the search failed or if the passed parameteres were incorrect.
        /// </summary>
        public static int Search(this byte[] TargetBlock, byte[] SearchedBlock)
        {
            return Search(TargetBlock, SearchedBlock, 0, TargetBlock.Length - 1);
        }

        /// <summary>
        ///   Searches a byte array inside another, returning the position whether it was found (the initial serching position must be supplied).
        ///   Returns -1 if the search failed or if the passed parameteres were incorrect.
        /// </summary>
        public static int Search(this byte[] TargetBlock, byte[] SearchedBlock, int InitialPosition)
        {
            return Search(TargetBlock, SearchedBlock, InitialPosition, TargetBlock.Length - 1);
        }

        /// <summary>
        ///   Searches a byte array inside another, returning the position whether it was found (search rank positions must be supplied).
        ///   Returns -1 if the search failed or if the passed parameteres were incorrect.
        /// </summary>
        public static int Search(this byte[] TargetBlock, byte[] SearchedBlock, int InitialPosition, int FinalPosition)
        {
            int TargetBlockLength = TargetBlock.Length,
                SearchedBlockLength = SearchedBlock.Length;

            if (TargetBlockLength == 0 || SearchedBlockLength == 0 || InitialPosition < 0
                || InitialPosition > FinalPosition || FinalPosition >= TargetBlockLength)
                return -1;

            int BlockPosition = InitialPosition;
            int MatchingCount = 0;
            int SearchedPosition = 0;
            int InitialComparisonPosition = -1;
            int InitialComparisonLimit = (FinalPosition + 1) - SearchedBlockLength;

            while (BlockPosition <= FinalPosition)
            {
                if (MatchingCount == 0 && BlockPosition > InitialComparisonLimit)
                    return -1;

                if (TargetBlock[BlockPosition] == SearchedBlock[SearchedPosition])
                {
                    MatchingCount++;

                    if (InitialComparisonPosition == -1)
                        InitialComparisonPosition = BlockPosition;

                    if (MatchingCount == SearchedBlockLength)
                        return InitialComparisonPosition;

                    SearchedPosition++;
                }
                else
                    if (InitialComparisonPosition > -1)
                    {
                        BlockPosition = InitialComparisonPosition;
                        MatchingCount = 0;
                        SearchedPosition = 0;
                        InitialComparisonPosition = -1;
                    }

                BlockPosition++;
            }

            return -1;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Gives, in a target byte array, another one readed from a stream, until consume a number of bytes or before some marks.
        /// Return the index of the found mark or -1 if the consumable bytes limit was reached.
        /// For efficiency, this capture is divided in two types...
        /// One simple, applicable to any underlying Stream, for when the marks only have one byte of lenght,
        /// and other complex for when the marks are of greater lenght (in that case the Stream must support Seek).
        /// </summary>
        public static int CaptureStreamReaderSegment(ref byte[] TargetSegment, BinaryReader TorrentReader, int BytesNumberLimit, params byte[][] LimitMarks)
        {
            if (BytesNumberLimit < 1 || LimitMarks.GetLength(0) < 1)
                throw new UsageAnomaly("Limits for stream reading are not well established.", new DataWagon("BytesNumberLimit", BytesNumberLimit)
                                                                                              .Add("LimitMarks", LimitMarks));
            // Note: It could be validated that each LimitMarks item is a byte array and not a null reference,
            //       but that is omitted to avoid performance impact.

            int MarksCount = LimitMarks.GetLength(0);
            int Ind = 0;
            int MarksMaxLength = 0;

            // Next, the lenght of the marks are determined
            bool MarksAreShort = true;

            for (Ind = 0; Ind < MarksCount; Ind++)
                if (LimitMarks[Ind].Length > 1)
                {
                    MarksAreShort = false;
                    if (LimitMarks[Ind].Length > MarksMaxLength)
                        MarksMaxLength = LimitMarks[Ind].Length;
                }

            if (MarksMaxLength > LimitMarkMaxLength)
                throw new UsageAnomaly("Segment limit marks exceeds the limit established at " + LimitMarkMaxLength.ToString() + ".", MarksMaxLength);

            if (MarksAreShort)    // Simple implementation, applicable to any stream
                return CaptureSegmentWithSimpleStreamReader(ref TargetSegment, TorrentReader, BytesNumberLimit, LimitMarks);

            // Complex implementetion, only applicable for seek-capable streams.
            if (!TorrentReader.BaseStream.CanSeek)
                throw new UsageAnomaly("A seek capable Stream is required when exists limit marks with more than one byte.", TorrentReader.BaseStream);

            return CaptureSegmentWithComplexStreamReader(ref TargetSegment, TorrentReader, BytesNumberLimit, MarksMaxLength, LimitMarks);
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Gives, in a target byte array, another one readed from a stream, until consume a number of bytes or before some marks conformed by only one byte.
        /// Return the index of the found mark or -1 if the consumable bytes limit was reached.
        /// </summary>
        private static int CaptureSegmentWithSimpleStreamReader(ref byte[] TargetSegment, BinaryReader TorrentReader, int BytesNumberLimit,
                                                                params byte[][] LimitMarks)
        {
            int CaptureEndingCause = -1;
            int MarksCount = LimitMarks.GetLength(0);
            int Ind = 0;
            int Pos = 0;
            int ConsumedBytesCount = 0;
            bool FoundedMark = false;
            byte Atom = 0;
            SimpleList<byte[]> Parts = new SimpleList<byte[]>();
            byte[] Part = new byte[0];
            int PartBytesCount = 0;

            Parts.Add(Part);

            try
            {
                while (ConsumedBytesCount < BytesNumberLimit)
                {
                    Atom = TorrentReader.ReadByte();
                    ConsumedBytesCount++;
                    FoundedMark = false;

                    for (Ind = 0; Ind < MarksCount; Ind++)
                        if (Atom == LimitMarks[Ind][0])
                        {
                            FoundedMark = true;
                            CaptureEndingCause = Ind;
                            break;
                        }

                    if (FoundedMark)
                        break;

                    if (PartBytesCount == BLOCK_SIZE_TINY)
                    {
                        Part = new byte[BLOCK_SIZE_TINY];
                        PartBytesCount = 0;
                        Parts.Add(Part);
                    }

                    Part[PartBytesCount] = Atom;
                    PartBytesCount++;
                }

                throw new UsageAnomaly("Stream has been readed/consumed until reach the " + BytesNumberLimit.ToString() + " bytes limit.",
                                       new DataWagon("ConsumedBytesCount", ConsumedBytesCount));
            }
#pragma warning disable 168
            catch (EndOfStreamException Anomaly)
            {   // The possible end of the stream is expected.
            };
#pragma warning restore 168

            TargetSegment = new byte[(BLOCK_SIZE_TINY * (Parts.Count - 1)) + PartBytesCount];
            Ind = 0;
            Pos = 0;
            foreach (byte[] Fragment in Parts)
            {
                Ind++;
                CopyByteArray(TargetSegment, (Ind < Parts.Count ? Fragment : BytesHandling.ExtractSegment(Fragment, 0, PartBytesCount)), Pos, (byte)0, false);
                Pos += BLOCK_SIZE_TINY;
            }

            return CaptureEndingCause;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Gives, in a target byte array, another one readed from a stream, until consume a number of bytes or before some marks conformed by more than one byte.
        /// Return the index of the found mark or -1 if the consumable bytes limit was reached.
        /// </summary>
        private static int CaptureSegmentWithComplexStreamReader(ref byte[] TargetSegment, BinaryReader TorrentReader, int BytesNumberLimit,
                                                                 int MarksMaxLength, params byte[][] LimitMarks)
        {
            int CaptureEndingCause = -1;
            int MarksCount = LimitMarks.GetLength(0);
            SimpleList<byte[]> Parts = new SimpleList<byte[]>();
            byte[] Part = new byte[0];
            int ConsumedBytesCount = 0;
            int ConsumedBytesTotal = 0;
            int FoundedPosition = 0;
            int Pos = 0;

            while ((ConsumedBytesCount = (Part = TorrentReader.ReadBytes(BLOCK_SIZE_TINY)).Length) > 0)
            {
                ConsumedBytesTotal += ConsumedBytesCount;
                Pos = -1;
                foreach (byte[] Mark in LimitMarks)
                {
                    Pos++;
                    if ((FoundedPosition = Search(Part, Mark)) >= 0)
                    {
                        CaptureEndingCause = Pos;
                        TorrentReader.BaseStream.Seek((Part.Length - (FoundedPosition + Mark.Length)) * -1, SeekOrigin.Current);
                        Part = ExtractSegment(Part, 0, FoundedPosition);
                        break;
                    }
                }

                if (FoundedPosition >= 0)
                {
                    Parts.Add(Part);
                    break;
                }

                if (ConsumedBytesTotal >= BytesNumberLimit)
                    throw new UsageAnomaly("Stream has been readed/consumed until reach the " + BytesNumberLimit.ToString() + " bytes limit.",
                                       new DataWagon("ConsumedBytesCount", ConsumedBytesCount));

                if (ConsumedBytesCount == BLOCK_SIZE_TINY)
                    Part = ExtractSegment(Part, 0, Part.Length - MarksMaxLength);

                Parts.Add(Part);

                // If less than the expected bytes were read, then means the stream's end was reached.
                if (ConsumedBytesCount < BLOCK_SIZE_TINY)
                    break;

                TorrentReader.BaseStream.Seek(MarksMaxLength * -1, SeekOrigin.Current);
            }

            TargetSegment = new byte[((BLOCK_SIZE_TINY - MarksMaxLength) * (Parts.Count - 1)) + Part.Length];
            Pos = 0;
            foreach (byte[] Fragment in Parts)
            {
                CopyByteArray(TargetSegment, Fragment, Pos, (byte)0, false);
                Pos += Fragment.Length;
            }

            return CaptureEndingCause;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        ///  Returns an array of bytes segments divided by a separator.
        /// </summary>
        public static byte[][] Segment(this byte[] TargetBlock, byte[] Separator)
        {
            if (TargetBlock.Length == 0 || Separator.Length == 0)
                return (new byte[0][]);

            int SeparatorLength = Separator.Length;
            int InitialPosition = 0, FinalPosition = 0;
            SimpleList<byte[]> SegmentsList = new SimpleList<byte[]>();

            while ((FinalPosition = Search(TargetBlock, Separator, InitialPosition)) >= 0)
            {
                SegmentsList.Add(ExtractSegment(TargetBlock, InitialPosition, (FinalPosition - InitialPosition)));
                InitialPosition = FinalPosition + SeparatorLength;
            }

            SegmentsList.Add(ExtractSegment(TargetBlock, InitialPosition));

            return SegmentsList.GetArray();
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a copy of a byte array, from a supplied position.
        /// </summary>
        static public byte[] ExtractSegment(this byte[] Data, int InitialPosition)
        {
            return ExtractSegment(Data, InitialPosition, (Data.Length - InitialPosition));
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a copy of a byte array, from a supplied position and bytes number.
        /// </summary>
        static public byte[] ExtractSegment(this byte[] Data, int InitialPosition, int Number)
        {
            byte[] Segment = new byte[Number];

            Buffer.BlockCopy(Data, InitialPosition, Segment, 0, Number);

            return Segment;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Determines if two byte array are equal.
        /// </summary>
        static public bool ByteArraysAreEqual(this byte[] First, byte[] Second)
        {
            if (First.Length != Second.Length)
                return false;

            for (int Pos = 0; Pos < First.Length; Pos++)
                if (First[Pos] != Second[Pos])
                    return false;

            return true;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Copies a byte array into another.
        /// </summary>
        static public void CopyByteArray(this byte[] Target, byte[] Source)
        {
            CopyByteArray(Target, Source, 0, (byte)0, true);
            return;
        }

        /// <summary>
        /// Copies a byte array into another, indicating target position.
        /// </summary>
        static public void CopyByteArray(this byte[] Target, byte[] Source, int OverwritingPosition)
        {
            CopyByteArray(Target, Source, OverwritingPosition, (byte)0, true);
            return;
        }

        /// <summary>
        /// Copies a byte array into another, indicating a filler for empty spaces.
        /// </summary>
        static public void CopyByteArray(this byte[] Target, byte[] Source, byte Filler)
        {
            CopyByteArray(Target, Source, 0, Filler, true);
            return;
        }

        /// <summary>
        /// Copies a byte array into another, indicating target position and a filler for empty spaces.
        /// </summary>
        static public void CopyByteArray(this byte[] Target, byte[] Source, int OverwritingPosition, byte Filler)
        {
            CopyByteArray(Target, Source, OverwritingPosition, Filler, true);
            return;
        }

        /// <summary>
        /// Copies a byte array into another, indicating target position, a filler for empty spaces and whether to apply filling or not.
        /// </summary>
        static public void CopyByteArray(this byte[] Target, byte[] Source, int OverwritingPosition, byte Filler, bool ApplyFilling)
        {
            int Count = Source.Length;
            int FillableSpace = (Target.Length - OverwritingPosition);

            if (FillableSpace < Count)
                Count = FillableSpace;

            Buffer.BlockCopy(Source, 0, Target, OverwritingPosition, Count);

            if (ApplyFilling && FillableSpace > Count)
                for (int Pos = (OverwritingPosition + Count); Pos < Target.Length; Pos++)
                    Target[Pos] = Filler;

            return;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Insert as prefix the length of the Source byte-array.
        /// </summary>
        static public byte[] PreppendLength(this byte[] Source)
        {
            var Length = BitConverter.GetBytes(Source == null ? 0 : Source.Length);
            return FusionateByteArrays(Length, Source);
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates a byte array from the concatenation of others in an array of byte-array.
        /// Optionally, the length (Int32) can be inserted as prefix (false to omit).
        /// </summary>
        static public byte[] FusionateByteArrays(bool PreppendLength, params byte[][] Arrays)
        {
            return FusionateByteArrays(Arrays.ToList(), PreppendLength);
        }

        /// <summary>
        /// Creates a byte array from the concatenation of others in an array of byte-array.
        /// </summary>
        static public byte[] FusionateByteArrays(params byte[][] Arrays)
        {
            return FusionateByteArrays(Arrays.ToList());
        }

        /// <summary>
        /// Creates a byte array from the concatenation of others in an IList of byte-array.
        /// Optionally, the length (Int32) can be inserted on the specified zero-based index (-1 to omit).
        /// </summary>
        static public byte[] FusionateByteArrays(IList<byte[]> Arrays, bool PreppendLength = false)
        {
            int Length = 0, Ind = 0;

            for (Ind = 0; Ind < Arrays.Count; Ind++)
                Length += (Arrays[Ind] == null ? 0 : Arrays[Ind].Length);

            byte[] Result = new byte[Length + (PreppendLength ? 4 : 0)];

            if (PreppendLength)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(Length), 0, Result, 0, 4);
                Length = 4;
            }
            else
                Length = 0;

            for (Ind = 0; Ind < Arrays.Count; Ind++)
            {
                if (Arrays[Ind] != null)
                {
                    Buffer.BlockCopy(Arrays[Ind], 0, Result, Length, Arrays[Ind].Length);
                    Length += Arrays[Ind].Length;
                }
            }

            return Result;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts a byte array in a string. Uses Unicode encoding.
        /// </summary>
        static public string BytesToStringUnicode(this byte[] BytesArray)
        {
            if (BytesArray == null || BytesArray.Length == 0)
                return "";

            return System.Text.Encoding.Unicode.GetString(BytesArray);
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts a string in a byte array. Uses Unicode encoding.
        /// </summary>
        static public byte[] StringToBytesUnicode(this string Text)
        {
            if (Text.IsAbsent())
                return (new byte[0]);

            return System.Text.Encoding.Unicode.GetBytes(Text);
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts a byte array in a string. Uses UTF-8 encoding.
        /// </summary>
        static public string BytesToString(this byte[] BytesArray)
        {
            if (BytesArray == null || BytesArray.Length == 0)
                return "";

            return System.Text.Encoding.UTF8.GetString(BytesArray);
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts a string in a byte array. Uses UTF-8 encoding.
        /// </summary>
        static public byte[] StringToBytes(this string Text)
        {
            if (Text.IsAbsent())
                return (new byte[0]);

            return System.Text.Encoding.UTF8.GetBytes(Text);
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts a binary string (bytes with text preceded by its length) to string. Uses UTF-8 encoding.
        /// </summary>
        public static string BinaryStringToString(this byte[] BinaryString)
        {
            return BinaryStringToString(BinaryString, 0);
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts a binary string (bytes with text preceded by its length) to string, starting from a supplied position. Uses UTF-8 encoding.
        /// </summary>
        public static string BinaryStringToString(this byte[] BinaryString, int Position)
        {
            if (BinaryString == null || BinaryString.Length < 4)
                return "";
            int Length = BitConverter.ToInt32(BinaryString, Position);
            return BytesToString(ExtractSegment(BinaryString, Position + sizeof(int), Length));
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts a binary string (bytes with text preceded by its length) to string, starting from a supplied position. Uses Unicode encoding.
        /// </summary>
        public static string BinaryStringToStringUnicode(this byte[] BinaryString, int Position)
        {
            if (BinaryString == null || BinaryString.Length < 4)
                return "";
            int Length = BitConverter.ToInt32(BinaryString, Position);
            return BytesToStringUnicode(ExtractSegment(BinaryString, Position + sizeof(int), Length));
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts a string to binary string (bytes with text preceded by its length). Uses UTF-8 encoding.
        /// </summary>
        public static byte[] StringToBinaryString(this string Text)
        {
            if (Text == null || Text.Length == 0)
                return BitConverter.GetBytes((int)0);
            return FusionateByteArrays(BitConverter.GetBytes((int)Text.Length), StringToBytes(Text));
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts a string to binary string (bytes with text preceded by its length). Uses Unicode encoding.
        /// </summary>
        public static byte[] StringToBinaryStringUnicode(this string Text)
        {
            if (Text == null || Text.Length == 0)
                return BitConverter.GetBytes((int)0);
            var Block = StringToBytesUnicode(Text);
            return FusionateByteArrays(BitConverter.GetBytes(Block.Length), Block);
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a supplied serializable object in binary format.
        /// </summary>
        public static byte[] SerializeToBytes(object Data)
        {
            if (Data == null)
                return null;

            var Formatter = new BinaryFormatter();
            // Formatter.Binder = BinderForWeakToStrongNamedAssembly;

            var Torrent = new MemoryStream();

            try
            {
                Formatter.Serialize(Torrent, Data);
            }
            catch (Exception Anomaly)
            {
                throw new UsageAnomaly("Data cannot be serialized as byte array.", Anomaly, Data);
            }
            finally
            {
                Torrent.Flush();
                Torrent.Close();
            }

            return Torrent.GetBuffer();
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a supplied binary serialized object to the specified type.
        /// </summary>
        public static TReturn DeserializeFromBytes<TReturn>(this byte[] BytesArray)
        {
            TReturn Result = default(TReturn);

            var Formatter = new BinaryFormatter();
            // Formatter.Binder = BinderForWeakToStrongNamedAssembly;

            var Torrent = new MemoryStream(BytesArray);

            try
            {
                Result = (TReturn)Formatter.Deserialize(Torrent);
            }
            catch (Exception Anomaly)
            {
                throw new UsageAnomaly("Byte array cannot be deserialized to the specified type.", Anomaly, BytesArray);
            }
            finally
            {
                Torrent.Close();
            }

            return Result;
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Deserializes an object from the supplied stream target.
        /// </summary>
        public static TTarget Deserialize<TTarget>(Stream Source)
        {
            TTarget Content;
            // PENDING: DESERIALIZE FROM XML BASED ON USER PREFERENCES (SOAP FORMATTER)

            var Formatter = new BinaryFormatter();
            // Formatter.Binder = BinderForWeakToStrongNamedAssembly;

            try
            {
                object Data = Formatter.Deserialize(Source);
                Content = (TTarget)Data;

                if (Content == null)
                    throw new UsageAnomaly("Deserialized data is not a formal element.", Data);
            }
            catch (Exception Anomaly)
            {
                throw new UsageAnomaly("Content cannot be binary deserialized from source.", Anomaly, Source);
            }
            finally
            {
                Source.Close();
            }

            return Content;
        }

        /// <summary>
        /// Serializes the supplied object into the supplied stream target.
        /// </summary>
        // SEE: http://msdn.microsoft.com/en-us/library/system.runtime.serialization.formatters.binary.binaryformatter.binder.aspx
        // SEE: http://www.diranieh.com/NETSerialization/BinarySerialization.htm
        // SEE: http://www.codeproject.com/Articles/20178/Draw-with-Mouse#AdvancedBinarySerialization%3aDeserializinganObjectIntoaDifferentTypeThantheOneItwasSerializedInto9
        // SEE: http://stackoverflow.com/questions/5170333/c-sharp-binaryformatter-deserialize-unable-to-find-assembly-after-ilmerge
        public static void Serialize<TTarget>(TTarget Content, Stream Target)
        {
            // PENDING: CONSIDER SERIALIZE IN TEXT/XML BASED ON USER PREFERENCES (SOAP FORMATTER DOES NOT ACCEPTS GENERIC TYPES)
            // BETTER: SERIALIZER USING THE SHARP-SERIALIZER
            // SEE: http://www.sharpserializer.com/en/index.html and http://sharpserializer.codeplex.com/

            var Formatter = new BinaryFormatter();
            //TEST THIS AGAINST SIGNED ASSEMBLY!     Formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
            // Formatter.Binder = BinderForWeakToStrongNamedAssembly;

            try
            {
                Formatter.Serialize(Target, Content);
            }
            catch (Exception Anomaly)
            {
                throw new UsageAnomaly("Content cannot be binary serialized.", Anomaly, Content);
            }
            finally
            {
                Target.Flush();
                Target.Close();
            }
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Compress and returns the supplied source bytes-array data. Optionally the use of the GZip algorithm can be specified (default is Deflate).
        /// NOTE: Usage of the GZip algorithm adds cyclic-redudancy checks, which is well suitted for publishing the result externally (as in files).
        /// </summary>
        public static byte[] Compress(byte[] Source, bool AsGZip = false)
        {
            using (var Result = new MemoryStream())
            {
                if (AsGZip)
                    using (var Compressor = new GZipStream(Result, CompressionMode.Compress))
                    {
                        Compressor.Write(Source, 0, Source.Length);
                        Compressor.Flush();
                        Compressor.Close();
                        return Result.ToArray();
                    }
                else
                    using (var Compressor = new DeflateStream(Result, CompressionMode.Compress))
                    {
                        Compressor.Write(Source, 0, Source.Length);
                        Compressor.Flush();
                        Compressor.Close();
                        return Result.ToArray();
                    }
            }
        }

        /// <summary>
        /// Decompress and returns the supplied source bytes-array data. Optionally the usage, in the source data, of the GZip algorithm can be indicated (default is Deflate).
        /// NOTE: The GZip algorithm has cyclic-redudancy checks, therefore is common when the result comes from external source (as from files).
        /// </summary>
        public static byte[] Decompress(byte[] Source, bool IsGZip = false)
        {
            int ReadBytesCount = 0, BlockSize = BLOCK_SIZE_BIG;
            var Buffer = new byte[BlockSize];
            var Segments = new List<byte[]>();
            byte[] Segment = null;

            using (var CompressedTorrent = new MemoryStream(Source))
            {
                if (IsGZip)
                    using (var Decompressor = new GZipStream(CompressedTorrent, CompressionMode.Decompress))
                    {
                        while ((ReadBytesCount = Decompressor.Read(Buffer, 0, BlockSize)) > 0)
                        {
                            Segment = (ReadBytesCount < BlockSize ? Buffer.ExtractSegment(0, ReadBytesCount) : Buffer);
                            Segments.Add(Segment);
                            Buffer = new byte[BlockSize];
                        }

                        Decompressor.Close();

                    }
                else
                    using (var Decompressor = new DeflateStream(CompressedTorrent, CompressionMode.Decompress))
                    {
                        while ((ReadBytesCount = Decompressor.Read(Buffer, 0, BlockSize)) > 0)
                        {
                            Segment = (ReadBytesCount < BlockSize ? Buffer.ExtractSegment(0, ReadBytesCount) : Buffer);
                            Segments.Add(Segment);
                            Buffer = new byte[BlockSize];
                        }

                        Decompressor.Close();
                    }
            }

            return FusionateByteArrays(Segments);
        }

        // =====================================================================================================================
        /// <summary>
        /// Allows to work with weak serialized assemblies.
        /// </summary>
        public class WeakToStrongNameUpgradeBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                try
                {
                    //Get the name of the assembly, ignoring versions and public keys. 
                    string shortAssemblyName = assemblyName.Split(',')[0];
                    var assembly = Assembly.Load(shortAssemblyName);
                    var type = assembly.GetType(typeName);
                    return type;
                }
                catch (Exception)
                {
                    //Revert to default binding behaviour. 
                    return null;
                }
            }
        } 

        // =====================================================================================================================
        public static T StructFromByteArray<T>(byte[] bytes) where T : struct
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int size = Marshal.SizeOf(typeof(T));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(bytes, 0, ptr, size);
                object obj = Marshal.PtrToStructure(ptr, typeof(T));
                return (T)obj;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        public static byte[] StructToByteArray<T>(T obj) where T : struct
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int size = Marshal.SizeOf(typeof(T));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(obj, ptr, true);
                byte[] bytes = new byte[size];
                Marshal.Copy(ptr, bytes, 0, size);
                return bytes;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        // -------------------------------------------------------------------------------------------
    }
}