using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace ODM
{
    public static class World
    {
        static List<Disposable> subsystems = new();

        public static void Init()
        {
            ObjectInitializer.Init();

            Register<ResourceCache>();

        }

        public static void Register(Disposable disposable)
        {
            subsystems.Add(disposable);
        }

        public static void Register<T>() where T : Disposable, new()
        {
            subsystems.Add(new T());
        }

        static List<Scene> sceneList = new();
        static List<sbyte> freeList = new();
        static object locker = new();

        public static Scene GetScene(sbyte id)
        {
            if(id < 0 || id >= sceneList.Count)
            {
                return null;
            }

            lock (locker)
            {
                return sceneList[id];
            }
        }

        internal static void RegisterScene(Scene scene)
        {
            Debug.Assert(scene != null && scene.sceneIndex == -1);

            sbyte id = -1;
            lock (locker)
            {
                if (freeList.Count > 0)
                {
                    id = freeList[freeList.Count - 1];
                    freeList.RemoveAt(freeList.Count - 1);
                    scene.sceneIndex = id;
                    sceneList[id] = scene;
                }
                else
                {
                    id = (sbyte)sceneList.Count;
                    scene.sceneIndex = id;
                    sceneList.Add(scene);
                }

            }               
            
        }

        internal static void DeRegisterScene(Scene scene)
        {
            Debug.Assert(scene != null && scene.sceneIndex != -1);

            lock (locker)
            {
                freeList.Add(scene.sceneIndex);
                sceneList[scene.sceneIndex] = null;              
            }

            scene.sceneIndex = -1;
        }

        public static void Update(double delta)
        {
            foreach(var scene in sceneList)
            {
                scene.Update(delta);
            }
        }

        public static void LateUpdate(double delta)
        {
            foreach (var scene in sceneList)
            {
                scene.LateUpdate(delta);
            }
        }

        public static void PreRender()
        {
            foreach (var scene in sceneList)
            {
                scene.PreRender();
            }
        }

        public static void PostRender()
        {
            foreach (var scene in sceneList)
            {
                scene.PostRender();
            }
        }

        public static void Exit()
        {
            while(sceneList.Count > 0)
            {
                var scene = sceneList[sceneList.Count - 1];
                scene?.Dispose();
                sceneList.RemoveAt(sceneList.Count - 1);
            }

            ObjectAllocator.Shutdown();

            for (int i = subsystems.Count - 1; i >= 0; i--)
            {
                var subsystem = subsystems[i];
                subsystem.Dispose();
            }

            subsystems.Clear();

        }

    }
}
