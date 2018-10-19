using RestSharp.Deserializers;

namespace FcomClient.Serialization.ApiObjects
{
	/// <summary>
	/// Represents API response containing the server registration.
	/// </summary>
	class ServerRegistrationResponse
	{
		[DeserializeAs(Name = "token")]
		public string Token { get; set; }

		[DeserializeAs(Name = "callsign")]
		public string Callsign { get; set; }

		[DeserializeAs(Name = "discord_id")]
		public long DiscordId { get; set; }

		[DeserializeAs(Name = "discord_name")]
		public string DiscordName { get; set; }

	}
}
