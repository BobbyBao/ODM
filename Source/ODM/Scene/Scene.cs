using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODM
{
    public class Scene : Disposable, IEnumerable<Entity>
    {
        List<Entity> entities = new();        

        public List<Entity> RootObjects { get => entities; }

        List<ComponentManager> componentManagers = new();

        internal sbyte sceneIndex = -1;
        
        public Scene()
        {
            World.RegisterScene(this);
        }

        public void Add(in Entity obj)
        {
            entities.Add(obj);

            Activate(obj);
        }

        public void Remove(in Entity obj)
        {
            Deactivate(obj);

            entities.Remove(obj);

            obj.Dispose();
        }

        public void RemoveAll()
        {
            while( entities.Count > 0)
            {
                var obj = entities[entities.Count - 1];
                Remove(obj);
            }
        }

        public ComponentManager GetComponentManager(int type)
        {
            for(int i = 0; i < componentManagers.Count; i++)
            {
                if (componentManagers[i].ComponentType == type)
                {
                    return componentManagers[i];
                }
            }

            return null;
        }

        public void Add(ComponentManager manager)
        {
            int index = componentManagers.BinarySearch(manager);
            if(index < 0)
            {
                componentManagers.Insert(~index, manager);             
            }
            else
            {
                componentManagers.Insert(index, manager);
            }

            manager.scene = this;
        }

        public void Update(double delta)
        {
            foreach (var component in componentManagers)
            {
                component.Update(delta);
            }

        }

        public void LateUpdate(double delta)
        {
            foreach (var component in componentManagers)
            {
                component.LateUpdate(delta);
            }

        }

        public void PreRender()
        {
            foreach (var component in componentManagers)
            {
                component.PreRender();
            }
        }

        public void PostRender()
        {
            foreach (var component in componentManagers)
            {
                component.PostRender();
            }
        }

        internal void Activate(in Entity obj)
        {
            ref Entity.Data data = ref obj.data;

            for (int i = 0; i < data.Components.Count; i++)
            {
                var component = data.Components[i];
                if (component != ComponentID.Null)
                {
                    StartComponent(component);
                }
            }

            data.SceneIndex = sceneIndex;

            foreach (var child in data.Children)
            {
                Activate(child);
            }

        }

        internal void Deactivate(in Entity obj)
        {
            ref Entity.Data data = ref obj.data;

            foreach (var child in data.Children)
            {
                Deactivate(child);
            }

            for (int i = 0; i < data.Components.Count; i++)
            {
                var component = data.Components[i];
                if (component != ComponentID.Null)
                {
                    StopComponent(component);
                }
            }

            data.SceneIndex = -1;

        }

        internal void StartComponent(ComponentID id)
        {
            var manager = GetComponentManager(id.TypeID);
            if(manager == null)
            {
                manager = ComponentManager.Create(id.TypeID);
                Add(manager);
            }

            manager.Activate(id);
        }

        internal void StopComponent(ComponentID id)
        {
            var manager = GetComponentManager(id.TypeID);
            if (manager == null)
            {
                manager = ComponentManager.Create(id.TypeID);
                Add(manager);
                manager.scene = this;
            }

            manager.Deactivate(id);
        }

        protected override void Destroy(bool disposing)
        {
            foreach(var so in RootObjects)
            {
                so.Dispose();
            }

            World.DeRegisterScene(this);
        }

        public IEnumerator<Entity> GetEnumerator()
        {
            return ((IEnumerable<Entity>)entities).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)entities).GetEnumerator();
        }
    }
}
