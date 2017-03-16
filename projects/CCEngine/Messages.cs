using System;
using OpenTK;
using OpenTK.Input;
using CCEngine;

namespace CCEngine
{
	public class MsgLog : IMessage
	{
		private string s;
		private int priority;

		public int LogLevel { get { return this.priority; } }

		public MsgLog(string s, int priority = 0)
		{
			this.s = s;
			this.priority = priority;
		}

		public override string ToString()
		{
			return this.s;
		}
	}

	public class MsgKeyDown : IMessage
	{
		public KeyboardKeyEventArgs e;

		public int LogLevel { get { return Logger.DEBUG; } }

		public MsgKeyDown(KeyboardKeyEventArgs e)
		{
			this.e = e;
		}

		public override string ToString()
		{
			return "Pressed {0}".F(e.Key);
		}
	}

	public class MsgGotoState : IMessage
	{
		public int state;

		public int LogLevel { get { return Logger.DEBUG; } }

		public MsgGotoState(int state)
		{
			this.state = state;
		}

		public override string ToString()
		{
			return "Goto state {0}".F(this.state);
		}
	}
}
