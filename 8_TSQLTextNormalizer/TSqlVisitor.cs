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
namespace TSQLTextNormalizer
{
    using Microsoft.SqlServer.TransactSql.ScriptDom;

    class myvisitor : TSqlFragmentVisitor
    {
        public override void ExplicitVisit(InPredicate node)
        {
            int numvalues = node.Values.Count;
            if (numvalues > 1)
            {
                for (int i = numvalues - 1; i > 0; i--)
                {
                    if (node.Values[i] is Literal)
                    {
                        node.Values.RemoveAt(i);
                    }
                }
            }

            base.ExplicitVisit(node);
        }

        public override void ExplicitVisit(NumericLiteral node)
        {
            node.Value = "0.1";
            base.ExplicitVisit(node);
        }

        public override void ExplicitVisit(MoneyLiteral node)
        {
            node.Value = "$1";
            base.ExplicitVisit(node);
        }

        public override void ExplicitVisit(BinaryLiteral node)
        {
            node.Value = "0xABCD";
            base.ExplicitVisit(node);
        }

        public override void ExplicitVisit(RealLiteral node)
        {
            node.Value = "0.5E-2";
            base.ExplicitVisit(node);
        }

        public override void ExplicitVisit(IntegerLiteral node)
        {
            node.Value = "0";
            base.ExplicitVisit(node);
        }

        public override void ExplicitVisit(StringLiteral node)
        {
            node.Value = "foo";
            base.ExplicitVisit(node);
        }
    }
}
