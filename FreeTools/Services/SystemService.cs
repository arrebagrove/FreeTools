using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Enumeration;
using Windows.System;

namespace FreeTools.Services
{
    public class SystemService : ISystemService
    {
        public string GetPackageName() => "Free Tools";

        public string GetPackageVersion() => $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";

        public async void LaunchUri(string url) => await Launcher.LaunchUriAsync(new Uri(url));

        public async Task<DeviceInformation> GetCameraIdAsync(Panel desiredCamera)
        {
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            var deviceId = devices.FirstOrDefault(device => device.EnclosureLocation != null && device.EnclosureLocation.Panel == desiredCamera);

            if (deviceId != null)
                return deviceId;

            throw new ArgumentException(string.Format("Camera {0} doesn't exist", desiredCamera));
        }
    }
}
