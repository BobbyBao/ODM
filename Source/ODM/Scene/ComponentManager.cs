using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODM
{
    public interface IComponentAllocator
    {
        ComponentManager CreateManager();
        Entity GetOwner(uint id);
        void SetOwner(uint id, Entity owner);
        void SetParent(in ComponentID id, Entity parent);
    }

    public class ComponentAllocator<T, V> : ObjectAllocator<T, V>, IComponentAllocator
        where T : IComponent<T> where V : new()
    {
        protected ChunkedArray<Entity> entities = new();

        Func<ComponentManager> creator = null;

        public ComponentAllocator(Func<ComponentManager> creator = null, int chunkSize = 1024, int step = 4) : base(chunkSize, step)
        {
            ComponentManager.componentAllocator[T.TypeID] = this;
            this.creator = creator;
        }

        public Entity GetOwner(uint id)
        {
            return entities[id & IndexMask];
        }

        public void SetOwner(uint id, Entity owner)
        {
            entities[id & IndexMask] = owner;
        }

        public void SetParent(in ComponentID id, Entity parent)
        {
            T t = (T)id;
            if(!t.IsValid)
            {
                return;
            }

            t.SetParent(parent);
        }

        public ComponentManager CreateManager()
        {
            if(creator != null)
            {
                return creator();
            }

            return new ComponentManager<T>();
        }

    }

    public abstract class ComponentManager : IComparable<ComponentManager>
    {
        public int ComponentType { get; }
        public int Priority { get; set; }

        protected List<uint> activeList = new List<uint>();
        public List<uint> ActiveList { get => activeList; }
        
        internal Scene scene;

        internal static IComponentAllocator[] componentAllocator = new IComponentAllocator[1024];

        public static void SetOwner(ComponentID id, Entity owner)
        {
            componentAllocator[id.TypeID].SetOwner(id.Id, owner);
        }

        public static Entity GetOwner(ComponentID id)
        {
            return componentAllocator[id.TypeID].GetOwner(id.Id);
        }

        public static void SetParent(ComponentID id, Entity parent)
        {
            componentAllocator[id.TypeID].SetParent(id, parent);
        }

        public static ComponentManager Create(int type)
        {
            return componentAllocator[type].CreateManager();
        }

        public ComponentManager(int componentType, int priority)
        {
            ComponentType = componentType;
            Priority = priority;
        }

        public int CompareTo(ComponentManager other)
        {
            return Priority.CompareTo(other.Priority);
        }

        public bool Has(uint id)
        {
            return activeList.BinarySearch(id) >= 0;
        }

        public int Find(uint id)
        {
            return activeList.BinarySearch(id);
        }

        private bool Activate(uint id)
        {
            int index = activeList.BinarySearch(id);
            if (index < 0)
            {
                activeList.Insert(~index, id);
                return true;
            }
            else
            {
                Debug.Assert(false);
                return false;
            }
        }

        private bool Deactivate(uint id)
        {
            int index = activeList.BinarySearch(id);
            if (index < 0)
            {
                return false;
            }

            activeList.RemoveAt(index);
            return true;
        }

        public void Activate(ComponentID componentID)
        {
            if(!Activate(componentID.Id))
            {
                return;
            }

            OnActive(componentID);
        }

        public void Deactivate(ComponentID componentID)
        {
            if (!Deactivate(componentID.Id))
            {
                return;
            }

            OnDeactive(componentID);
        }

        protected virtual void OnActive(ComponentID id)
        {
        }

        protected virtual void OnDeactive(ComponentID id)
        {
        }

        public virtual void Update(double delta)
        {
        }

        public virtual void LateUpdate(double delta)
        {
        }

        public virtual void PreRender()
        {
        }

        public virtual void PostRender()
        {
        }

    }

    public class ComponentManager<T> : ComponentManager where T : IComponent<T>
    {
        static bool updateListener = typeof(T).IsAssignableTo(typeof(IUpateListener));
        static bool lateUpdateListener = typeof(T).IsAssignableTo(typeof(ILateUpateListener));
        static bool preRenderListener = typeof(T).IsAssignableTo(typeof(IPreRenderListener));
        static bool postRenderListener = typeof(T).IsAssignableTo(typeof(IPostRenderListener));

        public ComponentManager(int priority = 0) : base(T.TypeID, priority)
        {
        }

        protected override void OnActive(ComponentID id)
        {
            T obj = (T)id;
            obj.OnActive(scene);
        }

        protected override void OnDeactive(ComponentID id)
        {
            T obj = (T)id;
            obj.OnDeactive(scene);
        }

        public override void Update(double delta)
        {
            if(updateListener)
            {
                for (int i = 0; i < ActiveList.Count; i++)
                {
                    T component = (T)activeList[i];
                    component.Update(delta);
                }
            }

        }

        public override void LateUpdate(double delta)
        {
            if (lateUpdateListener)
            {
                for (int i = 0; i < ActiveList.Count; i++)
                {
                    T component = (T)activeList[i];
                    component.LateUpdate(delta);
                }
            }
        }

        public override void PreRender()
        {
            if (preRenderListener)
            {
                for (int i = 0; i < ActiveList.Count; i++)
                {
                    T component = (T)activeList[i];
                    component.PreRender();
                }
            }
        }

        public override void PostRender()
        {
            if (postRenderListener)
            {
                for (int i = 0; i < ActiveList.Count; i++)
                {
                    T component = (T)activeList[i];
                    component.PostRender();
                }
            }
        }

        public void ForEach(Action<uint> action)
        {
            for(int i = 0; i < activeList.Count; i++)
            {
                action(activeList[i]);
            }
        }

    }
}
