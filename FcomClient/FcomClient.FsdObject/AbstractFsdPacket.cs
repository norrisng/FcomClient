using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FcomClient.FsdObject
{
	abstract class AbstractFsdPacket
	{
		/// <summary>
		/// The time at which the packet was received.
		/// </summary>
		public abstract DateTime Timestamp { get; set; }

		/// <summary>
		/// The original packet, minus the TCP headers.
		/// </summary>
		public abstract string PacketString { get; set; }

		/// <summary>
		/// The callsign of the station sending the packet.
		/// </summary>
		public abstract string Sender { get; set; }

		/// <summary>
		/// The callsign of the station recieving the packet.
		/// </summary>
		public abstract string Recipient { get; set; }
	}
}
