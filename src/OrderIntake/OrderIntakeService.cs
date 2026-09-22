using System.Text.Json;

namespace OrderIntake;

public sealed class OrderIntakeService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    // Kept as an instance method (not static): the assessment specifies a service
    // with a public processing method, and no static state or DI is required here.
#pragma warning disable CA1822
    public OrderResult Process(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return OrderResult.Malformed("Input must be a non-empty JSON object.");

        RawOrder? raw;
        try
        {
            raw = JsonSerializer.Deserialize<RawOrder>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return OrderResult.Malformed("Input is not valid JSON or does not match the expected order shape.");
        }

        if (raw is null)
            return OrderResult.Malformed("Input must be a JSON object, not a JSON null.");

        var (order, errors) = OrderValidator.Validate(raw);
        return order is not null ? OrderResult.Accepted(order) : OrderResult.Rejected(errors);
    }
#pragma warning restore CA1822
}
