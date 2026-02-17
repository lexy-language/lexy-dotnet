using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Parser.Symbols;

public class Symbols : ISymbols
{
    private readonly Dictionary<string, DocumentSymbols> symbols = new();
    private readonly Dictionary<INode, IReadOnlyList<VariableEntry>> nodeVariables = new();
    private readonly LexyScriptNode lexyScriptNode;

    public Symbols(LexyScriptNode lexyScriptNode)
    {
        this.lexyScriptNode = Assert.NotNull(lexyScriptNode, nameof(lexyScriptNode));
    }

    public void AddNodeVariables(INode node, IReadOnlyList<VariableEntry> result)
    {
        nodeVariables.TryAdd(node, result);
    }

    public SymbolDescription GetDescription(IFile file, Position position)
    {
        Assert.NotNull(file, nameof(file));

        var document = GetDocumentSymbols(file);
        if (document == null)
        {
            throw new InvalidOperationException($"Couldn't find document: {file}");
        }

        return document.GetDescription(position);
    }

    public Signatures GetSignatures(IFile file, Position position)
    {
        Assert.NotNull(file, nameof(file));

        var document = GetDocumentSymbols(file);
        if (document == null)
        {
            throw new InvalidOperationException($"Couldn't find document: {file}");
        }

        return document.GetSignatures(position);
    }

    public SuggestionsResult GetSuggestions(IFile file, Position position)
    {
        var document = GetDocumentSymbols(file);
        if (document == null)
        {
            throw new InvalidOperationException($"Couldn't find document: {file}");
        }

        var token = document.GetToken(position);
        if (string.IsNullOrWhiteSpace(token?.Value))
        {
            return new SuggestionsResult();
        }

        var nodesInScope = document.GetNodesInScope(position);
        var result = new List<Suggestion>();
//        result.AddRange(AddLocalVariables(document, position));
//        result.AddRange(AddComponentsAndMembers(document, position));
//        result.AddRange(AddLibraryFunctions(document, position));
        AddVariables(nodesInScope, result);
        AddSuggestions(nodesInScope, result, position.AddEndColumn(-1));

        var filter = Filter(result, token);

        return new SuggestionsResult(filter, result, token.Value);
    }

    private static List<Suggestion> Filter(List<Suggestion> result, Token token)
    {
        return token switch
        {
            MemberAccessToken memberAccessLiteralToken => FilterMemberAccess(result, memberAccessLiteralToken.Parts),
            IncompleteMemberAccessToken incompleteMemberAccessToken => FilterMemberAccess(result, incompleteMemberAccessToken.Parts),
            _ => result.Where(value => value.Name.StartsWith(token.Value)).ToList()
        };
    }

    private static string MapDescription(IObjectMember member)
    {
        return member switch
        {
            ObjectFunction => $"function: {member.Type}",
            ObjectVariable => $"variable: {member.Type}",
            ObjectNestedType => $"type: {member.Type}",
            _ => throw new InvalidOperationException("Invalid member: " + member)
        };
    }

    private static List<Suggestion> FilterMemberAccess(IEnumerable<Suggestion> result, string[] parts)
    {
        var members = GetMembers(result, parts);
        if (members == null)
        {
            return new List<Suggestion>();
        }

        return members
            .Select(member => new Suggestion(member.Name, MapDescription(member), SymbolKind.ObjectVariable, member.Type))
            .ToList();
    }

    private static IEnumerable<IObjectMember> GetMembers(IEnumerable<Suggestion> result, string[] parts)
    {
        var path = IdentifierPath.Parse(parts);
        var suggestion = result.FirstOrDefault(value => value.Name == path.RootIdentifier);
        if (suggestion?.Type == null)
        {
            return null;
        }

        var type = suggestion.Type as ObjectType;

        while (path.Path.Length >= 3)
        {
            path = path.ChildrenPath();
            if (path.RootIdentifier == string.Empty) break;

            type = type.MemberType(path.RootIdentifier) as ObjectType;
            if (type == null)
            {
                return null;
            }
        }

        path = path.HasChildIdentifiers ? path.ChildrenPath() : path;

        return path.RootIdentifier == string.Empty
            ? type.Members
            : type.Members.Where(member => member.Name.StartsWith(path.RootIdentifier));
    }

    private IEnumerable<Suggestion> AddLocalVariables(DocumentSymbols document, Position position)
    {
        throw new NotImplementedException();
    }

    private IEnumerable<Suggestion> AddComponentsAndMembers(DocumentSymbols document, Position position)
    {
        throw new NotImplementedException();
    }

    private IEnumerable<Suggestion> AddLibraryFunctions(DocumentSymbols document, Position position)
    {
        throw new NotImplementedException();
    }


    private void AddVariables(IEnumerable<INode> nodesInScope, List<Suggestion> result)
    {
        foreach (var node in nodesInScope)
        {
            if (nodeVariables.TryGetValue(node, out var variables))
            {
                var entries = variables.Select(Map);
                result.AddRange(entries);
            }
        }
    }

    private static Suggestion Map(VariableEntry entry)
    {
        var kind = GetKind(entry);
        var description = entry.ToString();
        return new Suggestion(entry.Name, description, kind, entry.Type);
    }

    private static SymbolKind GetKind(VariableEntry entry)
    {
        var kind = entry.VariableSource switch
        {
            VariableSource.Parameters => SymbolKind.ParameterVariable,
            VariableSource.Results => SymbolKind.ResultVariable,
            VariableSource.Code => SymbolKind.Variable,
            VariableSource.Type => SymbolKind.ObjectVariable,
            _ => throw new ArgumentOutOfRangeException(nameof(entry.VariableSource), "Value: " + entry.VariableSource)
        };
        return kind;
    }

    private static void AddSuggestions(IReadOnlyList<INode> nodes, List<Suggestion> suggestions, Position position)
    {
        for (var index = nodes.Count - 1; index >= 0; index--)
        {
            var node = nodes[index];
            var nodeSuggestions = node.GetSuggestions();
            AddSuggestions(suggestions, nodeSuggestions, node, position);
        }
    }

    private static void AddSuggestions(List<Suggestion> suggestions, SuggestionEdit[] nodeSuggestions, INode node, Position position)
    {
        if (nodeSuggestions == null) return;

        foreach (var suggestion in nodeSuggestions)
        {
            if (suggestion.Scope == SuggestionsScope.Children
             || suggestion.Scope == SuggestionsScope.CurrentLevel && node.Area.Includes(position))
            {
                suggestion.Update(suggestions);
            }
        }
    }

    private DocumentSymbols GetDocumentSymbols(IFile file)
    {
        return symbols.TryGetValue(file.FullPath, out var value) ? value : null;
    }

    public IDocumentSymbols Document(IFile file)
    {
        var fileFullPath = file.FullPath;
        if (symbols.TryGetValue(fileFullPath, out var value))
        {
            return value;
        }

        var documentSymbols = new DocumentSymbols(lexyScriptNode);
        symbols.Add(fileFullPath, documentSymbols);
        return documentSymbols;
    }
}
