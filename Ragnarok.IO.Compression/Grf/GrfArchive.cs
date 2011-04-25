using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Ionic.Zlib;
using Ragnarok.IO.Compression.Cryptography;

namespace Ragnarok.IO.Compression
{
    /// <summary>
    /// A class for interacting with Ragnarok Online grf files.
    /// </summary>
    public class GrfArchive : IDisposable
    {
        private const int GRF_NAMELEN = 0x100;
        private const int GRF_ITEM_SIZE = 0x11 + GRF_NAMELEN;

        private static readonly string GRF_HEADER = "Master of Magic";
        private static readonly int GRF_HEADER_LEN = GRF_HEADER.Length;
        private static readonly int GRF_HEADER_MID_LEN = GRF_HEADER.Length + 0xF;
        private static readonly int GRF_HEADER_FULL_LEN = GRF_HEADER.Length + 0x1F;

        private static byte[] CryptWatermark = new byte[]
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 
            0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E
        };

        private int m_IntVersion;
        private GrfItemCollection m_Items;
        private bool m_AllowCrypt;
        private bool m_ForceRepack = false;

        internal FileStream FileStream;
        internal bool Modified;
        internal bool IsDisposed = false;

        #region Properties
        /// <summary>
        /// Wether or not the <see cref="GrfArchive"/> is read only.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the <see cref="GrfArchive"/> is closed.
        /// </exception>
        public bool IsReadOnly
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(this.GetType().Name);

                return !FileStream.CanWrite;
            }
        }
        /// <summary>
        /// The version of the <see cref="GrfArchive"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the <see cref="GrfArchive"/> is closed.
        /// </exception>
        public Version Version
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(this.GetType().Name);

                return new Version((m_IntVersion & 0xFF00) >> 8, m_IntVersion & 0x00FF);
            }
        }
        /// <summary>
        /// A <see cref="GrfItemCollection"/> with the items of the <see cref="GrfArchive"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the <see cref="GrfArchive"/> is closed.
        /// </exception>
        public GrfItemCollection Items
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(this.GetType().Name);

                return m_Items;
            }
        }
        /// <summary>
        /// The amount of allocated but unused space in the <see cref="GrfArchive"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the <see cref="GrfArchive"/> is closed.
        /// </exception>
        public int UnusedSpace
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(this.GetType().Name);

                throw new NotImplementedException();
            }
        }

        #endregion

        internal GrfArchive() { }

        #region Public API
        /// <summary>
        /// Creates a new <see cref="GrfArchive"/> with the default version.
        /// </summary>
        /// <param name="filename">The path and filename were to create the new <see cref="GrfArchive"/>.</param>
        public GrfArchive(string filename) : this(filename, new Version(2, 0)) { }
        /// <summary>
        /// Creates a new <see cref="GrfArchive"/> with a specific version.
        /// </summary>
        /// <param name="filename">The path and filename were to create the new <see cref="GrfArchive"/>.</param>
        /// <param name="version">The version of the <see cref="GrfArchive"/>.</param>
        public GrfArchive(string filename, Version version)
        {
            FileStream = File.Open(filename, FileMode.Create);

            m_IntVersion = ((version.Major & 0xFF) << 8) | (version.Minor & 0xFF);

            if (m_IntVersion > 0x200)
                m_IntVersion = 0x200;
            else if (m_IntVersion < 0x100)
                m_IntVersion = 0x100;

            ReadHeader();
        }
        /// <summary>
        /// Merges another <see cref="GrfArchive"/> with this <see cref="GrfArchive"/>.
        /// </summary>
        /// <param name="grf">The <see cref="GrfArchive"/> to add the current one.</param>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the <see cref="GrfArchive"/> is closed.
        /// </exception>
        public void Merge(GrfArchive grf)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(this.GetType().Name);

            for (int i = 0; i < grf.m_Items.Count; i++)
                m_Items.Add(grf.m_Items[i]);
        }
        /// <summary>
        /// Extracts the contents of the <see cref="GrfArchive"/> to a 
        /// specified location.
        /// </summary>
        /// <param name="path">The path where to extract the files to.</param>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the <see cref="GrfArchive"/> is closed.
        /// </exception>
        public void ExtractTo(string path)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(this.GetType().Name);

            path = path.Replace('/', '\\');
            if (path != string.Empty && path[path.Length - 1] != '\\')
                path += "\\";

            for (int i = 0; i < m_Items.Count; i++)
                m_Items[i].ExtractTo(path + m_Items[i].FullName);
        }
        /// <summary>
        /// Writes all unwritten data to the disk.
        /// </summary>
        public void Flush()
        {
            if (FileStream.CanWrite && Modified)
            {
                switch (m_IntVersion & 0xFF00)
                {
                    case 0x0200: FlushVer2(); break;
                    case 0x0100: FlushVer1(); break;
                    default: throw new GrfException();
                }
            }
        }
        /// <summary>
        /// Repacks all data and releases all data.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the <see cref="GrfArchive"/> is closed.
        /// </exception>
        public void Repack()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(this.GetType().Name);

            if (!FileStream.CanWrite)
                throw new InvalidOperationException("The GrfArchive is read-only.");

            Close();
        }
        /// <summary>
        /// Writes all unwritten data to the disk and releases all data.
        /// </summary>
        public void Close()
        {
            ((IDisposable)this).Dispose();
        }
        /// <summary>
        /// Opens an existing <see cref="GrfArchive"/>.
        /// </summary>
        /// <param name="filename">The path to the <see cref="GrfArchive"/>.</param>
        /// <param name="isreadonly">Wether or not to open the file as read only.</param>
        /// <returns>The opened <see cref="GrfArchive"/>.</returns>
        public static GrfArchive Open(string filename, bool isreadonly)
        {
            if (string.IsNullOrEmpty(filename))
                throw new GrfException();

            GrfArchive grf = new GrfArchive();
            grf.FileStream = isreadonly ? File.OpenRead(filename) : File.Open(filename, FileMode.OpenOrCreate);
            grf.ReadHeader();

            return grf;
        }

        #endregion
        #region Private API

        private static string SwapNibbles(IntPtrEx src, int len)
        {
            using (IntPtrEx pBuffer = new IntPtrEx(new byte[len]))
            {
                return SwapNibbles(pBuffer, src, len).Read<string>();
            }
        }

        private static byte[] SwapNibbles(byte[] dst, IntPtrEx src, int len)
        {
            for (int i = 0; i < len; i++)
                dst[i] = (byte)((src[i] << 4) | (src[i] >> 4));

            return dst;
        }

        private static IntPtrEx SwapNibbles(IntPtrEx dst, string src, int len)
        {
            using (IntPtrEx pSrc = new IntPtrEx(src))
            {
                return SwapNibbles(dst, pSrc, len);
            }
        }

        private static IntPtrEx SwapNibbles(IntPtrEx dst, byte[] src, int len)
        {
            for (int i = 0; i < len; i++)
                dst[i] = (byte)((src[i] << 4) | (src[i] >> 4));

            return dst;
        }

        private static IntPtrEx SwapNibbles(IntPtrEx dst, IntPtrEx src, int len)
        {
            for (int i = 0; i < len; i++)
                dst[i] = (byte)((src[i] << 4) | (src[i] >> 4));

            return dst;
        }

        private void ReadHeader()
        {
            if (FileStream.CanWrite && FileStream.Length < 1)
            {
                byte[] zbuf = ZlibStream.CompressBuffer(new byte[0]);
                byte[] zero = new byte[4];
                int zlen_le = EndianConverter.LittleEndian(zbuf.Length),
                    zero_fcount = EndianConverter.LittleEndian(7),
                    create_ver = EndianConverter.LittleEndian(m_IntVersion);

                FileStream.Write(Encoding.ASCII.GetBytes(GRF_HEADER), 0, GRF_HEADER_LEN);
                FileStream.Write(CryptWatermark, 0, CryptWatermark.Length);
                FileStream.Write(zero, 0, 4);
                FileStream.Write(zero, 0, 4);
                FileStream.Write(BitConverter.GetBytes(zero_fcount), 0, 4);
                FileStream.Write(BitConverter.GetBytes(create_ver), 0, 4);
                FileStream.Write(BitConverter.GetBytes(zlen_le), 0, 4);
                FileStream.Write(zero, 0, 4);
                FileStream.Write(zbuf, 0, zbuf.Length);

                FileStream.Seek(0, SeekOrigin.Begin);
            }

            byte[] buf = new byte[GRF_HEADER_FULL_LEN];
            FileStream.Read(buf, 0, buf.Length);

            if (buf[GRF_HEADER_LEN + 1] == 1)
            {
                m_AllowCrypt = true;
                // 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E
                for (byte i = 0; i < 0xF; i++)
                    if (buf[GRF_HEADER_LEN + i] != i)
                        throw new GrfException();
            }
            else if (buf[GRF_HEADER_LEN] == 0)
            {
                m_AllowCrypt = false;
                // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
                for (byte i = 0; i < 0xF; i++)
                    if (buf[GRF_HEADER_LEN + i] != 0)
                        throw new GrfException();
            }
            else
            {
                throw new GrfException();
            }

            using (IntPtrEx pBuffer = new IntPtrEx(buf))
            {
                m_Items = new GrfItemCollection(this);

                m_IntVersion = EndianConverter.LittleEndian(pBuffer.Read<int>(GRF_HEADER_MID_LEN + 0xC));
                m_Items.Capacity = EndianConverter.LittleEndian(pBuffer.Read<int>(GRF_HEADER_MID_LEN + 8))
                    - EndianConverter.LittleEndian(pBuffer.Read<int>(GRF_HEADER_MID_LEN + 4))
                    - 7;

                FileStream.Seek(EndianConverter.LittleEndian(pBuffer.Read<int>(GRF_HEADER_MID_LEN) + GRF_HEADER_FULL_LEN), SeekOrigin.Begin);
            }

            switch (m_IntVersion & 0xFF00)
            {
                case 0x0200: ReadVer2Info(); break;
                case 0x0100: ReadVer1Info(); break;
                default: throw new GrfException();
            }
        }

        private void ReadVer1Info()
        {
            if (m_IntVersion > 0x103)
                throw new GrfException();

            int offset = (int)FileStream.Position;
            int len2, len = (int)FileStream.Length - offset;
            byte[] buffer = new byte[len], namebuf = new byte[GRF_NAMELEN];

            try
            {
                FileStream.Read(buffer, 0, len);

                string name = null;
                GrfItem item;

                using (IntPtrEx pBuffer = new IntPtrEx(buffer))
                {
                    for (int i = offset = 0; i < m_Items.Capacity; i++)
                    {
                        len = EndianConverter.LittleEndian(pBuffer.Read<int>(offset));
                        offset += 4;

                        if (m_IntVersion < 0x101)
                        {
                            len2 = pBuffer.Read<string>(offset).Length;
                            if (len2 >= GRF_NAMELEN)
                                throw new GrfException();

                            name = SwapNibbles(pBuffer + offset, len2);
                        }
                        else if (m_IntVersion < 0x104)
                        {
                            offset += 2;
                            len2 = len - 6;
                            if (len2 >= GRF_NAMELEN)
                                throw new GrfException();

                            SwapNibbles(namebuf, pBuffer + offset, len2);
                            name = GrfCrypt.DecryptNameVer1(namebuf, len2);

                            len -= 2;
                        }

                        offset += len;

                        item = GrfItem.CreateV1
                        (
                            this,
                            name,
                            EndianConverter.LittleEndian(pBuffer.Read<int>(offset)) - EndianConverter.LittleEndian(pBuffer.Read<int>(offset + 8)) - 0x02CB,
                            EndianConverter.LittleEndian(pBuffer.Read<int>(offset + 4)) - 0x92CB,
                            EndianConverter.LittleEndian(pBuffer.Read<int>(offset + 8)),
                            (GrfFileFlags)pBuffer[offset + 0xC],
                            EndianConverter.LittleEndian(pBuffer.Read<int>(offset + 0xD)) + GRF_HEADER_FULL_LEN
                        );

                        m_Items.GrfAdd(item);

                        offset += 0x11;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void ReadVer2Info()
        {
            if (m_IntVersion != 0x200)
                throw new GrfException();

            byte[] buf = new byte[8], zbuf;
            int len, len2;

            FileStream.Read(buf, 0, 8);
            len = EndianConverter.LittleEndian(BitConverter.ToInt32(buf, 0));

            if (0 == (len2 = EndianConverter.LittleEndian(BitConverter.ToInt32(buf, 4))))
                return;

            zbuf = new byte[len];
            FileStream.Read(zbuf, 0, len);

            using (IntPtrEx pBuffer = new IntPtrEx(ZlibStream.UncompressBuffer(zbuf)))
            {
                string name = null;
                GrfItem item;

                try
                {
                    for (int i = 0, offset = 0; i < m_Items.Capacity; i++, offset += 0x11)
                    {
                        name = pBuffer.Read<string>(offset);
                        len = name.Length + 1;

                        if (len >= GRF_NAMELEN)
                            throw new GrfException();

                        offset += len;

                        item = GrfItem.CreateV2
                        (
                            this,
                            name,
                            EndianConverter.LittleEndian(pBuffer.Read<int>(offset)),
                            EndianConverter.LittleEndian(pBuffer.Read<int>(offset + 4)),
                            EndianConverter.LittleEndian(pBuffer.Read<int>(offset + 8)),
                            (GrfFileFlags)pBuffer[offset + 0xC],
                            EndianConverter.LittleEndian(pBuffer.Read<int>(offset + 0xD)) + GRF_HEADER_FULL_LEN
                        );

                        m_Items.GrfAdd(item);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        private void FlushFile(int i)
        {
            if (m_Items[i] is GrfDirectoryInfo)
                return;

            GrfFileInfo gfile = (GrfFileInfo)m_Items[i];
            byte[] buf = ZlibStream.CompressBuffer(gfile.Data);
            gfile.CompressedLength = buf.Length;
            gfile.AlignedCompressedLength = gfile.CompressedLength;

            if ((gfile.Flags & (GrfFileFlags.MixCrypt | GrfFileFlags.Des_0x14)) != GrfFileFlags.None)
            {
                gfile.AlignedCompressedLength += gfile.CompressedLength % 8;
                Array.Resize<byte>(ref buf, gfile.AlignedCompressedLength);
                buf = GrfCrypt.EncryptFileBuffer(buf, gfile.CompressedLength, gfile.Flags);
            }

            int writeOffset = m_Items.FindUnused(gfile.AlignedCompressedLength);
            if (writeOffset == 0)
            {
                // grf_find_unused returned 0 -> append
                FileStream.Seek(0, SeekOrigin.End);
            }
            else
            {
                FileStream.Seek(writeOffset, SeekOrigin.Begin);
            }

            gfile.Position = (int)FileStream.Position;

            FileStream.Write(buf, 0, buf.Length);
        }

        private void FlushVer1()
        {
            byte[] buffer = new byte[m_Items.Count * GRF_ITEM_SIZE];
            int i, offset, len, writeOffset;
            GrfFileInfo gfile = null;

            try
            {
                m_Items.SortToPosition();

                using (IntPtrEx pBuffer = new IntPtrEx(buffer))
                {
                    for (i = offset = 0; i < Items.Count; i++)
                    {
                        if (m_Items[i] is GrfFileInfo)
                        {
                            gfile = (GrfFileInfo)m_Items[i];

                            if (m_ForceRepack)
                            {
                                gfile.CompressedLength = 0;
                                gfile.AlignedCompressedLength = 0;
                                gfile.Position = 0;
                            }

                            if (gfile.CompressedLength == 0 &&
                                gfile.AlignedCompressedLength == 0 &&
                                gfile.Position == 0 &&
                                gfile.Length != 0)
                            {
                                if (gfile.CheckExtension())
                                    gfile.Flags = (gfile.Flags & ~GrfFileFlags.MixCrypt) | GrfFileFlags.Des_0x14;
                                else
                                    gfile.Flags = (gfile.Flags & ~GrfFileFlags.Des_0x14) | GrfFileFlags.MixCrypt;

                                FlushFile(i);
                            }
                        }

                        len = m_Items[i].FullName.Length + 1;
                        if (m_IntVersion < 0x101)
                        {
                            pBuffer.Write<int>(offset, len);
                            SwapNibbles(pBuffer + offset + 4, m_Items[i].FullName, len);
                            offset += 4 + len;
                        }
                        else if (m_IntVersion < 0x104)
                        {
                            pBuffer.Write<int>(offset, len + 6);
                            offset += 4;
                            SwapNibbles(pBuffer + offset + 6, GrfCrypt.EncryptNameVer1(m_Items[i].Name, len), len);
                            offset += len + 6;
                        }

                        if (m_Items[i] is GrfDirectoryInfo)
                        {
                            pBuffer.Write<int>(offset, EndianConverter.LittleEndian(GrfItem.GRFFILE_DIR_SZSMALL + GrfItem.GRFFILE_DIR_SZORIG + 0x02CB));
                            pBuffer.Write<int>(offset + 4, EndianConverter.LittleEndian(GrfItem.GRFFILE_DIR_SZFILE + 0x92CB));
                            pBuffer.Write<int>(offset + 8, EndianConverter.LittleEndian(GrfItem.GRFFILE_DIR_SZORIG));
                            pBuffer.Write<int>(offset + 0xD, EndianConverter.LittleEndian(GrfItem.GRFFILE_DIR_OFFSET - GRF_HEADER_FULL_LEN));
                        }
                        else
                        {
                            pBuffer.Write<int>(offset, EndianConverter.LittleEndian(gfile.CompressedLength + gfile.Length + 0x02CB));
                            pBuffer.Write<int>(offset + 4, EndianConverter.LittleEndian(gfile.AlignedCompressedLength + 0x92CB));
                            pBuffer.Write<int>(offset + 8, EndianConverter.LittleEndian(gfile.Length));
                            pBuffer.Write<int>(offset + 0xD, EndianConverter.LittleEndian(gfile.Position - GRF_HEADER_FULL_LEN));
                        }

                        pBuffer.Write<byte>(offset + 0xC, (byte)(m_Items[i].Flags & GrfFileFlags.File));

                        offset += 0x11;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            FileStream.Seek(0, SeekOrigin.End);
            writeOffset = (int)FileStream.Position;
            FileStream.Write(buffer, 0, offset);
            FileStream.Seek(GRF_HEADER_MID_LEN, SeekOrigin.End);

            FileStream.Write(BitConverter.GetBytes(EndianConverter.LittleEndian(writeOffset - GRF_HEADER_FULL_LEN)), 0, 4);
            FileStream.Write(BitConverter.GetBytes(0), 0, 4);
            FileStream.Write(BitConverter.GetBytes(EndianConverter.LittleEndian(i + 0 + 7)), 0, 4);
        }

        private void FlushVer2()
        {
            byte[] buffer = new byte[m_Items.Count * GRF_ITEM_SIZE];
            IntPtrEx pName;
            int i, offset, len;
            GrfFileInfo gfile = null;

            try
            {
                m_Items.SortToPosition();

                using (IntPtrEx pBuffer = new IntPtrEx(buffer))
                {
                    for (i = offset = 0; i < Items.Count; i++)
                    {
                        len = m_Items[i].FullName.Length + 1;
                        using (pName = new IntPtrEx(m_Items[i].FullName))
                        {
                            Marshal.Copy(pName, buffer, offset, len);
                        }

                        offset += len;

                        if (m_Items[i] is GrfFileInfo)
                        {
                            gfile = (GrfFileInfo)m_Items[i];

                            if (m_ForceRepack)
                            {
                                gfile.CompressedLength = 0;
                                gfile.AlignedCompressedLength = 0;
                                gfile.Position = 0;
                            }

                            if (gfile.CompressedLength == 0 &&
                                gfile.AlignedCompressedLength == 0 &&
                                gfile.Position == 0 &&
                                gfile.Length != 0)
                            {
                                if (!m_AllowCrypt)
                                    gfile.Flags &= ~(GrfFileFlags.MixCrypt | GrfFileFlags.Des_0x14);

                                FlushFile(i);
                            }

                            pBuffer.Write<int>(offset, EndianConverter.LittleEndian(gfile.CompressedLength));
                            pBuffer.Write<int>(offset + 4, EndianConverter.LittleEndian(gfile.AlignedCompressedLength));
                            pBuffer.Write<int>(offset + 8, EndianConverter.LittleEndian(gfile.Length));
                            pBuffer.Write<int>(offset + 0xD, EndianConverter.LittleEndian(gfile.Position - GRF_HEADER_FULL_LEN));
                        }
                        else
                        {
                            pBuffer.Write<int>(offset, EndianConverter.LittleEndian(GrfItem.GRFFILE_DIR_SZSMALL));
                            pBuffer.Write<int>(offset + 4, EndianConverter.LittleEndian(GrfItem.GRFFILE_DIR_SZFILE));
                            pBuffer.Write<int>(offset + 8, EndianConverter.LittleEndian(GrfItem.GRFFILE_DIR_SZORIG));
                            pBuffer.Write<int>(offset + 0xD, EndianConverter.LittleEndian(GrfItem.GRFFILE_DIR_OFFSET - GRF_HEADER_FULL_LEN));
                        }

                        pBuffer.Write<byte>(offset + 0xC, (byte)m_Items[i].Flags);

                        offset += 0x11;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            Array.Resize<byte>(ref buffer, offset);
            buffer = ZlibStream.CompressBuffer(buffer);

            m_Items.SortToPosition();

            int writeOffset = m_Items.FindUnused(8 + buffer.Length);
            if (writeOffset == 0)
            {
                FileStream.Seek(0, SeekOrigin.End);
                writeOffset = (int)FileStream.Position;
            }
            else
            {
                FileStream.Seek(writeOffset, SeekOrigin.Begin);
            }

            FileStream.Write(BitConverter.GetBytes(EndianConverter.LittleEndian(buffer.Length)), 0, 4);
            FileStream.Write(BitConverter.GetBytes(EndianConverter.LittleEndian(offset)), 0, 4);
            FileStream.Write(buffer, 0, buffer.Length);

            FileStream.Seek(GRF_HEADER_MID_LEN, SeekOrigin.Begin);

            FileStream.Write(BitConverter.GetBytes(EndianConverter.LittleEndian(writeOffset - GRF_HEADER_FULL_LEN)), 0, 4);
            FileStream.Write(BitConverter.GetBytes(0), 0, 4);
            FileStream.Write(BitConverter.GetBytes(EndianConverter.LittleEndian(i + 7)), 0, 4);
        }

        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                FileStream.Close();
            }

            IsDisposed = true;
        }

        void IDisposable.Dispose()
        {
            Flush();

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
