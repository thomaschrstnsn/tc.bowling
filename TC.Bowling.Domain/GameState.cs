namespace TC.Bowling.Domain;

public abstract record GameState
{
    public sealed record FirstRoll(Frame Frame) : GameState;

    public sealed record SecondRoll(Frame Frame) : GameState;

    public sealed record FinalBonusRoll(Frame Frame) : GameState;

    public sealed record Complete : GameState;

    private GameState() { }
}