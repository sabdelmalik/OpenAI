using AdvancedAligner.Examples;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAiAPI;
using TAParser;

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

            // DI
            var host= CreateHostBuilder().Build();
            Application.Run(host.Services.GetRequiredService<MainForm>());
        }

        /// <summary>
        /// Configure DI services
        /// </summary>
        /// <param name="services"></param>
        private static IHostBuilder CreateHostBuilder() => Host.CreateDefaultBuilder()
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
            });


    }
}