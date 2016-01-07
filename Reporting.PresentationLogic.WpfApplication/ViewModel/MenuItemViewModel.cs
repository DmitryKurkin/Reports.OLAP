namespace Reporting.PresentationLogic.WpfApplication.ViewModel
{
    using System.Collections.Generic;
    using System.Windows.Data;
    using System.Windows.Input;

    using OrdersPlanApp.MvvmLight.ViewModel;

    /// <summary>
    /// Models a menu item
    /// </summary>
    public class MenuItemViewModel : NotifyDataErrorInfoViewModelBase
    {
        /// <summary>
        /// The header of the menu item
        /// </summary>
        private string _header;

        /// <summary>
        /// The command the menu item is bound to
        /// </summary>
        private ICommand _command;

        /// <summary>
        /// Gets or sets the header of the menu item
        /// </summary>
        public string Header
        {
            get { return _header; }
            set
            {
                if (_header == value) return;

                _header = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the command the menu item is bound to
        /// </summary>
        public ICommand Command
        {
            get { return _command; }
            set
            {
                if (_command == value) return;

                _command = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets the collection of the menu item's subitems
        /// </summary>
        public ListCollectionView Subitems { get; } = new ListCollectionView(new List<MenuItemViewModel>());
    }
}