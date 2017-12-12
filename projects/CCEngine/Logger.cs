using System;
using CCEngine;

namespace CCEngine
{
	public enum LogLevel
	{
		Never,
		Debug,
		Info,
		Warn,
		Error,
	}

	public class Logger : IMessageHandler
	{
		public const int ERROR = 0;
		public const int WARN  = 1;
		public const int INFO  = 2;
		public const int DEBUG = 3;
		public const int NEVER = 4;

		private static readonly string[] prefix =
		{
			"[NEVER]",
			"[DEBUG]",
			"[INFO ]",
			"[WARN ]",
			"[ERROR]",
		};

		private int minlevel;

		public Logger(Game p, LogLevel minlevel)
		{
			this.minlevel = (int)minlevel;
		}

		public void Log(LogLevel priority, string fmt, params object[] args)
		{
			var pr = (int)priority;
			if(pr >= this.minlevel)
				Console.WriteLine("{0} {1}".F(prefix[pr], fmt.F(args)));
		}

		public void OnMessage(IMessage msg)
		{
			this.Log(msg.LogLevel, "{0}", msg);
		}
	}
}
