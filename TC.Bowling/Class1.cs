namespace TC.Bowling;
public class Class1
{

}


public record RolledPins
{
    public int Pins {get; }
    public RolledPins(int pins)
    {
       if (pins < 0) 
       {
            throw new ArgumentOutOfRangeException();
       }
       if (pins > 10)
       {
            throw new ArgumentOutOfRangeException();
       }

       Pins = pins;       
    }
}

public abstract record FrameScore
{
    public record FirstRoll
    {
       public FirstRoll(RolledPins pins) : base(pins); 
    }

    public record SecondRoll
    {
        public SecondRoll(RolledPins first, RolledPins second) : base(pins);
        public SecondRoll(FirstRoll first, RolledPins second) : this(first.)
    }

    public IReadOnlyCollection<RolledPins> RolledPins {get; init; }

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

        RollPins = rolls.ToImmutableList();
    }
}
