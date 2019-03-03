using System;
using OpenTK;
using OpenTK.Input;
using CCEngine;
using CCEngine.ECS;
using CCEngine.Simulation;

namespace CCEngine
{
	public class MsgGotoState : IMessage
	{
		public int state;

		public LogLevel LogLevel { get { return LogLevel.Debug; } }

		public MsgGotoState(int state)
		{
			this.state = state;
		}

		public override string ToString()
		{
			return "Goto state {0}".F(this.state);
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

		public LogLevel LogLevel { get { return LogLevel.Debug; } }

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
}
