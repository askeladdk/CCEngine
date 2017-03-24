﻿using System;
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

	public class MsgMouseMove : IMessage
	{
		public MouseMoveEventArgs e;
		
		public int LogLevel { get { return Logger.NEVER; } }

		public MsgMouseMove(MouseMoveEventArgs e)
		{
			this.e = e;
		}

		public override string ToString()
		{
			return "Mouse position ({0}, {1})".F(e.X, e.Y);
		}
	}

	public class MsgSpawnEntity : IMessage
	{
		public int entityId;

		public int LogLevel { get { return Logger.DEBUG; } }

		public MsgSpawnEntity(int entityId)
		{
			this.entityId = entityId;
		}

		public override string ToString()
		{
			return "Spawn entity #{0}".F(entityId);
		}
	}

	public class MsgKillEntity : IMessage
	{
		public int entityId;

		public int LogLevel { get { return Logger.DEBUG; } }

		public MsgKillEntity(int entityId)
		{
			this.entityId = entityId;
		}

		public override string ToString()
		{
			return "Kill entity #{0}".F(entityId);
		}
	}
}
