using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Server
{
    static TcpListener listener;
    static int port = 5050;
    static int connectionCount = 0;

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        Console.WriteLine("Сервер запущено.");
        Console.WriteLine("Очікування підключень...");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Нове підключення!");

            Thread thread = new Thread(HandleClient);
            thread.Start(client);
        }
    }

    static void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        var endPoint = client.Client.RemoteEndPoint;

        /*if (endPoint != null)
            Console.WriteLine("Клієнт підключився з: " + endPoint);*/

        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);

        string clientName = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        string clientIP = (client.Client.RemoteEndPoint).ToString();

        int currentNumber;
        connectionCount++;
        currentNumber = connectionCount;
        Console.WriteLine($"<{clientName}> ({clientIP}) підключився на сервер!");

        /*string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine("Отримано ім'я: " + receivedMessage);*/

        byte[] response = Encoding.UTF8.GetBytes($"Ваш номер підключення: {currentNumber}");
        stream.Write(response, 0, response.Length);

        client.Close();
    }
}





// client






using System;
using System.Net.Sockets;
using System.Text;

class Client
{
    static string serverIP = "127.0.0.1";
    static int port = 5050;

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        Console.Write("Введіть своє ім'я: ");
        string name = Console.ReadLine();

        try
        {
            TcpClient client = new TcpClient(serverIP, port);
            Console.WriteLine("Успішно підключено до сервера.");

            NetworkStream stream = client.GetStream();
            byte[] buffer = Encoding.UTF8.GetBytes(name);
            stream.Write(buffer, 0, buffer.Length);

            byte[] responseBuffer = new byte[1024];
            int bytesRead = stream.Read(responseBuffer, 0, responseBuffer.Length);
            string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);

            Console.WriteLine(response);
            Console.WriteLine("Натисніть Enter, щоб вийти...");
            Console.ReadLine();

            client.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Помилка: " + ex.Message);
        }
    }
}
