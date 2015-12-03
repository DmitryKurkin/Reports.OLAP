namespace Reporting.PresentationLogic.WpfApplication
{
    using System.Threading;
    using System.Windows;

    using GalaSoft.MvvmLight.Threading;

    using log4net;
    using log4net.Config;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        /// <summary>
        /// The logger
        /// </summary>
        private static readonly ILog Log;

        /// <summary>
        /// Initializes the <see cref="App"/> class
        /// </summary>
        static App()
        {
            Thread.CurrentThread.Name = "UI THREAD";

            XmlConfigurator.Configure();

            Log = LogManager.GetLogger(typeof (App));
            Log.Info("Starting...");

            DispatcherHelper.Initialize();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Exit"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.Windows.ExitEventArgs"/> that contains the event data.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            Log.Info("Exiting...");
            LogManager.Shutdown();

            base.OnExit(e);
        }
    }
}