using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Net.Sockets;
using System.Windows.Markup;
using System.Windows;
using System.Net.NetworkInformation;
using System.Threading;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Threading;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.ComTypes;




public class TCPClient
{
    // TCP RX event
    public delegate void TCP_own_socket_event(byte[] sender, EventArgs e);
    public event TCP_own_socket_event TCP_RX_Callback;

    public TcpClient client;

    public bool disconnect = false;


    /// <summary>
    /// Creates TCP socket
    /// </summary>
    public void create(String address, Int32 port)
    {
        client = new TcpClient(address, port);
        listen();
        Application.Current.Exit += CloseHandler;
    }

    /// <summary>
    /// Creates TCP socket
    /// </summary>
    public void create(TcpClient Client)
    {
        client = Client;
        listen();
        Application.Current.Exit += CloseHandler;
    }


    // Close socket when app is shutting down
    private void CloseHandler(object sender, ExitEventArgs e)
    {
        Close();
    }

    public void Close()
    {
        disconnect = true;
        //client.Client.Disconnect(true);
        client.Dispose();
        client.Close();
    }

    public void send(byte[] buffer)
    {
        NetworkStream stream = client.GetStream();
        stream.Write(buffer, 0, buffer.Length);
        stream.Flush();
    }

    public void send(string buffer)
    {
        NetworkStream stream = client.GetStream();
        //Byte[] payload = System.Text.Encoding.ASCII.GetBytes(buffer + "\n");
        Byte[] payload = System.Text.Encoding.ASCII.GetBytes(buffer);
        stream.Write(payload, 0, payload.Length);
        stream.Flush();
    }

    /// <summary>
    /// Continuously listenes to a given TCP connection
    /// </summary>
    public async void listen()
    {
        disconnect = false;
        while (disconnect == false && client.Connected)
        {
            await HandleCl(client);
        }
        client.Close();
    }

    /// <summary>
    /// Asyncron receive process (called by listen())
    /// </summary>
    private async Task HandleCl(TcpClient CL, int maxBytest = 1446)
    {
        try
        {
            if (disconnect == false)
            {
                NetworkStream ns = CL.GetStream();
                StreamReader sr = new StreamReader(ns);
                byte[] byt = new byte[maxBytest];
                int i = await ns.ReadAsync(byt, 0, maxBytest);
                byte[] res = byt.Take(i).ToArray();
                TCP_RX_Callback?.Invoke(res, EventArgs.Empty);
            }
        }
        catch { disconnect = true; }
    }

}




public class TCPServer
{

    // implementation ressources:

    // https://learn.microsoft.com/de-de/dotnet/fundamentals/networking/sockets/tcp-classes
    // https://codingvision.net/c-simple-tcp-server


    public List<TcpClient> clients = new List<TcpClient>();

    // TCP RX event
    public delegate void TCP_own_socket_event(TcpClient CL, byte[] sender, EventArgs e);
    public event TCP_own_socket_event TCP_RX_Callback;

    public TcpListener server;

    public bool open = false;

    public async void Open(String address, Int32 port)
    {
        server = new TcpListener(IPAddress.Parse(address), port);
        server.Start();
        open = true;

        await Task.Run(() => HandleClients());
    }

    public async void Open(Int32 port)
    {
        server = new TcpListener(GetLocalIPAddress(), port);
        server.Start();
        open = true;

        await Task.Run(() => HandleClients());
    }

    public void Close()
    {
        open = false;
        server.Stop();

        foreach (TcpClient client in clients)
        {
            client.Dispose();
            client.Close();
        }
        clients.Clear();
    }

    public void send(TcpClient client, string buffer)
    {
        NetworkStream stream = client.GetStream();
        Byte[] payload = System.Text.Encoding.ASCII.GetBytes(buffer + "\n");
        //Byte[] payload = System.Text.Encoding.ASCII.GetBytes(buffer);
        stream.Write(payload, 0, payload.Length);
        stream.Flush();
    }

    public void send(TcpClient client, byte[] buffer)
    {
        NetworkStream stream = client.GetStream();
        stream.Write(buffer, 0, buffer.Length);
        stream.Flush();
    }

    private void HandleClients()
    {
        while (open)   //we wait for a connection
        {
            try
            {
                TcpClient client = server.AcceptTcpClient();  //if a connection exists, the server will accept it

                clients.Add(client);

                Task.Run(() => HandleCl(client));
            }
            catch { }
        }
        //Close();
    }

    /// <summary>
    /// Asyncron receive process (called by listen())
    /// </summary>
    private async Task HandleCl(TcpClient CL, int maxBytest = 1446)
    {
        bool active = true;
        while (active)
        {
            try
            {
                NetworkStream ns = CL.GetStream();
                StreamReader sr = new StreamReader(ns);
                byte[] byt = new byte[maxBytest];
                int i = await ns.ReadAsync(byt, 0, maxBytest);
                byte[] res = byt.Take(i).ToArray();
                if (i == 0) { active = false; }
                else { TCP_RX_Callback?.Invoke(CL, res, EventArgs.Empty); }
            }
            catch { active = false; }
        }
        clients.Remove(CL);
    }


    public IPAddress GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip;
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    public string GetClientIP(TcpClient Client)
    {
        return ((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString();
    }

    public int GetClientPort(TcpClient Client)
    {
        return ((IPEndPoint)Client.Client.RemoteEndPoint).Port;
    }
}




public class UDPSocket
{

    // UDP RX event
    public delegate void UDP_RX_Event(byte[] sender, EventArgs e);
    public event UDP_RX_Event UDP_RX_Callback;

    // UDP sockets
    private static UdpClient RX_socket;
    private UdpClient TX_socket;


    /// <summary>
    /// Start listing on your own IP to receive incoming data
    /// or set rx IP manually
    /// </summary>
    public void Start(UInt16 Port, string own_ip = "")
    {
        try
        {
            // Get local IP address
            if (own_ip == "") own_ip = GetLocalIPAddress();
            //Console.WriteLine("automatic IP: " + GetLocalIPAddress());

            // Starting RX on UDP socket
            IPEndPoint ipLocalEndPoint = new IPEndPoint(IPAddress.Parse(own_ip), Port);

            RX_socket = new UdpClient(ipLocalEndPoint);
            RX_socket.BeginReceive(new System.AsyncCallback(UDPRXCallBack), RX_socket);
        }
        catch { MessageBox.Show("UDP Verbindung konnte nicht aufgebaut werden! (Firewall/OwnIP-Funktion)"); }
    }

    /// <summary>
    /// CallBack function, when package is received
    /// </summary>
    private void UDPRXCallBack(IAsyncResult result)
    {
        //Console.WriteLine("UDP received");
        try
        {
            // Read RX data
            UdpClient socket = result.AsyncState as UdpClient;
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            //IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("192.168.1.222"), 11000);
            byte[] RX_bytes = RX_socket.EndReceive(result, ref remoteEP);

            // Fire RX callback
            UDP_RX_Callback?.Invoke(RX_bytes, EventArgs.Empty);

            // Continue with RX
            socket.BeginReceive(new AsyncCallback(UDPRXCallBack), socket);
        }
        catch { }
    }

    /// <summary>
    /// Function for sending string data via UDP
    /// </summary>
    public void Send(string recipient_ip, int recipient_port, string data)
    {
        try
        {
            //TX_socket = new UdpClient();
            IPEndPoint addr = new IPEndPoint(IPAddress.Parse(recipient_ip), recipient_port);
            //IPEndPoint addr = new IPEndPoint(IPAddress.Parse(recipient_ip), 11000);
            //TX_socket.Send(System.Text.Encoding.ASCII.GetBytes(data + "\r"), data.Length, addr);
            //TX_socket.Send(System.Text.Encoding.ASCII.GetBytes(data), data.Length, addr);
            RX_socket.Send(System.Text.Encoding.ASCII.GetBytes(data), data.Length, addr);
            //TX_socket.Close();
        }
        catch { }
    }

    /// <summary>
    /// Function for sending bytearray data via UDP
    /// </summary>
    public void Send(string recipient_ip, int recipient_port, Byte[] data)
    {
        try
        {
            //TX_socket = new UdpClient();
            IPEndPoint addr = new IPEndPoint(IPAddress.Parse(recipient_ip), recipient_port);
            //TX_socket.Send(data, data.Length, addr);
            RX_socket.Send(data, data.Length, addr);
            //TX_socket.Close();
        }
        catch { }
    }

    /// <summary>
    /// Function to end UDP socket
    /// 
    /// When still in receive mode, an exception will be thrown,
    /// when the socket is being closed (no problem!)
    /// </summary>
    public void Stop()
    {
        if (RX_socket == null) { return; }

        RX_socket.Close();
    }

    public string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                //return "192.168.8.191";
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }


}
