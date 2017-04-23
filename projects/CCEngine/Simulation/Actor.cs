using System;
using System.Drawing;
using System.Collections.Generic;

namespace CCEngine.Simulation
{

	public interface IHealth
	{
		int HP { get; }
		int MaxHP { get; }
		int TakeDamage(int amount);
	}

	public interface IPosition
	{
		Point WorldPos { get; }
		int Facing { get; set; }
	}

	public interface IOccupySpace
	{
		Point TopLeft { get; }
		Point CenterPos { get; }
		Foundation Foundation { get; }
	}

	public interface IActor
	{
		SpriteArt SpriteArt { get;  }
		IPosition Position { get;  }
	}

	public interface IActorBase
	{
		TComponent GetComponent<TComponent>();
	}

	public interface ITechnoBase
	{

	}

	public interface IActorRenderable
	{
		SpriteArt SpriteArt { get; }
		IPosition Position { get; }
	}

	public class ActorPosition : IPosition
	{
		private Point position;
		private int facing;

		public Point WorldPos { get { return position; } }

		public int Facing
		{
			get { return facing; }
			set { facing = value; }
		}
	}

	public class Techno : IActor, IActorRenderable
	{
		private TechnoType technoType;
		private IPosition position;

		public Techno(TechnoType technoType)
		{
			position = new ActorPosition();
		}

		public TechnoTypes Type { get { return technoType.Type; } }
		public SpriteArt SpriteArt { get { return technoType.Art; } }
		public IPosition Position { get { return position; } }
	}

	public abstract class ActorProcessor<TEntityView>
	{
		private Dictionary<int, TEntityView> views = new Dictionary<int, TEntityView>();

		public void Process()
		{
			var t = typeof(IActor);
			foreach(var e in views)
				Process(e.Value);
		}

		protected abstract void Process(TEntityView entity);
	}

	public class ActorRegistry
	{
		private Dictionary<int, IActor> entities = new Dictionary<int, IActor>();
		private int idCounter = 0;

		public event Action<int> OnEntityAdded;
		public event Action<int> OnEntityKilled;

		public TEntityView GetAs<TEntityView>(int entityId)
		{
			return (TEntityView)entities[entityId];
		}

		public int Spawn(IActor entity)
		{
			var entityId = ++idCounter;
			entities[entityId] = entity;
			OnEntityAdded.Invoke(entityId);
			return entityId;
		}
	}
}
