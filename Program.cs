using System;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;

//Console 
Console.SetWindowSize(Math.Min(500, Console.LargestWindowWidth), Math.Min(500, Console.LargestWindowHeight));
Console.BackgroundColor = ConsoleColor.Black;
Console.ForegroundColor = ConsoleColor.Green;



static bool ModifyHostsFile(string entry)
{
    try
    {
        using (StreamWriter w = File.AppendText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts")))
        {
            w.WriteLine(entry);
            return true;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return false;
    }
}

string[] entries = { "127.0.0.1     www.google.com", "127.0.0.1     google.com", "127.0.0.1     youtube.com", "127.0.0.1     www.youtube.com", "127.0.0.1     facebook.com", "127.0.0.1     www.bing.com", "127.0.0.1     bing.com", "127.0.0.1     yahoo.com", "127.0.0.1     www.yahoo.com" };
for (int i = 0; i < entries.Length; i++)
{
    if (ModifyHostsFile(entries[i]))
    {
        Console.WriteLine("Entry added successfully....");
    }
}



static NetworkInterface GetActiveEthernetOrWifiNetworkInterface()
{
    var Nic = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(
        a => a.OperationalStatus == OperationalStatus.Up &&
        (a.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || a.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
        a.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily.ToString() == "InterNetwork"));

    return Nic;
}
static void SetDNS(string DnsString)
{
    string[] Dns = { DnsString };
    var CurrentInterface = GetActiveEthernetOrWifiNetworkInterface();
    if (CurrentInterface == null) return;

    ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
    ManagementObjectCollection objMOC = objMC.GetInstances();
    foreach (ManagementObject objMO in objMOC)
    {
        if ((bool)objMO["IPEnabled"])
        {
            if (objMO["Description"].ToString().Equals(CurrentInterface.Description))
            {
                ManagementBaseObject objdns = objMO.GetMethodParameters("SetDNSServerSearchOrder");
                if (objdns != null)
                {
                    objdns["DNSServerSearchOrder"] = Dns;
                    objMO.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
                }
            }
        }
    }
}
SetDNS("127.0.0.1");