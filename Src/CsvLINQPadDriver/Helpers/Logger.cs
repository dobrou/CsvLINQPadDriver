﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CsvLINQPadDriver.Helpers
{
    public class Logger : List<string>
    {
        public static bool LogEnabled = false;
        public static readonly Logger Instance = new Logger();
        
        private readonly Stopwatch sw = new Stopwatch();

        public static void Log(string str, params object[] parameters)
        {
            if (!LogEnabled)
                return;
            Instance.AddLog(str,parameters);
        }

        private void AddLog(string str, params object[] parameters)
        {
            if (!LogEnabled)
                return;
            Add(string.Format(DateTime.Now.ToString("HH:mm:ss.fff") + ": (" + sw.ElapsedMilliseconds + "ms) " + str, parameters));
            sw.Restart();
        }

        public override string ToString()
        {
            return string.Join("\n", this);
        }

        public string[] Last10()
        {
            return Enumerable.Reverse(this).Take(10).ToArray();
        }
    }
}
