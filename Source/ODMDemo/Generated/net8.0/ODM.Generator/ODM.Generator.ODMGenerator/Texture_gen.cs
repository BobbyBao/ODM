namespace ODMDemo
{
    public partial struct Texture
    {        
        private uint id;
        public ObjectID Id { get => new(TypeID, id); set => id = value.ShortID; }
        public static int TypeID { get; } = ObjectID.GetTypeIndex<Texture>();
        public uint Index => (uint)(id & 0xffffff);
        public byte Version => (byte)((id >> 24) & 0xff);
        public bool IsNull => id == 0;
        public bool IsValid => id != 0 && manager.CheckVersion(id);
        internal ref Data data => ref manager[id];

        public static bool operator ==(in Texture left, in Texture right)
        {
            return left.id == right.id;
        }

        public static bool operator !=(in Texture left, in Texture right)
        {
            return left.id != right.id;
        }

        public static implicit operator ObjectID(in Texture obj)
        {
            return new ObjectID(TypeID, obj.id);
        }

        public static explicit operator Texture(in ObjectID obj)
        {
            if(obj.TypeID != TypeID)
            {
                return default;
            }

            return new Texture(obj.Id);
        }

        public static explicit operator Texture(uint id)
        {
            return new Texture(id);
        }

        public Texture()
        {
            this.id = manager.Create().ShortID;
        }

        public Texture(uint id)
        {
            this.id = id;
        }

        public void Dispose()
        {
            manager.Free(Id);
        }

        public static ResourceManager<Texture, Data> manager;

        public static void __Init()
        {
            manager = new ();
        }
    }
}