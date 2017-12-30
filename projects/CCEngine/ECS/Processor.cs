using System;
using System.Collections.Generic;
using System.Linq;

namespace CCEngine.ECS
{
	/// <summary>
	/// Abstract entity processor.
	/// </summary>
	public abstract class Processor : IDisposable
	{
		private Registry registry;

		public abstract bool IsActive { get; }
		public abstract bool IsRenderLoop { get; }
		public abstract void Process(object e);

		public virtual void Dispose()
		{
			registry.RemoveProcessor(this);
		}

		protected Processor(Registry registry)
		{
			registry.AddProcessor(this);
			this.registry = registry;
		}

		protected Registry Registry
		{
			get { return registry; }
		}
	}

	/// <summary>
	/// Abstract entity processor that processes a single entity at a time.
	/// Processing time is O(n) where n is the number of filtered entities.
	/// </summary>
	public abstract class SingleProcessor : Processor
	{
		protected abstract IFilter Filter { get; }
		protected abstract void Process(object e, int entityId);

		protected SingleProcessor(Registry registry) : base(registry) { }

		public override void Process(object e)
		{
			foreach (var entityId in Filter.Entities)
				Process(e, entityId);
		}
	}

	/// <summary>
	/// Abstract entity processor that processes all unique non-reflexive pairs of entities.
	/// Processing time is O(n^2) where n is the number of filtered entities.
	/// </summary>
	public abstract class PairProcessor : Processor
	{
		protected abstract IFilter Filter { get; }
		protected abstract void Process(object e, int entityId1, int entityId2);

		protected PairProcessor(Registry registry) : base(registry) { }

		public override void Process(object e)
		{
			var entities = Filter.Entities;
			var length = entities.Count();
			for (var i = 0; i < length; i++)
				for (var j = 1 + i; j < length; j++)
					Process(e, entities.ElementAt(i), entities.ElementAt(j));
		}
	}

	public abstract class QueueProcessor<TMessage> : Processor
	{
		private Queue<TMessage> messages = new Queue<TMessage>();

		public void OnMessage(TMessage message)
		{
			messages.Enqueue(message);
		}

		protected abstract int MaxMessagesPerFrame { get; }

		protected abstract void Process(object e, TMessage message);

		protected QueueProcessor(Registry registry) : base(registry) { }

		public override void Process(object e)
		{
			int max = MaxMessagesPerFrame;
			int count = messages.Count;
			if (max > 0)
				count = Math.Min(MaxMessagesPerFrame, messages.Count);
			while (count-- > 0)
				Process(e, messages.Dequeue());
		}
	}
}
