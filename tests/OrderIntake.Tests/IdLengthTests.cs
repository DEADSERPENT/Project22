namespace OrderIntake.Tests;

public class IdLengthTests
{
    private static string OrderJsonWithOrderId(string orderId) => $$"""
        {
          "orderId": "{{orderId}}",
          "patientId": "PAT-505",
          "specimenId": "SP-9005",
          "specimenType": "Blood",
          "priority": "Routine",
          "collectionDate": "2026-01-10",
          "requestedTests": ["Glucose"]
        }
        """;

    [Fact]
    public void OrderId_ExactlyTwentyCharacters_IsAccepted()
    {
        var orderId = new string('A', 20);
        var result = new OrderIntakeService().Process(OrderJsonWithOrderId(orderId));

        Assert.Equal(OrderStatus.Accepted, result.Status);
        Assert.Equal(orderId, result.Order!.OrderId);
    }

    [Fact]
    public void OrderId_TwentyOneCharacters_ProducesMaxLength()
    {
        var orderId = new string('A', 21);
        var result = new OrderIntakeService().Process(OrderJsonWithOrderId(orderId));

        Assert.Equal(OrderStatus.Rejected, result.Status);
        Assert.Contains(result.Errors, e => e.Field == "orderId" && e.Code == "MAX_LENGTH");
    }
}
