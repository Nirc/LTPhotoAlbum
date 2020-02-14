using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LTPhotoAlbum
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false);
                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddOptions();
                    services.Configure<AppSettings>(context.Configuration.GetSection("AppSettings"));
                    services.AddSingleton<IHostedService, ConsoleApp>();
                    services.AddSingleton<IConsoleProcessor, ConsoleProcessor>();

                    var appSettings = context.Configuration.GetSection("AppSettings").Get<AppSettings>();

                    services.AddHttpClient<IPhotoAlbumClient, PhotoAlbumClient>((sp, client) =>
                    {
                        client.BaseAddress = new Uri(appSettings.BaseAddress);
                        client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                        client.Timeout = TimeSpan.FromSeconds(appSettings.TimeoutSeconds);
                    });
                })
                .UseConsoleLifetime();

            try
            {
                await host.RunConsoleAsync();
            }
            catch(Exception e)
            {
                // Log the error to a centralized log location
            }
        }
    }
}