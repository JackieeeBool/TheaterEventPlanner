using TheaterEventPlanner.Data.Definition;

namespace TheaterEventPlanner.Data;

public interface IDataHandler
{
    public Tuple<Result, IEnumerable<Actor>, Event?> CreateEvent(Event myEvent);

    public Tuple<Result, IEnumerable<Actor>, Event?> EditEvent(Event myEvent, int eventId);

    public Result DeleteEvent(int eventId);

    public List<Event> GetAllEvents();

    public Tuple<Result, Event?> GetEventById(int eventId);
}