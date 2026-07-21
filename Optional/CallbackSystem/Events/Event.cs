using System;

public abstract class Event 
{
    public event Action<Event> OnEventCompleted;
    public bool Completed { get; private set; } = false;

    protected void CompleteEvent()
    {
        Completed = true;
        OnEventCompleted?.Invoke(this);
    }

    public virtual void OnPushed() { }
    public virtual void OnCompleted() { }
    public virtual void Tick() { }
}