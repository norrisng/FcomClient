using System.Collections.Generic;

namespace FcomClient.Serialization.ApiObjects
{
	public class ForwardedMessage
	{
		public string token { get; set; }
		public List<Message> messages { get; set; }
	}

	public class Message
	{
		public string timestamp { get; set; }
		public string sender { get; set; }
		public string receiver { get; set; }
		public string message { get; set; }
	}

}
