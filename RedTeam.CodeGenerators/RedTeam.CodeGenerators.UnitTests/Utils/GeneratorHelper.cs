using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace RedTeam.CodeGenerators.UnitTests.Utils;

internal class GeneratorHelper
{
    public static (ImmutableArray<Diagnostic> Diagnostics, string Output) GetGeneratedOutput<T>(string source,
        params Type[] assemblyReferences)
        where T : IIncrementalGenerator, new()
    {

        var refs = assemblyReferences.Select(x => x.Assembly.Location).ToList();
        refs.Insert(0, typeof(T).Assembly.Location);

        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Concat(refs.Select(x => MetadataReference.CreateFromFile(x)));

        var compilation = CSharpCompilation.Create(
            "RedTeam.Extensions.Console.CodeGenerators.UnitTests",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var originalTreeCount = compilation.SyntaxTrees.Length;
        var generator = new T();

        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var trees = outputCompilation.SyntaxTrees.ToList();

        return (diagnostics, trees.Count != originalTreeCount ? trees[^1].ToString() : string.Empty);
    }
}
