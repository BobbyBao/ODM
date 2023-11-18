namespace ODMDemo
{
    public partial struct Transform
    {        
        private uint id;
        public ObjectID Id { get => new(TypeID, id); set => id = value.ShortID; }
        public static int TypeID { get; } = ObjectID.GetTypeIndex<Transform>();
        public uint Index => (uint)(id & 0xffffff);
        public byte Version => (byte)((id >> 24) & 0xff);
        public bool IsNull => id == 0;
        public bool IsValid => id != 0 && allocator.CheckVersion(id);
        internal ref Data data => ref allocator[id];
        public Entity Owner { get => allocator.GetOwner(id); set => allocator.SetOwner(id, value); }

        public static bool operator ==(in Transform left, in Transform right)
        {
            return left.id == right.id;
        }

        public static bool operator !=(in Transform left, in Transform right)
        {
            return left.id != right.id;
        }

        public static implicit operator ObjectID(in Transform obj)
        {
            return new ObjectID(TypeID, obj.id);
        }

        public static explicit operator Transform(in ObjectID obj)
        {
            if(obj.TypeID != TypeID)
            {
                return default;
            }

            return new Transform(obj.Id);
        }

        public static explicit operator Transform(uint id)
        {
            return new Transform(id);
        }

        public Transform()
        {
            id = allocator.Create().ShortID;
        }

        public Transform(uint id)
        {
            this.id = id;
        }

        public void Dispose()
        {
            allocator.Free(Id);
        }

        public static ComponentAllocator<Transform, Data> allocator;

        public static void __Init()
        {
            allocator = new(()=> new ComponentManager<Transform>() { Priority = 0 });
        }
    }
}