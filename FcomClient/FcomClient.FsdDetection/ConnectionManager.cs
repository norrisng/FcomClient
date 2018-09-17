using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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

					///* This just shows the user the detalis of the connection */
					///* TODO: remove this at some point */
					//Console.WriteLine("(" + i + ")");
					//Console.WriteLine("------");
					//Console.WriteLine("Name: {0}\nDescription: {1}", h.FriendlyName, h.Description);
					//Console.WriteLine("IP addresses:");
					//foreach (string s in h.IpAddresses)
					//{
					//	// print detected IPs
					//	Console.WriteLine("\t" + s);
					//}
					///* End */

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
