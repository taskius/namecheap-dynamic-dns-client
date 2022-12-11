using log4net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DynDnsClient
{
    internal class DnsUpdater
    {
        private const string Url = "https://dynamicdns.park-your-domain.com/update?host={0}&domain={1}&password={2}";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public DnsUpdater()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        public DnsUpdateResult Update(string domain, IEnumerable<string> hosts, string password)
        {
            var result = new DnsUpdateResult();

            foreach (string host in hosts)
            {
                string url = CreateUrl(domain, host, password);
                bool success = Request(url);

                result.Add(host, success);
            }

            return result;
        }

        private static bool Request(string url)
        {
            using (var client = new WebClient())
            {
                try
                {
                    string response = client.DownloadString(url);
                    if (response.Contains("<ErrCount>0</ErrCount>"))
                    {
                        return true;
                    }

                    var regex = new Regex("<Description>(.*)</Description>");
                    var match = regex.Match(response);
                    if (match.Success)
                    {
                        var message = match.Value;

                        if (match.Groups.Count == 2)
                        {
                            message = match.Groups[1].Value;
                        }

                        throw new Exception("Failed to update dns:\nError Description: " + message);
                    }

                    // If there's no description then just log debug for now. logs will still show an error updating the dns
                    Log.Debug(response);
                    return false;
                }
                catch (Exception e)
                {
                    Log.Error("Unable to update records with " + url, e);
                    return false;
                }
            }
        }

        private static string CreateUrl(string domain, string host, string password)
        {
            return string.Format(Url, host, domain, password);
        }
    }
}