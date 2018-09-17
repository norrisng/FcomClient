using SharpPcap;
using System;
using System.Collections.Generic;
using System.Net;

namespace FcomClient.FsdDetection
{
	/// <summary>
	/// A wrapper for SharpPcap's ICaptureDevice.
	/// </summary>
	class HardwareDevice
	{
		/// <summary>
		/// User-recognizable name of the device.
		/// This name is also used in the Windows Control Panel.
		/// (under Network and Internet > Network Connection)
		/// </summary>
		public string FriendlyName { get; }

		/// <summary>
		/// A brief description of the device.
		/// Tends to be prety verbose, however.
		/// (e.g. "Network Adapter 'Microsoft' on local host")
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// MAC address of the hardware device.
		/// </summary>
		public string MacAddress { get; }

		/// <summary>
		/// List of IP addresses associated with the hardware device.
		/// </summary>
		public List<String> IpAddresses { get; }

		/// <summary>
		/// The SharpPcap object representing the hardware device.
		/// </summary>
		public SharpPcap.ICaptureDevice Device { get; set; }

		/// <summary>
		/// Creates a HardwareDevice object based on its SharpPcap representation.
		/// </summary>
		/// <param name="dev"></param>
		public HardwareDevice(ICaptureDevice dev)
		{
			this.Device = dev;
			string input = dev.ToString();
			IpAddresses = new List<String>();

			List<String> detectedAddrs = new List<String>();
			IPAddress[] localIps = Dns.GetHostAddresses(Dns.GetHostName());

			foreach (IPAddress ip in localIps)
			{
				string addr = ip.ToString();

				// scrub out the %xxx at the end (if present)
				int percentSignPos = addr.IndexOf("%");
				if (percentSignPos > 0)
					addr = addr.Substring(0, percentSignPos);

				detectedAddrs.Add(addr);
			}

			// break everything up so we can parse line-by-line
			string[] deviceFields = input.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string line in deviceFields)
			{
				if (line.StartsWith("FriendlyName"))
				{
					FriendlyName = line.Replace("FriendlyName: ", "");
				}
				else if (line.StartsWith("Description"))
				{
					Description = line.Replace("Description: ", "");
				}
				/*	IP or MAC address
					Only add if the IP address here exists in the detected list
					(that is, the list returned by Dns.GetHostAddresses() )
				*/
				else if (line.StartsWith("Addr:"))
				{
					string detectedIp = line.Replace("Addr:      ", "");

					// MAC address
					if (detectedIp.Contains("HW addr: "))
						MacAddress = detectedIp.Replace("HW addr: ", "");

					// IP address
					if (detectedAddrs.Contains(detectedIp))
						IpAddresses.Add(detectedIp);
				}

			}

		}

	}
}
