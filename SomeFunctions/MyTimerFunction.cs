using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SomeFunctions;

public class MyTimerFunction
{
    private readonly ILogger<MyTimerFunction> _logger;

    public MyTimerFunction(ILogger<MyTimerFunction> logger)
    {
        _logger = logger;
    }

    [Function("MyTimerFunction")]
    public IActionResult Run([TimerTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
    }
}