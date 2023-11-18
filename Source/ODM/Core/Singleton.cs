

namespace ODM
{
    public class Singleton<T> : Disposable where T : class
    {
        protected static T self;
        public static T Instance
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => self;
        }

        public Singleton()
        {
            self = Unsafe.As<T>(this);
        }

    }

}
