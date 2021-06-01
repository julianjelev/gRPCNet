using gRPCNet.Client.Services;
using gRPCNet.Modbus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace gRPCNet.Client
{
    //https://medium.com/@tocalai/create-windows-service-using-net-core-console-application-dc2f278bbe42
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // Run with console or service
            var isService = !(Debugger.IsAttached || args.Contains("--console"));
            var pathToContentRoot = Directory.GetCurrentDirectory();

            if (isService)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                pathToContentRoot = Path.GetDirectoryName(pathToExe);
            }
            
            var host = Host.CreateDefaultBuilder()
                .UseContentRoot(pathToContentRoot)
                .UseEnvironment(isService ? Environments.Production : Environments.Development)
                .UseSystemd() // for linux systemd
                .ConfigureAppConfiguration(config => 
                {
                    config.SetBasePath(pathToContentRoot)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile("certificate.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) => 
                {
                    services
                    .AddLogging(logging => logging.AddConfiguration(hostContext.Configuration.GetSection("Logging")))
                    .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information)
                    .Configure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromSeconds(30));

                    // register FileLogger service
                    services.AddTransient<IFileLogger>(s => new FileLogger(hostContext.Configuration, pathToContentRoot));

                    // register application services
                    //services.AddTransient<IModbusSlaveDevice, ModbusSlaveDevice>();
                    //services.AddTransient<IModbusMessageService, ModbusMessageService>();
                    services.AddTransient<ITextMessageService, TextMessageService>();
                    services.AddSingleton<GrpcChannelService>();

                    services.AddHostedService<TcpHostedService>();
                    services.AddHostedService<KeepaliveHostedService>();
                });

            //if (isService)
            //{
            //    await host.RunAsServiceAsync();
            //}
            //else
            //{
                //await host.RunConsoleAsync();
                await host
                    .UseConsoleLifetime(options =>
                    {
                        options.SuppressStatusMessages = false;
                    })
                    .RunConsoleAsync();
            //}
        }
    }
}
