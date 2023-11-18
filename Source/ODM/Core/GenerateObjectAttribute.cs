using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODM
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class GenerateSerializeAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    public class GenerateObjectAttribute : Attribute
    {
        public Type DataType { get; set; }
        public Type ManagerType { get; set; }
    }

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    public class GenerateResourceAttribute : Attribute
    {
        public Type DataType { get; set; }
        public Type ManagerType { get; set; }
    }

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    public class GenerateComponentAttribute : Attribute
    {
        public Type DataType { get; set; }
        public Type ManageType { get; set; }
        public int Priority { get; set; } = 0;
        public int ChunkSize { get; set; } = 1024;
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class GenerateHeaderAttribute : Attribute
    {
        public string Header { get; }
        public GenerateHeaderAttribute(string str)
        {
            Header = str;
        }
    }

}
