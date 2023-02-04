namespace TC.Bowling.Domain;

public sealed record Frame
{
    public int Number { get; }

    public Frame(int number)
    {
        Number = number switch
        {
            < 1 => throw new ArgumentOutOfRangeException(
                nameof(number),
                number,
                "Frames start at 1"),
            > 10 => throw new ArgumentOutOfRangeException(
                nameof(number),
                number,
                "Frames end at 10"),
            _ => number
        };
    }

    public static Frame First => new(1);
}