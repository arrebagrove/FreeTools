using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace FreeTools.Services
{
    public interface ISystemService
    {
        string GetPackageName();

        string GetPackageVersion();

        void LaunchUri(string url);

        Task<DeviceInformation> GetCameraIdAsync(Panel desiredCamera);
    }
}
