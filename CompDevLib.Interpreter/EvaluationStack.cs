#define MEMORY_CHECK
using System;
using System.Runtime.InteropServices;

namespace CompDevLib.Interpreter
{
    /// <summary>
    /// stack with fixed size.
    /// </summary>
    public sealed class EvaluationStack : IDisposable
    {
        private const int DefaultMaxObjectCount = 128;
        private const int DefaultBufferSize = 4 * 1024;
        private readonly byte[] _buffer;
        private readonly object[] _objects;
        private unsafe byte* _pointer;
        private GCHandle _gcHandle;
        private int _offset;
        private int _objCount;
        private bool _disposed;

        public int Position => _offset;

        public unsafe EvaluationStack(int dataBufferSize = DefaultBufferSize, int maxObjectCount = DefaultMaxObjectCount)
        {
            _buffer = new byte[dataBufferSize];
            _objects = new object[maxObjectCount];
            _gcHandle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
            _pointer = (byte*) _gcHandle.AddrOfPinnedObject().ToPointer();
        }

        /// <summary>
        /// Allocate memory of given size on the buffer. The memory will not be initialized.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="ppData"></param>
        /// <returns></returns>
        /// <exception cref="OutOfMemoryException"></exception>
        public unsafe int Allocate(int size, byte** ppData)
        {
            var offset = _offset;
            _offset += size;
            *ppData = _pointer + offset;
            
#if MEMORY_CHECK
            if (_offset >= _buffer.Length)
                throw new OutOfMemoryException("EvaluationStack is full.");
#endif
            
            return offset;
        }

        public unsafe int PushUnmanaged<T>(T value) where T : unmanaged
        {
            var offset = _offset;
            _offset += sizeof(T);

#if MEMORY_CHECK
            if (_offset >= _buffer.Length)
                throw new OutOfMemoryException("EvaluationStack is full.");
#endif

            *(T*)(_pointer + offset) = value;
            return offset;
        }

        public unsafe T PopUnmanaged<T>() where T : unmanaged
        {
            _offset -= sizeof(T);

#if MEMORY_CHECK
            if (_offset < 0)
                throw new OutOfMemoryException($"Unable to pop data of size {sizeof(T)} from EvaluationStack.");
#endif

            return *(T*)(_pointer + _offset);
        }

        public unsafe T GetUnmanaged<T>(int offset) where T : unmanaged
        {
#if MEMORY_CHECK
            if (offset + sizeof(T) > _offset)
                throw new OutOfMemoryException($"Unable to get data of type {nameof(T)} at offset {offset} from EvaluationStack.");
#endif
            var value = *(T*)(_pointer + offset);
            return value;
        }

        public unsafe int PushObject<T>(T value) where T : class
        {
            var index = _objCount;
            var offset = _offset;

            _offset += sizeof(int);
            _objCount++;

#if MEMORY_CHECK
            if (_offset >= _buffer.Length)
                throw new OutOfMemoryException("EvaluationStack is full.");

            if (_objCount >= _objects.Length)
                throw new OutOfMemoryException("EvaluationStack is full.");
#endif

            *(int*)(_pointer + offset) = index;
            _objects[index] = value;
            
            return offset;
        }

        public unsafe T PopObject<T>() where T : class
        {
            _offset -= sizeof(int);
            _objCount--;

#if MEMORY_CHECK
            if (_offset < 0 || _objCount < 0)
                throw new OutOfMemoryException($"Unable to pop data of type {nameof(T)} from EvaluationStack.");
#endif

            var index = *(int*)(_pointer + _offset);
            
#if MEMORY_CHECK
            if (index != _objCount)
                throw new Exception($"Internal index {index} is not as expected {_objCount}.");
#endif
            
            var result = (T)_objects[index];
            _objects[index] = null;

            return result;
        }
        
        public unsafe T GetObject<T>(int offset) where T : class
        {
#if MEMORY_CHECK
            if (offset + sizeof(int) > _offset)
                throw new OutOfMemoryException($"Unable to get data of type {nameof(T)} at offset {offset} from EvaluationStack.");
#endif
            
            var index = *(int*)(_pointer + offset);
            return (T)_objects[index];
        }

        public void Clear()
        {
            _offset = 0;
            _objCount = 0;
        }

        private unsafe void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
            }
            
            _gcHandle.Free();
            _pointer = null;
            
            _disposed = true;
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~EvaluationStack() => Dispose(false);
    }
}