//------------------------------------------------------------------------------
//    The MIT License (MIT)
//    
//    Copyright (c) Arvind Shyamsundar
//    
//    Permission is hereby granted, free of charge, to any person obtaining a copy
//    of this software and associated documentation files (the "Software"), to deal
//    in the Software without restriction, including without limitation the rights
//    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//    copies of the Software, and to permit persons to whom the Software is
//    furnished to do so, subject to the following conditions:
//    
//    The above copyright notice and this permission notice shall be included in all
//    copies or substantial portions of the Software.
//    
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//    SOFTWARE.
//
//    This sample code is not supported under any Microsoft standard support program or service. 
//    The entire risk arising out of the use or performance of the sample scripts and documentation remains with you. 
//    In no event shall Microsoft, its authors, or anyone else involved in the creation, production, or delivery of the scripts
//    be liable for any damages whatsoever (including, without limitation, damages for loss of business profits,
//    business interruption, loss of business information, or other pecuniary loss) arising out of the use of or inability
//    to use the sample scripts or documentation, even if Microsoft has been advised of the possibility of such damages.
//------------------------------------------------------------------------------
// <summary>
//   This example shows how to use the TransactSql.ScriptDom parser to 'normalize' batch text and retain only unique query patterns
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TSQLTextNormalizer
{
    using System;
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            // Parameters we expect:
            // .SQL filename to normalize
            // output filename
            // compat level (80...160)
            // whether to treat the batches as case sensitive (true / false)

            if (4 != args.Length)
            {
                WriteMessageAndExit("Usage: Normalize <path to input .SQL file> <path to output .SQL file> <compatibility level> <case sensitive true / false>");
            }

            if (!File.Exists(args[0]))
            {
                WriteMessageAndExit("Input file was not found; exiting.");
            }

            if (File.Exists(args[1]))
            {
                Console.Write("Output file already exists. Overwrite? Press Y or N.");
                char decision = Console.ReadKey().KeyChar;
                if (string.Equals(decision.ToString(), "n", StringComparison.CurrentCultureIgnoreCase))
                {
                    WriteMessageAndExit("You selected not to overwrite; exiting.");
                }
            }

            int compatLevel = 160;
            if (!int.TryParse(args[2], out compatLevel))
            {
                WriteMessageAndExit("Invalid value for compatibility level. Valid values range from 80 to 160.");
            }

            Console.WriteLine();

            using (var input = new FileStream(args[0], FileMode.Open))
            {
                using (var output = new FileStream(args[1], FileMode.Create))
                {
                    bool caseSensitive = true;
                    bool.TryParse(args[3], out caseSensitive);

                    Console.WriteLine(new TSqlNormalizer().Normalize(input, output, compatLevel, caseSensitive));
                    Console.WriteLine("Done!");
                }
            }
        }

        private static void WriteMessageAndExit(string msg)
        {
            Console.WriteLine(msg);
            Environment.Exit(1);
        }
    }
}
