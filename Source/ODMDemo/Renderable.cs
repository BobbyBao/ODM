using ODM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SDL2.SDL;

namespace ODMDemo
{
    [GenerateComponent]
    public partial struct Renderable : IComponent<Renderable> ,IUpateListener, IPostRenderListener
    {
        public struct Data
        {
            public Texture texture;
            public Vector4 uv;
            public Vector2 center;
            public Data()
            {
            }
        }

        public Texture Texture
        {
            get => data.texture;
            set
            {
                data.texture = value;
            }
        }

        public void Update(double delta)
        {
        }

        public void PostRender()
        {
            ref var data = ref this.data;

            if(!data.texture.IsValid)
            {
                return;
            }

            var srcRect = new SDL_Rect
            {
                x = 0,
                y = 0,
                w = data.texture.Width,
                h = data.texture.Height
            };
            Transform transform = Owner.GetComponent<Transform>();

            SDL_Rect destRect = new()
            {
                x = (int)transform.Postion.X,
                y = (int)transform.Postion.Y,
                w = (int)(srcRect.w * transform.Scale),
                h = (int)(srcRect.h * transform.Scale)
            };

            SDL_Point center = new SDL_Point();
            SDL_RenderCopyEx(DemoApp.renderer, data.texture.Handle, ref srcRect, ref destRect, transform.Rotation, ref center, SDL_RendererFlip.SDL_FLIP_NONE);
        }

    }
}
