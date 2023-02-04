namespace TC.Bowling.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
        var x = new Scoring(27);
    }

    [Test]
    public void Test1()
    {
        "abcdef".Should().StartWith("ab").And.EndWith("ef");
        Assert.Pass();
    }
}