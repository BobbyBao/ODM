namespace ODM
{
    public static partial class ObjectInitializer
    {
        public static void Init()
        {
			ODMDemo.Character.__Init();
			ODMDemo.Renderable.__Init();
			ODMDemo.Texture.__Init();
			ODMDemo.Transform.__Init();

        }
    }
}