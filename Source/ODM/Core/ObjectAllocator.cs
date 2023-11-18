using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ODM
{

    public interface IObject<T> : IDisposable where T : IObject<T>
    {
        static abstract int TypeID { get; }
        static abstract implicit operator ObjectID(in T obj);
        static abstract explicit operator T(in ObjectID obj);
        static abstract explicit operator T(uint obj);
        public bool IsValid { get; }
        public uint Index { get; }
        public byte Version { get; }

        public void Serialize(ISerializer serializer)
        {
        }

        public void Destroy()
        {
        }
    }

    public struct ObjectID : IEquatable<ObjectID>
    {
        public int TypeID;
        public uint Id;
        public uint ShortID => Id;
        public uint Index => (uint)(Id & 0xffffff);
        public byte Version => (byte)((Id >> 24) & 0xff);
        public bool IsNull => Id == 0;
        public bool IsValid => Id != 0;
        
        public bool Is<T>() where T : IObject<T>
        {
            return TypeID == T.TypeID;
        }

        public static readonly ObjectID Null = new ObjectID();

        public ObjectID(int type)
        {
            this = ObjectAllocator.Create(type);
        }

        public ObjectID(int type, uint id)
        {
            this.TypeID = type;
            this.Id = id;
        } 

        public void Dispose()
        {
            ObjectAllocator.Destroy(this);
        }

        public override bool Equals(object obj)
        {
            return obj is ObjectID other && TypeID == other.TypeID &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TypeID, Id);
        }

        public bool Equals(ObjectID other)
        {
            return TypeID == other.TypeID && Id == other.Id;
        }

        public static bool operator ==(in ObjectID left, in ObjectID right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in ObjectID left, in ObjectID right)
        {
            return !(left == right);
        }

        static List<Type> types = new List<Type>();
#if DEBUG
        static Dictionary<Type, int> typeMap = new();
#endif
        public static int GetTypeIndex<T>() where T : IObject<T> => GetTypeIndex(typeof(T));

        public static int GetTypeIndex(Type type)
        {
            lock (types)
            {
                int index = 0;
#if DEBUG
                if(typeMap.TryGetValue(type, out index))
                {
                    return index;
                }
#endif
                index = types.Count;
                types.Add(type);
#if DEBUG
                typeMap.Add(type, index);
#endif
                return index;
            }
        }

        public static Type GetType(int typeIndex)
        {            
            return types[typeIndex];
        }
    }

    public struct TypeIndex
    {
        public int Index;

    }

    public abstract class ObjectAllocator : Disposable
    {
        public const int IndexBits = 24;
        public const uint IndexMask = 0xffffff;
        public const uint VersionMask = 0xff000000;

        public int TypeID { get; }

        protected ChunkedArray<byte> version = new();

        public ObjectAllocator(int typeID)
        {
            TypeID = typeID;
            managers[typeID] = this;
        }

        [MethodImpl(768)]
        public bool CheckVersion(in ObjectID id)
        {
            return id.Version == version[id.Index];
        }

        [MethodImpl(768)]
        public bool CheckVersion(in uint id)
        {
            return id >> IndexBits == version[id & IndexMask];
        }

        public abstract ObjectID Create();

        public abstract void Free(ObjectID id);

        public readonly static ObjectAllocator[] managers = new ObjectAllocator[1024];

        public static ObjectID Create(int type)
        {
            return managers[type].Create();
        }

        public static void Destroy(ObjectID id)
        {
            managers[id.TypeID].Free(id);
        }

        public static void Shutdown()
        {
            foreach (var manager in managers)
            {
                manager?.Dispose();
            }
        }
    }

    public class ObjectAllocator<T, V> : ObjectAllocator where T : IObject<T> where V : new()
    {
        ChunkedPool<V> manager;

        public ObjectAllocator(int chunkSize = 256, int step = 4) : base(T.TypeID)
        {
            manager = new(chunkSize, step);
        }

        public ref V this[uint id]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get
            {
                return ref manager[id & IndexMask];
            }
        }

        public ref V this[ObjectID id]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get
            {
                return ref manager[id.Index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public ObjectID Add(in V v)
        {
            var index = manager.Add(v);
            var ver = ++version[index];
            return new(TypeID, index | (uint)(ver << 24));
        }

        public override ObjectID Create()
        {
            return Add(new V());
        }

        public override void Free(ObjectID id)
        {
            T obj = (T)id;
            obj.Destroy();   

            manager.Free(id.Index);            
        }

    }
}
