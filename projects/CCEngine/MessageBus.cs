using System;
using System.Collections.Generic;

namespace CCEngine
{
	public interface IMessage
	{
		LogLevel LogLevel { get; }
	}

	public interface IMessageHandler
	{
		void OnMessage(IMessage message);
	}

	public class MessageBus
	{
		private readonly Queue<IMessage> queue = new Queue<IMessage>();
		private event Action<IMessage> OnMessage;

		public void Subscribe(IMessageHandler handler)
		{
			this.OnMessage += handler.OnMessage;
		}

		public void Unsubscribe(IMessageHandler handler)
		{
			this.OnMessage -= handler.OnMessage;
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
				this.OnMessage.Invoke(m);
			}
		}
	}

	public static class MessageExtensions
	{
		public static bool Is<T>(this IMessage msgIn, out T msgOut)
			where T : class, IMessage
		{
			msgOut = msgIn as T;
			return msgOut != null;
		}
	}
}
