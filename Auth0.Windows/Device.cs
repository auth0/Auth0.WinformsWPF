namespace Auth0.Windows
{
    public class Device : IDeviceIdProvider
    {
        public string GetDeviceId()
        {
            return "Windows Device";
        }
    }
}
