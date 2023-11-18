using ODM;
using SDL2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDL2.SDL;

namespace ODMDemo
{
    internal class DemoApp : IDisposable
    {
        private IntPtr window;
        public static IntPtr renderer;
        public static uint width = 1920;
        public static uint height = 1080;
        private bool running = true;
        Stopwatch stopwatch = new Stopwatch();
        Scene scene;

        public static string MediaPath = "../../../../Media/";
        
        public DemoApp()
        {

            SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);

            /* init SDL window */
            window = SDL_CreateWindow("NextGraphics", SDL.SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED,
                (int)width, (int)height, SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            SDL_SysWMinfo info = new SDL_SysWMinfo();
            SDL_GetWindowWMInfo(window, ref info);

            renderer = SDL_CreateRenderer(window, -1, SDL_RendererFlags.SDL_RENDERER_SOFTWARE);

            World.Init();

            ODM.ObjectInitializer.Init();

            var texture = ResourceCache.Load<Texture>(MediaPath + "Sprite01.png");

            scene = new Scene();

            for(int i = 0; i < 1000; i++)
            {
                var entity = new Entity
                {
                    new Transform
                    {
                        Postion = MathUtil.RandomVector2(),
                        Scale = 0.1f,
                    },

                    new Renderable
                    {
                        Texture = texture,
                    },

                    new Character
                    {
                        DestPoint = MathUtil.RandomVector2()
                    }
                };

                scene.Add(entity);
            }

        }

        public void Run()
        {
            stopwatch.Start();
            while (running)
            {
                while (SDL_PollEvent(out var e) != 0)
                {
                    switch (e.type)
                    {
                        case SDL.SDL_EventType.SDL_QUIT:
                            running = false;
                            break;
                        case SDL_EventType.SDL_WINDOWEVENT:
                            switch (e.window.windowEvent)
                            {
                                case SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                                case SDL_WindowEventID.SDL_WINDOWEVENT_MINIMIZED:
                                case SDL_WindowEventID.SDL_WINDOWEVENT_MAXIMIZED:

                                    OnResize((uint)e.window.data1, (uint)e.window.data2);
                                    break;
                            }

                            break;
                        case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                        case SDL_EventType.SDL_MOUSEBUTTONUP:
                            OnMouseButton(e.button);
                            break;
                        case SDL_EventType.SDL_MOUSEMOTION:
                            OnMouseMove(e.motion);
                            break;
                        case SDL_EventType.SDL_MOUSEWHEEL:
                            OnMouseWheel(e.wheel);
                            break;
                        case SDL_EventType.SDL_KEYDOWN:
                        case SDL_EventType.SDL_KEYUP:
                            OnKey(e.key);
                            break;
                        default:
                            break;
                    }
                }

                var delta = stopwatch.Elapsed.TotalSeconds;
                stopwatch.Restart();

                Update(delta);

                Render();

            }

            Dispose();

            SDL_DestroyRenderer(renderer);
            SDL_DestroyWindow(window);

            SDL.SDL_Quit();
        }

        private void OnResize(uint w, uint h)
        {
            if (w == width && h == height)
            {
                return;
            }

            width = w;
            height = h;
        }

        void OnMouseButton(in SDL_MouseButtonEvent e)
        {
        }

        void OnMouseMove(in SDL_MouseMotionEvent e)
        {
        }

        void OnMouseWheel(in SDL_MouseWheelEvent e)
        {
        }

        void OnKey(in SDL_KeyboardEvent e)
        {
        }

        private void Update(double delta)
        {

            World.Update(delta);

            World.LateUpdate(delta);

        }

        private void Render()
        {
            SDL_SetRenderDrawColor(renderer, 150, 150, 150, 255);
            SDL_RenderClear(renderer);

            World.PreRender();

            World.PostRender();

            SDL_RenderPresent(renderer);
        }

        public void Dispose()
        {
            scene?.Dispose();

            World.Exit();
        }
    }
}
