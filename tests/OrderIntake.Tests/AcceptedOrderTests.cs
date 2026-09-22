namespace OrderIntake.Tests;

public class AcceptedOrderTests
{
    [Theory]
    [InlineData("blood", "urgent")]
    [InlineData("BLOOD", "URGENT")]
    [InlineData("Blood", "Urgent")]
    public void MixedCaseSpecimenTypeAndPriority_AreAcceptedAndNormalized(string specimenType, string priority)
    {
        var json = $$"""
            {
              "orderId": "ORD-1005",
              "patientId": "PAT-505",
              "specimenId": "SP-9005",
              "specimenType": "{{specimenType}}",
              "priority": "{{priority}}",
              "collectionDate": "2026-01-10",
              "requestedTests": ["Glucose", "CompleteBloodCount"],
              "senderNote": "ignore me"
            }
            """;

        var result = new OrderIntakeService().Process(json);

        Assert.Equal(OrderStatus.Accepted, result.Status);
        Assert.Empty(result.Errors);
        Assert.Equal(SpecimenType.Blood, result.Order!.SpecimenType);
        Assert.Equal(Priority.Urgent, result.Order.Priority);
        Assert.Equal("ORD-1005", result.Order.OrderId);
        Assert.Equal("PAT-505", result.Order.PatientId);
        Assert.Equal("SP-9005", result.Order.SpecimenId);
        Assert.Equal(new DateOnly(2026, 1, 10), result.Order.CollectionDate);
        Assert.Equal(["Glucose", "CompleteBloodCount"], result.Order.RequestedTests);
    }
}
