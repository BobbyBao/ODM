namespace ODM
{
    public partial struct Entity
    {        
        private uint id;
        public ObjectID Id { get => new(TypeID, id); set => id = value.ShortID; }
        public static int TypeID { get; } = ObjectID.GetTypeIndex<Entity>();
        public uint Index => (uint)(id & 0xffffff);
        public byte Version => (byte)((id >> 24) & 0xff);
        public bool IsNull => id == 0;
        public bool IsValid => id != 0 && manager.CheckVersion(id);
        internal ref Data data => ref manager[id];

        public static bool operator ==(in Entity left, in Entity right)
        {
            return left.id == right.id;
        }

        public static bool operator !=(in Entity left, in Entity right)
        {
            return left.id != right.id;
        }

        public static implicit operator ObjectID(in Entity obj)
        {
            return new ObjectID(TypeID, obj.id);
        }

        public static explicit operator Entity(in ObjectID obj)
        {
            if(obj.TypeID != TypeID)
            {
                return default;
            }

            return new Entity(obj.Id);
        }

        public static explicit operator Entity(uint id)
        {
            return new Entity(id);
        }

        public Entity()
        {
            this.id = manager.Create().ShortID;
        }

        public Entity(uint id)
        {
            this.id = id;
        }

        public void Dispose()
        {
            manager.Free(Id);
        }

        public static ObjectAllocator<Entity, Data> manager;

        public static void __Init()
        {
            manager = new ();
        }
    }
}