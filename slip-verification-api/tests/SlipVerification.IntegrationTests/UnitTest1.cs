namespace SlipVerification.IntegrationTests;

/// <summary>
/// Basic integration test to ensure test infrastructure is working
/// </summary>
public class UnitTest1
{
    [Fact]
    public void SampleIntegrationTest_ShouldPass()
    {
        // Arrange
        var expected = "Integration";

        // Act
        var actual = "Integration";

        // Assert
        Assert.Equal(expected, actual);
    }
}
