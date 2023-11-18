using System.Runtime.CompilerServices;

namespace ODM
{
    public class Disposable : IDisposable
    {
        private bool disposed;

        [System.Runtime.Serialization.IgnoreDataMember]
        public bool IsDisposed => disposed;

        public void Dispose()
        {
            Dispose(true);
            // Tell the garbage collector not to call the finalizer
            // since all the cleanup will already be done.
            GC.SuppressFinalize(this);
        }

        // If disposing is true, it was called explicitly and we should dispose managed objects.
        // If disposing is false, it was called by the finalizer and managed objects should not be disposed.
        protected virtual void Dispose(bool disposing)
        {
            if(!disposed)
            {
                Destroy(disposing);

                disposed = true;
            }
        }

        protected virtual void Destroy(bool disposing)
        {
        }

        ~Disposable()
        {
            Dispose(false);
        }

    }

    public class RefCounted :  IDisposable
    {
        int _refCount = 1;

        [System.Runtime.Serialization.IgnoreDataMember]
        public int RefCount => _refCount;

        [System.Runtime.Serialization.IgnoreDataMember]
        public bool IsDisposed => _refCount == 0;

        public int AddRef()
        {
            return Interlocked.Increment(ref _refCount);
        }

        public void Dispose()
        {
            Release();
        }

        public void Release()
        {
            if (Interlocked.Decrement(ref _refCount) == 0)
            {
                Destroy(true);

                // Tell the garbage collector not to call the finalizer
                // since all the cleanup will already be done.
                GC.SuppressFinalize(this);
            }
        }

        ~RefCounted()
        {
            Destroy(false);
        }

        protected virtual void Destroy(bool disposing)
        {
        }
    }

}
