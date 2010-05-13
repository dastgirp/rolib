using System;
using System.IO;

namespace Ragnarok.IO.Compression
{
    public class PartialStream : Stream
    {
        private Stream m_BaseStream;
        private long m_Start;
        private long m_Length;

        #region Properties

        public override bool CanRead
        {
            get { return m_BaseStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return m_BaseStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return m_BaseStream.CanWrite; }
        }

        public override long Length
        {
            get { return m_Length; }
        }

        public override long Position
        {
            get
            {
                long ret = m_BaseStream.Position - m_Start;

                if (ret < 0 || ret > m_Length)
                    throw new ArgumentOutOfRangeException();

                return ret;
            }
            set { Seek(value, SeekOrigin.Begin); }
        }

        public Stream BaseStream
        {
            get { return m_BaseStream; }
        }

        public long Start
        {
            get { return m_Start; }
        }

        #endregion

        public PartialStream(Stream basestream, long start, long length)
        {
            m_BaseStream = basestream;
            m_Start = start;
            m_Length = (m_BaseStream.Length - start < length) ? m_BaseStream.Length - start : length;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int ret;

            try
            {
                if (Position + offset + count > m_Length)
                    count = m_Length - (Position + offset);

                ret = m_BaseStream.Read(buffer, offset, count);
            }
            catch (ArgumentOutOfRangeException)
            {
                ret = 0;
            }

            return ret;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    offset = (m_Start + offset > m_Length) ? m_Length : m_Start + offset;
                    break;
                case SeekOrigin.Current:
                    
                    break;
                case SeekOrigin.End:
                    offset = (m_Start + m_Length - offset < 0) ? 0 : m_Start + m_Length - offset;
                    break;
            }

            origin = SeekOrigin.Begin;
            return m_BaseStream.Seek(offset, origin) - m_Start;
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException();
        }

        public override void Flush()
        {
            m_BaseStream.Flush();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            m_BaseStream.Write(buffer, offset, count);
        }
    }
}
