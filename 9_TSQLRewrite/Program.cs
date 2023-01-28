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
//   This example shows how to use the TransactSql.ScriptDom parser to potentially change script tokens
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SQLScriptDomSamples
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.SqlServer.TransactSql.ScriptDom;

    class Program
    {
        static void Main(string[] args)
        {
            using (var rdr = new StreamReader(@"..\..\..\sampleproc.sql"))
            {
                IList<ParseError> errors = null;
                var parser = new TSql150Parser(true, SqlEngineType.All);
                var tree = parser.Parse(rdr, out errors);

                foreach (ParseError err in errors)
                {
                    Console.WriteLine(err.Message);
                }

                Console.WriteLine("Example of replacing tokens:");
                Console.WriteLine("============================");

                // access, manipulate, and then print the script tokens
                // this first loop is very simple, it just replaces < sign with > sign
                for (int tmpLoop = tree.FirstTokenIndex; tmpLoop <= tree.LastTokenIndex; tmpLoop++)
                {
                    if (tree.ScriptTokenStream[tmpLoop].TokenType == TSqlTokenType.LessThan)
                    {
                        //Console.WriteLine(string.Format("Token {0} is a 'less than' sign; changing it to 'greater than' sign", tmpLoop));
                        tree.ScriptTokenStream[tmpLoop].TokenType = TSqlTokenType.GreaterThan;
                        tree.ScriptTokenStream[tmpLoop].Text = ">";
                    }

                    // print the output, which would include the re-written token
                    Console.Write(tree.ScriptTokenStream[tmpLoop].Text);
                }

                Console.WriteLine("Example of dropping and injecting multiple tokens:");
                Console.WriteLine("==================================================");

                // access, manipulate, and then print the script tokens
                // this second loop is shows how to potentially delete and inject new tokens and build a new stream
                // given we already modified the < sign to a > sign in the previous loop, we will look for a > sign
                // and replace it with a >= sign
                var outTokens = new List<TSqlParserToken>();
                for (int tmpLoop = tree.FirstTokenIndex; tmpLoop <= tree.LastTokenIndex; tmpLoop++)
                {
                    if (tree.ScriptTokenStream[tmpLoop].TokenType == TSqlTokenType.GreaterThan)
                    {
                        //Console.WriteLine(string.Format("Token {0} is a 'less than' sign; dropping and injecting a 'greater than equal to' sign in the output", tmpLoop));
                        // we ignore the original token and inject two tokens in the output; > and =
                        outTokens.Add(new TSqlParserToken(TSqlTokenType.GreaterThan, ">"));
                        outTokens.Add(new TSqlParserToken(TSqlTokenType.EqualsSign, "="));
                    }
                    else
                    {
                        // add the token as-is
                        outTokens.Add(tree.ScriptTokenStream[tmpLoop]);
                    }
                }
                // print the output, which would include the re-written token
                Console.Write(string.Join(string.Empty, outTokens.Select(t => t.Text)));
            }
        }
    }
}
