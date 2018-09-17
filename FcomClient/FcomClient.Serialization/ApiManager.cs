using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FcomClient.Serialization.ApiObjects;
using RestSharp;
using RestSharp.Deserializers;

namespace FcomClient.Serialization
{
	/// <summary>
	/// A wrapper for the FCOM server API.
	/// </summary>
	class ApiManager
	{
		/// <summary>
		/// Server address, read from the file server_location.txt
		/// </summary>
		private readonly string SERVER_ADDRESS = System.IO.File.ReadAllText("server_location.txt");

		/// <summary>
		/// API endpoint for submitting messages to be forwarded.
		/// </summary>
		private readonly string MESSAGE_FORWARDING_ENDPOINT = "/api/v1/messaging";

		/// <summary>
		/// API endpoint for registering/confirming a token.
		/// </summary>
		private readonly string REGISTRATION_ENDPOINT = "/api/v1/register";

		/// <summary>
		/// API registration token, provided by the bot and entered into FcomClient by the user.
		/// </summary>
		public string Token { get; set; }

		/// <summary>
		/// The online callsign the user is logged in as.
		/// </summary>
		public string Callsign;

		/// <summary>
		/// ID number of the Discord user associated with the token.
		/// </summary>
		public long DiscordId { get; }

		/// <summary>
		/// Display name of the Discord user associated with the token.
		/// </summary>
		public string DiscordName { get; }

		/// <summary>
		/// Internal object for interacting with the FCOM server API.
		/// </summary>
		private RestClient client;

		/// <summary>
		/// Initializes the API manager, 
		/// passing the given token to the server for confirmation.
		/// </summary>
		public ApiManager(string token, string callsign)
		{
			client = new RestClient(SERVER_ADDRESS);
			RestRequest registerRequest 
				= new RestRequest(REGISTRATION_ENDPOINT, Method.GET);

			registerRequest.AddParameter("token", token);
			registerRequest.AddParameter("callsign", callsign);

			registerRequest.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

			//IRestResponse<ServerRegistrationResponse> response 
			//	= client.Execute<ServerRegistrationResponse>(registerRequest);
			var response = client.Execute<ServerRegistrationResponse>(registerRequest);

			if (response.IsSuccessful)
			{
				var deserializer = new JsonDeserializer();
				ServerRegistrationResponse returnPayload = deserializer.Deserialize<ServerRegistrationResponse>(response);

				this.Callsign = returnPayload.Callsign;
				this.DiscordId = returnPayload.DiscordId;
				this.DiscordName = returnPayload.DiscordName;
				this.Token = returnPayload.Token;
			}
			else
			{
				// do nothing
			}

		}

		/// <summary>
		/// Forwards the given FSD message to the FCOM server.
		/// </summary>
		/// <param name="pm">Message to forward.</param>
		public void ForwardMessage(FsdObject.FsdMessage pm)
		{
			// convert timestamp to Unix time
			DateTimeOffset dateTimeOffset = new DateTimeOffset(pm.Timestamp);
			long unixTimestamp = dateTimeOffset.ToUnixTimeMilliseconds();

			/* Sample JSON (after server rewrite):
			   NOTE: currently, the API does not parse beyond the first message.

			{
				"token": 	"Wjve5p45aTojv6yzRr72FKs9K1py8ze2auFbB8g328o",
				"messages":	[
					{
						"timestamp": "2018-05-01 10:40:00 PM",
						"sender": "XX_SUP",
						"receiver": "DAL1107",
						"message": "IMMA SWING ZE MIGHTY BAN-HAMMER MUAHAHAHAHA"
					}
				]
			}

			 */

			Console.Write(" -- Forwarding to {0}...", SERVER_ADDRESS);

			// Build the API request
			var request = new RestRequest(MESSAGE_FORWARDING_ENDPOINT, Method.POST)
			{
				RequestFormat = DataFormat.Json
			};

			// Build the JSON object

			var messagePayload = new Message
			{
				timestamp = unixTimestamp.ToString(),
				sender = pm.Sender,
				receiver = pm.Recipient,
				message = pm.Message,
			};

			/* The API accepts a list of messages, but currently only processes the first in the list*/
			var messageList = new List<Message>
			{
				messagePayload
			};

			request.AddBody(new ForwardedMessage
			{
				token = this.Token,
				messages = messageList
			});

			IRestResponse response = client.Execute(request);
			var content = response.Content;

			if (response.IsSuccessful)
				Console.WriteLine("Success!");
			else
				Console.WriteLine("Fail!");

		}
	}
}
