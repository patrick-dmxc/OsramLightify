using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml.Linq;
using System.IO;
namespace Osram.Lightify
{
    public class OsramController
    {
        const string devId = "yourdevid";
        const string urlbase = "https://emea-srmprod01-api.arrayent.com:8081/";
        const string username = "yourusername";
        const string password = "yourpassword";

        [Route("api/osram/{group}/On")]
        [HttpGet]
        public bool On(string group)
        {
            SendOsramCommand("group" + group + ",on");
            return true;
        }

        [Route("api/osram/{group}/Off")]
        [HttpGet]
        public bool Off(string group)
        {
            SendOsramCommand("group" + group + ",off");
            return true;
        }

        [Route("api/osram/{group}/State")]
        [HttpGet]
        public OsramDevice State(string group)
        {
            return GetState(group);
        }

        private static void SendOsramCommand(string value)
        {

            string token = GetOsramToken(username, password).securityToken;

            string action = "zdk/services/zamapi/setDeviceAttribute?secToken=" + token + "&value=";

            string end = "&devId=" + devId + "&name=DeviceAction";
            string request = urlbase + action + value + end;

            WebClient wc = new WebClient();
            var data = wc.DownloadString(request);
        }

        private static OsramToken GetOsramToken(string username, string password)
        {
            string payload = "{\"username\" : \"" + username + "\",\"password\" : \"" + password + "\"}";
            string url = "https://emea-srmprod01-api.arrayent.com:8081/acc/applications/OSRAMService/sessions";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.UserAgent = "Lightify/1.0.5 CFNetwork/711.1.16 Darwin/14.0.0";
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/json";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {


                streamWriter.Write(payload);
                streamWriter.Flush();
                streamWriter.Close();
            }
            OsramToken os = new OsramToken();
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                os = JsonConvert.DeserializeObject<OsramToken>(result);

            }

            return os;
        }

        private static OsramDevice GetState(string group)
        {
            string token = GetOsramToken(username, password).securityToken;
            string url = "zdk/services/zamapi/getDeviceAttributesWithValues?devId=" + devId + "&deviceTypeId=377&secToken=" + token;
            string request = urlbase + url;

            WebClient wc = new WebClient();
            string data = wc.DownloadString(request);
            XElement x = XElement.Parse(data);
            var list = (from r in x.Descendants("attrList") select r).ToList();
            var statusElement = "GroupStatus" + group;
            var nameElement = "GroupName" + group;
            var lightStatus = x;
            var lightName = x;
            foreach (var item in list)
            {
                string name = item.Element("name").Value.ToString();
                if (name.Equals(statusElement))
                {
                    lightStatus = item;
                }
                if (name.Equals(nameElement))
                {
                    lightName = item;
                }
            }
            var obj = new OsramDevice();
            obj.Name = lightName.Element("value").Value.ToString();
            string value = lightStatus.Element("value").Value.ToString();
            string[] split = value.Split(',');

            string statestring = split[1];
            bool state = false;
            if (split[3].Equals("1"))
            {
                state = true;
            }

            obj.State = state;
            obj.Level = Convert.ToInt32(split[4]);
            obj.ColorTemp = Convert.ToInt32(split[5]);
            return obj;
        }
    }
}
