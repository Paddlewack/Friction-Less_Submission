public interface ITriggerable
{
    public int TriggeredCount { get; }
    void Trigger();
}