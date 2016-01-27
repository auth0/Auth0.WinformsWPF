using System;

namespace Auth0.Windows
{
    public class DeviceIdProvider : IDeviceIdProvider
    {
        public string GetDeviceId()
        {
            return Environment.MachineName;
        }
    }
}
