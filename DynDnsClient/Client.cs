﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using DynDnsClient.Properties;
using log4net;

namespace DynDnsClient
{
    public class Client : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly ExternalIpAddress externalIpAddress;
        private readonly NamecheapClient namecheapClient;
        private readonly Hosts hosts;

        private Timer timer;

        public Client()
        {
            externalIpAddress = new ExternalIpAddress();
            namecheapClient = new NamecheapClient();
            
            hosts = new Hosts();
            hosts.Changed += OnHostsChangeUpdate;
        }

        public void RunContinuously()
        {
            Log.Info("Running client continuously");
            
            timer = new Timer(OnPeriodicUpdate, null, TimeSpan.Zero, Settings.Default.Period);
        }

        public void RunOnce()
        {
            Log.Info("Running client once");

            // Force update by specifying that last known IP address is unknown
            string lastKnownExternalIpAddress = Settings.Default.LastKnownExternalIpAddress;
            Settings.Default.LastKnownExternalIpAddress = null;

            OnPeriodicUpdate(null);

            // Restore last known IP address
            Settings.Default.LastKnownExternalIpAddress = lastKnownExternalIpAddress;
        }

        private void OnHostsChangeUpdate(object sender, EventArgs e)
        {
            string[] addedHosts = hosts.AddedSinceLastRead();
            
            if (addedHosts.Any())
            {
                Update(addedHosts);
            }
        }

        private void OnPeriodicUpdate(object state)
        {
            // Get external IP
            string ipAddress = externalIpAddress.Get();
            if (ipAddress == null)
            {
                Log.Error("Cannot proceed since external IP address is unknown.");
                return;
            }

            // Determine if external IP has changed
            if (ipAddress == Settings.Default.LastKnownExternalIpAddress)
            {
                Log.Info("External IP has not changed, is still " + ipAddress);
                return;
            }

            Log.InfoFormat(
                "External IP has changed from {0} to {1}, proceed with updating Namecheap records",
                Settings.Default.LastKnownExternalIpAddress,
                ipAddress);

            if (Update(hosts.Read()))
            {
                Settings.Default.LastKnownExternalIpAddress = ipAddress;
            }
        }

        private bool Update(IEnumerable<string> hosts)
        {
            // Update Namecheap records
            DnsUpdateResult result = namecheapClient.Update(
                Settings.Default.Domain,
                hosts,
                Settings.Default.Password);

            if (result.IsSuccess)
            {
                Log.Info("Successfully updated Namecheap records." + Environment.NewLine + result);
            }
            else
            {
                Log.Error("Unable to update Namecheap records" + Environment.NewLine + result);
            }

            return result.IsSuccess;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (hosts != null)
                {
                    hosts.Dispose();
                }

                if (timer != null)
                {
                    timer.Dispose();
                }
            }
        }

        #endregion
    }
}