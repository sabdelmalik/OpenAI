using AdvancedAligner.ExampleEditor;
using AdvancedAligner.Examples;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAiAPI;
using TAParser;
using Serilog;
using Serilog.Core;
using Serilog.Enrichers.CallerInfo;


namespace AdvancedAligner
{
    internal static class Program
    {
        // used for Dependency Injection
        public static IServiceProvider ServiceProvider { get; private set; }

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
                        allowedAssemblies: new[] { "Aligner", "AlignmentService", "OpenAiService", "Parser", "Tokens" },
                        filePathDepth: 3
                        )
                    .Enrich.WithProperty("Application", "Aligner")
                    .WriteTo.File("logs/aligner.log",
                                rollingInterval: RollingInterval.Day,
                                retainedFileCountLimit: 50,
                                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Namespace}] ({Method}) {Message} {SourceFile} {LineNumber} {NewLine} {Exception}")
                    .CreateLogger();

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
                services.AddSingleton<OpenAiService>();
                services.AddSingleton<HebrewBibleParser>();
                services.AddSingleton<OshbParser>();
                services.AddSingleton<TahotParser>();
                services.AddSingleton<TargetParser>();
                services.AddSingleton<AlignmentService>();
                services.AddSingleton<ExamplesDatabase>();
                services.AddSingleton<ExampleSelector>();
                services.AddSingleton<ReferenceListSelection>();
                services.AddSingleton<ReferenceRangeSelection>();
                services.AddSingleton<ExampleEditorForm>();
                services.AddSingleton<ExampleEditorForm>();
            });


    }
}