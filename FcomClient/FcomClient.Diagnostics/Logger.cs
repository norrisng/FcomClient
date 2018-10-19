using System;

namespace FcomClient.Diagnostics
{
	class Logger
	{
		/// <summary>
		/// Name of the log file.
		/// </summary>
		private string Filename = "log.txt";

		/// <summary>
		/// Default constructor. Saves all log messages to "log.txt".
		/// </summary>
		public Logger()
		{

		}

		/// <summary>
		/// Constructor for saving all log messages to a specific file.
		/// </summary>
		/// <param name="Filename">Name of file to save to</param>
		public Logger(string Filename)
		{
			this.Filename = Filename;
		}

		/// <summary>
		/// Saves a log message to the specified file.
		/// The timestamp is automatically appended to the front of the message.
		/// </summary>
		/// <param name="msg"></param>
		public void Log(string msg)
		{
			// YYYY-MM-DD hh:mm:ssZ
			DateTime timestamp = DateTime.UtcNow;
			string logMessage = String.Format("{0}: {1}", timestamp.ToString("u"), msg);
			System.IO.File.AppendAllLines(Filename, new string[] { logMessage });
		}

	}
}
