using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;

namespace StructuralAnalyzer
{
    public class Parser
    {
        public SyntaxTree Parse(string filePath)
        {
            var code = File.ReadAllText(filePath);
            return CSharpSyntaxTree.ParseText(code);
        }
    }
}