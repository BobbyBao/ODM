using StbImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDL2;
using static SDL2.SDL;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ODMDemo
{
    [GenerateResource]
    public partial struct Texture : IResource<Texture>
    {
        public struct Data
        {
            public IntPtr texture;
            public int width, height;
        }

        public int Width => data.width;
        public int Height => data.height;
        public IntPtr Handle => data.texture;

        public unsafe bool Load(Stream stream)
        {
            int width, height, channels;

            var context = new StbImage.stbi__context(stream);

            var result = StbImage.stbi__load_and_postprocess_8bit(context, &width, &height, &channels, (int)ColorComponents.RedGreenBlueAlpha);
            if (result == null)
            {
                return false;
            }

            if (width == 0 || height == 0)
            {
                return false;
            }


            uint format = SDL_PIXELFORMAT_ABGR8888;

            var surf = SDL_CreateRGBSurfaceWithFormatFrom((nint)result, width, height,
                                                      channels * 8, channels * width, format);

            var texture = SDL_CreateTextureFromSurface(DemoApp.renderer, surf);
            SDL_FreeSurface(surf);
            Marshal.FreeHGlobal(new IntPtr(result));

            data.width = width;
            data.height = height;
            data.texture = texture;
            return true;
        }

        public void Destroy()
        {
            if(data.texture != 0)
            {
                SDL_DestroyTexture(data.texture);
            }
        }


    }
}
