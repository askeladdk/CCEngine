using System;
using System.Collections.Generic;
using System.Linq;
using CCEngine.Collections;
using CCEngine.Simulation;

namespace CCEngine.Algorithms
{
	public interface IGrid<PathConstraints>
	{
		IEnumerable<Tuple<CPos, int>> GetPassableNeighbors(PathConstraints pc, CPos cpos);
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
			return Math.Abs(goal.X - cpos.X) + Math.Abs(goal.Y - cpos.Y);
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

			CPos current;
			while (frontier.TryDequeue(out current))
			{
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
				foreach (var neighbor in grid.GetPassableNeighbors(pc, current))
				{
					var next = neighbor.Item1;
					var cost = neighbor.Item2;
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

		public static IEnumerable<CardinalDirection> ConvertPathToDirections(
			IEnumerable<CPos> path)
		{
			if(!path.Any())
				yield break;
			var last = path.First();
			foreach(var cell in path.Skip(1))
			{
				var angle = BinaryAngle.Between(last.X, last.Y, cell.X, cell.Y);
				last = cell;
				yield return angle.CardinalDirection;
			}
		}

		public static IEnumerable<Vector2I> PathToVectors(
			IEnumerable<CPos> path)
		{
			if(!path.Any())
				yield break;
			var last = path.First();
			foreach(var cell in path.Skip(1))
			{
				var v = new Vector2I(cell.X - last.X, cell.Y - last.Y);
				last = cell;
				yield return v;
			}
		}
	}
}
