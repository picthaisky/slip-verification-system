namespace SlipVerification.UnitTests;

/// <summary>
/// Basic test to ensure test infrastructure is working
/// </summary>
public class UnitTest1
{
    [Fact]
    public void SampleTest_ShouldPass()
    {
        // Arrange
        var expected = 1;

        // Act
        var actual = 1;

        // Assert
        Assert.Equal(expected, actual);
    }
}
