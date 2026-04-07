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

    [Function(MyFunctionNames.MyTimerFunction)]
    public void Run([TimerTrigger("0 */5 * * * *")] TimerInfo timerInfo)
    {
        _logger.LogInformation("Timer function executed at: {Time}", DateTime.UtcNow);

        if (timerInfo.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {Next}", timerInfo.ScheduleStatus.Next);
        }
    }
}
