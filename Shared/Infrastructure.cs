using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared;

public static class InfrastructureExtensions
{
    public static void MapOurFallback(this WebApplication app)
    {
        app.MapFallback(async context =>
        {
            while (true)
            {
                try
                {
                    var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
                    var path = Path.Combine(env.WebRootPath, "index.html");
                    var text = File.ReadAllText(path);
                    var config = context.RequestServices.GetRequiredService<IConfiguration>();
                    var secret = config.GetValue<string>("SharedConfig:SomeKey");  // This is lazy, should extract key to variable
                    text = text.Replace("THESECRET", secret);

                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync(text);
                    return;
                }
                catch (FileNotFoundException)
                {
                    // Likely just waiting for UI to finish building
                    await Task.Delay(500);
                } 
            }
        });
    }
}
