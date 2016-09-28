using FreeTools.Helpers;
using FreeTools.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
using Windows.Storage;

namespace FreeTools.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly ISystemService _systemService;

        public MainViewModel(INavigationService navigationService, ISystemService systemService)
        {
            if (navigationService == null)
                throw new ArgumentNullException(nameof(navigationService));

            if (systemService == null)
                throw new ArgumentNullException(nameof(systemService));

            _navigationService = navigationService;
            _systemService = systemService;

            UpdateTorchStatus(false);
        }

        private MediaCapture _mediaCapture;
        private TorchControl _torchControl;

        private bool _backgroundMediaPlayerActive, _torchSupported, _flashTurnedOn;
        private double _flashPowerValue;
        private RelayCommand _stopBackgroundMediaPlayerCommand, _goToScannerCommand;

        public string PackageName
        {
            get { return _systemService.GetPackageName().ToUpper(); }
        }

        public bool FlashPowerSupported
        {
            get { return this.TorchSupported && this.FlashTurnedOn && _torchControl.PowerSupported; }
        }

        public bool BackgroundMediaPlayerActive
        {
            get { return _backgroundMediaPlayerActive; }
            private set { Set(nameof(BackgroundMediaPlayerActive), ref _backgroundMediaPlayerActive, value); }
        }

        public bool TorchSupported
        {
            get { return _torchSupported; }
            private set { Set(nameof(TorchSupported), ref _torchSupported, value); }
        }

        public bool FlashTurnedOn
        {
            get { return _flashTurnedOn; }
            set
            {
                if (!Set(nameof(FlashTurnedOn), ref _flashTurnedOn, value)) return;

                UpdateTorchStatus(value);
            }
        }

        public double FlashPowerValue
        {
            get { return _flashPowerValue; }
            set
            {
                if (!Set(nameof(FlashPowerValue), ref _flashPowerValue, value)) return;

                if (_torchControl == null || !this.TorchSupported || !_torchControl.Enabled || !_torchControl.PowerSupported) return;

                _torchControl.PowerPercent = Convert.ToSingle(value);

                SettingsHelper.AddOrUpdateValue("flashPowerValue", value);
            }
        }

        public RelayCommand StopBackgroundMediaPlayerCommand
        {
            get
            {
                if (_stopBackgroundMediaPlayerCommand == null)
                {
                    _stopBackgroundMediaPlayerCommand = new RelayCommand(() =>
                    {
                        try
                        {
                            BackgroundMediaPlayer.Current.Pause();

                            BackgroundMediaPlayer.Shutdown();

                            this.BackgroundMediaPlayerActive = false;
                        }
                        catch (Exception exception)
                        {
                            System.Diagnostics.Debug.WriteLine($"{nameof(StopBackgroundMediaPlayerCommand)}: {exception.Message}");
                        }
                    });
                }

                return _stopBackgroundMediaPlayerCommand;
            }
        }

        public RelayCommand GoToScannerCommand
        {
            get
            {
                if (_goToScannerCommand == null)
                {
                    _goToScannerCommand = new RelayCommand(() => 
                    {
                        this.FlashTurnedOn = false;

                        _navigationService.NavigateTo(nameof(ViewModelLocator.Scanner));
                    });
                }

                return _goToScannerCommand;
            }
        }

        private async void UpdateTorchStatus(bool isOn)
        {
            try
            {
                await InitTorchControl();
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"{nameof(UpdateTorchStatus)}: {exception.Message}");
            }

            if (!isOn && _torchControl.Enabled)
            {
                _torchControl.Enabled = false;
                _mediaCapture.Dispose();

                return;
            }

            if (isOn && !_torchControl.Enabled)
            {
                var videoEncodingProperties = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Vga);
                var videoStorageFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync("temp.mp4", CreationCollisionOption.ReplaceExisting);

                await _mediaCapture.StartRecordToStorageFileAsync(videoEncodingProperties, videoStorageFile);

                _torchControl.Enabled = true;
            }
        }

        private async Task<bool> InitTorchControl()
        {
            var cameraId = await _systemService.GetCameraIdAsync(Panel.Back);

            _mediaCapture?.Dispose();

            _mediaCapture = new MediaCapture();

            await _mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
            {
                VideoDeviceId = cameraId.Id
            });

            var videoDeviceController = _mediaCapture.VideoDeviceController;

            _torchControl = videoDeviceController.TorchControl;

            this.TorchSupported = _torchControl.Supported;

            if (this.TorchSupported && _torchControl.PowerSupported)
            {
                this.FlashPowerValue = SettingsHelper.GetValueOrDefault("flashPowerValue", 0.5);
            }

            RaisePropertyChanged(nameof(FlashPowerSupported));

            return true;
        }

        public void UpdateBackgroundPlayerStatus()
        {
            this.BackgroundMediaPlayerActive = BackgroundMediaPlayer.IsMediaPlaying();
        }
    }
}