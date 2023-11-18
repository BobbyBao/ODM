namespace ODMDemo
{
    public partial struct Character
    {        
        private uint id;
        public ObjectID Id { get => new(TypeID, id); set => id = value.ShortID; }
        public static int TypeID { get; } = ObjectID.GetTypeIndex<Character>();
        public uint Index => (uint)(id & 0xffffff);
        public byte Version => (byte)((id >> 24) & 0xff);
        public bool IsNull => id == 0;
        public bool IsValid => id != 0 && allocator.CheckVersion(id);
        internal ref Data data => ref allocator[id];
        public Entity Owner { get => allocator.GetOwner(id); set => allocator.SetOwner(id, value); }

        public static bool operator ==(in Character left, in Character right)
        {
            return left.id == right.id;
        }

        public static bool operator !=(in Character left, in Character right)
        {
            return left.id != right.id;
        }

        public static implicit operator ObjectID(in Character obj)
        {
            return new ObjectID(TypeID, obj.id);
        }

        public static explicit operator Character(in ObjectID obj)
        {
            if(obj.TypeID != TypeID)
            {
                return default;
            }

            return new Character(obj.Id);
        }

        public static explicit operator Character(uint id)
        {
            return new Character(id);
        }

        public Character()
        {
            id = allocator.Create().ShortID;
        }

        public Character(uint id)
        {
            this.id = id;
        }

        public void Dispose()
        {
            allocator.Free(Id);
        }

        public static ComponentAllocator<Character, Data> allocator;

        public static void __Init()
        {
            allocator = new(()=> new ComponentManager<Character>() { Priority = 0 });
        }
    }
}