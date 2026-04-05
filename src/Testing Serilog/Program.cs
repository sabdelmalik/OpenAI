using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.CallerInfo;

namespace Testing_Serilog
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
    .Enrich.WithCallerInfo(
        includeFileInfo: true,
        allowedAssemblies: new[] { "TestingSerilog" },
        filePathDepth: 3
    ).Enrich.WithProperty("Application", "Aligner")
    .WriteTo.File("logs/aligner.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 50,
                //            outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] ({Caller}) {Message} {NewLine} {Exception}")
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Namespace}] ({Method}) {Message} {SourceFile} {LineNumber} {NewLine} {Exception}")
    .CreateLogger();


            //Log.Logger = new LoggerConfiguration()
            //    .MinimumLevel.Debug()
            //    .WriteTo.File("logs/Aligner.log",
            //        rollingInterval: RollingInterval.Day,
            //        retainedFileCountLimit: 7)
            //    .CreateLogger();

            // DI
            var host = CreateHostBuilder().Build();
        Application.Run(host.Services.GetRequiredService<MainForm>());
        }

        /// <summary>
        /// Configure DI services
        /// </summary>
        /// <param name="services"></param>
        private static IHostBuilder CreateHostBuilder() => Host.CreateDefaultBuilder()
            .UseSerilog() // Use Serilog for logging
            .ConfigureServices((context, services) =>
            {
                services.AddTransient<MainForm>();
            });


    }
}