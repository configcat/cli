using System;
using System.Collections.Generic;

namespace ConfigCat.Cli.Models;

public class EvalResult
{
    public required object Value { get; init; }

    public required string VariationId { get; init; }

    public required DateTime FetchTime { get; init; }

    public required IReadOnlyDictionary<string, object> User { get; init; }

    public required bool IsDefaultValue { get; init; }

    public required int ErrorCode { get; init; }

    public required string ErrorMessage { get; init; }

    public required bool TargetingMatch { get; init; }
}