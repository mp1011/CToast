using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CToast;

namespace CToastTest
{
    [TestFixture]
    public class CToastTester
    {
        private Context mContext;

        private Context Context
        {
            get
            {
                if(mContext == null)
                    mContext = new Context("@import(main.toast)");
                return mContext;
            }
        }

        [Test]
        public void TestEquality()
        {
            AssertEval("5=@ident(5)", true);
            AssertEval("[1,2,3] = @makeRange(1,3)", true);
            AssertEval("[] = t:[1]", true);
            AssertEval("4 = @ident(5)", false);
            AssertEval("[1,2,3,4] = @makeRange(1,3)", false);
            AssertEval("[3] = t:[1]", false);
            AssertEval("[1,2,3] = @makeRange(1,4)", false);
            AssertEval("(1,(1+1),(1+1+1)) = [1,2,3]", true);
        }

        [Test]
        public void TestLogic()
        {
            AssertEval("1=1 or 1=0", true);
            AssertEval("1=1 and 1=0", false);
            AssertEval("not (1=0 OR 2=0)", true);
        }

        [Test]
        public void TestInfinites()
        {
            AssertEvalBySyntax("@take(5,@naturalNumbers())", "0,1,2,3,4");
            AssertEvalBySyntax("@map(@(i)-> i*10, @take(5,@naturalNumbers()))", "0,10,20,30,40");
            AssertEvalBySyntax("@take(6,@fib2())", "0,1,1,2,3,5");
        }

        [Test]
        public void TestSort()
        {
            AssertEvalBySyntax("@quickSort([10,4,1,7,2])", "1,2,4,7,10");
            AssertEvalBySyntax("@mergeSort([10,4,1,7,2])", "1,2,4,7,10");
            AssertEvalBySyntax("@bubbleSort([10,4,1,7,2])", "1,2,4,7,10");

        }

        private void AssertEval<T>(string expression, T expected)
        {
            var node = CToast.Parser.Parse(expression, Context);
            AssertEval<T>(node, expression, expected);
        }

        private void AssertEval<T>(Node tree, string expression, T expected)
        {
            var result = tree.EvaluateFull(Context);
            if (!result.IsAtomic)
                Assert.Fail();

            Assert.AreEqual(result.TypedValue<T>(default(T)), expected);
        }

        private void AssertEvalBySyntax(string expression, string expected)
        {
            var node = CToast.Parser.Parse(expression, Context);
            var result = node.EvaluateFull(Context);
            string resultSyntax = new SyntaxRenderer().Render(result);
            Assert.AreEqual(resultSyntax,expected);
        }

 
    }
}
