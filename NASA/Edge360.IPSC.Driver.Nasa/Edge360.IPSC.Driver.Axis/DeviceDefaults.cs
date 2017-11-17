using System;
using System.Linq;
using CNL.IPSecurityCenter.Driver;

namespace Edge360.IPSC.Driver.Axis
{
    /// <summary>
    /// Provides default values for the device class.
    /// </summary>
    internal static class DeviceDefaults
    {
        /// <summary>
        /// Returns the default port.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static int DefaultPort(INetworkedDevice device)
        {
            const int port = 80;

            return device.Port != 0 ? device.Port : port;

            //TODO: Set the default port
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the default username.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static string DefaultUsername(ISecureDevice device)
        {
            const string username = "";

            return !string.IsNullOrEmpty(device.Username) ? device.Username : username;

            //TODO: Set the default username
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the default password.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static string DefaultPassword(ISecureDevice device)
        {
            const string password = "";

            //Check the device username to allow for blank passwords
            return !string.IsNullOrEmpty(device.Username) ? device.Password : password;

            //TODO: Set the default password
            //throw new NotImplementedException();
        }
    }
}
