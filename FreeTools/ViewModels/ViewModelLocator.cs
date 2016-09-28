using FreeTools.Services;
using FreeTools.Views;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;

namespace FreeTools.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register(GetNavigationService);
            SimpleIoc.Default.Register<ISystemService, SystemService>();

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<ScannerViewModel>();
        }

        private INavigationService GetNavigationService()
        {
            var navigationService = new NavigationService();

            navigationService.Configure(nameof(Main), typeof(MainPage));
            navigationService.Configure(nameof(Scanner), typeof(ScannerPage));

            return navigationService;
        }

        public MainViewModel Main
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }

        public ScannerViewModel Scanner
        {
            get { return ServiceLocator.Current.GetInstance<ScannerViewModel>(); }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
