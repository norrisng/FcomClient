using FcomClient.FsdDetection;
using System;
using System.Collections.Generic;
using SharpPcap;
using FcomClient.Serialization;
using FcomClient.FsdObject;

namespace FcomClient.UI
{
	class FcomClient
	{
		static string callsign = "";
		static ApiManager am;

		static void Main(string[] args)
		{
			bool isRegistered = false;

			while (!isRegistered)
			{
				Console.Write("\nPlease enter your exact callsign, then press Enter: ");
				callsign = Console.ReadLine();

				Console.Write("\nPlease enter the verification code from Discord, then press Enter:\n");
				string token = Console.ReadLine();

				Console.WriteLine("\nRegistering token with Discord bot...");
				am = new ApiManager(token, callsign);
				
				if (am.DiscordId != null)
				{
					Console.WriteLine("Registered {0} to Discord user {1} ({2})", callsign, am.DiscordName, am.DiscordId);
					isRegistered = true;
				}
				else
				{
					Console.WriteLine("Could not register!");
				}
			}		

			Console.Write("\nDetecting connections...\n\n");
			ConnectionManager cm = new ConnectionManager();
			List<HardwareDevice> connections = cm.Connections;

			// TODO: completely decouple SharpPcap from the UI

			ICaptureDevice device;

			if (connections.Count == 1)
			// Auto-select the only connection available
			{
				device = connections[0].Device;
			}
			else
			// Otherwise, show all available connections to the user,
			// and ask for the correct one
			{
				int i = 0;
				foreach (HardwareDevice h in connections)
				{
					Console.WriteLine("({0})", i);
					Console.WriteLine("------");
					Console.WriteLine("Name: {0}\nDescription: {1}", h.FriendlyName, h.Description);
					Console.WriteLine("IP addresses:");
					foreach (string s in h.IpAddresses)
					{
						// print detected IPs
						Console.WriteLine("\t" + s);
					}

					i++;
				}				

				int deviceNumber = -1;
				Console.WriteLine("\nWhich of the above is your internet connection?");

				// Ignore invalid inputs
				while (deviceNumber < 0 || deviceNumber >= connections.Count)
				{
					Console.Write("Enter the corresponding number: ");
					Int32.TryParse(Console.ReadLine(), out deviceNumber);
				}

				device = connections[deviceNumber].Device;

			}

			// force a wait so that the user can see what's happening
			System.Threading.Thread.Sleep(700);

			// register event handler
			device.OnPacketArrival +=
				new SharpPcap.PacketArrivalEventHandler(OnIncomingFsdPacket);

			// open device for capturing
			int readTimeoutMilliseconds = 2000;
			device.Open(DeviceMode.Normal, readTimeoutMilliseconds);

			// set filter to tcp port 6809
			device.Filter = "tcp port 6809";

			Console.WriteLine("\nYou may now minimize this window. To quit, simply close it.");
			Console.WriteLine("When you're done, close this window and send \"remove\" to the Discord bot!\n\n");

			// Start capturing packets indefinitely
			device.Capture();

			// note: this line is uncreachable, since we're capturing indefinitely
			device.Close();
		}

		/// <summary>
		/// Event handler for incoming FSD packets.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		static void OnIncomingFsdPacket(object sender, CaptureEventArgs e)
		{			
			DateTime timestamp = DateTime.Now;

			// First, create a FsdPacket object from the packet
			FsdPacket pkt = new FsdPacket(timestamp, e.Packet.Data);			

			// FsdPacket trims the newline, so we have to grab the byte[] ourselves
			string pktString = System.Text.Encoding.UTF8.GetString(e.Packet.Data);

			// Only do something if it's a PM
			if (/*pkt.PacketString.EndsWith("\n") && */pkt.PacketString.StartsWith("#TM"))
			{
				FsdMessage pm = new FsdMessage(timestamp, pkt.PacketString);

				// ignore certain messages:

				// this includes under-the-hood ones to SERVER/FP/DATA...
				bool isServerMessage =
					string.Equals(pm.Sender, "server", StringComparison.OrdinalIgnoreCase) ||
					string.Equals(pm.Recipient, "server", StringComparison.OrdinalIgnoreCase) ||
					string.Equals(pm.Recipient, "fp", StringComparison.OrdinalIgnoreCase) ||
					string.Equals(pm.Recipient, "data", StringComparison.OrdinalIgnoreCase)
					;

				// private/frequency messages not addressed to the user...
				bool isAddressedToUser = pm.Message.StartsWith(callsign, StringComparison.OrdinalIgnoreCase) ||
										string.Equals(pm.Recipient, callsign, StringComparison.OrdinalIgnoreCase);

				// and self-addressed messages:
				bool isSelfMessage = string.Equals(pm.Sender, callsign, StringComparison.OrdinalIgnoreCase);

				// putting all of the above conditions together:
				if (!isServerMessage && isAddressedToUser && !isSelfMessage)
				{
					Console.WriteLine("{0} ({1}): \n{2}",
										pm.Sender, pm.Timestamp.ToLongTimeString(), pm.Message);
					am.ForwardMessage(pm);

					// Do not forward messages sent over the frequency, that aren't addressed to the user
					//if (pm.Message.StartsWith(callsign))
					//	am.ForwardMessage(pm);
				}				

			}



		}

	}
}
