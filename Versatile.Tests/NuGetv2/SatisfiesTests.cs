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
    public class NuGetv2SatisfiesTests
    {
        NuGetv2 v1 = new NuGetv2(1, 0, 0, 0);
        NuGetv2 v11 = new NuGetv2(1, 1, 0, 0);
        NuGetv2 v2 = new NuGetv2(2, 0, 0, 0);
        NuGetv2 v202 = new NuGetv2(2, 0, 2, 0);
        NuGetv2 v202a = new NuGetv2(2, 0, 2, "alpha");
        NuGetv2 v310ab = new NuGetv2(3, 1, 0, "alpha.beta");
        NuGetv2 v000a1 = new NuGetv2(0, 0, 0, "alpha1");
        NuGetv2 v000b1 = new NuGetv2(0, 0, 0, "beta1");
        NuGetv2 v000a2 = new NuGetv2(0, 0, 0, "alpha2");
        NuGetv2 v000a0 = new NuGetv2(0, 0, 0, "alpha");
        NuGetv2 v090 = new NuGetv2(0, 9, 0, 0);
        NuGetv2 v186 = new NuGetv2(1, 8, 6, 0);
        NuGetv2 v090a1 = new NuGetv2(0, 9, 0, "alpha1");
        NuGetv2 v010a1 = new NuGetv2(0, 10, 0, "alpha1");
        NuGetv2 v090a2 = new NuGetv2(0, 9, 0, "alpha2");
        NuGetv2 v090b1 = new NuGetv2(0, 9, 0, "beta1");
        NuGetv2 v090b2 = new NuGetv2(0, 9, 0, "beta2");

        
        [Fact]
        public void CanSatisfyLessThan()
        {

            BinaryExpression e = NuGetv2.GetBinaryExpression(ExpressionType.LessThan, v1, v2);
            Assert.NotNull(e);
            BinaryExpression e2 = NuGetv2.GetBinaryExpression(ExpressionType.LessThan, v000a2, v090a1);
            e2 = NuGetv2.GetBinaryExpression(ExpressionType.LessThan, v000a1, v010a1);
            Assert.True(NuGetv2.InvokeBinaryExpression(e2)); 
            Assert.True(NuGetv2.InvokeBinaryExpression(NuGetv2.GetBinaryExpression(ExpressionType.LessThan, v202a, v202)));
            Assert.True(NuGetv2.InvokeBinaryExpression(NuGetv2.GetBinaryExpression(ExpressionType.LessThan, v090a1, v090b2)));
            Assert.False(NuGetv2.InvokeBinaryExpression(NuGetv2.GetBinaryExpression(ExpressionType.LessThan, v000a1, v000a0)));
            Assert.True(NuGetv2.InvokeBinaryExpression(NuGetv2.GetBinaryExpression(ExpressionType.LessThan, v090b1, v090b2)));
            Assert.True(NuGetv2.InvokeBinaryExpression(NuGetv2.GetBinaryExpression(ExpressionType.LessThan, v090a1, v090b2)));
            Assert.True(NuGetv2.InvokeBinaryExpression(NuGetv2.GetBinaryExpression(ExpressionType.LessThan, v090a2, v090b1)));
        }

        [Fact]
        public void CanSatisfyLessThanOrEqual()
        {
            BinaryExpression e = NuGetv2.GetBinaryExpression(ExpressionType.LessThanOrEqual, v1, v1);
            Assert.NotNull(e);
            Assert.True(NuGetv2.InvokeBinaryExpression(e));
            BinaryExpression e2 = NuGetv2.GetBinaryExpression(ExpressionType.LessThanOrEqual, v090a1, v090);
            Assert.True(NuGetv2.InvokeBinaryExpression(e2));
            e2 = NuGetv2.GetBinaryExpression(ExpressionType.LessThanOrEqual, v000a1, v010a1);
            Assert.True(NuGetv2.InvokeBinaryExpression(e2)); 
            e2 = NuGetv2.GetBinaryExpression(ExpressionType.LessThanOrEqual, v000a0, v000a1);
            Assert.True(NuGetv2.InvokeBinaryExpression(e2));
            Assert.True(NuGetv2.InvokeBinaryExpression(NuGetv2.GetBinaryExpression(ExpressionType.LessThanOrEqual, v090, v186)));
        }


        [Fact]
        public void CanSatisfyRangeIntersect()
        {
            Assert.True(NuGetv2.RangeIntersect(ExpressionType.LessThan, v1, ExpressionType.LessThan, v11));
            Assert.False(NuGetv2.RangeIntersect(ExpressionType.LessThan, v1, ExpressionType.GreaterThan, v11));
            Assert.True(NuGetv2.RangeIntersect(ExpressionType.GreaterThan, v11, ExpressionType.GreaterThan, v11));
            Assert.False(NuGetv2.RangeIntersect(ExpressionType.LessThan, v090b1, ExpressionType.GreaterThan, v11));
            Assert.True(NuGetv2.RangeIntersect(ExpressionType.LessThan, v090b1, ExpressionType.GreaterThan, v090a2));
            Assert.True(NuGetv2.RangeIntersect(ExpressionType.GreaterThan, v090a2, ExpressionType.LessThan, v186));
        }
    }

}
