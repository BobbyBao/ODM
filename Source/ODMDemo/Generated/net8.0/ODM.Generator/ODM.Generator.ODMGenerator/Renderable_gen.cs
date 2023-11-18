namespace ODMDemo
{
    public partial struct Renderable
    {        
        private uint id;
        public ObjectID Id { get => new(TypeID, id); set => id = value.ShortID; }
        public static int TypeID { get; } = ObjectID.GetTypeIndex<Renderable>();
        public uint Index => (uint)(id & 0xffffff);
        public byte Version => (byte)((id >> 24) & 0xff);
        public bool IsNull => id == 0;
        public bool IsValid => id != 0 && allocator.CheckVersion(id);
        internal ref Data data => ref allocator[id];
        public Entity Owner { get => allocator.GetOwner(id); set => allocator.SetOwner(id, value); }

        public static bool operator ==(in Renderable left, in Renderable right)
        {
            return left.id == right.id;
        }

        public static bool operator !=(in Renderable left, in Renderable right)
        {
            return left.id != right.id;
        }

        public static implicit operator ObjectID(in Renderable obj)
        {
            return new ObjectID(TypeID, obj.id);
        }

        public static explicit operator Renderable(in ObjectID obj)
        {
            if(obj.TypeID != TypeID)
            {
                return default;
            }

            return new Renderable(obj.Id);
        }

        public static explicit operator Renderable(uint id)
        {
            return new Renderable(id);
        }

        public Renderable()
        {
            id = allocator.Create().ShortID;
        }

        public Renderable(uint id)
        {
            this.id = id;
        }

        public void Dispose()
        {
            allocator.Free(Id);
        }

        public static ComponentAllocator<Renderable, Data> allocator;

        public static void __Init()
        {
            allocator = new(()=> new ComponentManager<Renderable>() { Priority = 0 });
        }
    }
}