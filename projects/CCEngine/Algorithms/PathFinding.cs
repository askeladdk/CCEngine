using System;
using System.Collections.Generic;
using System.Linq;
using CCEngine.Collections;
using CCEngine.Simulation;

namespace CCEngine.Algorithms
{
	public interface IGrid<PathConstraints>
	{
		IEnumerable<(CPos, int)> GetPassableNeighbors(PathConstraints pc, CPos cpos);
		Land GetLandAt(CPos cpos);
	}

	/// <summary>
	/// Path finding algorithms.
	/// http://gabrielgambetta.com/path1.html
	/// http://www.policyalmanac.org/games/aStarTutorial.htm
	/// http://www.redblobgames.com/pathfinding/a-star/introduction.html
	/// </summary>
	public class PathFinding
	{
		private static int ManhattanDistance(CPos goal, CPos cpos)
		{
			return Math.Abs(goal.CellX - cpos.CellX) + Math.Abs(goal.CellY - cpos.CellY);
		}

		/// <summary>
		/// Attempts to find a path from start to goal.
		/// </summary>
		/// <param name="grid"></param>
		/// <param name="start"></param>
		/// <param name="goal"></param>
		/// <returns>The path in reverse or null.</returns>
		public static IEnumerable<CPos> AStar<PathConstraints>(
			IGrid<PathConstraints> grid, CPos start, CPos goal, PathConstraints pc)
		{
			var frontier = PriorityQueue.Min<int, CPos>();
			var cameFrom = new Dictionary<CPos, CPos>();
			var costSoFar = new Dictionary<CPos, int>();

			cameFrom[start] = start;
			costSoFar[start] = 0;
			frontier.Enqueue(ManhattanDistance(goal, start), start);

			KeyValuePair<int, CPos> curkv;
			while (frontier.TryDequeue(out curkv))
			{
				var current = curkv.Value;
				// Path found.
				if (current == goal)
				{
					while(current != start)
					{
						yield return current;
						current = cameFrom[current];
					}
					yield return start;
					yield break;
				}

				// Explore all passable adjacent nodes.
				foreach (var (next, cost) in grid.GetPassableNeighbors(pc, current))
				{
					var newCost = costSoFar[current] + cost;

					if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
					{
						var priority = newCost + ManhattanDistance(goal, next);
						cameFrom[next] = current;
						costSoFar[next] = newCost;
						frontier.Enqueue(priority, next);
					}
				}
			}

			// No path found.
			yield break;
		}
	}

	public interface IFlowField
	{
		bool IsDestination(CPos pos);
		bool TryGetDirection(CPos pos, out CardinalDirection dir);
		bool TryGetNextCell(CPos pos, out CardinalDirection dir, out CPos next);
		Land GetLandAt(CPos cpos);
		int SpeedAt(CPos cpos, SpeedType speedType, int speed);
	}

	public class SingularFlowField<PathConstraints> : IFlowField
	{
		private IGrid<PathConstraints> grid;
		private CPos destination;
		private Dictionary<CPos, CardinalDirection> field;

		private static Dictionary<CPos, CardinalDirection> Calculate(
			IGrid<PathConstraints> grid, CPos source, CPos destination, PathConstraints pathConstraints)
		{
			var field = new Dictionary<CPos, CardinalDirection>();
			var path = PathFinding.AStar(grid, source, destination, pathConstraints);

			if(path.Any())
			{
				// A* generator returns path in reverse
				var last = path.First();
				foreach(var cell in path.Skip(1))
				{
					field[cell] = BinaryAngle.Between(cell.CellX, cell.CellY, last.CellX, last.CellY).CardinalDirection;
					last = cell;
				}
			}

			return field;
		}

		public SingularFlowField(IGrid<PathConstraints> grid, CPos source, CPos destination,
			PathConstraints pathConstraints)
		{
			this.grid = grid;
			this.destination = destination;
			this.field = Calculate(grid, source, destination, pathConstraints);
		}

		public bool IsDestination(CPos pos)
		{
			return pos == destination;
		}

		public bool TryGetDirection(CPos pos, out CardinalDirection dir)
		{
			return field.TryGetValue(pos, out dir);
		}

		public bool TryGetNextCell(CPos pos, out CardinalDirection dir, out CPos next)
		{
			if(TryGetDirection(pos, out dir))
			{
				var dv = dir.ToVector();
				next = pos.Translate(dv.X, dv.Y);
				return true;
			}

			next = new CPos();
			return false;
		}

		public Land GetLandAt(CPos cpos)
		{
			return this.grid.GetLandAt(cpos);
		}

		public int SpeedAt(CPos cpos, SpeedType speedType, int speed)
		{
			return this.grid.GetLandAt(cpos).Speed(speedType, speed);
		}
	}
}
