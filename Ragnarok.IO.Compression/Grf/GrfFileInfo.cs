using System.IO;
using Ragnarok.IO.Compression.Cryptography;
using Ionic.Zlib;

namespace Ragnarok.IO.Compression
{
    /// <summary>
    /// A file entry in a <see cref="GrfArchive"/>.
    /// </summary>
    public class GrfFileInfo : GrfItem
    {
        private byte[] m_Data;
        #region Properties

        /// <summary>
        /// The size on the the file on the disk.
        /// </summary>
        public int AlignedCompressedLength
        {
            get { return m_AlignedCompressedLength; }
            internal set { m_AlignedCompressedLength = value; }
        }
        /// <summary>
        /// The compressed size of the file.
        /// </summary>
        public int CompressedLength
        {
            get { return m_CompressedLength; }
            internal set { m_CompressedLength = value; }
        }
        /// <summary>
        /// The size of the original file.
        /// </summary>
        public int Length
        {
            get { return m_Length; }
        }
        /// <summary>
        /// The position in the <see cref="GrfArchive"/> where this file is located.
        /// </summary>
        public int Position
        {
            get { return m_Position; }
            internal set { m_Position = value; }
        }
        /// <summary>
        /// The contents of the file.
        /// </summary>
        public byte[] Data
        {
            get
            {
                if (m_Data != null)
                    return m_Data;

                if (CompressedLength <= 0)
                    throw new GrfException();

                byte[] buf = new byte[AlignedCompressedLength];

                m_Grf.FileStream.Seek(Position, SeekOrigin.Begin);
                m_Grf.FileStream.Read(buf, 0, buf.Length);

                byte[] zbuf = GrfCrypt.DecryptFileBuffer(buf, CompressedLength, Flags);

                Data = ZlibStream.UncompressBuffer(zbuf);

                return m_Data;
            }
            internal set
            {
                m_Data = value;

                if (value != null)
                    m_Length = m_Data.Length;
            }
        }

        #endregion
        #region Internal API

        internal GrfFileInfo()
        {

        }

        internal GrfFileInfo(GrfArchive grf, string name, GrfFileFlags flags, byte[] data)
        {
            m_Grf = grf;
            m_Flags = flags;
            m_Name = EncodeName(name);
            m_Hash = NameHash(m_Name);
            Data = data;
        }

        #endregion

        /// <summary>
        /// Extracts the file to a specific location.
        /// </summary>
        /// <param name="path">The location where to extract this file to.</param>
        public override void ExtractTo(string path)
        {
            path = path.Replace('/', '\\');
            int i = path.LastIndexOf('\\');
            if (i > 0 && !Directory.Exists(path.Substring(0, i)))
                Directory.CreateDirectory(path.Substring(0, i));

            File.WriteAllBytes(path, Data);
        }
    }
}
