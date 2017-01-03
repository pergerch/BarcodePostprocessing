namespace BarcodePostprocessingWPF
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using Microsoft.HockeyApp;

    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Main configuration of HockeySDK
            HockeyClient.Current.Configure("9a6434d251fb4bc897eb13025fe58cbc");

            // Optional: Should only used in debug builds. Register an event-handler to get exceptions in HockeySDK code that are "swallowed" (like problems writing crashlogs etc.)
#if DEBUG
            ((HockeyClient)HockeyClient.Current).OnHockeySDKInternalException += (sender, args) =>
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            };
#endif
            
            // Send crashes to the HockeyApp server
            await HockeyClient.Current.SendCrashesAsync(true);

            // Check for updates on the HockeyApp server. Try catch is needed for debugging?!?!
            try
            {
                await HockeyClient.Current.CheckForUpdatesAsync(true, () =>
                {
                    if (Current.MainWindow != null)
                    {
                        Current.MainWindow.Close();
                    }
                    return true;
                });
            }
            catch (NullReferenceException)
            {
                
            }
            
        }

    }
}