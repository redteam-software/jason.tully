using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RedTeam.Extensions.CodeGenerators;
internal class NamedAttributeParameterValueExtractor
{
    /// <summary>
    /// Extracts key/value pairs from an attribute's list of named arguments.
    /// </summary>
    /// <param name="attribute"></param>
    /// <param name="argumentNames"></param>
    /// <returns></returns>
    public static ImmutableDictionary<string, string> Extract(AttributeData attribute, params string[] argumentNames)
    {
        var dictionary = new Dictionary<string, string>();
        var argumentNamesAsHashSet = argumentNames.ToImmutableHashSet();

        foreach (var parameter in attribute.NamedArguments)
        {
            // match name with service registration configuration
            var name = parameter.Key;
            var value = parameter.Value.Value;

            if (string.IsNullOrEmpty(name) || value == null)
                continue;

            if (argumentNamesAsHashSet.Contains(name))
            {

                dictionary.Add(name, parameter.Value.ToCSharpString());
            }

        }

        return dictionary.ToImmutableDictionary();

    }
}
