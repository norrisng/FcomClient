using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FcomClient.FsdObject
{
	class FsdMessage : FsdPacket
	{
		/// <summary>
		/// Contents of the message.
		/// </summary>
		public string Message { get; }

		public FsdMessage(DateTime timestamp, string packetString) : base(timestamp, packetString)
		{
			string[] contents = packetString.Split(':');

			// Parse message fields.
			// Format: #TM<sender>:<recipient>:<message>
			for (int i = 0; i < contents.Length; i++)
			{
				switch (i)
				{
					case 0:     // sender						
						base.Sender = contents[0].Substring(3);
						break;
					case 1:     // recipient
						// if it's a frequency message, it'll be addressed to "@xxyyy" (i.e. 1xx.yyy MHz)
						base.Recipient = contents[1].Replace("@", "1");
						break;
					case 2:     // message contents
						this.Message = contents[2];
						break;
					default:    // message contents, if message contains colons
						this.Message += ":" + contents[i];
						break;
				}
			}
		}

	}
}
