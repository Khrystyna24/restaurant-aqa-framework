using System.Text.Json;

namespace Restaurant_Aqa_Framework.Support;

/// <summary>A two-column `| label | value |` data table, as business people would write it. Compared against
/// an API response by mapping each business label to the response property it describes
/// (<see cref="PropertyNames"/>), so feature files say "average occupancy" and never "averageOccupancy".</summary>
public sealed class Fields : Dictionary<string, string>
{
    /// <summary>Business label -> response JSON property. A label not listed here is looked up as-is.</summary>
    private static readonly Dictionary<string, string> PropertyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["type"] = "dishType",
        ["status"] = "state",
        ["capacity"] = "totalCapacity",
        ["average occupancy"] = "averageOccupancy",
    };

    public Fields() : base(StringComparer.OrdinalIgnoreCase)
    {
    }

    public static Fields From(DataTable table)
    {
        var fields = new Fields();
        foreach (var row in table.Rows)
            fields[row[0]] = row[1];

        return fields;
    }

    /// <summary>Every label in this table must equal the response property it describes; returns one line
    /// per mismatch, empty when all match.</summary>
    public IReadOnlyList<string> Mismatches(JsonElement json)
    {
        var problems = new List<string>();
        foreach (var (label, expected) in this)
        {
            var name = PropertyNames.GetValueOrDefault(label, label);
            var property = json.EnumerateObject().FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
            if (property.Value.ValueKind == JsonValueKind.Undefined)
            {
                problems.Add($"{label}: missing from the response");
                continue;
            }

            var actual = property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString() : property.Value.GetRawText();
            if (actual != expected)
                problems.Add($"{label}: expected '{expected}' but was '{actual}'");
        }

        return problems;
    }
}

public enum SortDirection
{
    Ascending,
    Descending,
}
