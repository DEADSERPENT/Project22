namespace OrderIntake.Tests;

public class BrokenJsonTests
{
    [Theory]
    [InlineData("{not valid json")]
    [InlineData("[1, 2, 3]")]
    [InlineData("\"just a string\"")]
    [InlineData("""{"orderId": 123}""")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MalformedInput_IsRejectedWithSingleMalformedInputError_AndDoesNotThrow(string? json)
    {
        // Calling Process directly (no try/catch here) proves no exception escapes:
        // if it threw, this test would fail with an unhandled-exception error, not an assertion failure.
        var result = new OrderIntakeService().Process(json!);

        Assert.Equal(OrderStatus.Rejected, result.Status);
        Assert.Null(result.Order);
        Assert.Single(result.Errors);
        Assert.Equal("$", result.Errors[0].Field);
        Assert.Equal("MALFORMED_INPUT", result.Errors[0].Code);
    }
}
