using System;
using System.Collections.Generic;
using CCEngine.Collections;

namespace CCEngine.Algorithms
{
	public interface IGrid
	{
		IEnumerable<Tuple<CPos, int>> GetPassableNeighbors(CPos cpos);
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

		public static IEnumerable<CPos> AStar(IGrid grid, CPos start, CPos goal)
		{
			var frontier = PriorityQueue.Min<int, CPos>();
			var cameFrom = new Dictionary<CPos, CPos>();
			var costSoFar = new Dictionary<CPos, int>();

			// Search from goal to start so that the path doesn't need to be reversed.
			cameFrom[goal] = goal;
			costSoFar[goal] = 0;
			frontier.Enqueue(ManhattanDistance(start, goal), goal);

			CPos current;
			while (frontier.TryDequeue(out current))
			{
				// Path found.
				if (current.Equals(start))
				{
					while(!current.Equals(goal))
					{
						yield return current;
						current = cameFrom[current];
					}
					yield return current;
					yield break;
				}

				// Explore all passable adjacent nodes.
				foreach (var neighbor in grid.GetPassableNeighbors(current))
				{
					var next = neighbor.Item1;
					var cost = neighbor.Item2;
					var newCost = costSoFar[current] + cost;

					if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
					{
						var priority = newCost + ManhattanDistance(start, next);
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
