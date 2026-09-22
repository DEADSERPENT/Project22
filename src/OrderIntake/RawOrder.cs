namespace OrderIntake;

internal sealed class RawOrder
{
    public string? OrderId { get; set; }
    public string? PatientId { get; set; }
    public string? SpecimenId { get; set; }
    public string? SpecimenType { get; set; }
    public string? Priority { get; set; }
    public string? CollectionDate { get; set; }
    public List<string>? RequestedTests { get; set; }
}
