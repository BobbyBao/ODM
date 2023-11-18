#define ENABLE_DEBUG
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ODM.Generator
{
   
    [Generator]
    public class ODMGenerator : ISourceGenerator
    {
        private const string targetNamespace = "ODM";

        public static INamedTypeSymbol? generateObjectAttribute;
        public static INamedTypeSymbol? generateResourceAttribute;
        public static INamedTypeSymbol? generateComponentAttribute;


        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
#if ENABLE_DEBUG
                //Debugger.Launch();
#endif
            }
#endif 
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
                return;

            Compilation compilation = context.Compilation;

            InitAttributes(compilation);

            var classSymbols = GetClassSymbols(compilation, receiver);

            StringBuilder stringBuilder = new StringBuilder();

            foreach (var classSymbol in classSymbols)
            {
                AttributeData attr;
                bool generated = false;
                if (GetAttribute(classSymbol, generateObjectAttribute!, out attr))
                {
                    GenerateObject(context, classSymbol, attr);
                    generated = true;
                }

                if (GetAttribute(classSymbol, generateResourceAttribute!, out attr))
                {
                    GenerateResource(context, classSymbol, attr);
                    generated = true;
                }

                if (GetAttribute(classSymbol, generateComponentAttribute!, out attr))
                {
                    GenerateComponent(context, classSymbol, attr);
                    generated = true;
                }

                if (generated)
                {                    
                    stringBuilder.AppendLine($"\t\t\t{classSymbol.ToDisplayString()}.__Init();");
                }
            }

            string source1 = $$"""
namespace {{targetNamespace}}
{
    public static partial class ObjectInitializer
    {
        public static void Init()
        {
{{stringBuilder.ToString()}}
        }
    }
}
""";

            context.AddSource($"ObjectInitializer_gen", SourceText.From(source1!, Encoding.UTF8));

        }

        private static void GenerateObject(GeneratorExecutionContext context, INamedTypeSymbol classSymbol, AttributeData attr)
        {
            string namespaceType = classSymbol.ContainingNamespace.ToDisplayString();

            string dataType = "Data";

            var dataTypeName = attr.NamedArguments.FirstOrDefault(e => e.Key.Equals("DataType")).Value.Value;
            if (dataTypeName != null)
            {
                dataType = dataTypeName.ToString();
            }

            string managerType = $"ObjectAllocator<{classSymbol.Name}, Data>";

            var managerTypeName = attr.NamedArguments.FirstOrDefault(e => e.Key.Equals("ManagerType")).Value.Value;
            if (managerTypeName != null)
            {
                managerType = $"{managerTypeName.ToString()}";
            }

            string source = $$"""
namespace {{namespaceType}}
{
    public partial struct {{classSymbol.Name}}
    {        
        private uint id;
        public ObjectID Id { get => new(TypeID, id); set => id = value.ShortID; }
        public static int TypeID { get; } = ObjectID.GetTypeIndex<{{classSymbol.Name}}>();
        public uint Index => (uint)(id & 0xffffff);
        public byte Version => (byte)((id >> 24) & 0xff);
        public bool IsNull => id == 0;
        public bool IsValid => id != 0 && manager.CheckVersion(id);
        internal ref {{dataType}} data => ref manager[id];

        public static bool operator ==(in {{classSymbol.Name}} left, in {{classSymbol.Name}} right)
        {
            return left.id == right.id;
        }

        public static bool operator !=(in {{classSymbol.Name}} left, in {{classSymbol.Name}} right)
        {
            return left.id != right.id;
        }

        public static implicit operator ObjectID(in {{classSymbol.Name}} obj)
        {
            return new ObjectID(TypeID, obj.id);
        }

        public static explicit operator {{classSymbol.Name}}(in ObjectID obj)
        {
            if(obj.TypeID != TypeID)
            {
                return default;
            }

            return new {{classSymbol.Name}}(obj.Id);
        }

        public static explicit operator {{classSymbol.Name}}(uint id)
        {
            return new {{classSymbol.Name}}(id);
        }

        public {{classSymbol.Name}}()
        {
            this.id = manager.Create().ShortID;
        }

        public {{classSymbol.Name}}(uint id)
        {
            this.id = id;
        }

        public void Dispose()
        {
            manager.Free(Id);
        }

        public static {{managerType}} manager;

        public static void __Init()
        {
            manager = new ();
        }
    }
}
""";

            context.AddSource($"{GetFullTypeName(classSymbol)}_gen", SourceText.From(source!, Encoding.UTF8));
        }

        private static void GenerateResource(GeneratorExecutionContext context, INamedTypeSymbol classSymbol, AttributeData attr)
        {
            string namespaceType = classSymbol.ContainingNamespace.ToDisplayString();

            string dataType = "Data";

            var dataTypeName = attr.NamedArguments.FirstOrDefault(e => e.Key.Equals("DataType")).Value.Value;
            if (dataTypeName != null)
            {
                dataType = dataTypeName.ToString();
            }

            string managerType = $"ResourceManager<{classSymbol.Name}, Data>";

            var managerTypeName = attr.NamedArguments.FirstOrDefault(e => e.Key.Equals("ManagerType")).Value.Value;
            if (managerTypeName != null)
            {
                managerType = $"{managerTypeName.ToString()}";
            }

            string source = $$"""
namespace {{namespaceType}}
{
    public partial struct {{classSymbol.Name}}
    {        
        private uint id;
        public ObjectID Id { get => new(TypeID, id); set => id = value.ShortID; }
        public static int TypeID { get; } = ObjectID.GetTypeIndex<{{classSymbol.Name}}>();
        public uint Index => (uint)(id & 0xffffff);
        public byte Version => (byte)((id >> 24) & 0xff);
        public bool IsNull => id == 0;
        public bool IsValid => id != 0 && manager.CheckVersion(id);
        internal ref {{dataType}} data => ref manager[id];

        public static bool operator ==(in {{classSymbol.Name}} left, in {{classSymbol.Name}} right)
        {
            return left.id == right.id;
        }

        public static bool operator !=(in {{classSymbol.Name}} left, in {{classSymbol.Name}} right)
        {
            return left.id != right.id;
        }

        public static implicit operator ObjectID(in {{classSymbol.Name}} obj)
        {
            return new ObjectID(TypeID, obj.id);
        }

        public static explicit operator {{classSymbol.Name}}(in ObjectID obj)
        {
            if(obj.TypeID != TypeID)
            {
                return default;
            }

            return new {{classSymbol.Name}}(obj.Id);
        }

        public static explicit operator {{classSymbol.Name}}(uint id)
        {
            return new {{classSymbol.Name}}(id);
        }

        public {{classSymbol.Name}}()
        {
            this.id = manager.Create().ShortID;
        }

        public {{classSymbol.Name}}(uint id)
        {
            this.id = id;
        }

        public void Dispose()
        {
            manager.Free(Id);
        }

        public static {{managerType}} manager;

        public static void __Init()
        {
            manager = new ();
        }
    }
}
""";

            context.AddSource($"{GetFullTypeName(classSymbol)}_gen", SourceText.From(source!, Encoding.UTF8));
        }

        public static bool HasInterface(ITypeSymbol symbol, string name)
        {
            return symbol.AllInterfaces.Any(i => i.Name == name);
        }

        private static void GenerateComponent(GeneratorExecutionContext context, INamedTypeSymbol classSymbol, AttributeData attr)
        {
            string namespaceType = classSymbol.ContainingNamespace.ToDisplayString();
            
            string dataType = "Data";

            var dataTypeName = attr.NamedArguments.FirstOrDefault(e => e.Key.Equals("DataType")).Value.Value;
            if (dataTypeName != null)
            {
                dataType = dataTypeName.ToString();
            }

            string allocatorType = $"ComponentAllocator<{classSymbol.Name}, {dataType}>";
            string managerType = $"ComponentManager<{classSymbol.Name}>";
            var managerTypeName = attr.NamedArguments.FirstOrDefault(e => e.Key.Equals("ManagerType")).Value.Value;
            if (managerTypeName != null)
            {
                managerType = managerTypeName.ToString();
            }

            int priority = 0;
            var p = attr.NamedArguments.FirstOrDefault(e => e.Key.Equals("Priority")).Value.Value;
            if(p != null)
            {
                p = (int)priority;
            }

            string source = $$"""
namespace {{namespaceType}}
{
    public partial struct {{classSymbol.Name}}
    {        
        private uint id;
        public ObjectID Id { get => new(TypeID, id); set => id = value.ShortID; }
        public static int TypeID { get; } = ObjectID.GetTypeIndex<{{classSymbol.Name}}>();
        public uint Index => (uint)(id & 0xffffff);
        public byte Version => (byte)((id >> 24) & 0xff);
        public bool IsNull => id == 0;
        public bool IsValid => id != 0 && allocator.CheckVersion(id);
        internal ref {{dataType}} data => ref allocator[id];
        public Entity Owner { get => allocator.GetOwner(id); set => allocator.SetOwner(id, value); }

        public static bool operator ==(in {{classSymbol.Name}} left, in {{classSymbol.Name}} right)
        {
            return left.id == right.id;
        }

        public static bool operator !=(in {{classSymbol.Name}} left, in {{classSymbol.Name}} right)
        {
            return left.id != right.id;
        }

        public static implicit operator ObjectID(in {{classSymbol.Name}} obj)
        {
            return new ObjectID(TypeID, obj.id);
        }

        public static explicit operator {{classSymbol.Name}}(in ObjectID obj)
        {
            if(obj.TypeID != TypeID)
            {
                return default;
            }

            return new {{classSymbol.Name}}(obj.Id);
        }

        public static explicit operator {{classSymbol.Name}}(uint id)
        {
            return new {{classSymbol.Name}}(id);
        }

        public {{classSymbol.Name}}()
        {
            id = allocator.Create().ShortID;
        }

        public {{classSymbol.Name}}(uint id)
        {
            this.id = id;
        }

        public void Dispose()
        {
            allocator.Free(Id);
        }

        public static {{allocatorType}} allocator;

        public static void __Init()
        {
            allocator = new(()=> new {{managerType}}() { Priority = {{priority}} });
        }
    }
}
""";

            context.AddSource($"{GetFullTypeName(classSymbol)}_gen", SourceText.From(source!, Encoding.UTF8));
        }

        public static bool GetAttribute(ISymbol symbol, INamedTypeSymbol attributeType, out AttributeData attr)
        {
            var attributes = symbol.GetAttributes()
                .Where(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeType));
            attr = attributes.FirstOrDefault();
            return attr != null;
        }

        public static string GetFullTypeName(ITypeSymbol symbol, string separator = ".")
        {
            string name = symbol.Name;
            ITypeSymbol containingType = symbol.ContainingType;
            while (containingType != null)
            {
                name = containingType.Name + separator + symbol.Name;

                containingType = containingType.ContainingType;

            }

            return name;
        }

        private static IEnumerable<INamedTypeSymbol> GetClassSymbols(Compilation compilation, SyntaxReceiver receiver)
        {
            return receiver.CandidateClasses.Select(clazz => GetClassSymbol(compilation, clazz));
        }

        private static INamedTypeSymbol GetClassSymbol(Compilation compilation, TypeDeclarationSyntax clazz)
        {
            var model = compilation.GetSemanticModel(clazz.SyntaxTree);
            var classSymbol = model.GetDeclaredSymbol(clazz)!;
            return classSymbol;
        }

        private void InitAttributes(Compilation compilation)
        {
            generateObjectAttribute = compilation.GetTypeByMetadataName($"{targetNamespace}.GenerateObjectAttribute")!;
            generateResourceAttribute = compilation.GetTypeByMetadataName($"{targetNamespace}.GenerateResourceAttribute")!;
            generateComponentAttribute = compilation.GetTypeByMetadataName($"{targetNamespace}.GenerateComponentAttribute")!;
        }

        private string GetAccessModifier(INamedTypeSymbol classSymbol)
        {
            return classSymbol.DeclaredAccessibility.ToString().ToLowerInvariant();
        }


    }
}