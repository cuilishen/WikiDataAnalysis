﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;
using ICSharpCode.SharpZipLib.BZip2;
using System.IO;

namespace DownloadWikiData
{
    class Program
    {
        const string titleListUrl = "https://dumps.wikimedia.org/zhwiki/20180501/zhwiki-20180501-pages-articles-multistream-index.txt.bz2";
        const string curlUrl = "https://zh.wikipedia.org/zh-tw/";//+"数学";
        static async Task<List<string>>GetTitleList()
        {
            HttpClient client = new HttpClient();
            Console.WriteLine("Downloading...");
            var stream = new MemoryStream();
            BZip2.Decompress(await client.GetStreamAsync(titleListUrl), stream, true);
            Console.WriteLine("Done");
            using (var f = new FileStream("tmp.txt", FileMode.OpenOrCreate))
            {
                var b = stream.ToArray();
                f.Write(b, 0, b.Length);
                f.Close();
            }
            var s = Encoding.UTF8.GetString(stream.ToArray()).Split('\n').Select(v =>
            {
                return v.Substring(v.IndexOf(':', v.IndexOf(':') + 1) + 1);
            }).ToArray();
            var blackList = new string[]
            {
                "Wikipedia:删除纪录/档案馆/2004年3月"
            };
            List<string> ans = new List<string>();
            foreach (var v in s) if (!blackList.Contains(v)) ans.Add(v);
            return ans;
        }
        static Random rand = new Random();
        static async void Run()
        {
            var s = await GetTitleList();
            using (StreamWriter writer = new StreamWriter("output.txt", true, Encoding.UTF8))
            {
                int progress = 0;
                int iterationCount = 1000;
                Parallel.For(0, iterationCount, _ =>
                {
                    int i = rand.Next(s.Count);
                    var url = curlUrl + System.Net.WebUtility.UrlEncode(s[i].Replace(' ', '_'));
                    Console.WriteLine(url);
                    Console.Write($"Downloading... ({System.Threading.Interlocked.Increment(ref progress)}/{iterationCount})".PadRight(Console.WindowWidth - 1, ' ') + "\r");
                    try
                    {
                        HttpClient client = new HttpClient();
                        string webContent = client.GetStringAsync(url).Result;
                        Console.Write($"Processing... ({webContent.Length})".PadRight(Console.WindowWidth - 1, ' ') + "\r");
                        webContent = new Runner().Run(webContent);
                        Console.Write($"Writing... ({webContent.Length})".PadRight(Console.WindowWidth - 1, ' ') + "\r");
                        lock (writer)
                        {
                            writer.WriteLine($"\r\n==================={s[i]}====================\r\n");
                            writer.WriteLine(webContent);
                        }
                        Console.Write($"Done ({webContent.Length})".PadRight(Console.WindowWidth - 1, ' ') + "\r");
                    }
                    catch (Exception error) { Console.WriteLine(error); }
                    //await Task.Delay(1000);
                });
                writer.Close();
            }
            Console.WriteLine("All Done.");
        }
        static async void Run1()
        {
            HttpClient client = new HttpClient();
            Console.WriteLine("Downloading...");
            string webContent = await client.GetStringAsync(curlUrl);
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter("origin.html", false, Encoding.UTF8))
            {
                writer.WriteLine(webContent);
                writer.Close();
            }
            Console.WriteLine("Processing...");
            webContent = new Runner().Run(webContent);
            Console.WriteLine("Writing...");
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter("output.txt", false, Encoding.UTF8))
            {
                writer.WriteLine(webContent);
                writer.Close();
            }
            Console.WriteLine("Done");
            Console.WriteLine(webContent);
        }
        static void Main(string[] args)
        {
            Run();
            Console.ReadLine();
        }
    }
}