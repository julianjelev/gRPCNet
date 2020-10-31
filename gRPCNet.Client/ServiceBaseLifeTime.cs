using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace gRPCNet.Client
{
    public static class ServiceBaseLiveTimeHostExtension
    {
        public static Task RunAsServiceAsync(this IHostBuilder hostBuilder, CancellationToken cancellationToken = default)
        {
            return hostBuilder
                .UseServiceBaseLifetime()
                .Build()
                .RunAsync(cancellationToken);
        }

        public static IHostBuilder UseServiceBaseLifetime(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices((hostContext, services) => services.AddSingleton<IHostLifetime, ServiceBaseLifeTime>());
        }
    }
    public class ServiceBaseLifeTime : ServiceBase, IHostLifetime
    {
        private readonly TaskCompletionSource<object> _delayStart;
        private IHostApplicationLifetime ApplicationLifetime { get; }

        public ServiceBaseLifeTime(IHostApplicationLifetime applicationLifetime)
        {
            _delayStart = new TaskCompletionSource<object>();
            ApplicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Stop();
            return Task.CompletedTask;
        }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() => _delayStart.TrySetCanceled());
            ApplicationLifetime.ApplicationStopping.Register(Stop);
            new Thread(Run).Start();// Otherwise this would block and prevent IHost.StartAsync from finishing. 
            return _delayStart.Task;
        }

        private void Run()
        {
            try
            {
                Run(this); // This blocks until the service is stopped.
                _delayStart.TrySetException(new InvalidOperationException("Stopped without starting"));
            }
            catch (Exception ex)
            {
                _delayStart.TrySetException(ex);
            }
        }

        // Called by base.Run when the service is ready to start.
        protected override void OnStart(string[] args)
        {
            _delayStart.TrySetResult(null);
            base.OnStart(args);
        }
        // Called by base.Stop. This may be called multiple times by  service Stop, ApplicationStopping, and StopAsync.
        // That's OK because StopApplication uses a CancellationTokenSource and prevents any recursion.
        protected override void OnStop()
        {
            ApplicationLifetime.StopApplication();
            base.OnStop();
        }
        protected override void OnPause()
        {
            // Custom action on pause
            base.OnPause();
        }
        protected override void OnContinue()
        {
            // Custom action on continue
            base.OnContinue();
        }
    }
}
