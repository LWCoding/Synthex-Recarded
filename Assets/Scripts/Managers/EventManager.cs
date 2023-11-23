using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager
{

    // Event data:
    public static List<GameEvent> registeredEvents = new();
    public static bool IsEventComplete(EventType eventType)
    {
        GameEvent foundEvent = registeredEvents.Find((e) => e.EventType == eventType);
        return foundEvent != null && foundEvent.IsCompleted();
    }
    public static void SetRegisteredEvents(List<GameEvent> events) => registeredEvents = events;
    public static void IncrementEventCounter(EventType eventType)
    {
        GameEvent foundEvent = registeredEvents.Find((e) => e.EventType == eventType);
        // Initialize the event if we can't find this event.
        if (foundEvent == null)
        {
            foundEvent = Globals.GetGameEvent(eventType);
            registeredEvents.Add(foundEvent);
        }
        // Increment the event counter.
        foundEvent.Increment();
    }
    public static void CompleteEvent(EventType eventType)
    {
        GameEvent foundEvent = registeredEvents.Find((e) => e.EventType == eventType);
        // Initialize the event if we can't find this event.
        if (foundEvent == null)
        {
            foundEvent = Globals.GetGameEvent(eventType);
            registeredEvents.Add(foundEvent);
        }
        // Set the event to be completed.
        foundEvent.SetCompleted();
    }

}
