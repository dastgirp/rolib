using System;
using System.Collections.Generic;
using System.IO;

namespace Ragnarok.IO.Compression
{
    /// <summary>
    /// A class within a <see cref="GrfArchive"/> that contains all the entries.
    /// </summary>
    public class GrfItemCollection : ICollection<GrfItem>
    {
        private GrfArchive m_Grf;
        private List<GrfItem> m_Items;

        #region Properties
        /// <summary>
        /// Gets a <see cref="GrfItem"/> by name.
        /// </summary>
        /// <param name="name">The name of the <see cref="GrfItem"/>.</param>
        /// <returns>A <see cref="GrfItem"/>.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when the filename is not found within the <see cref="GrfArchive"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the <see cref="GrfArchive"/> is closed.
        /// </exception>
        public GrfItem this[string name]
        {
            get
            {
                if (m_Grf.IsDisposed)
                    throw new ObjectDisposedException(m_Grf.GetType().Name);

                int i = IndexOf(name);

                if (i < 0)
                    throw new IndexOutOfRangeException();

                return m_Items[i];
            }
        }
        /// <summary>
        /// Gets a <see cref="GrfItem"/> by index.
        /// </summary>
        /// <param name="index">The index of the <see cref="GrfItem"/>.</param>
        /// <returns>A <see cref="GrfItem"/>.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when the filename is not found within the <see cref="GrfArchive"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the <see cref="GrfArchive"/> is closed.
        /// </exception>
        public GrfItem this[int index]
        {
            get
            {
                if (m_Grf.IsDisposed)
                    throw new ObjectDisposedException(m_Grf.GetType().Name);

                return m_Items[index];
            }
        }
        /// <summary>
        /// The total count of <see cref="GrfItem"/> entries in the <see cref="GrfArchive"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the <see cref="GrfArchive"/> is closed.
        /// </exception>
        public int Count
        {
            get
            {
                if (m_Grf.IsDisposed)
                    throw new ObjectDisposedException(m_Grf.GetType().Name);

                return m_Items.Count;
            }
        }
        /// <summary>
        /// Wether or not this collection can be modified.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the <see cref="GrfArchive"/> is closed.
        /// </exception>
        public bool IsReadOnly
        {
            get
            {
                if (m_Grf.IsDisposed)
                    throw new ObjectDisposedException(m_Grf.GetType().Name);

                return m_Grf.IsReadOnly;
            }
        }
        /// <summary>
        /// Gets or sets the total number of <see cref="GrfItem"/> objects
        /// the collection can hold without resizing.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the <see cref="GrfArchive"/> is closed.
        /// </exception>
        public int Capacity
        {
            get
            {
                if (m_Grf.IsDisposed)
                    throw new ObjectDisposedException(m_Grf.GetType().Name);

                return m_Items.Capacity;
            }
            set
            {
                if (m_Grf.IsDisposed)
                    throw new ObjectDisposedException(m_Grf.GetType().Name);

                m_Items.Capacity = value;
            }
        }

        #endregion
        #region Internal API

        internal GrfItemCollection(GrfArchive grf)
        {
            m_Items = new List<GrfItem>();
            m_Grf = grf;
        }

        internal void GrfAdd(GrfItem item)
        {
            m_Items.Add(item);
        }

        internal bool GrfRemove(int index)
        {
            if (index >= m_Items.Count || index < 0)
                return false;

            m_Items.RemoveAt(index);
            m_Grf.Modified = true;

            return true;
        }

        internal int FindUnused(int length)
        {
            if (m_Items.Count < 1)
                return 0;

            //m_Items.Sort(new Comparison<GrfItem>(PositionSort));

            int a;
            GrfFileInfo cur, next;

            for (int i = 0; i < m_Items.Count - 1; i++)
            {
                if (m_Items[i] is GrfDirectoryInfo)
                    continue;

                cur = (GrfFileInfo)m_Items[i];
                if (cur.Position == 0 || cur.Length == 0)
                    continue;

                next = (GrfFileInfo)m_Items[i + 1];

                a = cur.Position + cur.Length;

                if (a - next.Position >= length)
                    return a;
            }

            return 0;
        }

        internal void SortToPosition()
        {
            m_Items.Sort(new Comparison<GrfItem>(PositionSort));
        }

        #endregion
        #region Public API
        /// <summary>
        /// Adds a file to the <see cref="GrfArchive"/>
        /// </summary>
        /// <param name="grfname">The filename inside the <see cref="GrfArchive"/>.</param>
        /// <param name="filename">The filename on the disk.</param>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the <see cref="GrfArchive"/> is closed.
        /// </exception>
        /// <exception cref="AccessViolationException">
        /// Thrown when the <see cref="GrfArchive"/> is read-only.
        /// </exception>
        public void AddFile(string grfname, string filename)
        {
            if (m_Grf.IsDisposed)
                throw new ObjectDisposedException(m_Grf.GetType().Name);

            if (m_Grf.IsReadOnly)
                throw new AccessViolationException("The GrfArchive is read-only");

            Add(grfname, GrfFileFlags.File, File.ReadAllBytes(filename));
        }
        /// <summary>
        /// Adds a file to the <see cref="GrfArchive"/>
        /// </summary>
        /// <param name="grfname">The filename inside the <see cref="GrfArchive"/>.</param>
        /// <param name="content">The contents of the file.</param>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the <see cref="GrfArchive"/> is closed.
        /// </exception>
        /// <exception cref="AccessViolationException">
        /// Thrown when the <see cref="GrfArchive"/> is read-only.
        /// </exception>
        public void AddFile(string grfname, Stream content)
        {
            if (m_Grf.IsDisposed)
                throw new ObjectDisposedException(m_Grf.GetType().Name);

            if (m_Grf.IsReadOnly)
                throw new AccessViolationException("The GrfArchive is read-only");

            using (MemoryStream ms = new MemoryStream())
            {
                int i = 0;
                byte[] buffer = new byte[1024 * 8];

                while ((i = content.Read(buffer, 0, buffer.Length)) > 0)
                    ms.Write(buffer, 0, i);

                Add(grfname, GrfFileFlags.File, ms.ToArray());
            }
        }
        /// <summary>
        /// Adds a file to the <see cref="GrfArchive"/>
        /// </summary>
        /// <param name="grfname">The filename inside the <see cref="GrfArchive"/>.</param>
        /// <param name="content">The contents of the file.</param>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the <see cref="GrfArchive"/> is closed.
        /// </exception>
        /// <exception cref="AccessViolationException">
        /// Thrown when the <see cref="GrfArchive"/> is read-only.
        /// </exception>
        public void AddFile(string grfname, byte[] content)
        {
            if (m_Grf.IsDisposed)
                throw new ObjectDisposedException(m_Grf.GetType().Name);

            Add(grfname, GrfFileFlags.File, content);
        }
        /// <summary>
        /// Adds a directory to the <see cref="GrfArchive"/>
        /// </summary>
        /// <param name="grfname">The directoryname inside the <see cref="GrfArchive"/>.</param>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the <see cref="GrfArchive"/> is closed.
        /// </exception>
        /// <exception cref="AccessViolationException">
        /// Thrown when the <see cref="GrfArchive"/> is read-only.
        /// </exception>
        public void AddDirectory(string grfname)
        {
            if (m_Grf.IsDisposed)
                throw new ObjectDisposedException(m_Grf.GetType().Name);

            Add(grfname, GrfFileFlags.None, null);
        }
        /// <summary>
        /// Adds a <see cref="GrfItem"/> to the <see cref="GrfArchive"/>.
        /// </summary>
        /// <param name="item">The <see cref="GrfItem"/> to add.</param>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the <see cref="GrfArchive"/> is closed.
        /// </exception>
        /// <exception cref="AccessViolationException">
        /// Thrown when the <see cref="GrfArchive"/> is read-only.
        /// </exception>
        public void Add(GrfItem item)
        {
            if (m_Grf.IsDisposed)
                throw new ObjectDisposedException(m_Grf.GetType().Name);

            if (m_Grf.IsReadOnly)
                throw new AccessViolationException("The GrfArchive is read-only");

            int i = IndexOf(item.Name);

            if (item is GrfDirectoryInfo)
            {
                GrfDirectoryInfo add = new GrfDirectoryInfo(m_Grf, item.Name, item.Flags);
                if (i < 0)
                    GrfAdd(add);
                else
                    m_Items[i] = add;
            }
            else if (item is GrfFileInfo)
            {
                GrfFileInfo add, file = (GrfFileInfo)item;

                add = new GrfFileInfo(m_Grf, item.Name, item.Flags, file.Data);

                if (i < 0)
                    GrfAdd(add);
                else
                    m_Items[i] = add;
            }
        }
        /// <summary>
        /// Removes a <see cref="GrfItem"/> from the <see cref="GrfArchive"/>.
        /// </summary>
        /// <param name="grfname">The internal filename of the <see cref="GrfItem"/>.</param>
        /// <returns>A <see cref="Boolean"/> indicating success of the operation.</returns>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the <see cref="GrfArchive"/> is closed.
        /// </exception>
        /// <exception cref="AccessViolationException">
        /// Thrown when the <see cref="GrfArchive"/> is read-only.
        /// </exception>
        public bool Remove(string grfname)
        {
            if (m_Grf.IsDisposed)
                throw new ObjectDisposedException(m_Grf.GetType().Name);

            return RemoveAt(IndexOf(grfname));
        }
        /// <summary>
        /// Removes a <see cref="GrfItem"/> from the <see cref="GrfArchive"/>.
        /// </summary>
        /// <param name="item">The <see cref="GrfItem"/> to remove.</param>
        /// <returns>A <see cref="Boolean"/> indicating success of the operation.</returns>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the <see cref="GrfArchive"/> is closed.
        /// </exception>
        /// <exception cref="AccessViolationException">
        /// Thrown when the <see cref="GrfArchive"/> is read-only.
        /// </exception>
        public bool Remove(GrfItem item)
        {
            if (m_Grf.IsDisposed)
                throw new ObjectDisposedException(m_Grf.GetType().Name);

            return RemoveAt(IndexOf(item));
        }
        /// <summary>
        /// Removes a <see cref="GrfItem"/> from the <see cref="GrfArchive"/>.
        /// </summary>
        /// <param name="index">The index of the <see cref="GrfItem"/>.</param>
        /// <returns>A <see cref="Boolean"/> indicating success of the operation.</returns>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the <see cref="GrfArchive"/> is closed.
        /// </exception>
        /// <exception cref="AccessViolationException">
        /// Thrown when the <see cref="GrfArchive"/> is read-only.
        /// </exception>
        public bool RemoveAt(int index)
        {
            if (m_Grf.IsDisposed)
                throw new ObjectDisposedException(m_Grf.GetType().Name);

            if (m_Grf.IsReadOnly)
                throw new AccessViolationException("The GrfArchive is read-only");

            if (index < m_Items.Count && index > 0 && m_Items[index] is GrfFileInfo)
                ((GrfFileInfo)m_Items[index]).Data = null;

            return GrfRemove(index);
        }
        /// <summary>
        /// Gets the index of a <see cref="GrfItem"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="GrfItem"/>.</param>
        /// <returns>The index of the <see cref="GrfItem"/> or -1 if the item is not found.</returns>
        public int IndexOf(string name)
        {
            uint hash = GrfItem.NameHash(name);
            for (int i = 0; i < m_Items.Count; i++)
            {
                if (m_Items[i].Hash == hash)
                    return i;
            }
            return -1;
        }
        /// <summary>
        /// Gets the index of a <see cref="GrfItem"/>.
        /// </summary>
        /// <param name="item">The <see cref="GrfItem"/>.</param>
        /// <returns>The index of the <see cref="GrfItem"/> or -1 if the item is not found.</returns>
        public int IndexOf(GrfItem item)
        {
            return m_Items.IndexOf(item);
        }
        /// <summary>
        /// Removes all <see cref="GrfItem"/> entries from the <see cref="GrfArchive"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the <see cref="GrfArchive"/> is closed.
        /// </exception>
        /// <exception cref="AccessViolationException">
        /// Thrown when the <see cref="GrfArchive"/> is read-only.
        /// </exception>
        public void Clear()
        {
            if (m_Grf.IsDisposed)
                throw new ObjectDisposedException(m_Grf.GetType().Name);

            if (m_Grf.IsReadOnly)
                throw new AccessViolationException("The GrfArchive is read-only");

            m_Items.Clear();
            m_Grf.Modified = true;
        }
        /// <summary>
        /// Determines wether a <see cref="GrfItem"/> exits within the <see cref="GrfArchive"/>.
        /// </summary>
        /// <param name="item">The <see cref="GrfItem"/>.</param>
        /// <returns>A <see cref="Boolean"/> indicating wether or not an item exits in the <see cref="GrfArchive"/>.</returns>
        public bool Contains(GrfItem item)
        {
            return m_Items.Contains(item);
        }
        /// <summary>
        /// Determines wether a path exits within the <see cref="GrfArchive"/>.
        /// </summary>
        /// <param name="name">The path for the item.</param>
        /// <returns>A <see cref="Boolean"/> indicating wether or not an item exits in the <see cref="GrfArchive"/>.</returns>
        public bool Contains(string name)
        {
            return IndexOf(name) >= 0;
        }
        /// <summary>
        /// Copies all <see cref="GrfItem"/> entries to an array.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">A <see cref="Int32"/> indicating the index of the array to start copying at.</param>
        public void CopyTo(GrfItem[] array, int arrayIndex)
        {
            m_Items.CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="GrfItemCollection"/>.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<GrfItem> GetEnumerator()
        {
            return m_Items.GetEnumerator();
        }

        #endregion

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_Items.GetEnumerator();
        }

        /// <summary>
        /// Adds a new <see cref="GrfItem"/> to the <see cref="GrfArchive"/>.
        /// </summary>
        /// <param name="grfname">The filename inside the <see cref="GrfArchive"/>.</param>
        /// <param name="flags">The <see cref="GrfFileFlags"/> for this entry.</param>
        /// <param name="data">The data of the file, or null if the <see cref="GrfItem"/> is a folder.</param>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the <see cref="GrfArchive"/> is closed.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the grfname is null or empty, or when the flags indicate a file and the data is null.
        /// </exception>
        /// <exception cref="AccessViolationException">
        /// Thrown when the <see cref="GrfArchive"/> is read-only.
        /// </exception>
        private void Add(string grfname, GrfFileFlags flags, byte[] data)
        {
            if (m_Grf.IsDisposed)
                throw new ObjectDisposedException(m_Grf.GetType().Name);

            if (m_Grf.IsReadOnly)
                throw new AccessViolationException("The GrfArchive is read-only");

            if (string.IsNullOrEmpty(grfname))
                throw new ArgumentNullException("filename");

            if ((flags & GrfFileFlags.File) == GrfFileFlags.File && data == null)
                throw new ArgumentNullException("data");

            GrfItem add;
            int i = IndexOf(grfname);

            if ((flags & GrfFileFlags.File) == GrfFileFlags.None || data == null)
                add = new GrfDirectoryInfo(m_Grf, grfname, flags);
            else
                add = new GrfFileInfo(m_Grf, grfname, flags, data);

            if (i >= 0)
                m_Items[i] = add;
            else
                m_Items.Add(add);

            m_Grf.Modified = true;
        }

        private static int PositionSort(GrfItem a, GrfItem b)
        {
            int pa = (a is GrfDirectoryInfo) ? -1 : ((GrfFileInfo)a).Position,
                pb = (b is GrfDirectoryInfo) ? -1 : ((GrfFileInfo)b).Position;

            return pa.CompareTo(pb);
        }
    }
}
