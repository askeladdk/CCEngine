using System;
using System.Runtime.Serialization;

namespace CCEngine.ECS
{
	public class NotFound : Exception
	{
		public NotFound()
			: base()
		{ }

		public NotFound(string message)
			: base(message)
		{ }

		public NotFound(string message, Exception innerException)
			: base(message, innerException)
		{ }

		public NotFound(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }
	}
}
