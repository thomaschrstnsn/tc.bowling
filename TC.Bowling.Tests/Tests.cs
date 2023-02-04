namespace TC.Bowling.Tests;

public class Tests
{
    [Test]
    public void Example()
    {
        // First frame
        var game = Game.New().Roll(1).Roll(4);
        game.Score.Should().Be(5);
        game.State.Should().BeOfType<GameState.FirstRoll>().Which.Frame.Number.Should().Be(2);

        // Second frame
        game = game.Roll(4);
        game.Score.Should().Be(9);
        game.State.Should().BeOfType<GameState.SecondRoll>().Which.Frame.Number.Should().Be(2);

        game = game.Roll(5);
        game.Score.Should().Be(14);

        // Third frame
        game = game.Roll(6).Roll(4);
        game.Score.Should().Be(24);

        // Fourth frame
        game = game.Roll(5);
        game.Score.Should().Be(34); // +5 in spare bonus for last frame
        game = game.Roll(5);
        game.Score.Should().Be(39);

        // Fifth frame
        game = game.Roll(10);

        // Sixth frame
        game = game.Roll(0).Roll(1);
        game.Score.Should().Be(61);

        // Seventh frame
        game = game.Roll(7).Roll(3);

        // Eighth frame
        game = game.Roll(6).Roll(4);

        // Ninth frame
        game = game.Roll(10);

        // Final frame
        game = game.Roll(2).Roll(8);
        game.State.Should().BeOfType<GameState.FinalBonusRoll>().Which.Frame.Number.Should().Be(10);

        game = game.Roll(6);
        game.Score.Should().Be(133);
        game.State.Should().BeOfType<GameState.Complete>();
    }
}