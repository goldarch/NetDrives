using System;
using System.Net;
using System.Threading;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Interop;
using Hardcodet.NetDrives.Platform.Interop.Instrumentation;
using NUnit.Framework;

namespace Platform_Tests
{
  [TestFixture]
  public class DummyTests
  {

    [SetUp]
    public void Init()
    {
    }


    [TearDown]
    public void Cleanup()
    {
    }

    [Test]
    public void TESTNAME()
    {
      var config = TestUtil.GetConfiguration(TestUtil.UncTempSecuredUser1);
      //config.DriveLetter = 'K';
      //config.NetworkLocation = config.NetworkLocation.Replace(@"10.0.0.30", "Krusty");

      var status = NetworkUtil.Connect(config);
      "Status: {0}".ToConsole(status);      
    }


    [Test]
    public void Check_connections()
    {
      var cons = NetworkConnection.GetInstances();
      foreach (NetworkConnection con in cons)
      {
        con.RemoteName.ToConsole();
        con.RemotePath.ToConsole();
        con.UserName.ToConsole();

        Uri uri = new Uri(con.RemoteName);
        Console.Out.WriteLine("uri.Host = {0}", uri.Host);
        Console.Out.WriteLine("UNC: " + uri.IsUnc);
        Console.Out.WriteLine("uri.Scheme = {0}", uri.Scheme);
        Console.Out.WriteLine("host name type: " + uri.HostNameType);
        Console.Out.WriteLine("authority: " + uri.Authority);
        "Resolved host: {0}".ToConsole(GetNetworkLocation(con.RemotePath));

        Console.Out.WriteLine("");
       
      }

      var c = NetworkConnection.CreateInstance();
    }

    private string GetNetworkLocation(string remoteName)
    {
      Uri uri = new Uri(remoteName);
      string hostName = uri.Host;

      var entry = Dns.GetHostEntry(hostName);
      foreach (var s in entry.Aliases)
      {
        Console.WriteLine("Alias: {0}", s);
      }

      foreach (var address in entry.AddressList)
      {
        Console.Out.WriteLine("Address: {0}", address);
      }

      Console.Out.WriteLine("entry.HostName = {0}", entry.HostName);


      remoteName = remoteName.ToLower();
      int ipcIndex = remoteName.IndexOf(@"\ipc$");
      if (ipcIndex > -1)
      {
        remoteName = remoteName.Substring(0, ipcIndex);
      }






      IPAddress adr;
      bool status = IPAddress.TryParse(hostName, out adr);
      if (!status)
      {
        //resolve IP address
      }

      return remoteName;
    }

    [Test]
    public void TESTNAMEx()
    {
      IPHostEntry entry = null;
      Dns.BeginGetHostEntry("Miranda", ar => entry = Dns.EndGetHostEntry(ar), null);

//      var entry = Dns.GetHostEntry("xxx");
//      Console.Out.WriteLine("entry = {0}", entry.AddressList.Length);

      Thread.Sleep(5000);


      entry.AddressList.Do(adr => adr.ToString().ToConsole());
    }


    [Test]
    public void xxx()
    {
      Uri uri = new Uri(@"\\10.0.0.30\temp\public$\");
      uri.Host.ToConsole();
      uri.LocalPath.ToConsole();
      uri.PathAndQuery.ToConsole();

      "Segments:".ToConsole();
      uri.Segments.Do(s => s.ToConsole());
    }

  }
}