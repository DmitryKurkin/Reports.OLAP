namespace Reporting.PresentationLogic.WpfApplication
{
    using Reporting.PresentationLogic.WpfApplication.ViewModel;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Closing += (s, e) => ViewModelLocator.Cleanup();
        }
    }
}