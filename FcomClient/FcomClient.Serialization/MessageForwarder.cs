using System;
using RestSharp;
using FcomClient.FsdObject;

namespace FcomClient.Serialization
{
	class MessageForwarder
	{
		/// <summary>
		/// Server address, read from the file server_location.txt
		/// </summary>
		private readonly string SERVER_ADDRESS = System.IO.File.ReadAllText("server_location.txt");

		/// <summary>
		/// API endpoint for submitting messages to be forwarded.
		/// </summary>
		private readonly string MESSAGE_FORWARDING_ENDPOINT = "messaging";

		/// <summary>
		/// API registration token. This hasn't been implemented yet, so for now it's just the build number.
		/// </summary>
		private string TOKEN = "20180626";

		private RestClient client;

		public MessageForwarder()
		{
			client = new RestClient(SERVER_ADDRESS);
		}

		/// <summary>
		/// Uploads a message to the API for forwarding via Discord.
		/// </summary>
		/// <param name="pm"></param>
		public void UploadMessage(FsdMessage pm)
		{
			// convert timestamp to Unix time
			DateTimeOffset dateTimeOffset = new DateTimeOffset(pm.Timestamp);
			long unixTimestamp = dateTimeOffset.ToUnixTimeMilliseconds();

			// Sorry for the ugly and messy-looking JSON :(
			string json = string.Format(
				"{{\"privateMessage\":{{\"token\":\"{0}\",\"timestamp\":\"{1}\",\"sender\":\"{2}\",\"receiver\":\"{3}\",\"message\":\"{4}\"}}}}",
				TOKEN, unixTimestamp, pm.Sender, pm.Recipient, pm.Message);
			Console.WriteLine("-- Forwarding to " + SERVER_ADDRESS);
			Console.WriteLine("-- " + json);

			var request = new RestRequest(MESSAGE_FORWARDING_ENDPOINT, Method.POST);
			request.AddParameter("application/json", json, ParameterType.RequestBody);

			IRestResponse response = client.Execute(request);
			var content = response.Content;

			// TODO: handle API response (success / error etc)
		}

	}
}
