namespace OrderIntake.Tests;

public class CollectionDateTests
{
    private static string OrderJsonWithDate(string date) => $$"""
        {
          "orderId": "ORD-1005",
          "patientId": "PAT-505",
          "specimenId": "SP-9005",
          "specimenType": "Blood",
          "priority": "Routine",
          "collectionDate": "{{date}}",
          "requestedTests": ["Glucose"]
        }
        """;

    [Theory]
    [InlineData("2026-02-30")]  // not a real calendar date
    [InlineData("2026-9-2")]    // not zero-padded
    [InlineData("20-09-2026")]  // wrong field order
    [InlineData("2026/09/20")]  // wrong separator
    public void InvalidDates_ProduceInvalidFormat(string date)
    {
        var result = new OrderIntakeService().Process(OrderJsonWithDate(date));

        Assert.Equal(OrderStatus.Rejected, result.Status);
        Assert.Contains(result.Errors, e => e.Field == "collectionDate" && e.Code == "INVALID_FORMAT");
    }

    [Fact]
    public void FutureDate_ProducesFutureDate()
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.Today).AddDays(1).ToString("yyyy-MM-dd");
        var result = new OrderIntakeService().Process(OrderJsonWithDate(tomorrow));

        Assert.Equal(OrderStatus.Rejected, result.Status);
        Assert.Contains(result.Errors, e => e.Field == "collectionDate" && e.Code == "FUTURE_DATE");
    }

    [Fact]
    public void TodayIsAccepted()
    {
        var today = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");
        var result = new OrderIntakeService().Process(OrderJsonWithDate(today));

        Assert.Equal(OrderStatus.Accepted, result.Status);
    }
}
