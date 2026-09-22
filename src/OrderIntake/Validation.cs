using System.Globalization;

namespace OrderIntake;

internal static class OrderValidator
{
    public static (Order? Order, List<ValidationError> Errors) Validate(RawOrder raw)
    {
        var errors = new List<ValidationError>();

        var orderId = ValidateId(raw.OrderId, "orderId", errors);
        var patientId = ValidateId(raw.PatientId, "patientId", errors);
        var specimenId = ValidateId(raw.SpecimenId, "specimenId", errors);
        var specimenType = ValidateEnum<SpecimenType>(raw.SpecimenType, "specimenType", errors);
        var priority = ValidateEnum<Priority>(raw.Priority, "priority", errors);
        var collectionDate = ValidateDate(raw.CollectionDate, errors);
        var requestedTests = ValidateTests(raw.RequestedTests, errors);

        if (errors.Count > 0)
            return (null, errors);

        return (new Order(orderId!, patientId!, specimenId!, specimenType!.Value, priority!.Value,
            collectionDate!.Value, requestedTests!), errors);
    }

    private static string? ValidateId(string? value, string field, List<ValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(new ValidationError(field, "REQUIRED", $"{field} is required."));
            return null;
        }
        if (value.Length > 20)
        {
            errors.Add(new ValidationError(field, "MAX_LENGTH",
                $"{field} must be at most 20 characters (was {value.Length})."));
            return null;
        }
        return value;
    }

    private static TEnum? ValidateEnum<TEnum>(string? value, string field, List<ValidationError> errors)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(new ValidationError(field, "REQUIRED", $"{field} is required."));
            return null;
        }
        if (!Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed) || int.TryParse(value, out _))
        {
            var allowed = string.Join(", ", Enum.GetNames<TEnum>());
            errors.Add(new ValidationError(field, "INVALID_VALUE", $"{field} must be one of: {allowed}."));
            return null;
        }
        return parsed;
    }

    private static DateOnly? ValidateDate(string? value, List<ValidationError> errors)
    {
        const string field = "collectionDate";
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(new ValidationError(field, "REQUIRED", $"{field} is required."));
            return null;
        }
        if (!DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            errors.Add(new ValidationError(field, "INVALID_FORMAT", $"{field} must be a real date in yyyy-MM-dd format."));
            return null;
        }
        if (date > DateOnly.FromDateTime(DateTime.Today))
        {
            errors.Add(new ValidationError(field, "FUTURE_DATE", $"{field} must not be in the future."));
            return null;
        }
        return date;
    }

    private static List<string>? ValidateTests(List<string>? tests, List<ValidationError> errors)
    {
        const string field = "requestedTests";
        if (tests is null || tests.Count == 0)
        {
            errors.Add(new ValidationError(field, "REQUIRED", $"{field} must contain at least one test."));
            return null;
        }
        if (tests.Any(string.IsNullOrWhiteSpace))
        {
            errors.Add(new ValidationError(field, "INVALID_VALUE", $"{field} must not contain empty test names."));
            return null;
        }
        var hasDuplicate = tests
            .GroupBy(t => t, StringComparer.OrdinalIgnoreCase)
            .Any(g => g.Count() > 1);
        if (hasDuplicate)
        {
            errors.Add(new ValidationError(field, "DUPLICATE", $"{field} must not contain duplicate test names."));
            return null;
        }
        return tests;
    }
}
