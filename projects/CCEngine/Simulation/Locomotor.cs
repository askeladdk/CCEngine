using System;
using System.Collections.Generic;
using CCEngine.Algorithms;

namespace CCEngine.Simulation
{
	public enum LocomotorState
	{
		Idle,
		PreMove,
		Moving,
		Stuck,
	}

	public abstract class Locomotor
	{
		private XPos position;
		private XPos lastPosition;
		private BinaryAngle facing;

		public XPos InterpolatedPosition(float alpha)
		{
			var x = Helpers.Lerp(lastPosition.X, position.X, alpha);
			var y = Helpers.Lerp(lastPosition.Y, position.Y, alpha);
			return new XPos(0, 0, x, y);
		}

		protected Locomotor(XPos position, BinaryAngle facing)
		{
			this.position = position;
			this.lastPosition = position;
			this.facing = facing;
		}

		public XPos Position
		{
			get => position;
			protected set
			{
				lastPosition = position;
				position = value;
			}
		}

		public BinaryAngle Facing
		{
			get => facing;
			protected set => facing = value;
		}

		public abstract void MoveTo(IFlowField field);
		public abstract void Process();
	}

	public class ImmobileLocomotor : Locomotor
	{
		public ImmobileLocomotor(XPos position, BinaryAngle facing) : base(position, facing)
		{
		}

		public override void MoveTo(IFlowField field)
		{
			// nothing
		}

		public override void Process()
		{
			// nothing
		}
	}

	public class DriveLocomotor : Locomotor
	{
		private MovementZone movementZone;
		private int moveSpeed;
		private LocomotorState state;
		private IFlowField field;
		private XPos nextPosition;
		private Vector2I movevec;

		public DriveLocomotor(XPos position, BinaryAngle facing, int moveSpeed, MovementZone movementZone)
			: base(position, facing)
		{
			this.moveSpeed = moveSpeed;
			this.movementZone = movementZone;
		}

		public override void MoveTo(IFlowField field)
		{
			this.field = field;
			state = LocomotorState.PreMove;
		}

		public override void Process()
		{
			// prepare moving to the next cell in the path
			if(state == LocomotorState.PreMove)
			{
				// destination reached?
				CardinalDirection dir;
				if(field.IsDestination(Position.CPos))
				{
					Position = Position; // hack to fix interpolation glitch when destination reached
					state = LocomotorState.Idle;
					field = null;
				}
				else if(field.TryGetDirection(Position.CPos, out dir))
				{
					var dirvec = dir.ToVector();
					var spdmul = field.GetLandAt(Position.CPos).SpeedMultiplier(movementZone);
					movevec = dirvec.Multiply(moveSpeed * spdmul);
					nextPosition = Position.CPos.Translate(dirvec.X, dirvec.Y).XPos;
					Facing = new BinaryAngle(dir);
					state = LocomotorState.Moving;
				}
				else
				{
					Position = Position; // hack to fix interpolation glitch when destination reached
					state = LocomotorState.Stuck;
					field = null;
					Console.WriteLine("I'm stuck!");
				}
			}

			// moving
			if(state == LocomotorState.Moving)
			{
				var diff = nextPosition.Difference(Position);
				var contheading = true;

				// reaching end of cell
				if( Math.Abs(diff.X) < Math.Abs(movevec.X) || Math.Abs(diff.Y) < Math.Abs(movevec.Y) )
				{
					// check if next cell in the path follows the same direction
					var dirvec = Facing.CardinalDirection.ToVector();
					var next = Position.CPos.Translate(dirvec.X, dirvec.Y);
					CardinalDirection nextdir;
					if(field.TryGetDirection(next, out nextdir))
						contheading = (nextdir == Facing.CardinalDirection);
					else
						contheading = false;
					state = LocomotorState.PreMove;
				}

				if(contheading)
					Position = Position.Translate(0, 0, movevec.X, movevec.Y);
				else
					Position = nextPosition;
			}
		}
	}
}