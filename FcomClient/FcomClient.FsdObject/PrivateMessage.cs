using System;

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
