namespace CCEngine.Simulation
{
	public enum MissionType
	{
		Move,
	}

	public interface IMission : IMessage
	{
		int EntityID { get; }
		MissionType MissionType { get; }
	}

	public class MissionMove : IMission
	{
		private int entityId;
		private CPos destination;

		public LogLevel LogLevel { get => LogLevel.Debug; }
		public int EntityID { get => entityId; }
		public MissionType MissionType { get => MissionType.Move; }
		public CPos Destination { get => destination; }

		public MissionMove(int entityId, CPos destination)
		{
			this.entityId = entityId;
			this.destination = destination;
		}

		public override string ToString()
		{
			return "Entity {0} -> Move To {1}".F(entityId, destination);
		}
	}
}