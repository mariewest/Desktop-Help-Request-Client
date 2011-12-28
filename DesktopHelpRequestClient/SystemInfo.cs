// Author: Marie West
// Email: marie@mariewest.me
// Website: http://mariewest.me
// Date: 2011-12-07
// Summary: Contains SystemInfo class, which gathers system information and posts it (in JSON) to a specified URL
//          (Please see App.config for the config file)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;        // For username
using System.Diagnostics;               // For processList
using System.Web.Extensions;            // For JSON output (JavaScriptSerializer)
using System.Web.Script.Serialization;  // For JSON output (JavaScriptSerializer)
using System.Net;                       // For POST
using System.IO;                        // For POST (Stream)
using System.Collections;               // For Hashtable

namespace DesktopHelpRequestClient
{
    class SystemInfo
    {
        // Constructor
        public SystemInfo()
        {
        }

        // Destructor
        ~SystemInfo()
        {
        }

        // Gather the system information
        public void GatherInfo()
        {
            this.timeStamp = DateTime.Now;
            this.username = WindowsIdentity.GetCurrent().Name;
            this.machineName = Environment.MachineName;
            this.processes = Process.GetProcesses();

            // Get local IP Address
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    this.ipAddress = ip.ToString();
                }
            }
        }

        // Post the system information to the specified URL
        // Return the reference number given by the server (if enabled) or "true" (if disabled)
        // Return empty string if there was a failure
        public string PostInfo(string name, string email, string comments)
        {
            // Create a request for the URL
            WebRequest request = WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["postURL"]);

            // Specify the protocol (POST)
            request.Method = "POST";

            // Create the POST data and convert it to a byte array
            string json = "json=" + this.GetInfoAsJSON(name, email, comments);
            byte[] byteArray = Encoding.UTF8.GetBytes(json);

            // Set the content length and type
            request.ContentLength = byteArray.Length;
            request.ContentType = "application/x-www-form-urlencoded";

            // Get the request stream and write the data to it
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            // Get the response from the server
            Dictionary<string, string> responseInfo = new Dictionary<string, string>();
            try
            {
                // Get stream containing response data sent by the server and read it
                WebResponse response = request.GetResponse();
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                // Deserialize the JSON response from the server
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                responseInfo = serializer.Deserialize<Dictionary<string, string>>(responseFromServer);

                // Close the streams
                reader.Close();
                response.Close();
            }
            catch(Exception e)
            {
                responseInfo.Add("success", "false");
            }

            // Close the streams
            dataStream.Close();

            // Return the reference number (empty string if there was a failure)
            if (responseInfo["success"] == "true")
            {
                if (responseInfo["reference_number"] == "")
                {
                    // Return dummy value (because returning "" indicates failure)
                    return "true";
                }
                else
                {
                    return responseInfo["reference_number"];
                }
            }
            else
            {
                return "";
            }
        }

        // Return the gathered information as JSON - helper function for PostInfo()
        public string GetInfoAsJSON(string name, string email, string comments)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string json = "";

            // Put the process info in a hashtable
            Hashtable processHash = new Hashtable();
            foreach (Process p in this.processes)
            {
                processHash.Add(p.Id.ToString(), p.ProcessName);
            }

            // Put all system info into one hashtable (so it can be serialized)
            Hashtable data = new Hashtable();
            data.Add("name", name);
            data.Add("email", email);
            data.Add("comments", comments);
            data.Add("timeStamp", this.timeStamp.ToString("MM/dd/yyyy h:mm tt"));
            data.Add("username", this.username);
            data.Add("ipAddress", this.ipAddress);
            data.Add("machineName", this.machineName);
            data.Add("processes", processHash);

            json = serializer.Serialize((object)data);

            return json;
        }

        // Data members
        private DateTime timeStamp;     // When the system info was gathered
        private string username;        // Name of the logged in user
        private string ipAddress;       // Local IP address of the machine
        private string machineName;     // Name of the machine
        private Process[] processes;    // All running processes
    }
}