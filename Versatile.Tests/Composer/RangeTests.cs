using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using Sprache;

using Versatile;

namespace Versatile.Tests
{
    public partial class ComposerTests
    {        
        [Fact]
        public void CanExpressLessThan()
        {
            BinaryExpression e = Range<Composer>.GetBinaryExpression(ExpressionType.LessThan, v1, v2);
            Assert.NotNull(e);
            BinaryExpression e2 = Range<Composer>.GetBinaryExpression(ExpressionType.LessThan, v000a2, v090p1);
            e2 = Range<Composer>.GetBinaryExpression(ExpressionType.LessThan, v000a1, v010p1);
            Assert.True(Range<Composer>.InvokeBinaryExpression(e2)); 
            Assert.True(Range<Composer>.InvokeBinaryExpression(Range<Composer>.GetBinaryExpression(ExpressionType.LessThan, v202a, v202)));
            Assert.False(Range<Composer>.InvokeBinaryExpression(Range<Composer>.GetBinaryExpression(ExpressionType.LessThan, v090p1, v090b2)));
            Assert.False(Range<Composer>.InvokeBinaryExpression(Range<Composer>.GetBinaryExpression(ExpressionType.LessThan, v000a1, v000a0)));
            Assert.True(Range<Composer>.InvokeBinaryExpression(Range<Composer>.GetBinaryExpression(ExpressionType.LessThan, v090b1, v090b2)));
            Assert.True(Range<Composer>.InvokeBinaryExpression(Range<Composer>.GetBinaryExpression(ExpressionType.LessThan, v090a2, v090b1)));
        }

        [Fact]
        public void CanExpressLessThanOrEqual()
        {
            BinaryExpression e = Range<Composer>.GetBinaryExpression(ExpressionType.LessThanOrEqual, v1, v1);
            Assert.NotNull(e);
            Assert.True(Range<Composer>.InvokeBinaryExpression(e));
            BinaryExpression e2 = Range<Composer>.GetBinaryExpression(ExpressionType.LessThanOrEqual, v090, v090p1);
            Assert.True(Range<Composer>.InvokeBinaryExpression(e2));
            e2 = Range<Composer>.GetBinaryExpression(ExpressionType.LessThanOrEqual, v000a1, v010p1);
            Assert.True(Range<Composer>.InvokeBinaryExpression(e2)); 
            e2 = Range<Composer>.GetBinaryExpression(ExpressionType.LessThanOrEqual, v000a0, v000a1);
            Assert.True(Range<Composer>.InvokeBinaryExpression(e2));
            Assert.True(Range<Composer>.InvokeBinaryExpression(Range<Composer>.GetBinaryExpression(ExpressionType.LessThanOrEqual, v090, v186)));
        }

        [Fact]
        public void TwoSidedRangeIntersect()
        {
            ComparatorSet<Composer> r1 = Composer.Grammar.TwoSidedIntervalRange.Parse(">= 2.0.0 < 2.4.8");
            ComparatorSet<Composer> r2 = Composer.Grammar.TwoSidedIntervalRange.Parse(">= 2.3 < 4.4.8-alpha1");
            ComparatorSet<Composer> r3 = Composer.Grammar.CaretRange.Parse("^3.0");
            ComparatorSet<Composer> r4 = Composer.Grammar.TildeRange.Parse("~5");
            Assert.True(Range<Composer>.Intersect(r1, r2));
            Assert.False(Range<Composer>.Intersect(r1, r3));
            Assert.False(Range<Composer>.Intersect(r1, r4));
            Assert.False(Range<Composer>.Intersect(r3, r4));
        }


        [Fact]
        public void CanOneSidedRangeIntersect()
        {
            Assert.True(Range<Composer>.Intersect(ExpressionType.LessThan, v1, ExpressionType.LessThan, v11));
            Assert.False(Range<Composer>.Intersect(ExpressionType.LessThan, v1, ExpressionType.GreaterThan, v11));
            Assert.True(Range<Composer>.Intersect(ExpressionType.GreaterThan, v11, ExpressionType.GreaterThan, v11));
            Assert.False(Range<Composer>.Intersect(ExpressionType.LessThan, v090b1, ExpressionType.GreaterThan, v11));
            Assert.True(Range<Composer>.Intersect(ExpressionType.LessThan, v090b1, ExpressionType.GreaterThan, v090a2));
            Assert.True(Range<Composer>.Intersect(ExpressionType.GreaterThan, v090a2, ExpressionType.LessThan, v186));
            string e;
            Assert.True(Composer.RangeIntersect("<5", "4.1", out e));
            Assert.True(Composer.RangeIntersect("<5", "<4.1", out e));
            Assert.False(Composer.RangeIntersect(">5", "<4.1", out e));
            Assert.True(Composer.RangeIntersect(">=5", "5", out e));
            Assert.True(Composer.RangeIntersect(">1", "1.*", out e));
            Assert.False(Composer.RangeIntersect(">X.4.2", "*.3", out e));
            Assert.True(!string.IsNullOrEmpty(e));
            Assert.True(Composer.RangeIntersect("4 - 9.3", "9", out e));
        }

        [Fact]
        public void CanRangeUnionIntersect()
        {
            List<ComparatorSet<Composer>> csl3 = Composer.Grammar.Range.Parse("4.* || >= 2.4.0 < 4.4.8 || <= 3");
            Assert.Equal(csl3.Count, 3);
            Assert.True(Range<Composer>.Intersect(Composer.Grammar.OneOrTwoSidedRange.Parse("4.1"), csl3));
            Assert.False(Range<Composer>.Intersect(Composer.Grammar.OneOrTwoSidedRange.Parse("9"), csl3));
            List<ComparatorSet<Composer>> csl4 = Composer.Grammar.Range.Parse("^4.0 || >5.4.0 <55.6.8 || <= 10");
            Assert.True(Range<Composer>.Intersect(Composer.Grammar.OneOrTwoSidedRange.Parse("55.6.7"), csl4));
            Assert.False(Range<Composer>.Intersect(Composer.Grammar.OneOrTwoSidedRange.Parse("55.6.8"), csl4));
        }
    }

}
