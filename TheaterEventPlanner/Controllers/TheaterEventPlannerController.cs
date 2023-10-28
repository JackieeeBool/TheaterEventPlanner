using Microsoft.AspNetCore.Mvc;
using TheaterEventPlanner.Data;
using TheaterEventPlanner.Data.Definition;

namespace TheaterEventPlanner.Controllers;

[ApiController]
[Route("/api")]
public class TheaterEventPlannerController : ControllerBase
{
    private readonly IDataHandler _dataHandler;

    private readonly ILogger<TheaterEventPlannerController> _logger;

    public TheaterEventPlannerController(ILogger<TheaterEventPlannerController> logger, IDataHandler dataHandler)
    {
        _dataHandler = dataHandler;
        _logger = logger;
    }

    [HttpPost("events")]
    public IActionResult CreateEvent([FromBody] Event sentEvent)
    {
        try
        {
            var (success, actors, conflictEvent) = _dataHandler.CreateEvent(sentEvent);
            if (success == Result.Conflict)
            {
                var actorNames = string.Join(", ", actors.Select(a => a.ActorName));
                return BadRequest($"Scheduling conflict detected. The following actors are already assigned to '{conflictEvent!.Name}' on '{conflictEvent.StartDate}': {actorNames}.");
            }
        }
        catch (Exception exception)
        {
            _logger.LogError("Exception: {message}, StackTrace: {stackTrace}", exception.Message, exception.StackTrace);
            return Problem(exception.Message);
        }

        return Created("events", sentEvent);
    }

    [HttpPut("events/{eventId:int}")]
    public IActionResult UpdateEventById([FromBody] Event sentEvent, [FromRoute] int eventId)
    {
        try
        {
            var (success, actors, conflictEvent) = _dataHandler.EditEvent(sentEvent, eventId);
            if (success == Result.NotFound)
                return NotFound();
            if (success == Result.Conflict)
            {
                var actorNames = string.Join(", ", actors.Select(a => a.ActorName));
                return BadRequest($"Scheduling conflict detected. The following actors are already assigned to '{conflictEvent!.Name}' on '{conflictEvent.StartDate}': {actorNames}.");
            }
        }
        catch (Exception exception)
        {
            _logger.LogError("Exception: {message}, StackTrace: {stackTrace}", exception.Message, exception.StackTrace);
            return Problem(exception.Message);
        }

        return Ok();
    }

    [HttpDelete("events/{eventId:int}")]
    public IActionResult DeleteEventById([FromRoute] int eventId)
    {
        try
        {
            if (_dataHandler.DeleteEvent(eventId) == Result.NotFound)
                return NotFound();
        }
        catch (Exception exception)
        {
            _logger.LogError("Exception: {message}, StackTrace: {stackTrace}", exception.Message, exception.StackTrace);
            return Problem(exception.Message);
        }

        return NoContent();
    }

    [HttpGet("events")]
    public IActionResult GetAllEvents()
    {
        try
        {
            return Ok(_dataHandler.GetAllEvents());
        }
        catch (Exception exception)
        {
            _logger.LogError("Exception: {message}, StackTrace: {stackTrace}", exception.Message, exception.StackTrace);
            return Problem(exception.Message);
        }
    }
    
    [HttpGet("events/{eventId:int}")]
    public IActionResult GetEventsById([FromRoute] int eventId)
    {
        try
        {
            var (foundResult, foundEvent) = _dataHandler.GetEventById(eventId);
            if (foundResult == Result.Success)
                return Ok(foundEvent);
        }
        catch (Exception exception)
        {
            _logger.LogError("Exception: {message}, StackTrace: {stackTrace}", exception.Message, exception.StackTrace);
            return Problem(exception.Message);
        }

        return NotFound();
    }
}