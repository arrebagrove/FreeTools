using FreeTools.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using RelayCommand = GalaSoft.MvvmLight.Command.RelayCommand;

namespace FreeTools.ViewModels
{
    public class ScannerViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly ISystemService _systemService;

        public ScannerViewModel(INavigationService navigationService, ISystemService systemService)
        {
            if (navigationService == null)
                throw new ArgumentNullException(nameof(navigationService));

            if (systemService == null)
                throw new ArgumentNullException(nameof(systemService));

            _navigationService = navigationService;
            _systemService = systemService;
        }

        private string _scannedText;
        private bool _isUrl;
        private RelayCommand _launchUrlCommand, _cancelCommand, _goToMainCommand;

        public string ScannedText
        {
            get { return _scannedText; }
            set
            {
                DispatcherHelper.RunAsync(() =>
                {
                    if (!Set(nameof(ScannedText), ref _scannedText, value)) return;

                    var nullOrWhiteSpace = string.IsNullOrWhiteSpace(value);

                    this.IsUrl = !nullOrWhiteSpace
                        ? Regex.IsMatch(value, @"[\:]\/{2}")
                        : false;

                    MessengerInstance.Send(!nullOrWhiteSpace);
                });
            }
        }

        public bool IsUrl
        {
            get { return _isUrl; }
            set { Set(nameof(IsUrl), ref _isUrl, value); }
        }

        public RelayCommand LaunchUrlCommand
        {
            get
            {
                if (_launchUrlCommand == null)
                {
                    _launchUrlCommand = new RelayCommand(() =>
                    {
                        if (!this.IsUrl) return;

                        _systemService.LaunchUri(this.ScannedText);

                        this.ScannedText = string.Empty;
                    });
                }

                return _launchUrlCommand;
            }
        }

        public RelayCommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(() =>
                    {
                        this.ScannedText = string.Empty;
                    });
                }

                return _cancelCommand;
            }
        }

        public RelayCommand GoToMainCommand
        {
            get
            {
                if (_goToMainCommand == null)
                {
                    _goToMainCommand = new RelayCommand(() =>
                    {
                        _navigationService.GoBack();
                    });
                }

                return _goToMainCommand;
            }
        }

        public RelayCommand TestCommand
        {
            get { return new RelayCommand(() => this.ScannedText = "http://microsoft.com"); }
        }

        public async Task<DeviceInformation> GetCameraIdAsync(Panel panel) => await _systemService.GetCameraIdAsync(panel);
    }
}