using System.Collections.Immutable;

namespace TC.Bowling.Domain;

public record Game
{
    public IImmutableList<FrameScore> Frames { get; init; }

    public Game(IImmutableList<FrameScore> frames)
    {
        if (frames.Count > 10)
        {
            throw new ArgumentOutOfRangeException(
                nameof(frames),
                frames,
                "A game cannot have more than 10 frames");
        }

        Frames = frames;
    }

    public GameState State => (Frames.Count, Frames.LastOrDefault()) switch
    {
        (0, _) =>
            new GameState.FirstRoll(Frame.First),

        // new frame
        (var frame and < 9, FrameScore.SecondRoll) =>
            new GameState.FirstRoll(new Frame(frame + 1)),

        // bonus roll (strike or spare in last frame)
        (var frame and 10, FrameScore.SecondRoll {Score: >= 10}) =>
            new GameState.FinalBonusRoll(new Frame(frame)),

        // strike (before the last frame)
        (var frame and < 10, FrameScore.FirstRoll {Score: 10}) =>
            new GameState.FirstRoll(new Frame(frame + 1)),

        // strike in the last frame
        (var frame and 10, FrameScore.FirstRoll {Score: 10}) =>
            new GameState.SecondRoll(new Frame(frame)),

        // second roll in a frame
        (var frame, FrameScore.FirstRoll) =>
            new GameState.SecondRoll(new Frame(frame)),

        // finished
        (10, _)
            => new GameState.Complete(),

        var (unexpectedFrame, unexpectedFrameScore)
            => throw new Exception($"Unexpected frame {unexpectedFrame} and frame score {unexpectedFrameScore}")
    };

    public Game Roll(int roll)
    {
        var rolledPins = new RolledPins(roll);
        FrameScore.FirstRoll NewFrame() => new(rolledPins);

        FrameScore.SecondRoll SecondRoll()
        {
            var currentFrame = Frames.Last();
            if (currentFrame is FrameScore.FirstRoll firstRoll)
            {
                return FrameScore.SecondRoll.FromFirstRoll(firstRoll, rolledPins);
            }

            throw new Exception($"game state mismatch, expected to perform a second roll but found {currentFrame}");
        }

        FrameScore.FinalBonusFrame FinalBonusRoll()
        {
            var currentFrame = Frames.Last();
            if (currentFrame is FrameScore.SecondRoll secondRoll)
            {
                return FrameScore.FinalBonusFrame.FromSecondRoll(secondRoll, rolledPins);
            }

            throw new Exception($"game state mismatch, expected to perform a third roll but found {currentFrame}");
        }

        return State switch
        {
            GameState.Complete =>
                throw new Exception("Cannot roll on a finished game"),

            GameState.FirstRoll =>
                this with {Frames = Frames.Add(NewFrame())},

            GameState.SecondRoll =>
                this with {Frames = Frames.SetItem(Frames.Count - 1, SecondRoll())},

            GameState.FinalBonusRoll =>
                this with {Frames = Frames.SetItem(Frames.Count - 1, FinalBonusRoll())},

            var unexpectedState =>
                throw new Exception($"Unexpected game state {unexpectedState}")
        };
    }

    public int Score
    {
        get
        {
            var result = 0;
            foreach (var (frame, index) in
                     Frames.Select(
                         (
                             frame,
                             index) => (frame, index)))
            {
                result += frame.Score;
                if (frame.Score == 10 && index < 9)
                {
                    // bonus for strike/spare before final frame
                    var rollsToInclude = frame switch
                    {
                        // strike
                        FrameScore.FirstRoll => 2,
                        // spare
                        FrameScore.SecondRoll => 1,

                        _ => throw new Exception($"Unexpected frame score {frame}"),
                    };

                    var nextIndex = index + 1;
                    if (Frames.Count > nextIndex)
                    {
                        var nextFrame = Frames[nextIndex];

                        var bonus = (rollsToInclude, nextFrame) switch
                        {
                            (_, FrameScore.FirstRoll firstRoll) => firstRoll.Score,
                            (1, FrameScore.SecondRoll secondRoll) => secondRoll.First.Pins,
                            (1, FrameScore.FinalBonusFrame bonusFrame) => bonusFrame.First.Pins,
                            (2, FrameScore.SecondRoll secondRoll) => secondRoll.Score,
                            (2, FrameScore.FinalBonusFrame bonusFrame) =>
                                bonusFrame.First.Pins + bonusFrame.Second.Pins,

                            var (unexpectedRollsToInclude, unexpectedFrame) =>
                                throw new Exception(
                                    $"Unexpected number of rolls to include in bonus {unexpectedRollsToInclude} with frame score {unexpectedFrame}")
                        };

                        result += bonus;
                    }
                }
            }

            return result;
        }
    }

    public static Game New() => new(ImmutableList<FrameScore>.Empty);
};