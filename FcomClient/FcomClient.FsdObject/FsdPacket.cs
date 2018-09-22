using System;

namespace FcomClient.FsdObject
{
	/// <summary>
	/// Representation of a FSD packet.
	/// </summary>
	class FsdPacket
	{

		/// <summary>
		/// The time at which the packet was received.
		/// </summary>
		public DateTime Timestamp { get; set; }

		/// <summary>
		/// The original packet, minus the TCP headers.
		/// </summary>
		public string PacketString { get; set; }

		/// <summary>
		/// The callsign of the station sending the packet.
		/// </summary>
		public string Sender { get; set; }

		/// <summary>
		/// The callsign of the station recieving the packet.
		/// </summary>
		public string Recipient { get; set; }

		/// <summary>
		/// Initializes an instance of FsdPacket.
		/// </summary>
		/// <param name="timestamp">The time at which the packet was received.</param>
		/// <param name="packetString">The FSD packet; e.g. "#TM..."</param>
		public FsdPacket(DateTime timestamp, string packetString)
		{
			this.PacketString = packetString;
			this.Timestamp = timestamp;
		}

		// TODO: create constructor that accepts sender/recipient as parameter

		/// <summary>
		/// Constructor for a FsdPacket object.
		/// </summary>
		/// <param name="timestamp">
		/// The time at which the packet was received.
		/// </param>
		/// <param name="packetString">
		/// The raw IPv4 packet, as captured, including the header.
		/// </param>
		public FsdPacket(DateTime timestamp, byte[] packetString)
		{
			int TCP_HEADER_SIZE = 54;
			this.Timestamp = timestamp;

			// first, chop off the header
			byte[] noHeader = new byte[packetString.Length - TCP_HEADER_SIZE];
			Buffer.BlockCopy(packetString, TCP_HEADER_SIZE, noHeader, 0, noHeader.Length);

			// then, convert to string and chop off the trailing newline
			this.PacketString = System.Text.Encoding.UTF8.GetString(noHeader).Trim();

			// raise event if PM
			if (this.IsPrivateMessage() && OnMessageArrival != null)
				OnMessageArrival();
		}

		public bool IsPrivateMessage()
		{
			if (this.PacketString == null || this.PacketString.Length < 4)
				return false;
			else if (this.PacketString.Substring(0, 3).Equals("#TM"))
				return true;
			else
				return false;
		}

		public void GetPayload()
		{

		}

		public delegate void msgEventRaiser();

		public event msgEventRaiser OnMessageArrival;

	}
}
