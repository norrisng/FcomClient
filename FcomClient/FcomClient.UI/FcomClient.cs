using FcomClient.FsdDetection;
using FcomClient.Diagnostics;
using System;
using System.Collections.Generic;
using SharpPcap;
using FcomClient.Serialization;
using FcomClient.FsdObject;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;       // for hiding the console window
using System.IO.Pipes;

namespace FcomClient.UI
{
	class FcomClient
	{
		// The following DLLImports are for hiding the console window
		[DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();
		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		static string callsign = "";
		static ApiManager am;
		static Logger logger = new Logger();    // log.txt

		static NamedPipeClientStream namedPipeClient;   // named pipe to FcomGui
		static readonly string FCOM_GUI_PIPE = "ca.norrisng.fcom";

		/// <summary>
		/// Main function
		/// 
		/// </summary>
		/// <param name="args">
		///		(Optional)
		///		Callsign (args[0]) and verification code (args[1]). 
		///		If not provided, the user is prompted to enter them via the commandline interface.
		/// </param>
		static void Main(string[] args)
		{
			// Callsign + verification code provided via arguments
			bool isStartedWithArgs = (args.Length == 2);

			try {
				logger.Log("Starting FcomClient...");

				bool isInputValid = false;
				Regex callsignFormat = new Regex(@"^(\d|\w|_|-)+$");

				// Callsign + verification code provided via arguments
				if (isStartedWithArgs)
				{
					// Connect to GUI via named pipe
					namedPipeClient = new NamedPipeClientStream(FCOM_GUI_PIPE);
					namedPipeClient.Connect();

					if (callsignFormat.IsMatch(args[0]))
					{
						isInputValid = true;
						callsign = args[0];

						logger.Log(String.Format("Client was started with the following arguments: {0} {1}", args[0], args[1]));

						am = new ApiManager(args[1], callsign);

						if (!am.IsRegistered)
						{
							logger.Log("[FCOM_API_ERROR] Couldn't register with API");
							SendPipeMessage(namedPipeClient, "FCOM_API_ERROR");
						}
					}
					else
					{
						// if invalid callsign format, ask the user via the command-line interface
						isInputValid = false;						
						SendPipeMessage(namedPipeClient, "FCOM_CLIENT_BAD_CALLSIGN");
					}

				}

				// ask user for callsign + verification code,
				// if not provided via args, or if args were invalid
				if (!isInputValid) {

					bool isRegistered = false;
					while (!isRegistered)
					{

						while (!isInputValid)
						{
							Console.Write("\nPlease enter your exact callsign, then press Enter: ");
							callsign = Console.ReadLine();

							if (callsignFormat.IsMatch(callsign))
								isInputValid = true;
							else
								Console.WriteLine("Invalid callsign!");
						}

						Console.Write("\nPlease enter the verification code from Discord, then press Enter:\n");
						string token = Console.ReadLine();

						logger.Log(String.Format("Callsign: \"{0}\", Token: \"{1}\"", callsign, token));
						
						Console.WriteLine("\nRegistering token with Discord bot...");						
						am = new ApiManager(token, callsign);
						logger.Log(String.Format("Register at {0}{1}", am.SERVER_ADDRESS, am.REGISTRATION_ENDPOINT));

						isRegistered = am.IsRegistered;
						if (isRegistered)
						{
							Console.WriteLine("Registered {0} to Discord user {1} ({2})", callsign, am.DiscordName, am.DiscordId);							
						}
						else
							Console.WriteLine("Couldn't register! You've either entered the incorrect code," +
												" or the server isn't responding.");
					}

				}

				Console.Write("\nDetecting connections...\n\n");
				ConnectionManager cm = new ConnectionManager();
				List<HardwareDevice> connections = cm.Connections;

				// TODO: completely decouple SharpPcap from the UI

				// Provide GUI with list of detected connections and wait for user response
				//ExternalGuiManager ext = new ExternalGuiManager();
				//ICaptureDevice device = ext.GetHardwareDevice(connections).Device;       // BLOCKING CALL

				ICaptureDevice device;

				// Auto-select the only connection available
				if (connections.Count == 1)
				{
					device = connections[0].Device;

					// No additional user input needed - close the console window if opened via GUI
					if (isStartedWithArgs)
					{
						ShowWindow(GetConsoleWindow(), 0);
						logger.Log("Hiding console window");
					}

				}
				// Otherwise, prompt the user for the correct one
				else
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

					bool parseSuccess = false;
					int deviceNumber = -1;
					Console.WriteLine("\nWhich of the above is your internet connection?");

					// Ignore invalid inputs
					while (deviceNumber < 0 || deviceNumber >= connections.Count || !parseSuccess)
					{
						Console.Write("Enter the corresponding number: ");
						parseSuccess = Int32.TryParse(Console.ReadLine(), out deviceNumber);
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

				// Hide console window
				if (isStartedWithArgs)
				{
					logger.Log("Hiding console window");
					ShowWindow(GetConsoleWindow(), 0);
				}

				Console.WriteLine("\nYou may now minimize this window. To quit, simply close it.");
				Console.WriteLine("When you're done, close this window and send \"remove\" to the Discord bot!\n\n");

				logger.Log("Starting FSD capture...");

				// Start capturing packets indefinitely
				device.Capture();

				// note: this line is uncreachable, since we're capturing indefinitely
				device.Close();
			}
			catch (Exception ex)
			{				
				// Dump stack trace to logfile
				logger.Log("[CRASH] " + ex.ToString());

				Console.WriteLine("\nFcomClient has crashed! Please see log.txt for error details.\n");

				if (args.Length == 0)
				{
					Console.WriteLine("Press any key to exit...");
					Console.ReadKey();
				}
				else if (isStartedWithArgs)
				{
					// Notify GUI of crash
					byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes("FCOM_CLIENT_CRASH");
					namedPipeClient.Write(messageBytes, 0, messageBytes.Length);
				}
			}
		}
		/// <summary>
		/// Event handler for incoming FSD packets.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		static void OnIncomingFsdPacket(object sender, CaptureEventArgs e)
		{			
			DateTime timestamp = DateTime.Now;

			// FsdPacket trims the newline, so we have to grab the byte[] ourselves
			string pktString = System.Text.Encoding.UTF8.GetString(e.Packet.Data);

			string[] inputs = pktString.Split(new String[] { "\n" }, StringSplitOptions.None);
			foreach (string line in inputs)
			{
				// Strip out the garbage that appears in between FSD packets
				string input = Regex.Replace(line, "^.*\\$", "$", RegexOptions.Multiline);
				input = Regex.Replace(input, "^.*#", "#", RegexOptions.Multiline);
				input = Regex.Replace(input, "^.*%", "%", RegexOptions.Multiline);
				input = Regex.Replace(input, "^.*@", "@", RegexOptions.Multiline);
				
				// First, create a FsdPacket object from the packet
				FsdPacket currPacket = new FsdPacket(timestamp, input);

				if (input.StartsWith("#TM"))
				{
					FsdMessage input_pm = new FsdMessage(timestamp, input);
				}
			}

			// First, create a FsdPacket object from the packet
			FsdPacket pkt = new FsdPacket(timestamp, e.Packet.Data);			

			// Only do something if it's a PM
			if (pkt.PacketString.StartsWith("#TM"))
			{
				FsdMessage pm = new FsdMessage(timestamp, pkt.PacketString);

				if (IsForwardMessage(pm))
				{
					string loggingString = String.Format("{0} > {1} ({2}):\"{3}\" ", 
														pm.Sender, 
														pm.Recipient, 
														pm.Timestamp.ToUniversalTime(), 
														pm.Message);
					Console.WriteLine(loggingString);
					logger.Log(String.Format("<{0}>", pkt.PacketString));
					logger.Log(loggingString);

					try
					{
						am.ForwardMessage(pm);
					}
					catch (FcomApiException ex)
					{
						logger.Log(ex.Message);
					}

					// Do not forward messages sent over the frequency, that aren't addressed to the user
					//if (pm.Message.StartsWith(callsign))
					//	am.ForwardMessage(pm);
				}				

			}

		}

		/// <summary>
		/// Helper method for determining if a FsdMessage should be forwarded.
		/// </summary>
		/// <param name="f">The FsdMessage in question</param>
		/// <returns>True if it should be forwarded, False otherwise</returns>
		static private bool IsForwardMessage(FsdMessage msg)
		{
			// Under-the-hood ones to SERVER/FP/DATA...
			bool isServerMessage =
				string.Equals(msg.Sender, "server", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(msg.Recipient, "server", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(msg.Recipient, "fp", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(msg.Recipient, "data", StringComparison.OrdinalIgnoreCase)
				;

			// on-frequency and private messages addressed to the user...
			
			// (NOTE: using string.StartsWith() results in partial matches (e.g. UAL1/UAL123), so use regex instead)
			// Regex: ^{callsign}( |,).*
			Regex frequencyMessagePattern = new Regex("^" + callsign + @"( |,).*", RegexOptions.IgnoreCase);
			bool isAddressedToUser = frequencyMessagePattern.IsMatch(msg.Message) ||
									string.Equals(msg.Recipient, callsign, StringComparison.OrdinalIgnoreCase);

			// self-addressed messages:
			bool isSelfMessage = string.Equals(msg.Sender, callsign, StringComparison.OrdinalIgnoreCase);

			return !isServerMessage && isAddressedToUser && !isSelfMessage;
		}

		/// <summary>
		/// Helper function for sending a message to the GUI.
		/// </summary>
		/// <param name="pipe">Connected named pipe client stream</param>
		/// <param name="message">The message to send to the GUI.</param>
		private static void SendPipeMessage(NamedPipeClientStream pipe, string message)
		{
			byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
			namedPipeClient.Write(messageBytes, 0, messageBytes.Length);
		}

	}
}
