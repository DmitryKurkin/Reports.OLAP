namespace Reporting.PresentationLogic.WpfApplication.ViewModel
{
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Ioc;
    using GalaSoft.MvvmLight.Views;

    using Microsoft.Practices.ServiceLocation;

    using Reporting.BusinessLogic;

    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            //SimpleIoc.Default.Register<IConnectionStringProvider, SettingsConnectionStringProvider>();

            //if (ViewModelBase.IsInDesignModeStatic)
            //{
            //    SimpleIoc.Default.Register<IClientService, Design.DesignClientService>();
            //}
            //else
            //{
            //    SimpleIoc.Default.Register<IClientService, NHibernateClientService>();
            //}

            //SimpleIoc.Default.Register<IDefaultsService, NHibernateDefaultsService>();
            //SimpleIoc.Default.Register<IShiftService, NHibernateShiftService>();
            //SimpleIoc.Default.Register<IPlaceGroupService, NHibernatePlaceGroupService>();
            //SimpleIoc.Default.Register<IEventService, NHibernateEventService>();
            //SimpleIoc.Default.Register<IModifierGroupService, NHibernateModifierGroupService>();
            //SimpleIoc.Default.Register<IExchangeRuleService, NHibernateExchangeRuleService>();
            //SimpleIoc.Default.Register<IItemService, NHibernateItemService>();
            //SimpleIoc.Default.Register<IBillService, NHibernateBillService>();
            //SimpleIoc.Default.Register<IStreamService, NHibernateStreamService>();
            //SimpleIoc.Default.Register<IPriceService, NHibernatePriceService>();
            //SimpleIoc.Default.Register<IStationService, NHibernateStationService>();
            //SimpleIoc.Default.Register<IIdentService, NHibernateIdentService>();
            //SimpleIoc.Default.Register<IBillPrintService, NHibernateBillPrintService>();

            //SimpleIoc.Default.Register<IWorkingWeekInfo, SettingsWorkingWeekInfo>();

            //SimpleIoc.Default.Register<ClientManager>();
            //SimpleIoc.Default.Register<ShiftManager>();
            //SimpleIoc.Default.Register<DiningRoomManager>();
            //SimpleIoc.Default.Register<LunchManager>();
            //SimpleIoc.Default.Register<ProductManager>();
            //SimpleIoc.Default.Register<OrderManager>();
            //SimpleIoc.Default.Register<StationManager>();
            //SimpleIoc.Default.Register<AuthenticationManager>();
            //SimpleIoc.Default.Register<PrintManager>();

            //SimpleIoc.Default.Register<INavigationService, NavigationService>();
            //SimpleIoc.Default.Register<IDialogService, XceedDialogService>();

            //SimpleIoc.Default.Register<LocalizationViewModel>();
            //SimpleIoc.Default.Register<LoginViewModel>();
            //SimpleIoc.Default.Register<ClientLogoutViewModel>(true);
            //SimpleIoc.Default.Register<OrdersPlanViewModel>(true);
            //SimpleIoc.Default.Register<OrderDetailsViewModel>(true);
            //SimpleIoc.Default.Register<ItemSelectorViewModel>(true);
            //SimpleIoc.Default.Register<OfflineViewModel>();

            //SimpleIoc.Default.Register<ViewModelBase>(() => SimpleIoc.Default.GetInstance<LoginViewModel>(),
            //    ViewConstants.Login);
            //SimpleIoc.Default.Register<ViewModelBase>(() => SimpleIoc.Default.GetInstance<OrdersPlanViewModel>(),
            //    ViewConstants.OrdersPlan);
            //SimpleIoc.Default.Register<ViewModelBase>(() => SimpleIoc.Default.GetInstance<ItemSelectorViewModel>(),
            //    ViewConstants.ItemSelector);
            //SimpleIoc.Default.Register<ViewModelBase>(() => SimpleIoc.Default.GetInstance<OfflineViewModel>(),
            //    ViewConstants.Offline);

            SimpleIoc.Default.Register<IDatabaseBridge, OdbcDatabaseBridge>();
            SimpleIoc.Default.Register<IExternalFilterProvider, PrimitiveExternalFilterProvider>();

            SimpleIoc.Default.Register<MainViewModel>();
        }

        public MainViewModel MainVm => ServiceLocator.Current.GetInstance<MainViewModel>();

        public static void Cleanup()
        {
        }
    }
}