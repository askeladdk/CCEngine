using System;
using CCEngine;

namespace CCEngine
{
	public class Logger : IMessageHandler
	{
		public const int ERROR = 0;
		public const int WARN  = 1;
		public const int INFO  = 2;
		public const int DEBUG = 3;
		public const int NEVER = 4;

		private static readonly string[] prefix =
		{
			"[ERROR]",
			"[WARN ]",
			"[INFO ]",
			"[DEBUG]",
		};

		private int loglevel;

		public Logger(Game p, int loglevel)
		{
			this.loglevel = Math.Min(loglevel, prefix.Length - 1);
		}

		public void HandleMessage(IMessage msg)
		{
			if (this.loglevel >= msg.LogLevel)
			{
				Console.WriteLine("{0} {1}".F(prefix[msg.LogLevel], msg));
			}
		}
	}
}
