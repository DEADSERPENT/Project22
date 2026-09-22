namespace OrderIntake.Tests;

public class AllErrorsAtOnceTests
{
    [Fact]
    public void BrokenOrder_ReportsExactCompleteErrorSet()
    {
        const string json = """
            {
              "orderId": "",
              "patientId": "PAT-505",
              "specimenId": "SP-9005",
              "specimenType": "plasma",
              "priority": "asap",
              "collectionDate": "2026-02-30",
              "requestedTests": []
            }
            """;

        var result = new OrderIntakeService().Process(json);

        Assert.Equal(OrderStatus.Rejected, result.Status);
        Assert.Null(result.Order);

        var expected = new HashSet<(string Field, string Code)>
        {
            ("orderId", "REQUIRED"),
            ("specimenType", "INVALID_VALUE"),
            ("priority", "INVALID_VALUE"),
            ("collectionDate", "INVALID_FORMAT"),
            ("requestedTests", "REQUIRED"),
        };
        var actual = result.Errors.Select(e => (e.Field, e.Code)).ToHashSet();

        Assert.Equal(expected, actual);
    }
}
