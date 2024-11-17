using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StructuralAnalyzer
{
    public class MicroserviceGenerator
    {
        private const string MICROSERVICE_NAMESPACE = "microservices";
        private readonly string _outputPath;

        public MicroserviceGenerator(string outputPath)
        {
            _outputPath = outputPath;
        }

        public void GenerateMicroservices(Dictionary<string, string> communities, Dictionary<string, Dictionary<string, Dictionary<string, int>>> callMatrix)
        {
            // Cria um grupo de comunidades usando os dados de callMatrix
            var groupedCommunities = new Dictionary<string, List<string>>();

            // Adiciona comunidades a partir do dicionário communities
            foreach (var community in communities)
            {
                if (!groupedCommunities.ContainsKey(community.Value))
                {
                    groupedCommunities[community.Value] = new List<string>();
                }
                groupedCommunities[community.Value].Add(community.Key);
            }

            // Adiciona comunidades a partir do dicionário callMatrix
            foreach (var classEntry in callMatrix)
            {
                foreach (var methodEntry in classEntry.Value)
                {
                    foreach (var method in methodEntry.Value)
                    {
                        if (!groupedCommunities.ContainsKey(classEntry.Key))
                        {
                            groupedCommunities[classEntry.Key] = new List<string>();
                        }
                        if (!groupedCommunities[classEntry.Key].Contains(method.Key))
                        {
                            groupedCommunities[classEntry.Key].Add(method.Key);
                        }
                    }
                }
            }

            foreach (var community in groupedCommunities)
            {
                var microserviceName = $"Microservice_{community.Key}";
                var microservicePath = Path.Combine(_outputPath, microserviceName);
                Directory.CreateDirectory(microservicePath);

                var interfaceMembers = new List<MemberDeclarationSyntax>();

                foreach (var method in community.Value)
                {
                    Console.WriteLine($"Generating method {method} in microservice {microserviceName}");
                    var className = method.Split('.').First();
                    var methodName = method.Split('.').Last();

                    if (callMatrix.ContainsKey(className) && callMatrix[className].ContainsKey(methodName))
                    {
                        var methodDeclarations = callMatrix[className][methodName].Keys.Select(m => SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), m)
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .WithBody(SyntaxFactory.Block())).ToList();

                        var classDeclaration = SyntaxFactory.ClassDeclaration(className)
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .AddMembers(methodDeclarations.ToArray());

                        var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{MICROSERVICE_NAMESPACE}.{microserviceName}"))
                            .AddMembers(classDeclaration);

                        var compilationUnit = SyntaxFactory.CompilationUnit()
                            .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
                            .AddMembers(namespaceDeclaration)
                            .NormalizeWhitespace();

                        var code = compilationUnit.ToFullString();
                        var filePath = Path.Combine(microservicePath, $"{className}.cs");
                        File.WriteAllText(filePath, code);

                        Console.WriteLine($"Generated file: {filePath}");

                        // Add method to interface
                        var interfaceMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), methodName)
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                        interfaceMembers.Add(interfaceMethod);
                    }
                    else
                    {
                        Console.WriteLine($"Method {methodName} in class {className} not found in callMatrix.");
                    }
                }

                // Generate interface
                var interfaceDeclaration = SyntaxFactory.InterfaceDeclaration($"I{microserviceName}")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddMembers(interfaceMembers.ToArray());

                var interfaceNamespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{MICROSERVICE_NAMESPACE}.{microserviceName}"))
                    .AddMembers(interfaceDeclaration);

                var interfaceCompilationUnit = SyntaxFactory.CompilationUnit()
                    .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
                    .AddMembers(interfaceNamespaceDeclaration)
                    .NormalizeWhitespace();

                var interfaceCode = interfaceCompilationUnit.ToFullString();
                var interfaceFilePath = Path.Combine(microservicePath, $"I{microserviceName}.cs");
                File.WriteAllText(interfaceFilePath, interfaceCode);

                Console.WriteLine($"Generated interface file: {interfaceFilePath}");
            }
        }
    }
}
