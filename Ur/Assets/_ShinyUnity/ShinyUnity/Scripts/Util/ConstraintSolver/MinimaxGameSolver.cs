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

using System.Threading.Tasks;
using UnityEngine;

namespace Shiny.Solver
{
    public class MinimaxGameSolver : IGameSolver
    {
        public int MaxDepth { get; set; } = int.MaxValue;

        public MinimaxGameSolver(int maxDepth)
        {
            MaxDepth = maxDepth;
        }

        public async Task<(int score, T state)> Solve<T>(IGameGraph<T> graph, T state)
            where T : GameNode
        {
            return await Solve(graph, state, MaxDepth, int.MinValue, int.MaxValue);
        }

        async Task<(int score, T state)> Solve<T>(IGameGraph<T> graph, T state, int depth, int alpha, int beta)
            where T : GameNode
        {
            var needMax = state.ActivePlayer == graph.SelfPlayer;

            // static evaluation of leaf board states. how likely is SelfPlayer to win with this board state?
            if (depth == 0 || graph.IsEnd(state))
            {
                return (graph.GetScore(state), state);
            }

            // force it to be async, but don't worry too much about sleeps
            if (depth % 4 == 0)
            {
                await Task.Delay(1);
            }

            var possibleNextStates = graph.GetReachable(state);
            if (needMax)
            {
                T bestChild = null;
                int bestScore = int.MinValue;
                foreach (var curr in possibleNextStates)
                {
                    var (score, _) = await Solve(graph, curr, depth - 1, alpha, beta);
                    
                    // take maximum of the two: Mathf.Max(bestScore, score)
                    if(score > bestScore)
                    {
                        bestScore = score;
                        bestChild = curr;
                    }

                    // prune
                    alpha = Mathf.Max(alpha, bestScore);
                    if(beta <= alpha)
                    {
                        break;
                    }
                }

                return (bestScore, bestChild);
            }
            else
            {
                T bestChild = null;
                int bestScore = int.MaxValue;
                foreach (var curr in possibleNextStates)
                {
                    var (score, _) = await Solve(graph, curr, depth - 1, alpha, beta);

                    // take minimum of the two: Mathf.Min(bestScore, score)
                    if(score < bestScore)
                    {
                        bestScore = score;
                        bestChild = curr;
                    }

                    // prune
                    beta = Mathf.Min(beta, bestScore);
                    if(beta <= alpha)
                    {
                        break;
                    }
                }

                return (bestScore, bestChild);
            }
        }
    }

}
