namespace OrderIntake;

public enum SpecimenType { Blood, Urine, Tissue, Saliva }

public enum Priority { Routine, Urgent }

public enum OrderStatus { Accepted, Rejected }

public sealed record ValidationError(string Field, string Code, string Message);

public sealed record Order(
    string OrderId,
    string PatientId,
    string SpecimenId,
    SpecimenType SpecimenType,
    Priority Priority,
    DateOnly CollectionDate,
    IReadOnlyList<string> RequestedTests);

public sealed record OrderResult(OrderStatus Status, Order? Order, IReadOnlyList<ValidationError> Errors)
{
    public static OrderResult Accepted(Order order) => new(OrderStatus.Accepted, order, []);

    public static OrderResult Rejected(IReadOnlyList<ValidationError> errors) => new(OrderStatus.Rejected, null, errors);

    public static OrderResult Malformed(string message) =>
        Rejected([new ValidationError("$", "MALFORMED_INPUT", message)]);
}
