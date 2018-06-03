﻿using Legends.Configurations;
using Legends.Core.DesignPattern;
using Legends.Core.Protocol;
using Legends.Core.Utils;
using Legends.Network;
using Legends.Records;
using Legends.World.Champions;
using Legends.World.Commands;
using SmartORM;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Legends
{
    class Program
    {
        static Logger logger = new Logger();

        public const string DATABASE_FILENAME = "database.smart";

        static void Main(string[] args)
        {
            logger.OnStartup();
            StartupManager.Instance.Initialize(Assembly.GetAssembly(typeof(AIUnitRecord)));
            logger.Write("Server started");
            Process.Start("StartGame.bat");
            Process.Start("StartGame2.bat");
            // Process.Start("StartGame3.bat");
            LoLServer.NetLoop();

            Console.ReadKey();
        }
        [StartupInvoke("SmartDB", StartupInvokePriority.First)]
        public static void LoadDatabase()
        {
            DatabaseManager.Instance.Initialize(Environment.CurrentDirectory + "/" + DATABASE_FILENAME, Assembly.GetAssembly(typeof(AIUnitRecord)));
            DatabaseManager.Instance.LoadTables();


        }
        [StartupInvoke("Protocol", StartupInvokePriority.Second)]
        public static void LoadProtocol()
        {
            ProtocolManager.Initialize(Assembly.GetExecutingAssembly(), true);
        }
    }
}