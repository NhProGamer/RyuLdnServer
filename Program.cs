﻿using System;
using System.Net;
using LanPlayServer.Stats;
using LanPlayServer.Utils;
using System.IO;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace LanPlayServer
{
    static class Program
    {
        private static readonly IPAddress Host = IPAddress.Parse(Environment.GetEnvironmentVariable("LDN_HOST") ?? "0.0.0.0");
        private static readonly int Port = int.Parse(Environment.GetEnvironmentVariable("LDN_PORT") ?? "30456");
        private static readonly string GamelistPath = Environment.GetEnvironmentVariable("LDN_GAMELIST_PATH") ?? "Utils/gamelist.json";
        private static readonly string StatsDirectory = Environment.GetEnvironmentVariable("LDN_STATS_DIRECTORY");
        private static readonly int IntervalMinutes =
            int.Parse(Environment.GetEnvironmentVariable("LDN_STATS_INTERVAL") ?? "2");

        private static LdnServer _ldnServer;
        private static Timer _statsTimer;

        static void Main()
        {
            Console.WriteLine();
            Console.WriteLine( "__________                     __ .__                  .____         .___        ");
            Console.WriteLine(@"\______   \ ___.__. __ __     |__||__|  ____  ___  ___ |    |      __| _/  ____  ");
            Console.WriteLine(@" |       _/<   |  ||  |  \    |  ||  | /    \ \  \/  / |    |     / __ |  /    \ ");
            Console.WriteLine(@" |    |   \ \___  ||  |  /    |  ||  ||   |  \ >    <  |    |___ / /_/ | |   |  \");
            Console.WriteLine(@" |____|_  / / ____||____/ /\__|  ||__||___|  //__/\_ \ |_______ \\____ | |___|  /");
            Console.WriteLine(@"        \/  \/            \______|         \/       \/         \/     \/      \/ ");
            Console.WriteLine();
            Console.WriteLine( "_________________________________________________________________________________");
            Console.WriteLine();
            Console.WriteLine("- Information");

            Console.Write($"\tReading '{GamelistPath}'...");
            GameList.Initialize(File.ReadAllText(GamelistPath));
            Console.WriteLine(" Done!");

            _statsTimer = new(TimeSpan.FromMinutes(IntervalMinutes))
            {
                AutoReset = true,
            };

            _statsTimer.Elapsed += DumpStats;
            _statsTimer.Start();

            _ldnServer = new(Host, Port);

            Console.Write($"\tLdnServer (port: {Port}) starting...");
            _ldnServer.Start();
            Console.WriteLine(" Done!");

            while (_ldnServer.IsAccepting)
            {
                Thread.Sleep(100);
            }

            _ldnServer.Dispose();
            _statsTimer.Close();
        }

        static void DumpStats(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Console.WriteLine($"[{elapsedEventArgs.SignalTime}] [StatsDumper] Writing json files...");
            StatsDumper.WriteJsonFiles(_ldnServer.All(), StatsDirectory).Wait();
            Console.WriteLine($"[{DateTime.Now}] [StatsDumper] Done.");
        }
    }
}