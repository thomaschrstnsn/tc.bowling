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
        (var frame and <= 9, FrameScore.SecondRoll) =>
            new GameState.FirstRoll(new Frame(frame + 1)),

        // strike in the last frame
        (var frame and 10, FrameScore.FirstRoll {Score: 10}) =>
            new GameState.SecondRoll(new Frame(frame)),

        // bonus roll (strike or spare in last frame)
        (var frame and 10, FrameScore.FinalFrameTwo {Score: >= 10}) =>
            new GameState.FinalBonusRoll(new Frame(frame)),

        // strike (before the last frame)
        (var frame and < 10, FrameScore.FirstRoll {Score: 10}) =>
            new GameState.FirstRoll(new Frame(frame + 1)),

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

        FrameScore.FinalFrameTwo FinalFrameRollTwo()
        {
            var currentFrame = Frames.Last();
            if (currentFrame is FrameScore.FirstRoll firstRoll)
            {
                return FrameScore.FinalFrameTwo.FromFirstRoll(firstRoll, rolledPins);
            }

            throw new Exception($"game state mismatch, expected to perform a second roll but found {currentFrame}");
        }

        FrameScore.FinalFrameThree FinalFrameRollThree()
        {
            var currentFrame = Frames.Last();
            if (currentFrame is FrameScore.FinalFrameTwo secondRoll)
            {
                return FrameScore.FinalFrameThree.FromFinalFrameTwo(secondRoll, rolledPins);
            }

            throw new Exception($"game state mismatch, expected to perform a third roll but found {currentFrame}");
        }

        var currentIndex = Frames.Count - 1;
        return State switch
        {
            GameState.Complete =>
                throw new Exception("Cannot roll on a finished game"),

            GameState.FirstRoll =>
                this with {Frames = Frames.Add(NewFrame())},

            GameState.SecondRoll {Frame.Number: 10} =>
                this with {Frames = Frames.SetItem(currentIndex, FinalFrameRollTwo())},

            GameState.SecondRoll =>
                this with {Frames = Frames.SetItem(currentIndex, SecondRoll())},

            GameState.FinalBonusRoll =>
                this with {Frames = Frames.SetItem(currentIndex, FinalFrameRollThree())},

            var unexpectedState =>
                throw new Exception($"Unexpected game state {unexpectedState}")
        };
    }

    public int Score()
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

                var bonus = BonusScoreFromNextRolls(nextFrameIndex: index + 1, rollsToInclude: rollsToInclude);
                result += bonus;
            }
        }

        return result;
    }

    private int BonusScoreFromNextRolls(
        int nextFrameIndex,
        int rollsToInclude)
    {
        if (Frames.Count <= nextFrameIndex)
        {
            return 0;
        }
        
        var currentBonusFrame = Frames[nextFrameIndex];
        var bonusFromThisFrame = currentBonusFrame.RolledPins.Take(rollsToInclude).Select(roll => roll.Pins).Sum();

        if (currentBonusFrame.RolledPins.Count < rollsToInclude)
        {
            // we did not get enough rolls, try to proceed to next frame (if present)
            return bonusFromThisFrame + BonusScoreFromNextRolls(
                nextFrameIndex: nextFrameIndex + 1,
                rollsToInclude: rollsToInclude - currentBonusFrame.RolledPins.Count);
        }

        return bonusFromThisFrame;
    }

    public static Game New() => new(ImmutableList<FrameScore>.Empty);
};