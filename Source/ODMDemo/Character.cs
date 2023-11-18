using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ODMDemo
{
    [GenerateComponent]
    public partial struct Character : IComponent<Character>, IUpateListener
    {
        public struct Data
        {
            public Vector2 destPoint;
            public float speed = 100.0f;

            public Transform transform;

            public Data()
            {
            }
        }

        public Vector2 DestPoint
        {
            get => data.destPoint;
            set
            {
                data.destPoint = value;
            }
        }

        public void OnActive(Scene scene)
        {
            data.transform = Owner.GetComponent<Transform>();
        }

        public void Update(double delta)
        {
            ref var data = ref this.data;
            float move = (float)(delta * data.speed);
            var dir = data.destPoint - data.transform.Postion;
            float dis = dir.Length();
            if(dis < move)
            {
                data.transform.Postion = data.destPoint;
                data.destPoint = MathUtil.RandomVector2();
            }
            else
            {
                data.transform.Postion += move * dir / dis;
            }

        }
    }
}
