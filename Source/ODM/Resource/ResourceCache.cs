namespace ODM
{
    public class ResourceCache : Singleton<ResourceCache>
    {
        public ResourceCache()
        {
        }

        public static ResourceID Load(byte type, string name)
        {
            Stream stream = File.OpenRead(name);
            return ResourceManager.Get(type)?.Load(stream) ?? default;
        }

        public static bool TryGet<T>(string name, out T id) where T : IResource<T>, new()
        {
            var manager = ResourceManager.Get(T.TypeID);
            if (manager == null)
            {
                id = default;
                return false;
            }

            if(!manager.TryGet(name, out var res))
            {
                id = default;
                return false;
            }

            id = (T)res;
            id.AddRef();
            return true;
        }

        public static void Register<T>(string name, T id) where T : IResource<T>, new()
        {
            var manager = ResourceManager.Get(T.TypeID);
            if (manager == null)
            {
                return;
            }

            manager.Register(name, id);
            id.AddRef();
        }

        public static T Load<T>(string name) where T: IResource<T>, new()
        {
            var manager = ResourceManager.Get(T.TypeID);
            if(manager == null)
            {
                return default;
            }

            return (T)manager.Load(name);
        }

        public static T Load<T>(Stream stream) where T : IResource<T>, new()
        {
            var manager = ResourceManager.Get(T.TypeID);
            if (manager == null)
            {
                return default;
            }

            return (T)manager.Load(stream);
        }
    }
}
