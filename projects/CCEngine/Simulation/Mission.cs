using CCEngine.ECS;

namespace CCEngine.Simulation
{
	public enum MissionType
	{
		Move,
	}

	public interface IMission : IMessage
	{
		Entity Entity { get; }
		MissionType MissionType { get; }
	}

	public class MissionMove : IMission
	{
		private Entity entity;
		private CPos destination;

		public LogLevel LogLevel { get => LogLevel.Debug; }
		public Entity Entity { get => entity; }
		public MissionType MissionType { get => MissionType.Move; }
		public CPos Destination { get => destination; }

		public MissionMove(Entity entity, CPos destination)
		{
			this.entity = entity;
			this.destination = destination;
		}

		public override string ToString()
		{
			return "Entity {0} -> Move To {1}".F(entity, destination);
		}
	}
}