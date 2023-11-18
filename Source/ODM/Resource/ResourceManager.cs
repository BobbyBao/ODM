using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace ODM
{
    public abstract class ResourceManager : ObjectAllocator
    {
        protected ChunkedArray<ResourceData> resourceData = new();

        Dictionary<string, ResourceID> resourceMap = new();

        public ResourceManager(int typeID) : base(typeID)
        {
        }

        [MethodImpl(768)]
        public ref ResourceData GetResourceData(ResourceID id)
        {
            return ref resourceData[id.Index];
        }

        [MethodImpl(768)]
        public ref ResourceData GetResourceData(uint id)
        {
            return ref resourceData[id & IndexMask];
        }

        public bool TryGet(string name, out ResourceID id)
        {
            return resourceMap.TryGetValue(name, out id);
        }

        public void Register(string name, ResourceID id)
        {
            resourceMap.Add(name, id);
        }

        public ResourceID Load(string name)
        {
            if(resourceMap.TryGetValue(name, out var obj))
            {
                return obj;
            }

            Stream stream = File.OpenRead(name);
            obj = Load(stream);
            if(obj.IsValid)
            {
                resourceMap.Add(name, obj);
            }
            else
            {
                stream.Seek(0, SeekOrigin.Begin);
                //
            }

            return obj;
        }

        public ResourceID Load(Stream stream)
        {
            return OnLoad(stream);
        }

        protected virtual ResourceID OnLoad(Stream stream)
        {
            return default;
        }

        [MethodImpl(768)]
        public static ResourceManager Get(int type)
        {
            return managers[type] as ResourceManager;
        }

        protected override void Destroy(bool disposing)
        {
            foreach(var it in resourceMap)
            {
                it.Value.Dispose();
            }

            resourceMap.Clear();
        }
    }

    public class ResourceManager<T, V> : ResourceManager where T : IResource<T>, new() where V : new()
    {
        ChunkedPool<V> manager = new();

        public ResourceManager() : base(T.TypeID)
        {
        }

        public ref V this[uint id]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get
            {
                return ref manager[id & IndexMask];
            }
        }

        public ResourceID Add(in V v)
        {
            var index = manager.Add(v);
            resourceData[index] = new ResourceData();
            return new(TypeID, index);
        }

        public override ResourceID Create()
        {
            return Add(new V());
        }

        public override void Free(ResourceID id)
        {
            ref var v = ref resourceData[id.Index];
            if(v.Release() == 0)
            {
                v.Dispose();
                manager.Free(id.Index & IndexMask);
            }
        }

        protected override ResourceID OnLoad(Stream stream)
        {
            T obj = new T();
            if (obj.Load(stream))
            {
                return obj;
            }
            return default;
        }
    }
}
