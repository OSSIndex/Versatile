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
        Composer v1 = new Composer(1, 0, 0);
        Composer v11 = new Composer(1, 1, 0);
        Composer v2 = new Composer(2, 0, 0);
        Composer v202 = new Composer(2, 0, 2);
        Composer v202a = new Composer(2, 0, 2, "alpha");
        Composer v310p234 = new Composer(3, 1, 0, "patch234");
        Composer v000a1 = new Composer(0, 0, 0, "alpha1");
        Composer v000b1 = new Composer(0, 0, 0, "beta1");
        Composer v000a2 = new Composer(0, 0, 0, "alpha2");
        Composer v000a0 = new Composer(0, 0, 0, "alpha");
        Composer v090 = new Composer(0, 9, 0);
        Composer v186 = new Composer(1, 8, 6);
        Composer v090p1 = new Composer(0, 9, 0, "patch1");
        Composer v010p1 = new Composer(0, 10, 0, "patch11");
        Composer v090a2 = new Composer(0, 9, 0, "alpha2");
        Composer v090b1 = new Composer(0, 9, 0, "beta1");
        Composer v090b2 = new Composer(0, 9, 0, "beta2");

        
        [Fact]
        public void CanSatisfyLessThan()
        {
            BinaryExpression e = Composer.GetBinaryExpression(ExpressionType.LessThan, v1, v2);
            Assert.NotNull(e);
            BinaryExpression e2 = Composer.GetBinaryExpression(ExpressionType.LessThan, v000a2, v090p1);
            e2 = Composer.GetBinaryExpression(ExpressionType.LessThan, v000a1, v010p1);
            Assert.True(Composer.InvokeBinaryExpression(e2)); 
            Assert.True(Composer.InvokeBinaryExpression(Composer.GetBinaryExpression(ExpressionType.LessThan, v202a, v202)));
            //Assert.True(Composer.InvokeBinaryExpression(Composer.GetBinaryExpression(ExpressionType.LessThan, v090p1, v090b2)));
            Assert.False(Composer.InvokeBinaryExpression(Composer.GetBinaryExpression(ExpressionType.LessThan, v000a1, v000a0)));
            Assert.True(Composer.InvokeBinaryExpression(Composer.GetBinaryExpression(ExpressionType.LessThan, v090b1, v090b2)));
            //Assert.True(Composer.InvokeBinaryExpression(Composer.GetBinaryExpression(ExpressionType.LessThan, v090p1, v090b2)));
            Assert.True(Composer.InvokeBinaryExpression(Composer.GetBinaryExpression(ExpressionType.LessThan, v090a2, v090b1)));
        }

        [Fact]
        public void CanSatisfyLessThanOrEqual()
        {
            BinaryExpression e = Composer.GetBinaryExpression(ExpressionType.LessThanOrEqual, v1, v1);
            Assert.NotNull(e);
            Assert.True(Composer.InvokeBinaryExpression(e));
            BinaryExpression e2 = Composer.GetBinaryExpression(ExpressionType.LessThanOrEqual, v090p1, v090);
            Assert.True(Composer.InvokeBinaryExpression(e2));
            e2 = Composer.GetBinaryExpression(ExpressionType.LessThanOrEqual, v000a1, v010p1);
            Assert.True(Composer.InvokeBinaryExpression(e2)); 
            e2 = Composer.GetBinaryExpression(ExpressionType.LessThanOrEqual, v000a0, v000a1);
            Assert.True(Composer.InvokeBinaryExpression(e2));
            Assert.True(Composer.InvokeBinaryExpression(Composer.GetBinaryExpression(ExpressionType.LessThanOrEqual, v090, v186)));
        }


        [Fact]
        public void CanSatisfyRangeIntersect()
        {
            Assert.True(Composer.RangeIntersect(ExpressionType.LessThan, v1, ExpressionType.LessThan, v11));
            Assert.False(Composer.RangeIntersect(ExpressionType.LessThan, v1, ExpressionType.GreaterThan, v11));
            Assert.True(Composer.RangeIntersect(ExpressionType.GreaterThan, v11, ExpressionType.GreaterThan, v11));
            Assert.False(Composer.RangeIntersect(ExpressionType.LessThan, v090b1, ExpressionType.GreaterThan, v11));
            Assert.True(Composer.RangeIntersect(ExpressionType.LessThan, v090b1, ExpressionType.GreaterThan, v090a2));
            Assert.True(Composer.RangeIntersect(ExpressionType.GreaterThan, v090a2, ExpressionType.LessThan, v186));
        }
    }

}
