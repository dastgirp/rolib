using System.IO;

namespace Ragnarok.IO.Compression
{
    /// <summary>
    /// A directory entry in a <see cref="GrfArchive"/>.
    /// </summary>
    public class GrfDirectoryInfo : GrfItem
    {
        internal GrfDirectoryInfo()
        {

        }

        internal GrfDirectoryInfo(GrfArchive grf, string name, GrfFileFlags flags)
        {
            m_Grf = grf;
            m_Flags = flags;
            m_Name = FilenameEncoding.Encode(name);
            m_Hash = NameHash(m_Name);

            m_AlignedCompressedLength = GRFFILE_DIR_SZFILE;
            m_CompressedLength = GRFFILE_DIR_SZSMALL;
            m_Length = GRFFILE_DIR_SZORIG;
            m_Position = GRFFILE_DIR_OFFSET;
        }

        /// <summary>
        /// Extracts the directory to a specific location.
        /// </summary>
        /// <param name="path">The location where to extract this directory to.</param>
        public override void ExtractTo(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
