using System;
using System.Collections.Generic;

public static class EventManager
{
    private static readonly Dictionary<Type, Event> currentEvents = new();

    private static readonly Dictionary<Type, List<EventSubscriber>> subscribers = new();

    public static void Tick()
    {
        foreach (var eventPair in currentEvents)
            eventPair.Value.Tick();
    }

    public static void PushEvent<TEvent>(TEvent eventToPush)
        where TEvent : Event
    {
        if(currentEvents.ContainsKey(typeof(TEvent)))
            return;
        
        currentEvents.Add(typeof(TEvent), eventToPush);
        eventToPush.OnEventCompleted += OnEventCompleted;
        
        eventToPush.OnPushed();
        InvokeSubscribers<TEvent>();
    }

    public static EventSubscriber SubscribeOnEvent<TEvent>(Action<TEvent> action)
        where TEvent : Event
    {
        var eventType = typeof(TEvent);
        var subscriber = new TypedEventSubscriber<TEvent>(action);

        if (subscribers.TryAdd(eventType, new List<EventSubscriber>() { subscriber }))
            return subscriber;

        subscribers[eventType].Add(subscriber);

        return subscriber;
    }
    
    public static void UnsubscribeOnEvent(EventSubscriber eventSubscriberAa)
    {
        subscribers.Remove(eventSubscriberAa.SubscriptionType);
    }

    private static void InvokeSubscribers<TEvent>()
        where TEvent : Event
    {
        if (!subscribers.ContainsKey(typeof(TEvent))) 
            return;

        var subscriberEvent = currentEvents[typeof(TEvent)];

        foreach (var subscriber in subscribers[typeof(TEvent)])
        {
            subscriber.InvokeSubscription(subscriberEvent);
        }
    }

    private static void OnEventCompleted(Event completedEvent)
    {
        currentEvents.Remove(completedEvent.GetType());
        
        completedEvent.OnEventCompleted -= OnEventCompleted;
        completedEvent.OnCompleted();
    }
}