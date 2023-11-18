using Collections.Pooled;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODM
{
    interface IEntity<T> : IObject<T> where T : IEntity<T>
    {
    }

    [GenerateObject]
    public partial struct Entity : IEntity<Entity>, IEnumerable<Entity>
    {
        public struct Data
        {
            public string name;
            public bool active = true;
            public PooledList<ComponentID> Components = new();
            public PooledList<Entity> Children = new();
            public Entity Parent;
            public sbyte SceneIndex = -1;

            public Data()
            {
            }

        }

        public Entity(string name) : this()
        {
            data.name = name;
        }

        public Entity Parent
        {  
            get => this.data.Parent;
            private set
            {
                this.data.Parent = value;

                for(int i = 0; i < this.data.Components.Count; i++)
                {
                    ComponentManager.SetParent((ComponentID)this.data.Components[i], value);
                }
            }
        }

        public Scene Scene => World.GetScene((sbyte)this.data.SceneIndex);

        public string Name 
        { 
            get => this.data.name;
            set=> this.data.name = value; 
        }

        public bool IsActive
        {
            get => this.data.active;
            set => this.data.active = value;
        }

        public IReadOnlyList<ComponentID> Components => data.Components;

        public bool HasComponent<T>() where T : IComponent<T>, new()
        {
            for (int i = 0; i < this.data.Components.Count; i++)
            {
                if (this.data.Components[i].TypeID == T.TypeID)
                {
                    return true;
                }
            }

            return false;
        }

        public T GetComponent<T>() where T : IComponent<T>, new()
        {
            for(int i = 0; i < this.data.Components.Count; i++)
            {
                if(this.data.Components[i].TypeID == T.TypeID)
                {
                    return (T)this.data.Components[i];
                }
            }

            return default;
        }

        public T GetOrCreate<T>() where T : IComponent<T>, new()
        {
            for (int i = 0; i < this.data.Components.Count; i++)
            {
                if (this.data.Components[i].TypeID == T.TypeID)
                {
                    return (T)this.data.Components[i];
                }
            }

            return AddComponent<T>();
        }

        public T AddComponent<T>() where T : IComponent<T>, new()
        {
            var component = new T();
            AddComponent(component);

            return component;
        }

        public void AddComponent(ComponentID component)
        {
            this.data.Components.Add(component);
            ComponentManager.SetOwner(component, this);
            var scene = Scene;
            scene?.StartComponent(component);
        }
      
        public void RemoveComponent(ComponentID component)
        {
            ComponentManager.SetOwner(component, default);

            this.data.Components.Remove(component); 
            var scene = Scene;
            scene?.StopComponent(component);
        }
        
        public int ChildCount => data.Children?.Count ?? 0;

        public Entity CreateChild(string name = "")
        {
            var e = new Entity(name);            
            return AddChild(e);
        }

        public Entity AddChild(Entity e)
        {
            ref var data = ref this.data;
            data.Children.Add(e);
            e.Parent = this;
            var scene = Scene;
            scene?.StartComponent(e.Id);
            return e;
        }

        public void RemoveChild(Entity e)
        {
            data.Children.Remove(e);
            
            var scene = Scene;
            scene?.StopComponent(e.Id);
        }

        public Entity GetChild(string name, bool recursive)
        {
            if (data.Children == null)
                return default;

            foreach (var c in data.Children)
            {
                if (c.Name == name)
                    return c;

                if (recursive)
                {
                    var node = c.GetChild(name, true);
                    if (node.IsValid)
                        return node;
                }
            }

            return default;
        }

        public void Add(Entity child)
        {
            AddChild(child);
        }

        public void Add<T>(in T c) where T : IComponent<T>, new()
        {
            AddComponent(c);
        }

        public IEnumerator<Entity> GetEnumerator()
        {
            return this.data.Children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.data.Children.GetEnumerator();
        }

        public void Destroy()
        {
            foreach (var child in data.Children)
            {
                child.Dispose();
            }

            foreach (var component in Components)
            {
                component.Dispose();
            }

        }

    }


}
