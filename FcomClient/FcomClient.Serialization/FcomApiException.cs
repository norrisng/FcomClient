using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
