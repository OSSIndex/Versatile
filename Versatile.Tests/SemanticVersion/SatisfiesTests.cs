
using System.Linq.Expressions;

using Sprache;
using Xunit;

using Versatile;

namespace Versatile.Tests
{
    public class SemanticVersionSatisfiesTests
    {
        SemanticVersion v1 = new SemanticVersion(1);
        SemanticVersion v11 = new SemanticVersion(1, 1);
        SemanticVersion v2 = new SemanticVersion(2);
        SemanticVersion v202 = new SemanticVersion(2, 0, 2);
        SemanticVersion v202a = new SemanticVersion(2, 0, 2, "alpha");
        SemanticVersion v310ab = new SemanticVersion(3, 1, 0, "alpha.beta");
        SemanticVersion v000a1 = new SemanticVersion(0, 0, 0, "alpha.1");
        SemanticVersion v000b1 = new SemanticVersion(0, 0, 0, "beta.1");
        SemanticVersion v000a2 = new SemanticVersion(0, 0, 0, "alpha.2");
        SemanticVersion v000a0 = new SemanticVersion(0, 0, 0, "alpha.0");
        SemanticVersion v090 = new SemanticVersion(0, 9, 0);
        SemanticVersion v186 = new SemanticVersion(1, 8, 6);
        SemanticVersion v090a1 = new SemanticVersion(0, 9, 0, "alpha.1");
        SemanticVersion v010a1 = new SemanticVersion(0, 10, 0, "alpha.1");
        SemanticVersion v090a2 = new SemanticVersion(0, 9, 0, "alpha.2");
        SemanticVersion v090b1 = new SemanticVersion(0, 9, 0, "beta.1");
        SemanticVersion v090b2 = new SemanticVersion(0, 9, 0, "beta.2");

        [Fact]
        public void CanSatisfyLessThan()
        {

            BinaryExpression e = SemanticVersion.GetBinaryExpression(ExpressionType.LessThan, v1, v2);
            Assert.NotNull(e);
            BinaryExpression e2 = SemanticVersion.GetBinaryExpression(ExpressionType.LessThan, v000a2, v090a1);
            e2 = SemanticVersion.GetBinaryExpression(ExpressionType.LessThan, v000a1, v010a1);
            Assert.False(SemanticVersion.InvokeBinaryExpression(e2)); //Compare only on pre-release
            Assert.True(SemanticVersion.InvokeBinaryExpression(SemanticVersion.GetBinaryExpression(ExpressionType.LessThan, v202a, v202)));
            Assert.True(SemanticVersion.InvokeBinaryExpression(SemanticVersion.GetBinaryExpression(ExpressionType.LessThan, v090a1, v090b2)));
            Assert.False(SemanticVersion.InvokeBinaryExpression(SemanticVersion.GetBinaryExpression(ExpressionType.LessThan, v000a1, v000a0)));
            Assert.True(SemanticVersion.InvokeBinaryExpression(SemanticVersion.GetBinaryExpression(ExpressionType.LessThan, v090b1, v090b2)));
            Assert.True(SemanticVersion.InvokeBinaryExpression(SemanticVersion.GetBinaryExpression(ExpressionType.LessThan, v090a1, v090b2)));
            Assert.True(SemanticVersion.InvokeBinaryExpression(SemanticVersion.GetBinaryExpression(ExpressionType.LessThan, v090a2, v090b1)));
        }

        [Fact]
        public void CanSatisfyLessThanOrEqual()
        {
            BinaryExpression e = SemanticVersion.GetBinaryExpression(ExpressionType.LessThanOrEqual, v1, v1);
            Assert.NotNull(e);
            Assert.True(SemanticVersion.InvokeBinaryExpression(e));
            BinaryExpression e2 = SemanticVersion.GetBinaryExpression(ExpressionType.LessThanOrEqual, v090a1, v090);
            Assert.True(SemanticVersion.InvokeBinaryExpression(e2));
            e2 = SemanticVersion.GetBinaryExpression(ExpressionType.LessThanOrEqual, v000a1, v010a1);
            Assert.False(SemanticVersion.InvokeBinaryExpression(e2)); //should compare on prerelease
            e2 = SemanticVersion.GetBinaryExpression(ExpressionType.LessThanOrEqual, v000a0, v000a1);
            Assert.True(SemanticVersion.InvokeBinaryExpression(e2));
            Assert.True(SemanticVersion.InvokeBinaryExpression(SemanticVersion.GetBinaryExpression(ExpressionType.LessThanOrEqual, v090, v186)));
        }


        [Fact]
        public void CanSatisfyRangeIntersect()
        {
            Assert.True(SemanticVersion.RangeIntersect(ExpressionType.LessThan, v1, ExpressionType.LessThan, v11));
            Assert.False(SemanticVersion.RangeIntersect(ExpressionType.LessThan, v1, ExpressionType.GreaterThan, v11));
            Assert.True(SemanticVersion.RangeIntersect(ExpressionType.GreaterThan, v11, ExpressionType.GreaterThan, v11));
            Assert.False(SemanticVersion.RangeIntersect(ExpressionType.LessThan, v090b1, ExpressionType.GreaterThan, v11));
            Assert.True(SemanticVersion.RangeIntersect(ExpressionType.LessThan, v090b1, ExpressionType.GreaterThan, v090a2));
            Assert.True(SemanticVersion.RangeIntersect(ExpressionType.GreaterThan, v090a2, ExpressionType.LessThan, v186));
        }

        [Fact]
        public void CanSatisfyXRange()
        {
            Assert.True(SemanticVersion.Satisfies(SemanticVersion.Grammar.SemanticVersion.Parse("0.0.0"), SemanticVersion.Grammar.XRange.Parse("*")));
            Assert.True(SemanticVersion.Satisfies(SemanticVersion.Grammar.SemanticVersion.Parse("1.4"), SemanticVersion.Grammar.XRange.Parse("1.x")));
            Assert.False(SemanticVersion.Satisfies(SemanticVersion.Grammar.SemanticVersion.Parse("2.0"), SemanticVersion.Grammar.XRange.Parse("1.x")));
            Assert.True(SemanticVersion.Satisfies(SemanticVersion.Grammar.SemanticVersion.Parse("4.4.3"), SemanticVersion.Grammar.XRange.Parse("4.4.x")));
            Assert.False(SemanticVersion.Satisfies(SemanticVersion.Grammar.SemanticVersion.Parse("4"), SemanticVersion.Grammar.XRange.Parse("4.4.x")));
        }

        [Fact]
        public void CanSatisfyTildeRange()
        {
            Assert.True(SemanticVersion.Satisfies(new SemanticVersion(1, 2, 4), SemanticVersion.Grammar.TildeRange.Parse("~1.2.3")));
            Assert.True(SemanticVersion.Satisfies(new SemanticVersion(1, 2, 1), SemanticVersion.Grammar.TildeRange.Parse("~1.2")));
            Assert.False(SemanticVersion.Satisfies(new SemanticVersion(1, 3), SemanticVersion.Grammar.TildeRange.Parse("~1.2")));
            //Assert.False(SemanticVersion.Satisfies(new SemanticVersion(1, 2), SemanticVersion.Grammar.CaretRange.Parse("^1.2.3")));
            //Assert.True(SemanticVersion.Satisfies(new SemanticVersion(0, 2, 5), SemanticVersion.Grammar.CaretRange.Parse("^0.2.3")));
        }

        [Fact]
        public void CanSatisfyCaretRange()
        {
            Assert.True(SemanticVersion.Satisfies(new SemanticVersion(1, 3), SemanticVersion.Grammar.CaretRange.Parse("^1.2.3")));
            Assert.True(SemanticVersion.Satisfies(new SemanticVersion(1, 4, 5), SemanticVersion.Grammar.CaretRange.Parse("^1.2.3")));
            Assert.False(SemanticVersion.Satisfies(new SemanticVersion(1, 2), SemanticVersion.Grammar.CaretRange.Parse("^1.2.3")));
            Assert.True(SemanticVersion.Satisfies(new SemanticVersion(0, 2, 5), SemanticVersion.Grammar.CaretRange.Parse("^0.2.3")));
        }
    }
}
