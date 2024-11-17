using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace StructuralAnalyzer
{
    public class StructuralVisitor : CSharpSyntaxWalker
    {
        private readonly Dictionary<string, Dictionary<string, Dictionary<string, int>>> _callMatrix;
        private string _currentClass;
        private string _currentMethod;

        public StructuralVisitor()
        {
            _callMatrix = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
        }

        public Dictionary<string, Dictionary<string, Dictionary<string, int>>> GetCallMatrix()
        {
            return _callMatrix;
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            _currentClass = node.Identifier.Text;
            if (_currentClass != null && !_callMatrix.ContainsKey(_currentClass))
            {
                _callMatrix[_currentClass] = new Dictionary<string, Dictionary<string, int>>();
            }
            base.VisitClassDeclaration(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            _currentMethod = node.Identifier.Text;
            if (_currentClass != null && _currentMethod != null && _callMatrix.ContainsKey(_currentClass) && !_callMatrix[_currentClass].ContainsKey(_currentMethod))
            {
                _callMatrix[_currentClass][_currentMethod] = new Dictionary<string, int>();
            }
            base.VisitMethodDeclaration(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var methodName = node.Expression.ToString();
            if (_currentClass != null && _currentMethod != null)
            {
                if (_currentClass != null && _currentMethod != null && _callMatrix.ContainsKey(_currentClass) && _callMatrix[_currentClass].ContainsKey(_currentMethod) && !_callMatrix[_currentClass][_currentMethod].ContainsKey(methodName))
                {
                    _callMatrix[_currentClass][_currentMethod][methodName] = 1;
                }
            }
            base.VisitInvocationExpression(node);
        }
    }
}
