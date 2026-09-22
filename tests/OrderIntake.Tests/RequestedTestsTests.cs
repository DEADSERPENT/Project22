namespace OrderIntake.Tests;

public class RequestedTestsTests
{
    private static string OrderJsonWithTests(string testsJsonArray) => $$"""
        {
          "orderId": "ORD-1005",
          "patientId": "PAT-505",
          "specimenId": "SP-9005",
          "specimenType": "Blood",
          "priority": "Routine",
          "collectionDate": "2026-01-10",
          "requestedTests": {{testsJsonArray}}
        }
        """;

    [Fact]
    public void EmptyList_IsRejectedWithRequired()
    {
        var result = new OrderIntakeService().Process(OrderJsonWithTests("[]"));

        Assert.Equal(OrderStatus.Rejected, result.Status);
        Assert.Contains(result.Errors, e => e.Field == "requestedTests" && e.Code == "REQUIRED");
    }

    [Fact]
    public void NamesDifferingOnlyByCase_AreRejectedAsDuplicate()
    {
        var result = new OrderIntakeService().Process(OrderJsonWithTests("""["Glucose", "GLUCOSE"]"""));

        Assert.Equal(OrderStatus.Rejected, result.Status);
        var duplicateErrors = result.Errors.Where(e => e.Field == "requestedTests" && e.Code == "DUPLICATE").ToList();
        Assert.Single(duplicateErrors);
    }
}
