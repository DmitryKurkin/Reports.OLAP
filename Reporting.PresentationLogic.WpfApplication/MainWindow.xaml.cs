namespace Reporting.PresentationLogic.WpfApplication
{
    using System.IO;
    using System.Xml.Linq;
    using System.Windows;

    using RadarSoft.Common;

    using Reporting.BusinessLogic;
    using Reporting.PresentationLogic.WpfApplication.ViewModel;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly IDatabaseBridge _dbBridge = new OdbcDatabaseBridge();

        public MainWindow()
        {
            InitializeComponent();

            Closing += (s, e) => ViewModelLocator.Cleanup();
        }

        private void ButtonConfigCube_OnClick(object sender, RoutedEventArgs e)
        {
            var contents = File.ReadAllText("c:\\work\\ClientGroupToPayment.xml");

            DataSetDescriptorBuilder.FilterProvider = new PrimitiveExternalFilterProvider();
            var dataSetDesc = DataSetDescriptorBuilder.Build(contents);

            var dataSet = DataSetBuilder.BuilDataSet(dataSetDesc, _dbBridge);

            var doc = XElement.Parse(contents);

            TolapAnalysis.Cube = OlapCubeBuilder.BuildCube(doc, dataSetDesc, dataSet);
            TolapAnalysis.Cube.Active = true;
        }

        private void ButtonSaveCube_OnClick(object sender, RoutedEventArgs e)
        {
            TolapAnalysis.SaveUncompressed("c:\\work\\uncompressed_GridState.bin", TStreamContent.GridState);
        }

        private void ButtonLoadCube_OnClick(object sender, RoutedEventArgs e)
        {
            TolapAnalysis.Load("c:\\work\\uncompressed_GridState.bin");
        }

        private void ToggleButtonNormal_OnChecked(object sender, RoutedEventArgs e)
        {
            TolapAnalysis.LoadDockFrom("..\\..\\..\\PanelsNone.xml");
        }

        private void ToggleButtonExpert_OnChecked(object sender, RoutedEventArgs e)
        {
            TolapAnalysis.LoadDockFrom("..\\..\\..\\PanelsFull.xml");
        }
    }
}