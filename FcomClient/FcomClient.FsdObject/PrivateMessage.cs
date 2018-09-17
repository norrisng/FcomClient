using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FcomClient.FsdObject
{
	class PrivateMessage : AbstractFsdPacket
	{
		public override DateTime Timestamp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public override string PacketString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public override string Sender { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public override string Recipient { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
	}
}
