using System.Collections.Generic;
using System.Net;
using SharpPcap;

namespace FcomClient.FsdDetection
{
	/// <summary>
	/// Helper class for managing network connections.
	/// </summary>
	class ConnectionManager
	{
		public List<HardwareDevice> Connections { get; set; }

		public ConnectionManager()
		{
			Connections = new List<HardwareDevice>();
			Filter();
		}

		/// <summary>
		/// Helper method for filtering out irrelevant connections.
		/// </summary>
		private void Filter()
		{
			CaptureDeviceList devices = CaptureDeviceList.Instance;

			// Get list of IPs with which we can use to filter out some of the detected connections
			IPAddress[] localIps = Dns.GetHostAddresses(Dns.GetHostName());

			// Filter out connections
			int candidate = -1;
			int i = 0;  // loop counter
			foreach (ICaptureDevice dev in devices)
			{
				string deviceDescription = dev.ToString();
				HardwareDevice h = new HardwareDevice(dev);
				h.Device = dev;
				if (h.IpAddresses.Count > 0)	// ignore connections with no IP addresses
				{
					Connections.Add(new HardwareDevice(dev));

					// if it turns out there's only one candidate,
					// then we can directly select it without asking the user.
					candidate = i;

				}
				i++;
			}
		}

		// TODO
		public ICaptureDevice GetPotentialConnections()
		{
			return null;
		}
	}
}
