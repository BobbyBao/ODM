using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ODM
{
    public struct ResourceData : IDisposable
    {
        internal int refCount = 1;
        public Guid guid;

        public ResourceID[] embeddedResources = Array.Empty<ResourceID>();

        public ResourceData()
        {
        }

        public void AddRef()
        {
            Interlocked.Increment(ref refCount);
        }

        public int Release()
        {
            return Interlocked.Decrement(ref refCount);
        }

        public void Dispose()
        {
            foreach (var resource in embeddedResources)
            {
                resource.Dispose();
            }
        }
    }

    public interface IResource<T> : IObject<T> where T : IResource<T>
    {
        public ref ResourceData ResourceData
        {
            [MethodImpl(768)]
            get => ref ResourceManager.Get(T.TypeID).GetResourceData(Index); 
        }

        public int RefCount
        {
            get => ResourceData.refCount;
        }

        public Guid Guid
        {
            get => ResourceData.guid;
        }

        public bool Load(Stream stream)
        {
            return false;
        }

        public void OnBuild()
        {
        }
    }

    public static class ResourceExt
    {
        public static void AddRef<T>(this IResource<T> resource ) where T : IResource<T>
        {
            resource.ResourceData.AddRef();
        }
        public static void Release<T>(this IResource<T> resource) where T : IResource<T>
        {
            resource.ResourceData.Release();
        }

    }
}
