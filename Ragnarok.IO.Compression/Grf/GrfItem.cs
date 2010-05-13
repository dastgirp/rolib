using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Ragnarok.IO.Compression
{
    /// <summary>
    /// The base class of an entry in a <see cref="GrfArchive"/>.
    /// </summary>
    public abstract class GrfItem
    {
        internal const int GRFFILE_DIR_SZFILE = 0x0714;
        internal const int GRFFILE_DIR_SZSMALL = 0x0449;
        internal const int GRFFILE_DIR_SZORIG = 0x055C;
        internal const int GRFFILE_DIR_OFFSET = 0x058A;

        private static string[] specialExts = new string[]
        {
	        ".gnd",
	        ".gat",
	        ".act",
	        ".str"
        };

        private static readonly Encoding NameEncoding = Encoding.GetEncoding(949);

        /// <summary>
        /// The parent <see cref="GrfArchive"/>.
        /// </summary>
        protected GrfArchive m_Grf;
        /// <summary>
        /// The <see cref="GrfFileFlags"/> for this entry.
        /// </summary>
        protected GrfFileFlags m_Flags;
        /// <summary>
        /// The name of this entry.
        /// </summary>
        protected string m_Name;
        /// <summary>
        /// The hash created from the name of this entry.
        /// </summary>
        protected uint m_Hash;
        /// <summary>
        /// The size on the the file on the disk.
        /// </summary>
        protected int m_AlignedCompressedLength;
        /// <summary>
        /// The compressed size of the file.
        /// </summary>
        protected int m_CompressedLength;
        /// <summary>
        /// The size of the original file.
        /// </summary>
        protected int m_Length;
        /// <summary>
        /// The position in the <see cref="GrfArchive"/> where this file is located.
        /// </summary>
        protected int m_Position;

        #region Properties
        /// <summary>
        /// The parent <see cref="GrfArchive"/>.
        /// </summary>
        public GrfArchive Archive
        {
            get { return m_Grf; }
        }
        /// <summary>
        /// The <see cref="GrfFileFlags"/> for this entry.
        /// </summary>
        public GrfFileFlags Flags
        {
            get { return m_Flags; }
            set
            {
                if ((value & GrfFileFlags.File) == GrfFileFlags.File && this is GrfDirectoryInfo)
                    throw new InvalidOperationException();
                else if ((value & GrfFileFlags.File) == GrfFileFlags.None && this is GrfFileInfo)
                    throw new InvalidOperationException();

                m_Flags = value;
            }
        }
        /// <summary>
        /// The full name of this entry.
        /// </summary>
        public string FullName
        {
            get { return m_Name; }
            set { throw new NotImplementedException(); }
        }
        /// <summary>
        /// The name of this entry.
        /// </summary>
        public string Name
        {
            get { return m_Name.Substring(m_Name.LastIndexOf('\\')); }
        }
        /// <summary>
        /// The hash created from the full name of this entry.
        /// </summary>
        public uint Hash
        {
            get { return m_Hash; }
        }

        #endregion
        /// <summary>
        /// Extracts this item to a specific location.
        /// </summary>
        /// <param name="path">The location where to extract this item to.</param>
        public abstract void ExtractTo(string path);
        /// <summary>
        /// Creates a hash from a filename.
        /// </summary>
        /// <param name="name">The filename.</param>
        /// <returns>A hash from the specified filename.</returns>
        public static uint NameHash(string name)
        {
            uint tmp = 0;

            name = EncodeName(name);

            for (int i = 0; i < name.Length; i++)
                tmp = (tmp << 5) + tmp + name.ToUpper()[i];

            return tmp;
        }
        /// <summary>
        /// Encodes the name with the proper encoding.
        /// </summary>
        /// <param name="name">The name to encode.</param>
        /// <returns>A <see cref="String"/> with the properly encoded filename.</returns>
        protected static string EncodeName(string name)
        {
            byte[] buffer = new byte[name.Length];
            IntPtr pName = Marshal.StringToHGlobalAnsi(name);
            Marshal.Copy(pName, buffer, 0, name.Length);
            Marshal.FreeHGlobal(pName);
            return NameEncoding.GetString(buffer).Replace('/', '\\');
        }

        internal bool CheckExtension()
        {
            return GrfItem.CheckExtension(m_Name);
        }

        internal static GrfItem CreateV1(GrfArchive grf, string name, int compressedLength, int compressedLengthAligned, int realLength, GrfFileFlags flags, int position)
        {
            GrfItem ret = Create(grf, name, compressedLength, compressedLengthAligned, realLength, flags, position);
            ret.m_Flags |= (CheckExtension(name) ? GrfFileFlags.Des_0x14 : GrfFileFlags.MixCrypt);
            return ret;
        }

        internal static GrfItem CreateV2(GrfArchive grf, string name, int compressedLength, int compressedLengthAligned, int realLength, GrfFileFlags flags, int position)
        {
            return Create(grf, name, compressedLength, compressedLengthAligned, realLength, flags, position);
        }

        private static GrfItem Create
        (
            GrfArchive grf,
            string name,
            int compressedLength,
            int compressedLengthAligned,
            int realLength,
            GrfFileFlags flags,
            int position
        )
        {
            GrfItem ret;

            if (IsDirectory(compressedLength, compressedLengthAligned, realLength, flags, position))
                ret = new GrfDirectoryInfo();
            else
                ret = new GrfFileInfo();

            ret.m_Grf = grf;
            ret.m_Name = EncodeName(name);
            ret.m_AlignedCompressedLength = compressedLengthAligned;
            ret.m_CompressedLength = compressedLength;
            ret.m_Length = realLength;
            ret.m_Flags = flags;
            ret.m_Position = position;
            ret.m_Hash = NameHash(ret.m_Name);

            return ret;
        }

        private static bool IsDirectory
        (
            int compressedLength,
            int compressedLengthAligned,
            int realLength,
            GrfFileFlags flags,
            int position
        )
        {
            return ((flags & GrfFileFlags.File) == GrfFileFlags.None) ||
                    ((compressedLengthAligned == GRFFILE_DIR_SZFILE) &&
                     (compressedLength == GRFFILE_DIR_SZSMALL) &&
                     (realLength == GRFFILE_DIR_SZORIG) &&
                     (position == GRFFILE_DIR_OFFSET));
        }

        private static bool CheckExtension(string filename)
        {
            if (specialExts.Length < 1)
                return false;

            if (string.IsNullOrEmpty(filename) || filename.Length < 4)
                return false;

            for (int i = 0; i < specialExts.Length; i++)
                if (filename.EndsWith(specialExts[i]))
                    return true;

            return false;
        }
    }
}
