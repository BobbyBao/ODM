using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
/// <summary>
/// Created on demand before each generation pass
/// </summary>
namespace ODM.Generator
{
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public IList<TypeDeclarationSyntax> CandidateClasses { get; } = new List<TypeDeclarationSyntax>();
        public IList<DelegateDeclarationSyntax> CandidateDelegates { get; } = new List<DelegateDeclarationSyntax>();

        /// <summary>
        /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
        /// </summary>
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // any field with at least one attribute is a candidate for being cloneable
            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax &&
                classDeclarationSyntax.AttributeLists.Count > 0)
            {
                CandidateClasses.Add(classDeclarationSyntax);
            }
            if (syntaxNode is StructDeclarationSyntax structDeclarationSyntax &&
                structDeclarationSyntax.AttributeLists.Count > 0)
            {
                CandidateClasses.Add(structDeclarationSyntax);
            }

            if (syntaxNode is InterfaceDeclarationSyntax interfaceDeclarationSyntax &&
                interfaceDeclarationSyntax.AttributeLists.Count > 0)
            {
                CandidateClasses.Add(interfaceDeclarationSyntax);
            }

            if (syntaxNode is DelegateDeclarationSyntax delegateDeclarationSyntax)
            {
                CandidateDelegates.Add(delegateDeclarationSyntax);
            }
        }
    }
}