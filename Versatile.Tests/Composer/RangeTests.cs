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
        public void CanRangeIntersect()
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
    }

}
