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

namespace Shiny.Threads
{
  using System;
  using System.IO;
  using System.Text;

  using UnityEngine;

  /// <summary>
  /// Redirects writes to System.Console to Unity3D's Debug.Log.
  /// </summary>
  /// <author>
  /// Jackson Dunstan, http://jacksondunstan.com/articles/2986
  /// </author>
  public static class UnitySystemConsoleRedirector
  {
    private class UnityTextWriter : TextWriter
    {
      private StringBuilder buffer = new StringBuilder();

      public override void Flush()
      {
        Debug.Log(buffer.ToString());
        buffer.Length = 0;
      }

      public override void Write(string value)
      {
        buffer.Append(value);
        if (value != null)
        {
          var len = value.Length;
          if (len > 0)
          {
            var lastChar = value[len - 1];
            if (lastChar == '\n')
            {
              Flush();
            }
          }
        }
      }

      public override void Write(char value)
      {
        buffer.Append(value);
        if (value == '\n')
        {
          Flush();
        }
      }

      public override void Write(char[] value, int index, int count)
      {
        Write(new string(value, index, count));
      }

      public override Encoding Encoding
      {
        get { return Encoding.Default; }
      }
    }

    public static void Redirect()
    {
      Console.SetOut(new UnityTextWriter());
    }
  }
}
