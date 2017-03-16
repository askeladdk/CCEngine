using System;
using System.Collections.Generic;

namespace CCEngine
{
	public interface IMessage
	{
		int LogLevel { get; }
	}

	public interface IMessageHandler
	{
		void HandleMessage(IMessage message);
	}

	public class MessageBus
	{
		private readonly HashSet<IMessageHandler> handlers = new HashSet<IMessageHandler>();
		private readonly Queue<IMessage> queue = new Queue<IMessage>();

		public void Subscribe(IMessageHandler handler)
		{
			this.handlers.Add(handler);
		}

		public void Unsubscribe(IMessageHandler handler)
		{
			this.handlers.Remove(handler);
		}

		public void SendMessage(IMessage message)
		{
			this.queue.Enqueue(message);
		}

		public void ProcessMessages()
		{
			while(this.queue.Count > 0)
			{
				IMessage m = this.queue.Dequeue();
				foreach (var h in this.handlers)
					h.HandleMessage(m);
			}
		}
	}
}
