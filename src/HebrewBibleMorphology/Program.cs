using AdvancedAligner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HebrewBibleMorphology
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
            //Application.Run(new MainForm());

            // DI
            var host = CreateHostBuilder().Build();
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
                services.AddSingleton<HebrewBibleParser>();
            });
    }
}