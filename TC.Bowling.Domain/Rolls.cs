using System.Collections.Immutable;

namespace TC.Bowling.Domain;

public record RolledPins
{
    public int Pins {get; }
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
        public RolledPins First { get; }
        public RolledPins Second { get; }

        private SecondRoll(
            RolledPins first,
            RolledPins second) : base(first, second)
        {
            if (first.Pins + second.Pins > 10)
            {
                throw new ArgumentException("Sum of pins in a frame cannot exceed 10");
            }
            
            First = first;
            Second = second;
        }

        public static SecondRoll FromFirstRoll(
            FirstRoll first,
            RolledPins second)
            => new(first.Pins, second);
    }

    public sealed record FinalFrameTwo(
        RolledPins First,
        RolledPins Second) : FrameScore(First, Second)
    {
        public static FinalFrameTwo FromFirstRoll(
            FirstRoll firstRoll,
            RolledPins second) =>
            new(firstRoll.Pins, second);
    }

    public sealed record FinalFrameThree : FrameScore
    {
        public RolledPins First { get; }
        public RolledPins Second { get; }
        public RolledPins Third { get; }

        private FinalFrameThree(
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

        public static FinalFrameThree FromFinalFrameTwo(
            FinalFrameTwo secondRoll,
            RolledPins third) =>
            new(
                secondRoll.First,
                secondRoll.Second,
                third);
    }

    public IImmutableList<RolledPins> RolledPins {get; }

    public int Score => RolledPins.Select(rp => rp.Pins).Sum();

    private FrameScore(params RolledPins[] rolls)
    {
        RolledPins = rolls.Length switch
        {
            < 1 => throw new ArgumentException("too few rolls"),
            > 3 => throw new ArgumentException("too many rolls"),
            _ => rolls.ToImmutableList()
        };
    }
}
