using FreeTools.Services;
using GalaSoft.MvvmLight;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using RelayCommand = GalaSoft.MvvmLight.Command.RelayCommand;

namespace FreeTools.ViewModels
{
    public class ScannerViewModel : ViewModelBase
    {
        private readonly ISystemService _systemService;

        public ScannerViewModel(ISystemService systemService)
        {
            if (systemService == null)
                throw new ArgumentNullException(nameof(systemService));

            _systemService = systemService;
        }

        private string _scannedText;
        private bool _isUrl;
        private RelayCommand _launchUrlCommand;

        public string ScannedText
        {
            get { return _scannedText; }
            set
            {
                if (!Set(nameof(ScannedText), ref _scannedText, value)) return;

                this.IsUrl = Regex.IsMatch(value, @"[\:]\/{2}");
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
                    });
                }

                return _launchUrlCommand;
            }
        }

        public async Task<DeviceInformation> GetCameraIdAsync(Panel panel) => await _systemService.GetCameraIdAsync(panel);
    }
}