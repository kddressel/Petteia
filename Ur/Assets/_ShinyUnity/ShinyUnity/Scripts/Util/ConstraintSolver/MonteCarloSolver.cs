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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// NOTE: This doesn't use UnityEngine on purpose. You can't use those APIs in threads.
namespace Shiny.Solver
{
  public class MonteCarloSolver : IOptimizationSolver
  {
    const int NumBatches = 50;

    int Tries;

    public MonteCarloSolver(int tries = 200000)
    {
      Tries = tries;
    }

    async Task<T> DoBatch<T>(IGraph<T> graph, int count)
      where T : Node
    {
      await Task.Delay(1);

      var tries = new List<T>();
      for (var i = 0; i < count; i++)
      {
        try
        {
          var node = graph.GetRandomEndNode();
          node.GCost = graph.GetCost(graph.Start, node);
          tries.Add(node);
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
        }
      }

      return GetBest(tries.ToArray());
    }

    // TODO: just template this class
    public async Task<T> Solve<T>(IGraph<T> graph)
      where T : Node
    {
      int batchSize = Tries / NumBatches;

      Console.WriteLine("Starting " + NumBatches + " solve batches of size " + batchSize + " for a total of " + Tries + " tries");

      var tasks = new List<Task<T>>();
      for (var i = 0; i < NumBatches; i++)
      {
        tasks.Add(DoBatch(graph, batchSize));
      }
      await Task.WhenAll(tasks.ToArray());

      Console.WriteLine("Solve batches finished");

      return GetBest(tasks.Select(t => t.Result).ToArray());
    }

    T GetBest<T>(T[] states)
      where T : Node
    {
      var bestCost = 99999f;
      var best = states.FirstOrDefault();
      for (var i = 0; i < states.Length; i++)
      {
        if (states[i].FCost < bestCost)
        {
          best = states[i];
          bestCost = states[i].FCost;
        }
      }

      return best;
    }
  }
}
