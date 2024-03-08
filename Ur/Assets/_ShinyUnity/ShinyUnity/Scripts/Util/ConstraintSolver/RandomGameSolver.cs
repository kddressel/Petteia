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

using Shiny.Threads;
using System.Linq;
using System.Threading.Tasks;

namespace Shiny.Solver
{
    public class RandomGameSolver : IGameSolver
    {
        public async Task<(int score, T state)> Solve<T>(IGameGraph<T> graph, T state)
            where T : GameNode
        {
            await Task.Delay(1);

            var needMax = state.ActivePlayer == graph.SelfPlayer;

            // weighted random pick from the possible options
            var possibleNextStates = graph.GetReachable(state).ToList();
            var weights = possibleNextStates.Select(option => 
                (float)graph.GetScore(option) * (needMax ? 1 : -1)
            ).ToList();

            var pick = possibleNextStates.WeightedRandomElement(weights);
            var idx = possibleNextStates.IndexOf(pick);
            return ((int)weights[idx], pick);
        }
    }
}
