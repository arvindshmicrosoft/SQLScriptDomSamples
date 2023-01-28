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
namespace SQLScriptDomSamples
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.SqlServer.TransactSql.ScriptDom;

    internal class TSqlNormalizer
    {
        internal string Normalize(Stream input, Stream output, int compatLevel, bool caseSensitive)
        {
            string retVal = string.Empty;

            Dictionary<string, Batch> normalizedBatches = new Dictionary<string, Batch>();

            TextReader rdr = new StreamReader(input);

            TSqlParser parser;

            SqlScriptGenerator scrgen = null;

            switch (compatLevel)
            {
                case 80:
                    {
                        scrgen = new Sql80ScriptGenerator();
                        parser = new TSql80Parser(true);
                        break;
                    }
                case 90:
                    {
                        scrgen = new Sql90ScriptGenerator();
                        parser = new TSql90Parser(true);
                        break;
                    }
                case 100:
                    {
                        scrgen = new Sql100ScriptGenerator();
                        parser = new TSql100Parser(true);
                        break;
                    }
                case 110:
                    {
                        scrgen = new Sql110ScriptGenerator();
                        parser = new TSql110Parser(true);
                        break;
                    }
                case 120:
                    {
                        scrgen = new Sql120ScriptGenerator();
                        parser = new TSql120Parser(true);
                        break;
                    }
                case 130:
                    {
                        scrgen = new Sql130ScriptGenerator();
                        parser = new TSql130Parser(true);
                        break;
                    }
                case 140:
                    {
                        scrgen = new Sql140ScriptGenerator();
                        parser = new TSql140Parser(true);
                        break;
                    }
                case 150:
                    {
                        scrgen = new Sql150ScriptGenerator();
                        parser = new TSql160Parser(true);
                        break;
                    }
                case 160:
                    {
                        scrgen = new Sql160ScriptGenerator();
                        parser = new TSql160Parser(true);
                        break;
                    }
                default:
                    {
                        return "Invalid compatibility level specified; exiting.";
                    }
            }

            IList<ParseError> errs;
            TSqlFragment frag = parser.Parse(rdr, out errs);

            StringBuilder sb = new StringBuilder();
            foreach (ParseError err in errs)
            {
                sb.AppendLine(err.Message + "@ line " + err.Line);
            }

            retVal += sb.ToString();

            Parallel.ForEach(
                (frag as TSqlScript).Batches,
                batch =>
                    {
                        myvisitor visit = new myvisitor();

                        StringBuilder origscript = new StringBuilder();

                        for (int tokIdx = batch.FirstTokenIndex; tokIdx <= batch.LastTokenIndex; tokIdx++)
                        {
                            origscript.Append(batch.ScriptTokenStream[tokIdx].Text);
                        }

                        batch.Accept(visit);

                        string script;

                        scrgen.GenerateScript(batch, out script);

                        string hashValue;

                        using (var hashProvider = new SHA1CryptoServiceProvider())
                        {
                            if (caseSensitive)
                            {
                                hashValue = Convert.ToBase64String(hashProvider.ComputeHash(Encoding.Unicode.GetBytes(script)));
                            }
                            else
                            {
                                hashValue = Convert.ToBase64String(hashProvider.ComputeHash(Encoding.Unicode.GetBytes(script.ToLowerInvariant())));
                            }
                        }

                        lock (normalizedBatches)
                        {
                            if (normalizedBatches.ContainsKey(hashValue))
                            {
                                normalizedBatches[hashValue].count++;
                            }
                            else
                            {
                                normalizedBatches.Add(
                                    hashValue,
                                    new Batch() { count = 1, normalizedText = script, sample = origscript.ToString() });
                            }
                        }
                    });

            // The normalized (tokenized) text is in the Batch.text member above. The below writes that out to a file
            var p = from b in normalizedBatches.Keys select new { hash = b, count = normalizedBatches[b] };

            StringBuilder sbfinal = new StringBuilder();

            foreach (string key in normalizedBatches.Keys)
            {
                string final = string.Format(
                    "-- {0} times:\r\n{1}",
                    normalizedBatches[key].count,
                    normalizedBatches[key].normalizedText);

                sbfinal.AppendLine(final);
                sbfinal.AppendLine("GO");
                sbfinal.AppendLine();
            }

            TextWriter wr = new StreamWriter(output);
            wr.Write(sbfinal.ToString());
            wr.Flush();
            wr.Dispose();
            wr = null;

            return retVal;
        }
    }

    internal class Batch
    {
        public string normalizedText;

        public string sample;

        public int count;
    }
}
