using System;
using System.Runtime.InteropServices;

namespace System
{
    /// <summary>
    /// An extended platform-specific type that is used to represent a pointer or a handle.
    /// </summary>
    public struct IntPtrEx : IDisposable
    {
        private IntPtr m_Pointer;
        private FreeParams m_Free;
        private GCHandle m_Handle;
        /// <summary>
        /// Returns a <see cref="Byte"/> at the current offset of the pointer, 
        /// the offset is not checked for validity.
        /// </summary>
        /// <param name="offset">The offset from the start of the pointer.</param>
        /// <returns>A <see cref="Byte"/> at the memory location of offset.</returns>
        public byte this[int offset]
        {
            get { return Marshal.ReadByte(m_Pointer, offset); }
            set { Marshal.WriteByte(m_Pointer, offset, value); }
        }
        /// <summary>
        /// Creates a new <see cref="IntPtrEx"/> from a <see cref="IntPtr"/>.
        /// </summary>
        /// <param name="ptr">The <see cref="IntPtr"/> to use as base.</param>
        public IntPtrEx(IntPtr ptr)
        {
            m_Free = FreeParams.None;
            m_Handle = new GCHandle();
            m_Pointer = ptr;
        }
        /// <summary>
        /// Creates a new <see cref="IntPtrEx"/> from a <see cref="Int32"/>.
        /// </summary>
        /// <param name="ptr">The memory location to use as pointer.</param>
        public IntPtrEx(int ptr)
        {
            m_Free = FreeParams.None;
            m_Handle = new GCHandle();
            m_Pointer = new IntPtr(ptr);
        }
        /// <summary>
        /// Creates a new <see cref="IntPtrEx"/> from a <see cref="Int64"/>.
        /// </summary>
        /// <param name="ptr">The memory location to use as pointer.</param>
        public IntPtrEx(long ptr)
        {
            m_Free = FreeParams.None;
            m_Handle = new GCHandle();
            m_Pointer = new IntPtr(ptr);
        }
        /// <summary>
        /// Creates a new <see cref="IntPtrEx"/> from a <see cref="String"/>.
        /// </summary>
        /// <remarks>
        /// Do not forget to free the <see cref="IntPtrEx"/>, using <see cref="IntPtrEx.Free()"/> method,
        /// after you've used it like this or you will create a memory leak.
        /// </remarks>
        /// <param name="str">The string to use the memory location of.</param>
        public IntPtrEx(string str)
        {
            m_Free = FreeParams.FreeHGlobal;
            m_Handle = new GCHandle();
            m_Pointer = Marshal.StringToHGlobalAnsi(str);
        }
        /// <summary>
        /// Creates a new <see cref="IntPtrEx"/> from a <see cref="Byte"/> array.
        /// </summary>
        /// <remarks>
        /// Do not forget to free the <see cref="IntPtrEx"/>, using <see cref="IntPtrEx.Free()"/> method,
        /// after you've used it like this or you will create a memory leak.
        /// </remarks>
        /// <param name="array">The <see cref="Byte"/> array to use the memory location of.</param>
        public IntPtrEx(byte[] array)
        {
            m_Free = FreeParams.FreeHandle;
            m_Handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            m_Pointer = m_Handle.AddrOfPinnedObject();
        }
        /// <summary>
        /// Reads a <see cref="{T}"/> at the start of the pointer.
        /// </summary>
        /// <remarks>
        /// You can only read a struct or string.
        /// </remarks>
        /// <typeparam name="T">A struct or string to read.</typeparam>
        /// <returns>A <see cref="{T}"/>.</returns>
        public T Read<T>()
        {
            return Read<T>(0);
        }
        /// <summary>
        /// Writes a <see cref="{T}"/> to the start of the pointer.
        /// </summary>
        /// <remarks>
        /// You can only write a struct or string.
        /// </remarks>
        /// <typeparam name="T">A struct or string to write.</typeparam>
        /// <param name="value">The <see cref="{T}"/> to write.</param>
        public void Write<T>(T value)
        {
            Write<T>(0, value);
        }
        /// <summary>
        /// Reads a <see cref="{T}"/> at an offset from the pointer.
        /// </summary>
        /// <remarks>
        /// You can only read a struct or string.
        /// </remarks>
        /// <typeparam name="T">A struct or string to read.</typeparam>
        /// <param name="offset">The offset to read from.</param>
        /// <returns>A <see cref="{T}"/>.</returns>
        public T Read<T>(int offset)
        {
            if (typeof(T) == typeof(byte))
                return (T)((object)Marshal.ReadByte(m_Pointer, offset));
            else if (typeof(T) == typeof(short))
                return (T)((object)Marshal.ReadInt16(m_Pointer, offset));
            else if (typeof(T) == typeof(int))
                return (T)((object)Marshal.ReadInt32(m_Pointer, offset));
            else if (typeof(T) == typeof(long))
                return (T)((object)Marshal.ReadInt64(m_Pointer, offset));
            else if (typeof(T) == typeof(float))
                return (T)((object)BitConverter.ToSingle(BitConverter.GetBytes(Read<int>(offset)), 0));
            else if (typeof(T) == typeof(double))
                return (T)((object)BitConverter.ToDouble(BitConverter.GetBytes(Read<long>(offset)), 0));
            else if (typeof(T) == typeof(string))
                return (T)((object)Marshal.PtrToStringAnsi(new IntPtr(m_Pointer.ToInt64() + offset)));
            else if (default(T) is ValueType)
                return (T)((object)Marshal.PtrToStructure(new IntPtr(m_Pointer.ToInt64() + offset), typeof(T)));
            else
                throw new InvalidOperationException("T can only be a System.ValueType or a System.String.");
        }
        /// <summary>
        /// Writes a <see cref="{T}"/> at an offset from the pointer.
        /// </summary>
        /// <remarks>
        /// You can only write a struct or string.
        /// </remarks>
        /// <typeparam name="T">A struct or string to write.</typeparam>
        /// <param name="offset">The offset to read from.</param>
        /// <param name="value">The <see cref="{T}"/> to write.</param>
        public void Write<T>(int offset, T value)
        {
            if (typeof(T) == typeof(byte))
                Marshal.WriteByte(m_Pointer, offset, (byte)((object)value));
            else if (typeof(T) == typeof(short))
                Marshal.WriteInt16(m_Pointer, offset, (short)((object)value));
            else if (typeof(T) == typeof(int))
                Marshal.WriteInt32(m_Pointer, offset, (int)((object)value));
            else if (typeof(T) == typeof(long))
                Marshal.WriteInt64(m_Pointer, offset, (long)((object)value));
            else if (typeof(T) == typeof(float))
                Marshal.WriteInt32(m_Pointer, offset, BitConverter.ToInt32(BitConverter.GetBytes((float)((object)value)), 0));
            else if (typeof(T) == typeof(double))
                Marshal.WriteInt64(m_Pointer, offset, BitConverter.ToInt64(BitConverter.GetBytes((double)((object)value)), 0));
            else if (typeof(T) == typeof(string))
                Marshal.Copy(((string)(object)value).ToCharArray(), 0, new IntPtr(m_Pointer.ToInt64() + offset), ((string)(object)value).Length);
            else if (default(T) is ValueType)
                Marshal.StructureToPtr(value, new IntPtr(m_Pointer.ToInt64() + offset), false);
            else
                throw new InvalidOperationException("T can only be a System.ValueType or a System.String.");
        }
        /// <summary>
        /// Frees all unmanaged memory.
        /// </summary>
        public void Free()
        {
            if (m_Free == FreeParams.FreeHandle)
                m_Handle.Free();
            else if (m_Free == FreeParams.FreeHGlobal)
                Marshal.FreeHGlobal(m_Pointer);
        }
        /// <summary>
        /// Creates a new <see cref="IntPtrEx"/> from a <see cref="IntPtr"/>.
        /// </summary>
        /// <param name="ptr">The <see cref="IntPtr"/> to use as base.</param>
        /// <returns>The <see cref="IntPtrEx"/>.</returns>
        public static implicit operator IntPtr(IntPtrEx ptr)
        {
            return ptr.m_Pointer;
        }
        /// <summary>
        /// Returns the underlaying <see cref="IntPtr"/> of a <see cref="IntPtrEx"/>.
        /// </summary>
        /// <param name="ptr">The <see cref="IntPtrEx"/>.</param>
        /// <returns>The underlaying <see cref="IntPtr"/>.</returns>
        public static implicit operator IntPtrEx(IntPtr ptr)
        {
            return new IntPtrEx(ptr);
        }
        /// <summary>
        /// Returns a <see cref="String"/> of the current <see cref="IntPtrEx"/>.
        /// </summary>
        /// <param name="ptr">The <see cref="IntPtrEx"/>.</param>
        /// <returns>The <see cref="String"/> at the memory location of the pointer.</returns>
        public static implicit operator string(IntPtrEx ptr)
        {
            return Marshal.PtrToStringAnsi(ptr.m_Pointer);
        }
        /// <summary>
        /// Adds a <see cref="Int32"/> to the pointer.
        /// </summary>
        /// <param name="ptr">The <see cref="IntPtrEx"/> to add to.</param>
        /// <param name="ofs">The <see cref="Int32"/> to add to the pointer.</param>
        /// <returns>A new <see cref="IntPtrEx"/> of the new memory location.</returns>
        public static IntPtrEx operator +(IntPtrEx ptr, int ofs)
        {
            return new IntPtrEx(ptr.m_Pointer.ToInt64() + ofs);
        }
        /// <summary>
        /// Subtracts a <see cref="Int32"/> to the pointer.
        /// </summary>
        /// <param name="ptr">The <see cref="IntPtrEx"/> to subtract from.</param>
        /// <param name="ofs">The <see cref="Int32"/> to subtract from the pointer.</param>
        /// <returns>A new <see cref="IntPtrEx"/> of the new memory location.</returns>
        public static IntPtrEx operator -(IntPtrEx ptr, int ofs)
        {
            return new IntPtrEx(ptr.m_Pointer.ToInt64() - ofs);
        }
        /// <summary>
        /// Increments the memory locaton of a <see cref="IntPtrEx"/>.
        /// </summary>
        /// <param name="ptr">The <see cref="IntPtrEx"/> to increment.</param>
        /// <returns>A new <see cref="IntPtrEx"/> of the new memory location.</returns>
        public static IntPtrEx operator ++(IntPtrEx ptr)
        {
            return new IntPtrEx(ptr.m_Pointer.ToInt64() + 1);
        }
        /// <summary>
        /// Decrements the memory locaton of a <see cref="IntPtrEx"/>.
        /// </summary>
        /// <param name="ptr">The <see cref="IntPtrEx"/> to decrement.</param>
        /// <returns>A new <see cref="IntPtrEx"/> of the new memory location.</returns>
        public static IntPtrEx operator --(IntPtrEx ptr)
        {
            return new IntPtrEx(ptr.m_Pointer.ToInt64() - 1);
        }

        void IDisposable.Dispose()
        {
            Free();
        }

        private enum FreeParams : byte { None, FreeHandle, FreeHGlobal }
    }
}
