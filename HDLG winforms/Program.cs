using HdlgFileProperty;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HDLG_winforms
{
    internal static class Program
    {
        public static IServiceProvider? ServiceProvider { get; private set; }
        static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<ImagePropertyGetter, ImagePropertyGetter>();
                    services.AddTransient<WordPropertyGetter, WordPropertyGetter>();
                    services.AddTransient<ExcelPropertyGetter, ExcelPropertyGetter>();
                    services.AddTransient<PdfPropertyGetter, PdfPropertyGetter>();
                    services.AddTransient<MainWindow>();
                });
        }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            //Application.Run(new MainWindow());
            IHost host = CreateHostBuilder().Build();
            ServiceProvider = host.Services;

            Application.Run(ServiceProvider.GetRequiredService<MainWindow>());
        }

    }
}