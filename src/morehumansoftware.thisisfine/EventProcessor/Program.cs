using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace morehumansoftware.thisisfine.EventProcessor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine($"Starting up event processor at {DateTime.Now}.");

            var host = new WebHostBuilder()
                .UseKestrel(options => options.ListenAnyIP(8080))
                .ConfigureLogging(logging => { logging.AddConsole(); })
                .Configure(app =>
                {
                    var logger = app.ApplicationServices.GetRequiredService<ILogger<Program>>();

                    app
                        .Run(
                            async context =>
                            {
                                try
                                {
                                    using (var scope = context.RequestServices.CreateScope())
                                    {
                                        var sleep = Environment.GetEnvironmentVariable("sleep", EnvironmentVariableTarget.Process);
                                        logger.LogInformation("Received  event.");
                                        if(string.IsNullOrWhiteSpace(sleep))
                                        {
                                            context.Response.StatusCode = StatusCodes.Status200OK;
                                        }
                                        else
                                        {
                                            int sleepTime;
                                            if (int.TryParse(sleep, out sleepTime))
                                            {
                                                Thread.Sleep(new TimeSpan(0, 0, sleepTime));
                                                context.Response.StatusCode = StatusCodes.Status200OK;
                                            }
                                            else
                                            {
                                                context.Response.StatusCode = StatusCodes.Status200OK;
                                            }
                                        } 
                                    }
                                }
                                catch (Exception e)
                                {
                                    var errorText = "An error occurred during message event processing.";
                                    logger.LogError(e,errorText);
                                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                                    context.Response.ContentType = "application/json";
                                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new {error = errorText}));
                                } 
                            }
                        );
                })
                .Build();

            host.Run();
        }

    }
}