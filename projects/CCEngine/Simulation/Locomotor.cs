using System;
using System.Collections.Generic;

namespace CCEngine.Simulation
{
	public enum LocomotorState
	{
		Idle,
		PreMove,
		Moving,
	}

	public abstract class Locomotor
	{
		private XPos position;
		private BinaryAngle facing;

		protected Locomotor(XPos position, BinaryAngle facing)
		{
			this.position = position;
			this.facing = facing;
		}

		public XPos Position
		{
			get => position;
			protected set => position = value;
		}

		public BinaryAngle Facing
		{
			get => facing;
			protected set => facing = value;
		}

		public abstract void MoveTo(CPos destination);
		public abstract void Process();
	}

	public class ImmobileLocomotor : Locomotor
	{
		public ImmobileLocomotor(XPos position, BinaryAngle facing) : base(position, facing)
		{
		}

		public override void MoveTo(CPos destination)
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
		private Stack<CPos> path;
		private XPos nextPosition;
		private Vector2I movevec;

		public DriveLocomotor(XPos position, BinaryAngle facing, int moveSpeed, MovementZone movementZone)
			: base(position, facing)
		{
			this.moveSpeed = moveSpeed;
			this.movementZone = movementZone;
		}

		public override void MoveTo(CPos destination)
		{
			var g = Game.Instance;
			path = new Stack<CPos>(g.Map.GetPath(Position.CPos, destination));
			if(path.Count > 1)
			{
				path.Pop();
				state = LocomotorState.PreMove;
			}
		}

		public override void Process()
		{
			var g = Game.Instance;
			//facing = BinaryAngle.Between(position.X, position.Y, g.mousePos.X, g.mousePos.Y);

			// prepare moving to the next cell in the path
			if(state == LocomotorState.PreMove)
			{
				CPos next;
				// get the next cell
				if(path.TryPop(out next))
				{
					nextPosition = new XPos(next);
					var dirvec = new Vector2I(next.X - Position.CellX, next.Y - Position.CellY);
					var spdmul = g.Map.GetCell(Position.CellId).Land.SpeedMultiplier(movementZone);
					movevec = dirvec.Multiply(moveSpeed * spdmul);
					Facing = BinaryAngle.Between(Position.CellX, Position.CellY, next.X, next.Y);
					state = LocomotorState.Moving;
				}
				// path empty
				else
				{
					state = LocomotorState.Idle;
					path = null;
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
					CPos next;
					// check if next cell in the path follows the same direction
					if(path.TryPeek(out next))
					{
						var f = BinaryAngle.Between(Position.CellX, Position.CellY, next.X, next.Y);
						contheading = (f == Facing);
					}
					else
					{
						contheading = false;
					}
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