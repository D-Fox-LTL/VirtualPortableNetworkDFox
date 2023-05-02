using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;





namespace VirtualPortableNetworkDFox
{
    class VirtualPortableNetworkDFox
    { 
      // Emulation of asynchronous constructors 
      // The string[] args is a variable that has all the values passed from the command line 
        static async Task Main(string[] args)
        {
            // Setup of a TCP listener on port 8080
            // The TcpListener class provides simple methods that listen for and accept incoming connection requests 
            var listener = new TcpListener(IPAddress.Any, 8080);
            listener.Start();
            Console.WriteLine("VPN server started, listening on port 8080");

            while (true)
            {
                try {
                    // Wait for a client to connect
                using var client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("Client connected: " + client.Client.RemoteEndPoint);

                // Forward all client traffic to Google DNS server
                    // Google DNS is a public DNS service that is provided by Google
                    // with the aim to make the Internet and the DNS system
                    // faster, safer, secure, and more reliable for all Internet users. -whatismydns.net
                    using var dnsClient = new TcpClient();
                await dnsClient.ConnectAsync(IPAddress.Parse("8.8.8.8"), 53);

                // Create network streams for both client and server
                    //A network stream is the connection between a writer and reader endpoint. - ni.com
                using var clientStream = client.GetStream();
                using var dnsStream = dnsClient.GetStream();

                // Pipe traffic from client to server and vice versa
                    // A pipe simply refers to a temporary software connection between
                    // two programs or commands. - techtarget.com
                using var clientTask = Task.Run(() => clientStream.CopyToAsync(dnsStream));
                using var serverTask = Task.Run(() => dnsStream.CopyToAsync(clientStream));

                // Wait for both tasks to complete
                await Task.WhenAll(clientTask, serverTask);

                // Close the connections
                client.Close();
                dnsClient.Close();

                Console.WriteLine("Client disconnected: " + client.Client.RemoteEndPoint);
                } catch (Exception ex) {
                    Console.WriteLine("Error occurred: " + ex.Message);
                }
            }
        }
    }
}