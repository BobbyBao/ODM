using ODM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ODMDemo
{
    [GenerateComponent]
    public partial struct Transform : IComponent<Transform> ,IUpateListener
    {
        public struct Data
        {
            public Vector2 pos;
            public float rot;
            public float scale = 1.0f;
            public bool dirty = true;
            public Matrix3x2 transform = Matrix3x2.Identity;

            public Data()
            {
            }
        }

        public Vector2 Postion
        {
            get => data.pos;
            set
            {
                data.pos = value;
            }
        }

        public float Rotation
        {
            get => data.rot;
            set
            {
                data.rot = value;
            }
        }

        public float Scale
        {
            get => data.scale;
            set
            {
                data.scale = value;
            }
        }

        public void Update(double delta)
        {
        }
    }
}
