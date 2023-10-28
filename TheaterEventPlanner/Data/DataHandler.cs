using TheaterEventPlanner.Data.Definition;

namespace TheaterEventPlanner.Data;

public class DataHandler : IDataHandler
{
    private List<Event> _myList = new();

    private Tuple<Result, IEnumerable<Actor>, Event?> CheckConflict(Event myEvent, Event? skippedEvent = null)
    {
        var myActors = myEvent.CastMembers.Select(m => m.ActorName).ToHashSet();
        if (myActors.Count <= 0)
            return Tuple.Create<Result, IEnumerable<Actor>, Event?>(Result.Success, Array.Empty<Actor>(), null);

        var conflictActors = new List<Actor>();
        foreach (var selectedEvent in _myList)
        {
            if (selectedEvent == skippedEvent)
                continue;
            if (myEvent.StartDate > selectedEvent.EndDate || myEvent.EndDate < selectedEvent.StartDate)
                continue;

            foreach (var selectedActor in selectedEvent.CastMembers)
            {
                if (myActors.Contains(selectedActor.ActorName))
                    conflictActors.Add(selectedActor);
            }
            
            if (conflictActors.Count > 0)
                return Tuple.Create<Result, IEnumerable<Actor>, Event?>(Result.Conflict, conflictActors, selectedEvent);
        }
        
        return Tuple.Create<Result, IEnumerable<Actor>, Event?>(Result.Success, Array.Empty<Actor>(), null);
    }

    public Tuple<Result, IEnumerable<Actor>, Event?> CreateEvent(Event myEvent)
    {
        if (myEvent.StartDate > myEvent.EndDate)
            return Tuple.Create<Result, IEnumerable<Actor>, Event?>(Result.BadRequest, Array.Empty<Actor>(), null);
        
        var conflictDetection = CheckConflict(myEvent);
        if (conflictDetection.Item1 == Result.Success)
            AddEventToList(myEvent);
        
        return conflictDetection;
    }

    public Tuple<Result, IEnumerable<Actor>, Event?> EditEvent(Event myEvent, int eventId)
    {
        if (myEvent.StartDate > myEvent.EndDate)
            return Tuple.Create<Result, IEnumerable<Actor>, Event?>(Result.BadRequest, Array.Empty<Actor>(), null);
        
        var (foundEventIndex, foundEvent) = FindEventById(eventId);
        if (foundEvent == null)
            return Tuple.Create<Result, IEnumerable<Actor>, Event?>(Result.NotFound, Array.Empty<Actor>(), null);
        
        var (result, conflictedActors, conflictEvent) = CheckConflict(myEvent, foundEvent);
        if (result != Result.Success) 
            return Tuple.Create(result, conflictedActors, conflictEvent);
        
        myEvent.Id = eventId;
        _myList[foundEventIndex] = myEvent;

        return Tuple.Create<Result, IEnumerable<Actor>, Event?>(Result.Success, Array.Empty<Actor>(), null);
    }

    public Result DeleteEvent(int eventId)
    {
        var (_, foundEvent) = FindEventById(eventId);
        if (foundEvent == null)
            return Result.NotFound;
        _myList.Remove(foundEvent);
        return Result.Success;
    }

    public List<Event> GetAllEvents() => _myList;

    public Tuple<Result, Event?> GetEventById(int eventId)
    {
        var (_, myEvent) = FindEventById(eventId);
        return (myEvent == null)
            ? Tuple.Create<Result, Event?>(Result.NotFound, null)
            : Tuple.Create<Result, Event?>(Result.Success, myEvent);
    }

    private void AddEventToList(Event myEvent)
    {
        var lastEvent = _myList.LastOrDefault();
        myEvent.Id = (lastEvent == null) ? 0 : lastEvent.Id + 1;
        _myList.Add(myEvent);
    }

    private Tuple<int, Event?> FindEventById(int eventId)
    {
        var eventIndex = _myList.FindIndex(e => e.Id == eventId);
        return eventIndex == -1 
            ? Tuple.Create<int, Event?>(-1, null)
            : Tuple.Create<int, Event?>(eventIndex, _myList[eventIndex]);
    } 
}