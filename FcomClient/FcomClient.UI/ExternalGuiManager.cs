using FcomClient.FsdDetection;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FcomClient.UI
{
	class ExternalGuiManager
	{

		private readonly string FCOM_GUI_PIPE_NAME = "FcomGuiPipe";

		public ExternalGuiManager()
		{

		}

		/// <summary>
		/// Sends a list of detected connections to the GUI.
		/// </summary>
		/// <param name="connections"></param>
		public HardwareDevice GetHardwareDevice(List<HardwareDevice> connections)
		{
			string serializedString = "";
			
			// serialize connections list to XML
			using (var sw = new StringWriter())
			{
				using (var xw = XmlWriter.Create(sw))
				{
					xw.WriteStartDocument();

					int i = 0;
					foreach (HardwareDevice device in connections)
					{
						xw.WriteStartElement("Connection");

						xw.WriteStartElement("ConnectionNumber");
						xw.WriteString(i.ToString());
						xw.WriteEndElement();

						xw.WriteStartElement("ConnectionName");
						xw.WriteString(device.FriendlyName);
						xw.WriteEndElement();

						xw.WriteStartElement("Description");
						xw.WriteString(device.Description);
						xw.WriteEndElement();

						xw.WriteStartElement("IpAddresses");
						string ipAddresses = "";
						foreach (string s in device.IpAddresses)
						{
							ipAddresses += s;
						}
						xw.WriteEndElement();

						xw.WriteEndElement();
						i++;
					}
					xw.WriteEndDocument();

				}
				serializedString = sw.ToString();
			}

			int choice = -1;

			using (NamedPipeClientStream pipe = new NamedPipeClientStream(FCOM_GUI_PIPE_NAME))
			{
				pipe.Connect();

				// Encode XML to byte array and send to GUI
				byte[] serializedStringBytes = Encoding.UTF8.GetBytes(serializedString);
				pipe.Write(serializedStringBytes, 0, serializedStringBytes.Length);

				// Read response from GUI
				byte[] responseBuffer = new byte[5];
				pipe.Read(responseBuffer, 0, responseBuffer.Length);
				choice = BitConverter.ToInt32(responseBuffer, 0);				

			}

			if (choice >= 0)
				return connections[choice];
			else
				return null;
		}

		public void Close()
		{
			// TODO: connection cleanup
		}
	}
}
