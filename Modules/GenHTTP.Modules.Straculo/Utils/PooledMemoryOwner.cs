using System.Buffers;

namespace GenHTTP.Modules.Straculo.Utils;

public sealed class PooledMemoryOwner : IMemoryOwner<byte>
{
    private readonly ArrayPool<byte> _pool;
    
    private byte[]? _buffer;

    public PooledMemoryOwner(byte[] buffer, int length, ArrayPool<byte> pool)
    {
        _buffer = buffer;

        Memory = new Memory<byte>(_buffer, 0, length);

        _pool = pool;
    }
    
    public Memory<byte> Memory { get; }
    
    public void Dispose()
    {
        if (_buffer == null)
        {
            return;
        }
        
        _pool.Return(_buffer);

        _buffer = null;
    }
}