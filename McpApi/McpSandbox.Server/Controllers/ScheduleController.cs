using Microsoft.AspNetCore.Mvc;

namespace McpSandbox.Server.Controllers;

public class ScheduleController : ControllerBase
{
    [HttpGet("/schedule")]
    public IActionResult GetSchedule()
    {
        var schedule = new
        {
            Monday = "Math",
            Tuesday = "Science",
            Wednesday = "History",
            Thursday = "Art",
            Friday = "Physical Education"
        };
        return Ok(schedule);
    }
}
