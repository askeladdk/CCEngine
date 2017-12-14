using System.Collections.Generic;
using System.Linq;
using CCEngine.Collections;
using CCEngine.Algorithms;
using CCEngine.ECS;

namespace CCEngine.Simulation
{
	class ActionMove : Action<CRadio>
	{
		private MissionMove mission;

		public override bool IsBlocking { get => true; }
		public override uint Lane { get => CRadio.LaneMovement; }

		public ActionMove(MissionMove mission)
		{
			this.mission = mission;
		}

		public override void Start(CRadio ctx)
		{
			HasStarted = true;

			var g = Game.Instance;
			CLocomotion loco;

			if(g.Registry.TryGetComponent<CLocomotion>(mission.EntityID, out loco))
			{
				var field = new SingularFlowField<MovementZone>(g.Map,
					loco.Position.CPos, mission.Destination, MovementZone.Track);
				loco.MoveTo(field);
			}
		}

		public override bool Process(CRadio ctx)
		{
			return true;
		}
	}

	public class CRadio : IComponent
	{
		public const uint LaneMovement = 1;

		private ActionList<CRadio> actionList;

		public ActionList<CRadio> ActionList { get => actionList; }

		public void Process()
		{
			actionList.Process(this);
		}

		public void Push(IMission mission)
		{
			Action<CRadio> action = null;

			switch(mission.MissionType)
			{
				case MissionType.Move:
					action = new ActionMove(mission as MissionMove);
					break;
			}

			if(action != null)
				actionList.Push(action);
		}

		public void Initialise(IAttributeTable table)
		{
			actionList = new ActionList<CRadio>();
		}
	}

	public class PRadioReceiver : QueueProcessor<IMessage>, IMessageHandler
	{
		private PRadioReceiver(Registry registry) : base(registry) { }

		public override bool IsActive { get => true; }
		public override bool IsRenderLoop { get => false; }
		protected override int MaxMessagesPerFrame { get => 0; }

		protected override void Process(float dt, IMessage message)
		{
			var g = Game.Instance;

			if(message is IMission)
			{
				var mission = message as IMission;
				var entityId = mission.EntityID;
				CRadio radio;
				if(Registry.TryGetComponent<CRadio>(entityId, out radio))
				{
					radio.Push(mission);
				}
			}
		}

		public static PRadioReceiver Attach(Registry registry)
		{
			return new PRadioReceiver(registry);
		}
	}

	public class PRadio : SingleProcessor
	{
		private IFilter filter;

		private PRadio(Registry registry) : base(registry)
		{
			filter = registry.Filter.All(typeof(CRadio));
		}

		protected override IFilter Filter { get => filter; }
		public override bool IsActive { get => true; }
		public override bool IsRenderLoop { get => false; }

		protected override void Process(float dt, int entityId)
		{
			var radio = Registry.GetComponent<CRadio>(entityId);
			radio.Process();
		}

		public static PRadio Attach(Registry registry)
		{
			return new PRadio(registry);
		}
	}
}