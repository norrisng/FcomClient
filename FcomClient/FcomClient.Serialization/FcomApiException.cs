using System;

namespace FcomClient.Serialization
{
	class FcomApiException : Exception
	{
		public FcomApiException()
		{
		}

		public FcomApiException(string message) : base(message)
		{
		}

		public FcomApiException(string message, Exception inner) : base(message, inner)
		{
		}

	}
}
