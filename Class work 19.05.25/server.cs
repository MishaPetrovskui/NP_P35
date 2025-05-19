using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text.Json;
using System.Drawing;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using System;

enum MessageType
{
    SELECT,
    NAME,
    QUESTION,
    QUESTIONanswer,
    CHOSEN,
    FINISH
}

enum questionType
{
    ANSWER,
    OPEN
}

class test
{
    public string name { get; set; }
    public List<questions> questions { get; set; }
    public test(string name, List<questions> questions)
    {
        this.name = name;
        this.questions = questions;
    }
    public List<questions> getAnsweredQuestions()
    {
        List<questions> answer = new List<questions>();
        foreach(var a in this.questions)
        {
            if(!a.isAnswered)
                answer.Add(a);
        }
        return answer;
    }
    public int getCorrect()
    {
        int answer = 0;
        foreach (var a in this.questions)
        {
            if (!a.isAnswered)
                answer++;
        }
        return answer;
    }
}

class questions
{
    public string text { get; set; }
    public List<string> answers { get; set; }
    public questionType type { set; get; }
    public string corect { get; set; }
    public bool isCorect { get; set; }
    public bool isAnswered { get; set; }
    public questions(string text, List<string> answers, string corect, questionType questionType) 
    {
        this.text = text;
        this.answers = answers;
        this.corect = corect;
        this.type = questionType;
    }
    public bool IsThisCorectAnswer(string answer)
    {
        isAnswered = true;
        if (answer.ToLower().Equals(corect.ToLower()))
            return true;
        return false;
    }
}

class questionsWithoutAnswer
{
    public string text { get; set; }
    public List<string> answers { get; set; }
    public questionType type { set; get; }
    public questionsWithoutAnswer(string text, List<string> answers, questionType questionType)
    {
        this.text = text;
        this.answers = answers;
        this.type = questionType;
    }
}

class User
{
    public string name { get; set; }
    public IPEndPoint IPPoint { get; set; }
    public test test { get; set; }
    public TcpClient tcpClient { get; set; }
}

class Message
{
    public string user { get; set; }
    public string text { get; set; }
    public MessageType type { get; set; }
    public byte[] data { get; set; }
    //public ConsoleColor color { get; set; }
}
class Server
{
    static TcpListener listener;
    static int port = 5000;
    static int clients = 1;
    static readonly object lockObj = new object();
    static Random rand = new Random();
    static List<test> testList = new List<test>();

    static List<string> GetQuestName()
    {
        List<string> list = new List<string>();
        foreach (test test in testList)
        {
            list.Add(test.name);
        }
        return list;
    }
    //static ConsoleColor Color;

    static List<ConsoleColor> colors = new List<ConsoleColor>
        {
            ConsoleColor.Green, ConsoleColor.Red, ConsoleColor.DarkGreen, ConsoleColor.Magenta, ConsoleColor.Blue, ConsoleColor.Cyan, ConsoleColor.Magenta, ConsoleColor.Yellow,
        };

    static void sendMessage(NetworkStream stream, string message, int buffsize = 1024)
    {
        if (stream == null)
            return;
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        stream.Write(buffer, 0, buffer.Length);
    }
    static string GetMessage(NetworkStream stream, int buffsize = 1024)
    {
        if (stream == null)
            return "";
        byte[] buffer = new byte[buffsize];
        stream.Read(buffer, 0, buffsize);
        string ret = Encoding.UTF8.GetString(buffer).Split(char.MinValue).First();

        return ret;
    }

    static void SendPackage(NetworkStream stream, Message package)
    {
        if (stream == null) return;
        byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(package));
        stream.Write(buffer);
    }

    static Message? GetPackage(NetworkStream stream, int bufsize = 4096)
    {
        if (stream == null) return null;
        byte[] buffer = new byte[bufsize];
        stream.Read(buffer, 0, bufsize);
        string value = Encoding.UTF8.GetString(buffer).Split(char.MinValue).First();
        Console.WriteLine($"Package got: {value}");
        return JsonSerializer.Deserialize<Message>(value
            );
    }

    static void Main(string[] args)
    {
        Console.OutputEncoding = UTF8Encoding.UTF8;
        Console.InputEncoding = UTF8Encoding.UTF8;


        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        Console.WriteLine("Server Started!");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Thread thread = new Thread(HandleClient);
            thread.Start(client);
        }
    }

    static List<User> Clients = new List<User>();
    static void Broadcast(Message message)
    {
        Console.Write($"{message.user}: ");
        //Console.ForegroundColor = message.color;
        Console.WriteLine(message.text);
        //Console.ResetColor();
        foreach (var item in Clients.ToArray())
        {
            try
            {
                sendMessage(item.tcpClient.GetStream(), JsonSerializer.Serialize(message));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
    static void Broadcast(string message)
    {
        Console.WriteLine($"Broadcast: {message}");
        foreach (var item in Clients.ToArray())
        {
            sendMessage(item.tcpClient.GetStream(), message);
        }
    }
    static void Broadcast(string message, TcpClient client)
    {
        Console.WriteLine($"Broadcast: {message}");
        foreach (var item in Clients.ToArray())
        {
            if (!item.Equals(client))
                sendMessage(item.tcpClient.GetStream(), message);
        }
    }

    public int SendQuestion(NetworkStream stream)
    {
        SendPackage(stream, new Message { type = MessageType.QUESTION, data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(testList)) });
        return 0;
    }

    static int secToAnswer = 10;
    static int readyPlayers;
    static bool GameStarted = false;
    static int number;
    static TcpClient CurrentPlayer = new TcpClient();
    
    static void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        Console.WriteLine();
        Console.WriteLine("New Client");
        //ConsoleColor Color = colors[rand.Next(0, colors.Count())];
        test ChoisenTest = new test("", null);
        var endPoint = client.Client.RemoteEndPoint.ToString();
        var stream = client.GetStream();
        Message message = GetPackage(stream);
        string name = "";
        if (message.type == MessageType.NAME)
        {
            name = GetMessage(stream);
            Console.WriteLine($"Name {name} | {endPoint.ToString()}");
            Clients.Add(new User { name = name, tcpClient = client });
            Broadcast(new Message { user = "SERVER", text = $"{name} connect to Server" });
        }

        try
        {
            while (true)
            {
                SendPackage(stream, new Message { type = MessageType.SELECT, data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(GetQuestName())) });
                message = GetPackage(stream);
                if (message.type == MessageType.CHOSEN)
                {
                    int action = Convert.ToInt32(Encoding.UTF8.GetString(message.data).Split(char.MinValue).First());
                }
                for (int i = 0; i < ChoisenTest.questions.Count; i++)
                {
                    questions a = ChoisenTest.questions[Convert.ToInt32(ChoisenTest.getAnsweredQuestions())];
                    ChoisenTest.
                }
            }

        }
        catch (Exception ex)
        {
            Broadcast($"{name} ({endPoint}) Disconected!", client);
        }
        finally
        {
            client.Close();
        }


    }

}
