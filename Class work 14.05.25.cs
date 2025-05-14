// infinityfree.com

using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text.Json;
using System.Drawing;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Net.Mime.MediaTypeNames;

class ConsoleWindow
{
    public Point from;
    public Point to;
    public ConsoleColor ForegroundColor;
    public ConsoleColor BackgroundColor;
    List<string> text = new List<string>();
    public object lockObj;
    public object lockMessages;
    public bool Boards { get; set; }
    public ConsoleWindow(Point from, Point to, object lockObj, object lockMessages, ConsoleColor ForegroundColor = ConsoleColor.White, ConsoleColor BackgroundColor = ConsoleColor.Black)
    {
        this.from = from;
        this.to = to;
        this.ForegroundColor = ForegroundColor;
        this.BackgroundColor = BackgroundColor;
        this.lockObj = lockObj;
        this.lockMessages = lockMessages;
    }

    public void DrawBoards()
    {
        lock (lockObj)
        {
            for (int i = from.Y - 1; i < to.Y; i++)
            {
                Console.SetCursorPosition(from.X - 1, i);
                Console.BackgroundColor = ConsoleColor.White;
                Console.Write(" ");
                Console.ResetColor();
            }
            for (int i = from.Y - 1; i < to.Y + 0; i++)
            {
                Console.SetCursorPosition(to.X + 1, i);
                Console.BackgroundColor = ConsoleColor.White;
                Console.Write(" ");
                Console.ResetColor();
            }
            for (int j = from.X - 1; j < to.X + 1; j++)
            {
                Console.SetCursorPosition(j, from.Y - 1);
                Console.BackgroundColor = ConsoleColor.White;
                Console.Write(" ");
                Console.ResetColor();
            }
            for (int j = from.X - 1; j < to.X + 2; j++)
            {
                Console.SetCursorPosition(j, to.Y);
                Console.BackgroundColor = ConsoleColor.White;
                Console.Write(" ");
                Console.ResetColor();
            }
        }
    }

    public void Draw()
    {
        lock (lockObj)
            lock (lockMessages)
            {
                int i = 0;
                int j = 0;
                if (Boards == true)
                    DrawBoards();
                foreach (var messages in text)
                {
                    Console.BackgroundColor = BackgroundColor;
                    Console.ForegroundColor = ForegroundColor;
                    for (int h = 0; h < messages.Length; h++)
                    {
                        if (i == to.X - from.X)
                        {
                            i = 0;
                            j++;
                        }

                        if (messages[h] == '\n')
                        {
                            i = 0;
                            j++;
                        }
                        else
                        {
                            Console.SetCursorPosition(from.X + i++, from.Y + j);
                            Console.Write(messages[h]);
                        }
                    }
                    Console.ResetColor();
                }
            }

    }

    public void DrawBackground()
    {
        lock (lockObj)
        {
            for (int i = from.Y; i < to.Y; i++)
            {
                for (int j = from.X; j < to.X + 1; j++)
                {
                    Console.SetCursorPosition(j, i);
                    Console.BackgroundColor = BackgroundColor;
                    Console.Write(" ");
                    Console.ResetColor();
                }
            }
        }
    }

    public void Clear()
    {
        lock (lockObj)
            lock (lockMessages)
            {
                text.Clear();
                DrawBackground();
            }
    }
    public void WriteLine(string message)
    {
        lock (lockMessages)
        {
            if (text.Count() >= to.Y - from.Y - 1)
            {
                text.Remove(text[0]);
            }
            while (message.Length > to.X - from.X)
            {
                if (text.Count() >= to.Y - from.Y - 1)
                {
                    text.Remove(text[0]);
                }
                string a = "";
                for (int i = 0; i < to.X - from.X; i++)
                {
                    a += message[0];
                    message = message.Remove(0, 1);
                }
                text.Add(a);
                a = "";
            }

            text.Add(message + "\n");
        }
    }

    public void Write(string message)
    {
        lock (lockMessages)
        {

            while (message.Length > to.X - from.X)
            {
                if (text.Count() > to.Y - from.Y)
                {
                    text.Remove(text[0]);

                }
                string a = "";
                for (int i = 0; i < to.X - from.X; i++)
                {
                    a += message[0];
                    message = message.Remove(0, 1);
                }

                text.Add(a);
                a = "";
            }
            if (text.Last().Length + message.Length < to.X - from.X && text.Last()[text.Last().Length - 1] != '\n')
            {
                text[text.Count() - 1] = text.Last() + message;
            }
            else
            {
                text.Add(message);
            }
        }
    }
}

class FTPClient
{
    static string ftpHost = "ftp://ftpupload.net";
    static string username = "if0_38982377";
    static string password = "YwtyTohUV5";
    static readonly object lockObj = new object();
    static readonly object lockMessages = new object();
    static Random rnd = new Random();
    static string FtpListDirectory(string path="")
    {
        try
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"{ftpHost}/{path}");
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential(username, password);
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            return reader.ReadToEnd();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error {ex.Message}");
        }
        return "";
    }
    static void FtpDownloadFile(string remotePath, string localPath="")
    {
        try
        {
            if (localPath.Length == 0) localPath = $"{Path.GetDirectoryName(Environment.ProcessPath)}/{Path.GetFileName(remotePath)}";
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"{ftpHost}/{remotePath}");
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(username, password);
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            using (FileStream ft = new FileStream(localPath, FileMode.Create))
            {
                responseStream.CopyTo(ft);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error {ex.Message}");
        }
    }
    static void FtpRemoveFile(string Path)
    {
        try
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"{ftpHost}/{Path}");
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            request.Credentials = new NetworkCredential(username, password);
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error {ex.Message}");
        }
    }
    static void FtpUploadFile(string localPath, string remotePath)
    {
        try
        {
            //if (localPath.Length == 0) localPath = $"{Path.GetDirectoryName(Environment.ProcessPath)}/{Path.GetFileName(remotePath)}";
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"{ftpHost}/{remotePath}");
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(username, password);
            byte[] fileContent = File.ReadAllBytes(localPath);
            request.ContentLength = fileContent.Length;
            using (Stream responseStream = request.GetRequestStream())
            {
                responseStream.Write(fileContent, 0, fileContent.Length);
            }
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error {ex.Message}");
        }
    }
    public static uint Menu(IEnumerable<string> Action)
    {
        uint active = 0;
        while (true)
        {
            lock (lockObj)
            {
                Console.SetCursorPosition(0, 1);
                for (int i = 0; i < Action.Count(); i++)
                {

                    if (i == active)
                        Console.WriteLine($" > {Action.ElementAt(i)}");
                    else
                        Console.WriteLine($"   {Action.ElementAt(i)}");
                }
            }

            if (Console.KeyAvailable)
            {
                ConsoleKey key = Console.ReadKey(true).Key;
                if (active > 0 && (key == ConsoleKey.UpArrow || key == ConsoleKey.W))
                    active--;
                else if ((key == ConsoleKey.DownArrow || key == ConsoleKey.S) && active < Action.Count() - 1)
                    active++;
                else if (key == ConsoleKey.Enter)
                {
                    //Console.Clear();
                    return active;
                }
            }
        }
    }
    public static void Window()
    {
        lock (lockObj)
        {
            Console.SetCursorPosition(40, 0);
            Console.WriteLine(FtpListDirectory());
        }
    }

    static ConsoleWindow window = new ConsoleWindow(new Point(40,0), new Point(60,20), lockObj, lockMessages, ConsoleColor.White, ConsoleColor.Green);
    static void Main(string[] args)
    {
        Console.OutputEncoding = UTF8Encoding.UTF8;
        Console.InputEncoding = UTF8Encoding.UTF8;
        window.DrawBackground();

        /*
                Console.WriteLine(FtpListDirectory());
                *//*FtpDownloadFile("ABOBA/text.txt");*//*

                Console.WriteLine("Deleting");
                FtpRemoveFile("ABOBA/text.txt");

                Console.WriteLine("Uploading");
                FtpUploadFile("C:\\Users\\student\\source\\repos\\Server NP_P35\\Server NP_P35\\bin\\Debug\\net7.0\\text.txt", "ABOBA/text.txt");*/
        /*Console.WriteLine(FtpListDirectory());*/
        new Thread(() => { Console.SetCursorPosition(0, 0); Console.WriteLine("=====MENU====="); }).Start();
        new Thread(() =>
        {
            while (true)
            {
                window.DrawBackground();
                window.Draw();
                lock (lockObj)
                {
                    Console.SetCursorPosition(0, 25);
                }
                Thread.Sleep(1400);
            }
        }).Start();
        
        List<string> Action = new List<string> {
                "Download file",
                "Remove file",
                "Upload file",
                "Print file",
                "Exit"
            };
        new Thread(() =>
        {
            while (true)
            {

                switch (Menu(Action))
                {
                    case 0: Console.Write("Frite way to file: "); var a = Console.ReadLine(); Console.WriteLine("Downloading..."); FtpDownloadFile(a); break;
                    case 1:
                        Console.Write("Frite way to file: "); var b = Console.ReadLine(); Console.WriteLine("Removing..."); FtpRemoveFile(b); break;
                    /*case 2:
                        Thread b = new Thread(() => start(5)); b.Start(); break;*/
                    case 3:
                        window.Write(FtpListDirectory()); break;
                    case 4: Environment.Exit(0); break;
                        // Menu(new List<string> { "Back", "Another back" })
                }
            }
        }).Start();
    }
}
