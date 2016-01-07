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
        /// The currently loaded data set config
        /// </summary>
        private DataSetConfigurationFile _currentDataSetConfig;

        /// <summary>
        /// The currently loaded cube config
        /// </summary>
        private CubeConfigurationFile _currentCubeConfig;

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

            ViewLoadVm = new ButtonViewModel
            {
                Command = new AsyncRelayCommand(
                    o => DoLoadView(),
                    o => !IsBusy,
                    o => IsBusy = true,
                    o => IsBusy = false)
            };

            SaveCubeVm = new ButtonViewModel
            {
                Command = new AsyncRelayCommand(
                    o => DoSaveCube((TOLAPAnalysis) o),
                    o => !IsBusy && _isCubeLoaded && _currentCubeConfig != null,
                    o => IsBusy = true,
                    o => IsBusy = false)
            };
            SaveCubeAsVm = new ButtonViewModel
            {
                Command = new AsyncRelayCommand(
                    o => DoSaveCubeAs((TOLAPAnalysis) o),
                    o => !IsBusy && _isCubeLoaded && _currentDataSetConfig != null,
                    o => IsBusy = true,
                    o => IsBusy = false)
            };
            DeleteCubeVm = new ButtonViewModel
            {
                Command = new AsyncRelayCommand(
                    o => DoDeleteCube((TOLAPAnalysis) o),
                    o => !IsBusy && _isCubeLoaded && _currentCubeConfig != null,
                    o => IsBusy = true,
                    o => IsBusy = false)
            };
        }

        /// <summary>
        /// Gets the menu items of the available data set configuration files
        /// </summary>
        public ListCollectionView DataSetMenuItems { get; } =
            new ListCollectionView(new List<MenuItemViewModel>());

        /// <summary>
        /// Gets the view model of the 'load view' button
        /// </summary>
        public ButtonViewModel ViewLoadVm { get; }

        /// <summary>
        /// Gets the view model of the 'save cube' button
        /// </summary>
        public ButtonViewModel SaveCubeVm { get; }

        /// <summary>
        /// Gets the view model of the 'save cube as...' button
        /// </summary>
        public ButtonViewModel SaveCubeAsVm { get; }

        /// <summary>
        /// Gets the view model of the 'delete cube' button
        /// </summary>
        public ButtonViewModel DeleteCubeVm { get; }

        /// <summary>
        /// Loads the view
        /// </summary>
        private void DoLoadView()
        {
            _userConfiguration = UserConfiguration.Load(Settings.Default.UserConfigurationFile);

            DoLoadDataSetMenu();
        }

        /// <summary>
        /// Loads the 'Data Sets' menu items
        /// </summary>
        private void DoLoadDataSetMenu()
        {
            UiAction(() =>
            {
                DataSetMenuItems.Clear();

                foreach (var dscf in _userConfiguration.DataSetFiles)
                {
                    var mi = new MenuItemViewModel {Header = dscf.DisplayName};

                    // add the 'default' menu item
                    mi.Subitems.AddAndCommit(new MenuItemViewModel
                    {
                        Header = "<Default>",
                        Command = new AsyncRelayCommand(
                            o => DoLoadCube(dscf, null, (TOLAPAnalysis) o),
                            o => !IsBusy,
                            o => IsBusy = true,
                            o => IsBusy = false)
                    });

                    foreach (var ccf in dscf.CubeFiles)
                    {
                        mi.Subitems.AddAndCommit(new MenuItemViewModel
                        {
                            Header = ccf.DisplayName,
                            Command = new AsyncRelayCommand(
                                o => DoLoadCube(dscf, ccf, (TOLAPAnalysis) o),
                                o => !IsBusy,
                                o => IsBusy = true,
                                o => IsBusy = false)
                        });
                    }

                    DataSetMenuItems.AddAndCommit(mi);
                }
            });
        }

        /// <summary>
        /// Loads the cube into the specified <see cref="TOLAPAnalysis"/>
        /// </summary>
        /// <param name="dataSetConfigurationFile">The data set config file</param>
        /// <param name="cubeConfigurationFile">The cube config file (can be null)</param>
        /// <param name="tolapAnalysis">The <see cref="TOLAPAnalysis"/> to load the cube into</param>
        private void DoLoadCube(
            DataSetConfigurationFile dataSetConfigurationFile,
            CubeConfigurationFile cubeConfigurationFile,
            TOLAPAnalysis tolapAnalysis)
        {
            _currentDataSetConfig = dataSetConfigurationFile;
            _currentCubeConfig = cubeConfigurationFile;

            var contents = File.ReadAllText(_currentDataSetConfig.FilePath);

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

                if (_currentCubeConfig != null)
                {
                    tolapAnalysis.Load(_currentCubeConfig.FilePath);
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
            UiAction(() =>
            {
                tolapAnalysis.SaveUncompressed(_currentCubeConfig.FilePath, TStreamContent.GridState);
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

            _currentCubeConfig = new CubeConfigurationFile("New cube config", dlg.FileName);

            _currentDataSetConfig.CubeFiles.Add(_currentCubeConfig);

            UiAction(() =>
            {
                // TODO: request display name
                tolapAnalysis.SaveUncompressed(_currentCubeConfig.FilePath, TStreamContent.GridState);
            });

            _userConfiguration.Save(Settings.Default.UserConfigurationFile);

            DoLoadDataSetMenu();
        }

        /// <summary>
        /// Deletes the cube of the specified <see cref="TOLAPAnalysis"/>
        /// </summary>
        /// <param name="tolapAnalysis">The <see cref="TOLAPAnalysis"/> whose cube to delete</param>
        private void DoDeleteCube(TOLAPAnalysis tolapAnalysis)
        {
            _currentDataSetConfig.CubeFiles.Remove(_currentCubeConfig);

            _userConfiguration.Save(Settings.Default.UserConfigurationFile);

            UiAction(() =>
            {
                if (tolapAnalysis.Cube == null) return;

                tolapAnalysis.Cube.Active = false;
            });

            _isCubeLoaded = false;

            _currentCubeConfig = null;

            DoLoadDataSetMenu();
        }
    }
}