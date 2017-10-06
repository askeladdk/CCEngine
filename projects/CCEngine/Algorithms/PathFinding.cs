using System;
using System.Collections.Generic;
using CCEngine.Collections;
using CCEngine.Simulation;

namespace CCEngine.Algorithms
{
	public interface IGrid
	{
		IEnumerable<Tuple<CPos, int>> GetPassableNeighbors(MovementZone mz, CPos cpos);
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
		public static IEnumerable<CPos> AStar(IGrid grid, CPos start, CPos goal, MovementZone mz)
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
					yield break;
				}

				// Explore all passable adjacent nodes.
				foreach (var neighbor in grid.GetPassableNeighbors(mz, current))
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
	}
}
