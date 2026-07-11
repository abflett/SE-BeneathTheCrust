using System;
using System.Text;
using Sandbox.ModAPI;
using VRage.Utils;

namespace WkKn
{
    internal static class XmlNetworkSerializer
    {
        internal static byte[] Serialize<TMessage>(TMessage message)
            where TMessage : class
        {
            if (message == null || MyAPIGateway.Utilities == null)
                return new byte[0];

            return Encoding.UTF8.GetBytes(MyAPIGateway.Utilities.SerializeToXML(message));
        }

        internal static bool TryDeserialize<TMessage>(byte[] messageBytes, string logDescription, out TMessage message)
            where TMessage : class
        {
            message = null;
            if (messageBytes == null || messageBytes.Length == 0 || MyAPIGateway.Utilities == null)
                return false;

            try
            {
                var xml = Encoding.UTF8.GetString(messageBytes);
                message = MyAPIGateway.Utilities.SerializeFromXML<TMessage>(xml);
                return message != null;
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLineAndConsole(
                    WkKnSession.LogPrefix + " failed to read " + logDescription + ": " + exception.Message);
                return false;
            }
        }
    }
}
