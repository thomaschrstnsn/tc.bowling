using System.Collections.Immutable;

namespace TC.Bowling.Domain;

public record RolledPins
{
    public int Pins {get; init; }
    public RolledPins(int pins)
    {
        Pins = pins switch
        {
            < 0 => throw new ArgumentOutOfRangeException(nameof(pins), pins, "Cannot roll less than 0"),
            > 10 => throw new ArgumentOutOfRangeException(nameof(pins), pins, "Cannot roll more than 10"),
            _ => pins
        };
    }
}

public abstract record FrameScore
{
    public sealed record FirstRoll(RolledPins Pins) : FrameScore(Pins);

    public sealed record SecondRoll : FrameScore
    {
        public RolledPins First { get; init; }
        public RolledPins Second { get; init; }

        public SecondRoll(
            RolledPins first,
            RolledPins second) : base(first, second)
        {
            if (first.Pins + second.Pins > 10)
            {
                throw new ArgumentOutOfRangeException("todo");
            }
            
            First = first;
            Second = second;
        }

        public static SecondRoll FromFirstRoll(
            FirstRoll first,
            RolledPins second)
            => new(first.Pins, second);

    }

    public sealed record FinalBonusFrame : FrameScore
    {
        public RolledPins First { get; init; }
        public RolledPins Second { get; init; }
        public RolledPins Third { get; init; }

        public FinalBonusFrame(
            RolledPins first,
            RolledPins second,
            RolledPins third) : base( first, second, third)
        {
            if (first.Pins != 10 && first.Pins + second.Pins != 10)
            {
                throw new ArgumentException("not a valid final bonus frame");
            }

            First = first;
            Second = second;
            Third = third;
        }

        public static FinalBonusFrame FromSecondRoll(
            SecondRoll secondRoll,
            RolledPins third) =>
            new(
                secondRoll.First,
                secondRoll.Second,
                third);
    }

    public IReadOnlyCollection<RolledPins> RolledPins {get; init; }

    public int Score => RolledPins.Select(rp => rp.Pins).Sum();

    private FrameScore(params RolledPins[] rolls)
    {
        if (rolls.Length < 1) 
        {
            throw new ArgumentException("too few rolls");
        }
        if (rolls.Length > 3) 
        {
            throw new ArgumentException("too many rolls");
        }

        RolledPins = rolls.ToImmutableList();
    }
}
