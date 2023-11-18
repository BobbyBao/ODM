using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODM
{

    public interface IComponent<T> : IObject<T> where T : IComponent<T>
    {
        Entity Owner { get; set; }

        public bool HasComponent<C>() where C : IComponent<C>, new()
        {
            for (int i = 0; i < Owner.Components.Count; i++)
            {
                if (Owner.Components[i].TypeID == C.TypeID)
                {
                    return true;
                }
            }

            return false;
        }

        public C GetComponent<C>() where C : IComponent<C>, new()
        {
            for (int i = 0; i < Owner.Components.Count; i++)
            {
                if (Owner.Components[i].TypeID == C.TypeID)
                {
                    return (C)Owner.Components[i];
                }
            }

            return default;
        }

        public void SetOwner(Entity owner)
        {
        }

        public void SetParent(Entity parent)
        {
        }

        public void OnActive(Scene scene)
        {
        }

        public void OnDeactive(Scene scene)
        {
        }

        public void Update(double delta)
        {
        }

        public void LateUpdate(double delta)
        {
        }

        public void PreRender()
        {
        }

        public void PostRender()
        {
        }
    }

    public interface IUpateListener
    {
        void Update(double delta);
    }

    public interface ILateUpateListener
    {
        void LateUpdate(double delta);
    }

    public interface IPreRenderListener
    {
        void PreRender();
    }

    public interface IPostRenderListener
    {
        void PostRender();
    }

}
