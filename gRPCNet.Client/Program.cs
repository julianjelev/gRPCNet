﻿using gRPCNet.Client.Services;
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
                .ConfigureAppConfiguration(config => 
                {
                    config.SetBasePath(pathToContentRoot)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile("certificate.json", optional: true, reloadOnChange: true);
                })
                .ConfigureHostConfiguration(config => { })
                .ConfigureServices((hostContext, services) => 
                {
                    services.AddLogging(logging =>
                    {
                        logging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                    })
                    .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);

                    // register FileLogger service
                    services.AddTransient<IFileLogger>(s => new FileLogger(hostContext.Configuration, pathToContentRoot));

                    // register application services
                    services.AddTransient<ISocketMessageService, SocketMessageService>();
                    services.AddSingleton<GrpcChannelService>();

                    services.AddHostedService<TcpHostedService>();
                    services.AddHostedService<KeepaliveHostedService>();

                    services.Configure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromSeconds(15));
                })
                .UseEnvironment(isService ? Environments.Production : Environments.Development);

            if (isService)
            {
                await host.RunAsServiceAsync();
            }
            else
            {
                await host.RunConsoleAsync();
            }
        }
    }
}
