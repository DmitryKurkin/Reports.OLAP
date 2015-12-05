namespace Reporting.PresentationLogic.WpfApplication.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Data;
    using System.Xml.Linq;

    using Microsoft.Win32;
    
    using OrdersPlanApp.MvvmLight.Commands;
    using OrdersPlanApp.MvvmLight.ViewModel;

    using RadarSoft.RadarCube.WPF.Analysis;
    using RadarSoft.RadarCube.WPF.Common;

    using Reporting.BusinessLogic;
    using Reporting.PresentationLogic.WpfApplication.Properties;

    /// <summary>
    /// Models the main view
    /// </summary>
    public class MainViewModel : NotifyDataErrorInfoViewModelBase
    {
        /// <summary>
        /// The currently loaded user config
        /// </summary>
        private UserConfiguration _userConfiguration;

        /// <summary>
        /// The database bridge
        /// </summary>
        private readonly IDatabaseBridge _dbBridge;

        /// <summary>
        /// Indicates whether the cube has been loaded
        /// </summary>
        private bool _isCubeLoaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class
        /// </summary>
        /// <param name="dbBridge">The database bridge</param>
        /// <param name="externalFilterProvider">The external filter provider</param>
        public MainViewModel(IDatabaseBridge dbBridge, IExternalFilterProvider externalFilterProvider)
        {
            if (dbBridge == null) throw new ArgumentNullException(nameof(dbBridge));
            if (externalFilterProvider == null) throw new ArgumentNullException(nameof(externalFilterProvider));

            _dbBridge = dbBridge;
            DataSetDescriptorBuilder.FilterProvider = externalFilterProvider;

            DataSetConfigFiles.CurrentChanged += DataSetConfigFiles_CurrentChanged;

            ViewLoadVm = new ButtonViewModel
            {
                Command = new AsyncRelayCommand(
                    o => DoLoadView(),
                    o => !IsBusy,
                    o => IsBusy = true,
                    o => IsBusy = false)
            };

            CubeLoadVm = new ButtonViewModel
            {
                Command = new AsyncRelayCommand(
                    o => DoLoadCube((TOLAPAnalysis) o),
                    o => !IsBusy && (DataSetConfigFiles.CurrentItem != null || CubeConfigFiles.CurrentItem != null),
                    o => IsBusy = true,
                    o => IsBusy = false)
            };
            CubeSaveVm = new ButtonViewModel
            {
                Command = new AsyncRelayCommand(
                    o => DoSaveCube((TOLAPAnalysis) o),
                    o => !IsBusy && _isCubeLoaded && CubeConfigFiles.CurrentItem != null,
                    o => IsBusy = true,
                    o => IsBusy = false)
            };
            CubeSaveAsVm = new ButtonViewModel
            {
                Command = new AsyncRelayCommand(
                    o => DoSaveCubeAs((TOLAPAnalysis) o),
                    o => !IsBusy && _isCubeLoaded && DataSetConfigFiles.CurrentItem != null,
                    o => IsBusy = true,
                    o => IsBusy = false)
            };
            CubeDeleteVm = new ButtonViewModel
            {
                Command = new AsyncRelayCommand(
                    o => DoDeleteCube((TOLAPAnalysis) o),
                    o => !IsBusy && _isCubeLoaded && CubeConfigFiles.CurrentItem != null,
                    o => IsBusy = true,
                    o => IsBusy = false)
            };
        }

        /// <summary>
        /// Unregisters this instance from the Messenger class.
        /// <para>
        /// To cleanup additional resources, override this method, clean
        ///             up and then call base.Cleanup().
        /// </para>
        /// </summary>
        public override void Cleanup()
        {
            DataSetConfigFiles.CurrentChanged -= DataSetConfigFiles_CurrentChanged;

            base.Cleanup();
        }

        /// <summary>
        /// Gets the available data set configuration files
        /// </summary>
        public ListCollectionView DataSetConfigFiles { get; } =
            new ListCollectionView(new List<DataSetConfigurationFile>());

        /// <summary>
        /// Gets the available cube configuration files
        /// </summary>
        public ListCollectionView CubeConfigFiles { get; } =
            new ListCollectionView(new List<CubeConfigurationFile>());

        /// <summary>
        /// Gets the view model of the 'load view' button
        /// </summary>
        public ButtonViewModel ViewLoadVm { get; }

        /// <summary>
        /// Gets the view model of the 'load cube' button
        /// </summary>
        public ButtonViewModel CubeLoadVm { get; }

        /// <summary>
        /// Gets the view model of the 'save cube' button
        /// </summary>
        public ButtonViewModel CubeSaveVm { get; }

        /// <summary>
        /// Gets the view model of the 'save cube as' button
        /// </summary>
        public ButtonViewModel CubeSaveAsVm { get; }

        /// <summary>
        /// Gets the view model of the 'delete cube' button
        /// </summary>
        public ButtonViewModel CubeDeleteVm { get; }

        /// <summary>
        /// Loads the view
        /// </summary>
        private void DoLoadView()
        {
            _userConfiguration = UserConfiguration.Load(Settings.Default.UserConfigurationFile);

            UiAction(() =>
            {
                CubeConfigFiles.Clear();
                DataSetConfigFiles.Clear();

                DataSetConfigFiles.AddRange(_userConfiguration.DataSetFiles);
                DataSetConfigFiles.MoveCurrentTo(null);
            });
        }

        /// <summary>
        /// Loads the cube into the specified <see cref="TOLAPAnalysis"/>
        /// </summary>
        /// <param name="tolapAnalysis">The <see cref="TOLAPAnalysis"/> to load the cube into</param>
        private void DoLoadCube(TOLAPAnalysis tolapAnalysis)
        {
            var currDataSetConfig = DataSetConfigFiles.Curr<DataSetConfigurationFile>();
            var currCubeConfig = CubeConfigFiles.Curr<CubeConfigurationFile>();

            var contents = File.ReadAllText(currDataSetConfig.FilePath);

            var dataSetDesc = DataSetDescriptorBuilder.Build(contents);

            var dataSet = DataSetBuilder.BuilDataSet(dataSetDesc, _dbBridge);

            var doc = XElement.Parse(contents);

            UiAction(() =>
            {
                var cube = OlapCubeBuilder.BuildCube(doc, dataSetDesc, dataSet);

                if (tolapAnalysis.Cube != null)
                {
                    tolapAnalysis.Cube.Active = false;
                }

                tolapAnalysis.Cube = cube;
                tolapAnalysis.Cube.Active = true;

                if (currCubeConfig != null)
                {
                    tolapAnalysis.Load(currCubeConfig.FilePath);
                }
            });

            _isCubeLoaded = true;
        }

        /// <summary>
        /// Saves the cube of the specified <see cref="TOLAPAnalysis"/>
        /// </summary>
        /// <param name="tolapAnalysis">The <see cref="TOLAPAnalysis"/> whose cube to save</param>
        private void DoSaveCube(TOLAPAnalysis tolapAnalysis)
        {
            var currCubeConfig = CubeConfigFiles.Curr<CubeConfigurationFile>();

            UiAction(() =>
            {
                tolapAnalysis.SaveUncompressed(currCubeConfig.FilePath, TStreamContent.GridState);
            });
        }

        /// <summary>
        /// Saves the cube of the specified <see cref="TOLAPAnalysis"/> as
        /// </summary>
        /// <param name="tolapAnalysis">The <see cref="TOLAPAnalysis"/> whose cube to save as</param>
        private void DoSaveCubeAs(TOLAPAnalysis tolapAnalysis)
        {
            var dlg = new SaveFileDialog
            {
                FileName = "CubeConfig",
                DefaultExt = ".xml",
                Filter = "XML documents (.xml)|*.xml"
            };

            if (dlg.ShowDialog() != true) return;

            var newCubeConfig = new CubeConfigurationFile("New cube config", dlg.FileName);

            var currDataSetConfig = DataSetConfigFiles.Curr<DataSetConfigurationFile>();
            currDataSetConfig.CubeFiles.Add(newCubeConfig);

            UiAction(() =>
            {
                // TODO: request display name
                tolapAnalysis.SaveUncompressed(newCubeConfig.FilePath, TStreamContent.GridState);

                CubeConfigFiles.AddAndCommit(newCubeConfig);
                CubeConfigFiles.MoveCurrentTo(newCubeConfig);
            });

            _userConfiguration.Save(Settings.Default.UserConfigurationFile);
        }

        /// <summary>
        /// Deletes the cube of the specified <see cref="TOLAPAnalysis"/>
        /// </summary>
        /// <param name="tolapAnalysis">The <see cref="TOLAPAnalysis"/> whose cube to delete</param>
        private void DoDeleteCube(TOLAPAnalysis tolapAnalysis)
        {
            var currDataSetConfig = DataSetConfigFiles.Curr<DataSetConfigurationFile>();
            var currCubeConfig = CubeConfigFiles.Curr<CubeConfigurationFile>();

            currDataSetConfig.CubeFiles.Remove(currCubeConfig);

            _userConfiguration.Save(Settings.Default.UserConfigurationFile);

            UiAction(() =>
            {
                DataSetConfigFiles.MoveCurrentTo(null);

                if (tolapAnalysis.Cube == null) return;

                tolapAnalysis.Cube.Active = false;
            });

            _isCubeLoaded = false;
        }

        /// <summary>
        /// Handles the <see cref="CollectionView.CurrentChanged"/> event of the <see cref="DataSetConfigFiles"/>
        /// </summary>
        /// <param name="sender">The event source</param>
        /// <param name="e">The event data</param>
        private void DataSetConfigFiles_CurrentChanged(object sender, EventArgs e)
        {
            CubeConfigFiles.Clear();

            var currDataSetConfig = DataSetConfigFiles.Curr<DataSetConfigurationFile>();
            if (currDataSetConfig == null) return;

            CubeConfigFiles.AddRange(currDataSetConfig.CubeFiles);
            CubeConfigFiles.MoveCurrentTo(null);
        }
    }
}