/*
 This file is part of HTML Directory List Generator.

 HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

 HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

 You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>. 
 */

using HdlgFileProperty;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using System.Globalization;

namespace HDLG_winforms
{
    internal static class Program
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        private static string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HDLG", "logs");
        private static string logFilePath = Path.Combine(logDirectory, "log.txt");

        /// <summary>
        /// Logger
        /// </summary>
        private static readonly Logger log = new LoggerConfiguration()
            .WriteTo.File(logFilePath, formatProvider: CultureInfo.CurrentCulture, rollingInterval: RollingInterval.Day, outputTemplate:
                "[{Timestamp:R} {Level:u3}] {Message:lj}{NewLine}{Exception}").MinimumLevel.Debug()
            .CreateLogger();

        static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<ImagePropertyGetter, ImagePropertyGetter>();
                    services.AddTransient<WordPropertyGetter, WordPropertyGetter>();
                    services.AddTransient<ExcelPropertyGetter, ExcelPropertyGetter>();
                    services.AddTransient<PdfPropertyGetter, PdfPropertyGetter>();
                    services.AddTransient<Mp3PropertyGetter, Mp3PropertyGetter>();
                    services.AddSingleton<Logger>(log);
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

            // 1. Forcer l'application à utiliser notre gestionnaire pour le thread UI
            // Il est important de définir ceci AVANT l'initialisation du Host ou des formulaires
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            // 2. Intercepter les exceptions sur le thread de l'interface utilisateur (UI)
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

            // 3. Intercepter les exceptions sur les threads d'arrière-plan
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            try
            {
                IHost host = CreateHostBuilder().Build();
                ServiceProvider = host.Services;

                Application.Run(ServiceProvider.GetRequiredService<MainWindow>());
            }
            finally
            {
                // Garantit que les logs en attente sont bien écrits sur le disque avant la fermeture
                log.Dispose();
            }
        }

        // Méthode appelée quand une erreur survient sur l'interface graphique (ex: un clic de bouton)
        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            // Affiche un message convivial à l'utilisateur
            AfficherMessageErreur("An unexpected error has occurred.", e.Exception);

            // L'application continuera de tourner après la fermeture de la boîte de dialogue.
        }

        // Méthode appelée quand une erreur survient en arrière-plan
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            AfficherMessageErreur("The application has encountered a fatal error.", ex);

            // Souvent, pour les erreurs de thread d'arrière-plan, il est plus sûr de fermer l'application
            // car l'état de la mémoire peut être corrompu.
            // Application.Exit();
        }

        // Fonction utilitaire pour éviter de répéter le code de la boîte de dialogue
        static void AfficherMessageErreur(string messageDeBase, Exception ex)
        {
            log.Fatal(ex, "Global unhandled exception intercepted.");
            string messageComplet = $"{messageDeBase}\n\nTechnical detail : {ex.Message}";
            MessageBox.Show(messageComplet, "Application error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}