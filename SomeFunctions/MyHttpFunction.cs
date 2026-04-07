using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Shared.Dtos;

namespace SomeFunctions;

public class MyHttpFunction
{
    private readonly ILogger<MyHttpFunction> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public MyHttpFunction(ILogger<MyHttpFunction> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    [Function(MyFunctionNames.GetProductsSummary)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        _logger.LogInformation("Fetching products summary from App1...");

        var client = _httpClientFactory.CreateClient("app1");
        var products = await client.GetFromJsonAsync<List<ProductResponse>>("api/products");

        var summary = new
        {
            TotalProducts = products?.Count ?? 0,
            TotalValue = products?.Sum(p => p.Price * p.StockQuantity) ?? 0,
            RetrievedAt = DateTime.UtcNow
        };

        return new OkObjectResult(summary);
    }
}
