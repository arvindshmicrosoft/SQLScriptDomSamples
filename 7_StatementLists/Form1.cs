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

using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SQLScriptDomSamples
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var rdr = new StringReader(textBox1.Text))
            {
                IList<ParseError> errors = null;
                var parser = new TSql160Parser(true, SqlEngineType.All);
                var tree = parser.Parse(rdr, out errors);

                foreach (ParseError err in errors)
                {
                    Console.WriteLine(err.Message);
                }

                var checker = new MyVisitor();

                tree.Accept(checker);

                var scrGen = new Sql150ScriptGenerator();
                string formattedSQL = null;
                scrGen.GenerateScript(tree, out formattedSQL);

                MessageBox.Show(formattedSQL);
            }
        }
    }

    class MyVisitor : TSqlFragmentVisitor
    {
        internal bool containsOnlySelects = true;

        public override void Visit(TSqlFragment node)
        {
            // We use this to check for, and remove any SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED statements
            // Also, as a proof of concept, the visitor also introduces a SET NOCOUNT ON statement as the first statement
            // We do this for TSqlBatch, TSqlProcedure as samples.
            // Other fragment types are potentially extensions of this sample.
            CheckandRewrite(node);
            base.Visit(node);
        }

        private void CheckandRewrite(TSqlFragment node)
        {
            var finalList = new List<TSqlStatement>();
            var toRemoveList = new List<TSqlStatement>();

            IList<TSqlStatement> stmtList = null;
            if (node is TSqlBatch)
            {
                stmtList = (node as TSqlBatch).Statements;
            }

            if (node is ProcedureStatementBodyBase)
            {
                stmtList = (node as CreateProcedureStatement).StatementList.Statements;
            }

            if (stmtList is null)
            {
                return;
            }

            foreach (TSqlStatement stmt in stmtList)
            {
                if (stmt is SetTransactionIsolationLevelStatement)
                {
                    if (IsolationLevel.ReadUncommitted ==
                        (stmt as SetTransactionIsolationLevelStatement).Level)
                    {
                        toRemoveList.Add(stmt);
                    }
                }
            }

            // Add a SET NOCOUNT ON statement
            finalList.Add(new PredicateSetStatement()
            {
                Options = SetOptions.NoCount,
                IsOn = true
            });

            // Remove any unwanted (SET TRANSACTION ISOLATION READ UNCOMMITTED) statements
            foreach (var stmtToRemove in toRemoveList)
            {
                stmtList.Remove(stmtToRemove);
            }

            finalList.AddRange(stmtList);

            stmtList.Clear();
            foreach(var stmt in finalList)
            {
                stmtList.Add(stmt);
            }
        }
    }
}
