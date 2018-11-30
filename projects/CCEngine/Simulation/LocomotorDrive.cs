using System;
using System.Collections.Generic;
using CCEngine.Algorithms;
using CCEngine.ECS;

namespace CCEngine.Simulation
{
	public enum DriveState
	{
		Idle,
		PreMove,
		Moving,
		Stuck,
	}

	public struct DriveComponent
	{
		public SpeedType SpeedType { get; private set; }
		public int Speed { get; private set; }
		public DriveState State { get; private set; }
		public IFlowField FlowField { get; private set; }
		public Vector2I MovementVector { get; private set; }
		public XPos Destination { get; private set; }

		public DriveComponent(SpeedType speedType, int speed)
		{
			this.SpeedType = speedType;
			this.Speed = speed;
			this.State = DriveState.Idle;
			this.FlowField = null;
			this.MovementVector = new Vector2I();
			this.Destination = new XPos();
		}

		public DriveComponent(SpeedType speedType, int speed,
			DriveState state, IFlowField flowField, Vector2I movementVector, XPos destination)
		{
			this.SpeedType = speedType;
			this.Speed = speed;
			this.State = state;
			this.FlowField = flowField;
			this.MovementVector = movementVector;
			this.Destination = destination;
		}

		public static object Build(ComponentBuilderArgs args)
		{
			var speedType = args.AttributeTable.Get<SpeedType>("Locomotor.SpeedType", SpeedType.Track);
			var speed = args.AttributeTable.Get<int>("Locomotor.Speed", 0);
			return new DriveComponent(speedType, speed);
		}
	}

	public class DriveSystem
	{
		private World world;

		public DriveSystem(World world)
		{
			this.world = world;
		}

		public void Process()
		{
			foreach(var entity in this.world.Registry.View<PoseComponent, DriveComponent>())
				Process(entity);
		}

		private void Process(Entity entity)
		{
			var pose      = this.world.Registry.Get<PoseComponent>(entity);
			var loco      = this.world.Registry.Get<DriveComponent>(entity);
			var position  = pose.Position;
			var facing    = pose.Facing;
			var speedType = loco.SpeedType;
			var speed     = loco.Speed;
			var state     = loco.State;
			var flowField = loco.FlowField;
			var movementVector   = loco.MovementVector;
			var destination = loco.Destination;
			var currentPosition = position.V1;
			var nextPosition = position.V1;

			// The PreMove state determines the next cell in the path and whether the
			// final destination has been reached.
			if(state == DriveState.PreMove)
			{
				CardinalDirection dir;

				// destination reached -> Idle
				if(flowField == null || flowField.IsDestination(currentPosition.CPos))
				{
					state = DriveState.Idle;
					flowField = null;
				}
				// Get the next cell in the path and the movement direction and speed -> Moving
				else if(flowField.TryGetDirection(currentPosition.CPos, out dir))
				{
					var dirvec = dir.ToVector();
					var spd = flowField.SpeedAt(currentPosition.CPos, speedType, speed);
					movementVector = dirvec.Multiply(spd);
					destination = currentPosition.CPos.Translate(dirvec.X, dirvec.Y).XPos;
					facing = new BinaryAngle(dir);
					state = DriveState.Moving;
				}
				// Destination not reached but no next cell found -> Stuck
				else
				{
					state = DriveState.Stuck;
					flowField = null;
					Console.WriteLine("I'm stuck!");
				}
			}

			// The Moving state moves towards the destination cell at the predetermined speed,
			// then it will transition the state back to PreMove.
			if(state == DriveState.Moving)
			{
				var diff = destination.Difference(currentPosition);
				var contheading = true;

				// reaching end of cell
				if( Math.Abs(diff.X) < Math.Abs(movementVector.X) || Math.Abs(diff.Y) < Math.Abs(movementVector.Y) )
				{
					// check if next cell in the path follows the same direction
					var dirvec = facing.CardinalDirection.ToVector();
					var next = currentPosition.CPos.Translate(dirvec.X, dirvec.Y);
					CardinalDirection nextdir;
					var notAtEndOfPath = flowField.TryGetDirection(next, out nextdir);
					contheading = notAtEndOfPath && (nextdir == facing.CardinalDirection);
					state = DriveState.PreMove;
				}

				if(contheading)
					nextPosition = currentPosition.Translate(0, 0, movementVector.X, movementVector.Y);
				else
					nextPosition = destination;
			}

			// Write the new state to the registry.
			position = position.Advance(nextPosition);
			pose = new PoseComponent(position, facing);
			loco = new DriveComponent(speedType, speed, state, flowField, movementVector, destination);
			this.world.Registry.Set(entity, pose);
			this.world.Registry.Set(entity, loco);
		}
	}
}
