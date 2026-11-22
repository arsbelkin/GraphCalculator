using System.Collections.Generic;
using System.Linq;
using GraphCalculatorAvalonia.ViewModels;

namespace GraphCalculatorAvalonia.Services;

public sealed record FunctionsSnapshot(string Json, bool HasContent);


public sealed class FunctionsDocumentState
{
    private FunctionsSnapshot? savedSnapshot;

    public static FunctionsSnapshot Capture(IEnumerable<FunctionViewModel> functions)
    {
        var rows = functions.ToArray();
        return new FunctionsSnapshot(FunctionsFileService.Serialize(rows),
            rows.Any(function => !string.IsNullOrWhiteSpace(function.Expression)));
    }

    public bool IsModified(FunctionsSnapshot current) => savedSnapshot is null
        ? current.HasContent
        : current.Json != savedSnapshot.Json;

    public bool RequiresSavePrompt(FunctionsSnapshot current) =>
        IsModified(current) && (current.HasContent || savedSnapshot?.HasContent == true);

    public void MarkSaved(FunctionsSnapshot snapshot) => savedSnapshot = snapshot;
}
