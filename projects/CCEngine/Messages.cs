using System;
using OpenTK;
using OpenTK.Input;
using CCEngine;
using CCEngine.ECS;
using CCEngine.Simulation;

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

		public int LogLevel { get { return Logger.NEVER; } }

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


	public class MsgMouseButton : IMessage
	{
		public MouseButtonEventArgs e;

		public int LogLevel { get { return Logger.NEVER; } }

		public MsgMouseButton(MouseButtonEventArgs e)
		{
			this.e = e;
		}

		public override string ToString()
		{
			return "Mouse button ({0}, {1})".F(e.Button, e.IsPressed);
		}
	}

	public class MsgSpawnEntity : IMessage
	{
		private string id;
		private TechnoType technoType;
		private IAttributeTable table;

		public string ID { get { return this.id; } }
		public TechnoType TechnoType { get { return this.technoType; } }
		public IAttributeTable Table { get { return this.table; } }

		public int LogLevel { get { return Logger.DEBUG; } }

		public MsgSpawnEntity(string id, TechnoType technoType, IAttributeTable table)
		{
			this.id = id;
			this.technoType = technoType;
			this.table = table;
		}

		public override string ToString()
		{
			return "Spawn ({0})".F(this.id);
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
			return "Entity #{0} killed".F(entityId);
		}
	}
}
