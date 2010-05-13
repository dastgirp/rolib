using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Ionic.Zlib;

namespace Ragnarok.IO.Compression
{
    /// <summary>
    /// A class for interacting with Ragnarok Online rgz files.
    /// </summary>
    public class RgzArchive
    {
        private FileStream m_FileStream;
        private ZlibStream m_ZStream;
        internal bool Modified;

        #region Properties
        /// <summary>
        /// Wether or not the <see cref="RgzArchive"/> is read only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return !m_FileStream.CanWrite; }
        }
        /// <summary>
        /// A <see cref="RgzItemCollection"/> with the items of the <see cref="RgzArchive"/>.
        /// </summary>
        public RgzItemCollection Items
        {
            get { return m_Items; }
        }
        /// <summary>
        /// The number of files in the <see cref="RgzArchive"/>.
        /// </summary>
        internal int ItemCount
        {
            get { return m_Items.Capacity; }
            set { m_Items.Capacity = value; }
        }

        #endregion

        internal RgzArchive() { }

        #region Public API
        /// <summary>
        /// Creates a new <see cref="RgzArchive"/>.
        /// </summary>
        /// <param name="filename">The path and filename were to create the new <see cref="RgzArchive"/>.</param>
        public RgzArchive(string filename)
        {
            m_FileStream = File.Open(filename, FileMode.Create);
            m_ZStream = new ZlibStream(m_FileStream, CompressionMode.Decompress);
        }

        /// <summary>
        /// Merges another <see cref="RgzArchive"/> with this <see cref="RgzArchive"/>.
        /// </summary>
        /// <param name="rgz">The <see cref="RgzArchive"/> to add the current one.</param>
        public void Merge(RgzArchive rgz)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Extracts the contents of the <see cref="RgzArchive"/> to a specified location.
        /// </summary>
        /// <param name="path">The path where to extract the files to.</param>
        public void ExtractTo(string path)
        {
            path = path.Replace('/', '\\');
            if (path != string.Empty && path[path.Length - 1] != '\\')
                path += "\\";
        }

        /// <summary>
        /// Writes all unwritten data to the disk and releases all data.
        /// </summary>
        public void Close()
        {
            m_ZStream.Close();
            m_FileStream.Close();
        }
        /// <summary>
        /// Opens an existing <see cref="RgzArchive"/>.
        /// </summary>
        /// <param name="filename">The path to the <see cref="RgzArchive"/>.</param>
        /// <param name="isreadonly">Wether or not to open the file as read only.</param>
        /// <returns></returns>
        public static RgzArchive Open(string filename, bool isreadonly)
        {
            if (string.IsNullOrEmpty(filename))
                throw new RgzException();

            RgzArchive rgz = new RgzArchive();
            rgz.m_FileStream = isreadonly ? File.OpenRead(filename) : File.Open(filename, FileMode.OpenOrCreate);

            return rgz;
        }

        #endregion
    }
}
