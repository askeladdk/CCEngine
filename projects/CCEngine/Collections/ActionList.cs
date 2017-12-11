using System;
using System.Collections.Generic;

namespace CCEngine.Collections
{
	public abstract class Action<Context>
	{
		private bool hasStarted;
		private bool isBlocked;

		public bool HasStarted
		{
			get => hasStarted;
			protected set => hasStarted = value;
		}

		public bool IsBlocked
		{
			get => isBlocked;
			protected set => isBlocked = value;
		}

		public abstract bool IsBlocking { get; }
		public abstract uint Lane { get; }

		public virtual bool Process(Context ctx)
		{
			return true;
		}

		public virtual void Start(Context ctx)
		{
			HasStarted = true;
		}

		public virtual void Block(Context ctx)
		{
			IsBlocked = true;
		}

		public virtual void Unblock(Context ctx)
		{
			IsBlocked = false;
		}
	}

	/// <summary>
	/// Generic action list.
	/// https://web.archive.org/web/20130419075542/http://sonargame.com/2011/06/05/action-lists/
	/// </summary>
	public sealed class ActionList<Context>
	{
		private readonly LinkedList<Action<Context>> actions = new LinkedList<Action<Context>>();

		public void Push(Action<Context> action)
		{
			actions.AddFirst(action);
		}

		public void Process(Context ctx)
		{
			uint laneMask = 0;

			for (var node = actions.First; node != null; node = node.Next)
			{
				var action = node.Value;
				var blocked = (action.Lane & laneMask) != 0;

				if (action.HasStarted)
				{
					if (blocked && !action.IsBlocked)
						action.Block(ctx);
					else if (!blocked && action.IsBlocked)
						action.Unblock(ctx);
				}

				if (blocked)
					continue;

				if (!action.HasStarted)
					action.Start(ctx);

				if (action.Process(ctx))
				{
					actions.Remove(node);
					continue;
				}

				if (action.IsBlocking)
					laneMask |= action.Lane;
			}
		}

		public void Clear()
		{
			actions.Clear();
		}

		public void Clear(uint laneMask)
		{
			var node = actions.First;
			while (node != null)
			{
				var action = node.Value;
				var next = node.Next;

				if ((action.Lane & laneMask) != 0)
					actions.Remove(node);

				node = next;
			}
		}

		public bool HasLane(uint laneMask)
		{
			foreach (var action in actions)
				if ((action.Lane & laneMask) != 0)
					return true;
			return false;
		}
	}
}
