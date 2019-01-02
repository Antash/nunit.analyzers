using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests
{
    internal static class TestHelpers
    {
        internal static async Task<(SyntaxNode Node, SemanticModel Model)> GetRootAndModel(string file)
        {
            var code = File.ReadAllText(file);
            var tree = CSharpSyntaxTree.ParseText(code);

            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToString("N"),
                syntaxTrees: new[] { tree },
                references: new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Assert).Assembly.Location)
                });

            var model = compilation.GetSemanticModel(tree);
            var root = await tree.GetRootAsync().ConfigureAwait(false);

            return (root, model);
        }

        internal static async Task RunAnalysisAsync<T>(string path, string[] diagnosticIds,
            Action<ImmutableArray<Diagnostic>> diagnosticInspector = null)
            where T : DiagnosticAnalyzer, new()
        {
            var code = File.ReadAllText(path);
            var diagnostics = await TestHelpers.GetDiagnosticsAsync(code, new T());
            Assert.That(diagnostics.Length, Is.EqualTo(diagnosticIds.Length), nameof(diagnostics.Length));

            foreach (var diagnosticId in diagnosticIds)
            {
                Assert.That(diagnostics.Any(_ => _.Id == diagnosticId), Is.True, diagnosticId);
            }

            diagnosticInspector?.Invoke(diagnostics);
        }

        internal static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(string code, DiagnosticAnalyzer analyzer)
        {
            var document = TestHelpers.Create(code);
            var root = await document.GetSyntaxRootAsync();
            var compilation = (await document.Project.GetCompilationAsync())
                .WithAnalyzers(ImmutableArray.Create(analyzer));
            return (await compilation.GetAnalyzerDiagnosticsAsync()).ToImmutableArray();
        }

        internal static Document Create(string code)
        {
            var name = "Test";
            var projectId = ProjectId.CreateNewId(name);

            var solution = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(projectId, name, name, LanguageNames.CSharp)
                .WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location))
                .AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location))
                .AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location))
                .AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(Assert).Assembly.Location));

            var documentId = DocumentId.CreateNewId(projectId);
            solution = solution.AddDocument(documentId, $"{name}.cs", SourceText.From(code));

            return solution.GetProject(projectId).Documents.First();
        }
    }
}
