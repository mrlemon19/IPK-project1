// projekt do ipk web klient pro kalkulacku
// autor: Jakub Lukas, xlukas18
// usage: ipkcpc [--help] -h <host> -p <port> -m <mode>

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

class Program
{
    static Socket? TCPSocket;
    static StreamWriter? TCPWriter;
    static StreamReader? TCPReader;

    static void print_help()
    {
        // vytiskne pomocnou zpravu
        Console.WriteLine("usage: ipkcpc [--hlep] -h <host> -p <port> -m <mode>");
        Console.WriteLine("where <host> is IPV4 address of server application will comunicate with");
        Console.WriteLine("<port> is number of port on witch the application will run at");
        Console.WriteLine("<mode> is protocol used for communication (tcp/udp)");
    }

    static void connect_udp(string serverIP, int port)
    {
        // udp
        // vytvoreni udp socketu a server endpointu
        Socket UDPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint server = new IPEndPoint(IPAddress.Parse(serverIP), port);

        string? inbuffer;
        while ((inbuffer = Console.ReadLine()) != null)
        {
            // prida opcode a payload length
            inbuffer = "\0" + Convert.ToChar(inbuffer.Length) + inbuffer;

            // odesle zpravu
            byte[] sendBytes = Encoding.ASCII.GetBytes(inbuffer);
            UDPSocket.SendTo(sendBytes, server);

            // prijme zpravu
            EndPoint receiveEndpoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] receiveBytes = new byte[1024];
            int bytesReceived = UDPSocket.ReceiveFrom(receiveBytes, ref receiveEndpoint);

            // prevede na ascii
            string recbuffer = Encoding.ASCII.GetString(receiveBytes, 0, bytesReceived);

            // vypise zpravu
            if (recbuffer[1] == '\0')
            {
                // OK
                Console.WriteLine("OK: " + recbuffer);
            }
            else
            {
                // ERROR
                Console.WriteLine("ERR: " + Encoding.ASCII.GetString(receiveBytes, 3, bytesReceived - 3));
            }
            }

        UDPSocket.Close();
    }

    static void connect_tcp(string serverIP, int port)
    {
        // vytvorit socket
        TCPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // vytvorit server endpoint pro komunikaci
        IPEndPoint server = new IPEndPoint(IPAddress.Parse(serverIP), port);

        // pripoji se na server
        TCPSocket.Connect(server);

        // vytvorit stream, nastavit writer a reader
        NetworkStream stream = new NetworkStream(TCPSocket);
        TCPWriter = new StreamWriter(stream);
        TCPReader = new StreamReader(stream);

        Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExitTCP);
        
        bool bye = false;
        string? inbuffer;
        while ((inbuffer = Console.ReadLine()) != null)
        {
            // posle zpravu
            TCPWriter.WriteLine(inbuffer);
            TCPWriter.Flush();

            // precte zpravu
            string message = TCPReader.ReadLine() ?? throw new ArgumentException("error: Readline error");
            Console.WriteLine(message);

            if (message == "BYE") 
            {
                bye = true;
                break;
            }
        }

        if (!bye) CloseTCP();

        // zavreni streamu
        stream.Close();
        if (TCPWriter != null) TCPWriter.Close();
        if (TCPWriter != null) TCPWriter.Close();
        if (TCPReader != null) TCPReader.Close();
    }

    static void CloseTCP()
    {
        // ruseni spojeni
        if (TCPWriter != null && TCPReader != null)
        {
            TCPWriter.WriteLine("BYE");
            TCPWriter.Flush();
            string? byeMessage = TCPReader.ReadLine() ?? throw new ArgumentException("error: Readline error");
            Console.WriteLine(byeMessage);
        }

        // zavre socket, writer a reader
        if (TCPSocket != null) TCPSocket.Close();
        if (TCPWriter != null) TCPWriter.Close();
        if (TCPReader != null) TCPReader.Close();
    }

    static void OnExitTCP(object? sender, ConsoleCancelEventArgs args)
    {
        // zavre tcp spojeni na C-c signal
        CloseTCP();
    }

    static void validArgs (string serverIP, int port)
    {
        try
        {
            if (port < 1023 || port > 65535) throw new ArgumentException("error: port number " + port +  "is not available");
            IPAddress.Parse(serverIP);
        }
        catch (Exception ex)
        {
            throw new ArgumentException("error: " + ex.Message);
        }
    }

    static void Main(string[] args)
    {
        string serverIP = "";
        int port = -1;
        string mode = "";
        
        // zpracovani argumentu
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--help")
            {
                print_help();
                return;
            }
            else if (args[i] == "-h") serverIP = args[i + 1];
            else if (args[i] == "-p") port = int.Parse(args[i + 1]);
            else if (args[i] == "-m") mode = args[i + 1];
            else continue;
        }

        // kontrola vstupnich argumentu
        validArgs(serverIP, port);

        if (mode == "udp") connect_udp(serverIP, port);
        else if (mode == "tcp") connect_tcp(serverIP, port);
        else throw new ArgumentException("error: invalid mode");
    }
}
