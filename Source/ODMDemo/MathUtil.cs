using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ODMDemo
{
    public static class MathUtil
    {
        static Random random = new Random();

        public static Vector2 RandomVector2()
        {
            return new Vector2(DemoApp.width * random.NextSingle(), DemoApp.height * random.NextSingle());
        }
    }
}
