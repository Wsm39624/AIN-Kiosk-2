using System;
using System.Buffers;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace AIN.Visitors.Mrz.Helpers
{
    public sealed class SecureMrzBuffer : IDisposable
    {
        private char[]? _buffer;

        public SecureMrzBuffer(int length)
        {
            _buffer = ArrayPool<char>.Shared.Rent(length);
        }

        public Span<char> AsSpan(int length) => _buffer.AsSpan(0, length);

        public void Dispose()
        {
            if (_buffer is null) return;
            
            CryptographicOperations.ZeroMemory(
                MemoryMarshal.AsBytes(_buffer.AsSpan()));
                
            ArrayPool<char>.Shared.Return(_buffer, clearArray: true);
            _buffer = null;
        }
    }
}