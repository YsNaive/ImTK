using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Buffers;
using System.Text;

namespace ImTK.Core
{
    public class NativeUtf8Buffer
    {
        public IntPtr Data { get; private set; }
        public int Length { get; private set; }
        public int Capacity { get; private set; }

        public bool IsEmpty => Data == IntPtr.Zero || Length == 0;

        public NativeUtf8Buffer()
        {
        }

        ~NativeUtf8Buffer()
        {
            DisposeNative();
        }

        private void DisposeNative()
        {
            if (Data != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(Data);
                Data = IntPtr.Zero;
                Capacity = 0;
                Length = 0;
            }
        }

        public void EnsureCapacity(int size)
        {
            if (Capacity >= size) return;
            int newCapacity = Math.Max(Capacity * 2, size);
            if (newCapacity < 64) newCapacity = 64;
            
            if (Data == IntPtr.Zero)
            {
                Data = Marshal.AllocCoTaskMem(newCapacity);
            }
            else
            {
                Data = Marshal.ReAllocCoTaskMem(Data, newCapacity);
            }
            Capacity = newCapacity;
        }

        public void SetString(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                Length = 0;
                if (Data != IntPtr.Zero)
                {
                    unsafe { ((byte*)Data)[0] = 0; }
                }
                return;
            }

            int byteCount = Encoding.UTF8.GetByteCount(text);
            EnsureCapacity(byteCount + 1);
            
            unsafe
            {
                fixed (char* pText = text)
                {
                    Encoding.UTF8.GetBytes(pText, text.Length, (byte*)Data, Capacity);
                }
                ((byte*)Data)[byteCount] = 0; // Null terminator
            }
            Length = byteCount;
        }

        public void SetText(ReadOnlySpan<byte> utf8Text)
        {
            EnsureCapacity(utf8Text.Length + 1);
            unsafe
            {
                utf8Text.CopyTo(new Span<byte>((byte*)Data, Capacity));
                ((byte*)Data)[utf8Text.Length] = 0;
            }
            Length = utf8Text.Length;
        }

        public override string ToString()
        {
            if (IsEmpty) return string.Empty;
            unsafe
            {
                return Encoding.UTF8.GetString((byte*)Data, Length);
            }
        }
    }

    [InterpolatedStringHandler]
    public ref struct ImTKUtf8StringHandler
    {
        private byte[] m_rentedArray;
        private Span<byte> m_span;
        public int WrittenCount;

        public ImTKUtf8StringHandler(int literalLength, int formattedCount)
        {
            int initialCapacity = literalLength + formattedCount * 16 + 1;
            m_rentedArray = ArrayPool<byte>.Shared.Rent(initialCapacity);
            m_span = m_rentedArray;
            WrittenCount = 0;
        }

        public void Dispose()
        {
            if (m_rentedArray != null)
            {
                ArrayPool<byte>.Shared.Return(m_rentedArray);
                m_rentedArray = null;
            }
        }
        
        public ReadOnlySpan<byte> WrittenSpan => m_span.Slice(0, WrittenCount);

        private void EnsureCapacity(int additional)
        {
            if (WrittenCount + additional <= m_span.Length) return;
            int newCapacity = Math.Max(m_span.Length * 2, WrittenCount + additional);
            var newArray = ArrayPool<byte>.Shared.Rent(newCapacity);
            m_span.Slice(0, WrittenCount).CopyTo(newArray);
            if (m_rentedArray != null)
            {
                ArrayPool<byte>.Shared.Return(m_rentedArray);
            }
            m_rentedArray = newArray;
            m_span = m_rentedArray;
        }

        public void AppendLiteral(string s)
        {
            if (string.IsNullOrEmpty(s)) return;
            int maxBytes = Encoding.UTF8.GetMaxByteCount(s.Length);
            EnsureCapacity(maxBytes);
            int bytesWritten = Encoding.UTF8.GetBytes(s, m_span.Slice(WrittenCount));
            WrittenCount += bytesWritten;
        }

        public void AppendFormatted<T>(T value)
        {
            if (value is IUtf8SpanFormattable utf8Formattable)
            {
                int bytesWritten;
                while (!utf8Formattable.TryFormat(m_span.Slice(WrittenCount), out bytesWritten, default, null))
                {
                    EnsureCapacity(m_span.Length);
                }
                WrittenCount += bytesWritten;
            }
            else if (value is ISpanFormattable formattable)
            {
                char[] temp = ArrayPool<char>.Shared.Rent(256);
                try
                {
                    int charsWritten;
                    while (!formattable.TryFormat(temp, out charsWritten, default, null))
                    {
                        ArrayPool<char>.Shared.Return(temp);
                        temp = ArrayPool<char>.Shared.Rent(temp.Length * 2);
                    }
                    int maxBytes = Encoding.UTF8.GetMaxByteCount(charsWritten);
                    EnsureCapacity(maxBytes);
                    int bytesWritten = Encoding.UTF8.GetBytes(temp.AsSpan(0, charsWritten), m_span.Slice(WrittenCount));
                    WrittenCount += bytesWritten;
                }
                finally
                {
                    ArrayPool<char>.Shared.Return(temp);
                }
            }
            else if (value is string s)
            {
                AppendLiteral(s);
            }
            else if (value != null)
            {
                AppendLiteral(value.ToString());
            }
        }

        public void AppendFormatted<T>(T value, string format)
        {
            if (value is IUtf8SpanFormattable utf8Formattable)
            {
                int bytesWritten;
                while (!utf8Formattable.TryFormat(m_span.Slice(WrittenCount), out bytesWritten, format, null))
                {
                    EnsureCapacity(m_span.Length);
                }
                WrittenCount += bytesWritten;
            }
            else if (value is ISpanFormattable formattable)
            {
                char[] temp = ArrayPool<char>.Shared.Rent(256);
                try
                {
                    int charsWritten;
                    while (!formattable.TryFormat(temp, out charsWritten, format, null))
                    {
                        ArrayPool<char>.Shared.Return(temp);
                        temp = ArrayPool<char>.Shared.Rent(temp.Length * 2);
                    }
                    int maxBytes = Encoding.UTF8.GetMaxByteCount(charsWritten);
                    EnsureCapacity(maxBytes);
                    int bytesWritten = Encoding.UTF8.GetBytes(temp.AsSpan(0, charsWritten), m_span.Slice(WrittenCount));
                    WrittenCount += bytesWritten;
                }
                finally
                {
                    ArrayPool<char>.Shared.Return(temp);
                }
            }
            else if (value is IFormattable fmt)
            {
                AppendLiteral(fmt.ToString(format, null));
            }
            else if (value != null)
            {
                AppendLiteral(value.ToString());
            }
        }
    }
}
