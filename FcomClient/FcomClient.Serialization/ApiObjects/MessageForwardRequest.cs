using System.Collections.Generic;
using RestSharp.Serializers;

namespace FcomClient.Serialization.ApiObjects
{
	/// <summary>
	/// Represents a message forwarding request.
	/// </summary>
	class MessageForwardRequest
	{
		[SerializeAs(Name ="token")]
		public string Token { get; set; }

		[SerializeAs(Name = "discord_id")]
		public long DiscordId { get; set; }

		[SerializeAs(Name = "messages")]
		public List<TextMessage> Messages { get; set; }

		public MessageForwardRequest(string token, long discordId)
		{
			this.Token = token;
			this.DiscordId = discordId;
		}

		public void AppendMessage()
		{

		}
	}

	class TextMessage {

		[SerializeAs(Name = "timestamp")]
		public string Timestamp { get; set; }

		[SerializeAs(Name = "sender")]
		public string Sender { get; set; }

		[SerializeAs(Name = "recipient")]
		public string Recipient { get; set; }

		[SerializeAs(Name = "message")]
		public string Message { get; set; }
	}

}
