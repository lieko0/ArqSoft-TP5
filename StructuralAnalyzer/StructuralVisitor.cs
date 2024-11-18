using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace StructuralAnalyzer
{
    public class StructuralVisitor : CSharpSyntaxWalker
    {
        private readonly Dictionary<string, Dictionary<string, Dictionary<string, int>>> _callMatrix;
        private string _currentClass;
        private string _currentMethod;
        private SemanticModel _semanticModel;
        public StructuralVisitor()
        {
            _callMatrix = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();

        }

        public Dictionary<string, Dictionary<string, Dictionary<string, int>>> GetCallMatrix()
        {
            return _callMatrix;
        }

        public void Visits(SyntaxTree tree, SyntaxNode node)
        {
            var compilation = CSharpCompilation.Create("Analysis")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(tree);

            _semanticModel = compilation.GetSemanticModel(tree);
            base.Visit(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            _currentClass = node.Identifier.Text;
            base.VisitClassDeclaration(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            _currentMethod = node.Identifier.Text;
            base.VisitMethodDeclaration(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var methodName = node.Expression.ToString();
            var invokedClass = GetInvokedClass(node.Expression);

            //Console.WriteLine($"{methodName} - {invokedClass} ");

            if (_currentClass != null && _currentMethod != null && invokedClass != null)
            {
                if (!_callMatrix.ContainsKey(_currentClass))
                {
                    _callMatrix[_currentClass] = new Dictionary<string, Dictionary<string, int>>();
                }
                if (!_callMatrix[_currentClass].ContainsKey(invokedClass.ToString()))
                {
                    _callMatrix[_currentClass][invokedClass.ToString()] = new Dictionary<string, int>();
                }
                if (!_callMatrix[_currentClass][invokedClass.ToString()].ContainsKey(methodName))
                {
                    _callMatrix[_currentClass][invokedClass.ToString()][methodName] = 0;
                }
                _callMatrix[_currentClass][invokedClass.ToString()][methodName]++;
            }

            base.VisitInvocationExpression(node);
        }

        private string GetInvokedClass(ExpressionSyntax expression)
        {
            if (expression is MemberAccessExpressionSyntax memberAccess)
            {
                //Console.WriteLine($"A: {expression.GetFirstToken().ToString()} - {expression.Kind().ToString()}");
                var symbolInfo = _semanticModel.GetSymbolInfo(memberAccess.Expression);
                if (symbolInfo.Symbol is ILocalSymbol localSymbol)
                {
                    return localSymbol.Type.Name;
                }
                else if (symbolInfo.Symbol is IFieldSymbol fieldSymbol)
                {
                    return fieldSymbol.Type.Name;
                }
                else if (symbolInfo.Symbol is IPropertySymbol propertySymbol)
                {
                    return propertySymbol.Type.Name;
                }
                else if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
                {
                    return methodSymbol.ContainingType.Name;
                }
                else if (symbolInfo.Symbol is INamedTypeSymbol namedTypeSymbol)
                {
                    return namedTypeSymbol.Name;
                }
                else if (symbolInfo.Symbol is INamespaceSymbol namespaceSymbol)
                {
                    return namespaceSymbol.Name;
                }
                else if (symbolInfo.Symbol is IEventSymbol eventSymbol)
                {
                    return eventSymbol.Type.Name;
                }
                else if (symbolInfo.Symbol is ITypeSymbol typeSymbol)
                {
                    return typeSymbol.Name;
                }
                
            }
            //Console.WriteLine($"R: {expression.GetFirstToken().ToString()} - {expression.Kind().ToString()}");
            return expression.GetFirstToken().ToString();
        }
    }
}
