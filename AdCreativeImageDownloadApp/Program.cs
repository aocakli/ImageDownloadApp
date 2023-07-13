

using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;

class Program
{
    static CancellationTokenSource cantoken;
    static int count = 1;
    static int parallelLimit = 1;
    static string path = "outputs";

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Enter the number of images to download:");
        count = Convert.ToInt32(Console.ReadLine());


        Console.WriteLine("Enter the maximum parallel download limit:");
        parallelLimit = Convert.ToInt32(Console.ReadLine());

        Console.WriteLine("Enter the save path (default: ./outputs)");
        var tempPath = Console.ReadLine();
        if (!string.IsNullOrEmpty(tempPath))
        {
            path = tempPath;
        }

        CheckFolder(path);


        cantoken = new CancellationTokenSource();

        Console.CancelKeyPress += Console_CancelKeyPress;



        await DownloadP(count);
        Console.ReadLine();
    }
    static int processIndex = 1;
    private static async void DownloadFile(int index)
    {

        using (WebClient client = new WebClient())
        {

            client.DownloadFileAsync(new Uri("https://picsum.photos/200/300"), $"{path}/" + index + ".png");
            //client.DownloadFileCompleted += Client_DownloadFileCompleted;

        }
    }
    static int y = 0;
    private static void Client_DownloadFileCompleted(object? sender, AsyncCompletedEventArgs e)
    {
        Console.Write($"Progress: {processIndex} / {count}");
        Console.SetCursorPosition(0, y);
        processIndex++;
    }

    public static async Task DownloadP(int count)
    {
        var parallelOptions = new ParallelOptions()
        {
            MaxDegreeOfParallelism = parallelLimit,
            CancellationToken = cantoken.Token,
        };
        Console.WriteLine($"Downloading {count} images ({parallelLimit} parallel downloads at most)");
        y = Console.GetCursorPosition().Top;

        try
        {
            var tsk = await Task.Factory.StartNew(() => Parallel.For(1, count+1, parallelOptions, i =>
            {
                
                DownloadFile(i);
                ClearCurrentConsoleLine();
                Console.Write($"Progress: {i} / {count}");
            }));
        }
        catch (Exception ex)
        {
            ClearCurrentConsoleLine();
            Console.WriteLine(ex.Message);
            CheckFolder(path);
        }        
    }

    public static void ClearCurrentConsoleLine()
    {
        int currentLineCursor = Console.CursorTop;
        Console.SetCursorPosition(0, Console.CursorTop);
        for (int i = 0; i < Console.WindowWidth; i++)
            Console.Write(" ");
        Console.SetCursorPosition(0, currentLineCursor);
    }

    private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        cantoken.Cancel();
    }

    static void CheckFolder(string _path)
    {
        var fullPath = System.AppDomain.CurrentDomain.BaseDirectory + _path;
        if (Directory.Exists(fullPath))
        {
            Directory.Delete(fullPath, true);
            Directory.CreateDirectory(fullPath);
        }
        else
        {
            Directory.CreateDirectory(_path);
        }
    }

}
