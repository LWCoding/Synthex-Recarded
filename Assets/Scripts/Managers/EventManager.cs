using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager
{

    public static List<GameEvent> RegisteredEvents = new();

    /// <summary>
    /// Get a registered event. If it doesn't yet exist, initializes it through 
    /// `Globals.GetGameEvent()`. Assumes the event is findable via ScriptableObject.
    /// </summary>
    /// <param name="eventType">The event type to search for.</param>
    /// <returns>The GameEvent corresponding to the inputted event type.</returns>
    public static GameEvent GetRegisteredEvent(EventType eventType)
    {
        GameEvent foundEvent = RegisteredEvents.Find((e) => e.EventType == eventType);
        // Initialize the event if we can't find this event.
        if (foundEvent == null)
        {
            foundEvent = Globals.GetGameEvent(eventType);
            RegisteredEvents.Add(foundEvent);
        }
        return foundEvent;
    }

    /// <summary>
    /// Finds if the specified event has been completed.
    /// </summary>
    /// <param name="eventType">The event type to search for.</param>
    /// <returns>True if event has been completed, otherwise false.</returns>
    public static bool IsEventComplete(EventType eventType)
    {
        GameEvent foundEvent = GetRegisteredEvent(eventType);
        return foundEvent != null && foundEvent.IsCompleted();
    }

    /// <summary>
    /// Increment the counter for the specified event.
    /// An event is considered complete when its limit has been reached.
    /// </summary>
    /// <param name="eventType">The event type to search for.</param>
    public static void IncrementEventCounter(EventType eventType)
    {
        GameEvent foundEvent = GetRegisteredEvent(eventType);
        // Increment the event counter.
        foundEvent.Increment();
    }
    
    /// <summary>
    /// Automatically completes the counter for the specified event.
    /// An event is considered complete when its limit has been reached.
    /// </summary>
    /// <param name="eventType">The event type to search for.</param>
    public static void CompleteEvent(EventType eventType)
    {
        GameEvent foundEvent = GetRegisteredEvent(eventType);
        // Set the event to be completed.
        foundEvent.SetCompleted();
    }

}
