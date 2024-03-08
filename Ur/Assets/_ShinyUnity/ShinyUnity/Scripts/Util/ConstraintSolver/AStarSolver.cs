// The MIT License (MIT)
// 
// Copyright (c) 2024 Shiny Dolphin Games LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using MS.Internal;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shiny.Solver
{
    public abstract class Node
    {
        public Node()
        {
            GCost = int.MaxValue;
            HeuristicCost = 0;
        }

        /// <summary>
        /// The known cost incurred going from the start node to this node.
        /// In pathfinding, this is the actual path distance from the start node to here (the lengths of the roads)
        /// For chess, it might be the number of pieces lost so far compared to the number of pieces taken.
        /// For desks, it would be the number of violated constraints + the sum of the cost of fuzzy constraints applied to this state
        /// </summary>
        public int GCost { get; set; }

        /// <summary>
        /// An estimation of the remaining "cost" from here until a solution.
        /// In pathfinding, this would be an esimate of the remaining cost from here to the goal node (usually either manhattan distance or euclidean (as the crow flies) distance.
        /// For chess it might be an estimate of the probability of losing the game with this as the starting state.
        /// For desks, it could be very hard to compute. might just have to be a guess based on how many non-terrible moves are available as neighbors, or let it be dijkstra
        /// </summary>
        public int HeuristicCost { get; set; }

        public int FCost => GCost + HeuristicCost;
    }

    public interface IGraph<T>
      where T : Node
    {
        bool IsEnd(T node);
        T Start { get; }

        // lazy neighbors evaluation. this will generate all the possible next nodes on invocation.
        IEnumerable<T> GetReachable(T node);

        // generates a complete end node. used to seed the system with some existing states that could be shuffled or used in monte carlo
        T GetRandomEndNode();

        /// <summary>
        /// The cost of a single move, from one state to a new state.
        /// For pathfinding this would be the edge length from one node to another
        /// For desks this would be any constraints violated by the move + the cost of the fuzzy constraints
        /// </summary>
        int GetCost(T from, T to);

        /// <summary>
        /// The estimated cost of reaching an end state from this state. If these are all equal, you're doing dijkstra instead of Astar
        /// In pathfinding, this would be an esimate of the remaining cost from here to the goal node (usually either manhattan distance or euclidean (as the crow flies) distance.
        /// For chess it might be an estimate of the probability of losing the game with this as the starting state.
        /// For desks, it could be very hard to compute. might just have to be a guess based on how many non-terrible moves are available as neighbors, or let it be dijkstra
        /// </summary>
        int GetHeuristicCost(T from);
    }

    public class AStarSolver : IOptimizationSolver
    {
        class NodeComparer<T> : IComparer<T>
          where T : Node
        {
            public int Compare(T a, T b) => a.FCost - b.FCost;
        }

        // returns a solution. look at Node.Parent to reconstruct the list of moves from back to front.
        public async Task<T> Solve<T>(IGraph<T> graph)
          where T : Node
        {
            var start = graph.Start;
            var open = new PriorityQueue<T>(1000, new NodeComparer<T>());
            open.Push(start);

            start.GCost = 0;
            start.HeuristicCost = graph.GetHeuristicCost(start);

            int iteration = 0;
            while (open.Count > 0)
            {
                var curr = open.Top;
                open.Pop();

                if (graph.IsEnd(curr))
                {
                    return curr;
                }

                foreach (var neighbor in graph.GetReachable(curr))
                {
                    var tentativeGCost = curr.GCost + graph.GetCost(curr, neighbor);
                    if (tentativeGCost < neighbor.GCost)
                    {
                        // This path to neighbor is better than any previous one. Record it!
                        neighbor.GCost = tentativeGCost;
                        neighbor.HeuristicCost = graph.GetHeuristicCost(neighbor);

                        // TODO: Check duplicates?
                        open.Push(neighbor);
                    }
                }

                // prevent going on forever
                if (iteration > 9999)
                {
                    return null;
                }

                // force it to be async, but don't worry too much about sleeps
                if (iteration % 100 == 0)
                {
                    await Task.Delay(1);
                }

                iteration++;
            }

            // unsolvable
            return null;
        }
    }

}
