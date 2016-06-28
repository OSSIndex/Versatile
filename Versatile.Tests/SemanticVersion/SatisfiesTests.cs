
using System.Linq.Expressions;

using Sprache;
using Xunit;

using Versatile;

namespace Versatile.Tests
{
    public partial class SemanticVersionTests
    {
       

        [Fact]
        public void CanSatisfyLessThan()
        {

            BinaryExpression e = Range<SemanticVersion>.GetBinaryExpression(ExpressionType.LessThan, v1, v2);
            Assert.NotNull(e);
            BinaryExpression e2 = Range<SemanticVersion>.GetBinaryExpression(ExpressionType.LessThan, v000a2, v090a1);
            e2 = Range<SemanticVersion>.GetBinaryExpression(ExpressionType.LessThan, v000a1, v010a1);
            Assert.True(Range<SemanticVersion>.InvokeBinaryExpression(e2));
            Assert.True(Range<SemanticVersion>.InvokeBinaryExpression(Range<SemanticVersion>.GetBinaryExpression(ExpressionType.LessThan, v202a, v202)));
            Assert.True(Range<SemanticVersion>.InvokeBinaryExpression(Range<SemanticVersion>.GetBinaryExpression(ExpressionType.LessThan, v090a1, v090b2)));
            Assert.False(Range<SemanticVersion>.InvokeBinaryExpression(Range<SemanticVersion>.GetBinaryExpression(ExpressionType.LessThan, v000a1, v000a0)));
            Assert.True(Range<SemanticVersion>.InvokeBinaryExpression(Range<SemanticVersion>.GetBinaryExpression(ExpressionType.LessThan, v090b1, v090b2)));
            Assert.True(Range<SemanticVersion>.InvokeBinaryExpression(Range<SemanticVersion>.GetBinaryExpression(ExpressionType.LessThan, v090a1, v090b2)));
            Assert.True(Range<SemanticVersion>.InvokeBinaryExpression(Range<SemanticVersion>.GetBinaryExpression(ExpressionType.LessThan, v090a2, v090b1)));
        }

        [Fact]
        public void CanSatisfyLessThanOrEqual()
        {
            BinaryExpression e = Range<SemanticVersion>.GetBinaryExpression(ExpressionType.LessThanOrEqual, v1, v1);
            Assert.NotNull(e);
            Assert.True(Range<SemanticVersion>.InvokeBinaryExpression(e));
            BinaryExpression e2 = Range<SemanticVersion>.GetBinaryExpression(ExpressionType.LessThanOrEqual, v090a1, v090);
            Assert.True(Range<SemanticVersion>.InvokeBinaryExpression(e2));
            e2 = Range<SemanticVersion>.GetBinaryExpression(ExpressionType.LessThanOrEqual, v000a1, v010a1);
            Assert.True(Range<SemanticVersion>.InvokeBinaryExpression(e2));
            e2 = Range<SemanticVersion>.GetBinaryExpression(ExpressionType.LessThanOrEqual, v000a0, v000a1);
            Assert.True(Range<SemanticVersion>.InvokeBinaryExpression(e2));
            Assert.True(Range<SemanticVersion>.InvokeBinaryExpression(Range<SemanticVersion>.GetBinaryExpression(ExpressionType.LessThanOrEqual, v090, v186)));
        }


        [Fact]
        public void CanSatisfyRangeIntersect()
        {
            Assert.True(Range<SemanticVersion>.Intersect(ExpressionType.LessThan, v1, ExpressionType.LessThan, v11));
            Assert.False(Range<SemanticVersion>.Intersect(ExpressionType.LessThan, v1, ExpressionType.GreaterThan, v11));
            Assert.True(Range<SemanticVersion>.Intersect(ExpressionType.GreaterThan, v11, ExpressionType.GreaterThan, v11));
            Assert.False(Range<SemanticVersion>.Intersect(ExpressionType.LessThan, v090b1, ExpressionType.GreaterThan, v11));
            Assert.True(Range<SemanticVersion>.Intersect(ExpressionType.LessThan, v090b1, ExpressionType.GreaterThan, v090a2));
            Assert.True(Range<SemanticVersion>.Intersect(ExpressionType.GreaterThan, v090a2, ExpressionType.LessThan, v186));
        }

        [Fact]
        public void CanSatisfyXRange()
        {
            Assert.True(Range<SemanticVersion>.Satisfies(SemanticVersion.Grammar.SemanticVersion.Parse("0.0.0"), SemanticVersion.Grammar.XRange.Parse("*")));
            Assert.True(Range<SemanticVersion>.Satisfies(SemanticVersion.Grammar.SemanticVersion.Parse("1.4"), SemanticVersion.Grammar.XRange.Parse("1.x")));
            Assert.False(Range<SemanticVersion>.Satisfies(SemanticVersion.Grammar.SemanticVersion.Parse("2.0"), SemanticVersion.Grammar.XRange.Parse("1.x")));
            Assert.True(Range<SemanticVersion>.Satisfies(SemanticVersion.Grammar.SemanticVersion.Parse("4.4.3"), SemanticVersion.Grammar.XRange.Parse("4.4.x")));
            Assert.False(Range<SemanticVersion>.Satisfies(SemanticVersion.Grammar.SemanticVersion.Parse("4"), SemanticVersion.Grammar.XRange.Parse("4.4.x")));
        }

        [Fact]
        public void CanSatisfyTildeRange()
        {
            Assert.True(Range<SemanticVersion>.Satisfies(new SemanticVersion(1, 2, 4), SemanticVersion.Grammar.TildeRange.Parse("~1.2.3")));
            Assert.True(Range<SemanticVersion>.Satisfies(new SemanticVersion(1, 2, 1), SemanticVersion.Grammar.TildeRange.Parse("~1.2")));
            Assert.False(Range<SemanticVersion>.Satisfies(new SemanticVersion(1, 3), SemanticVersion.Grammar.TildeRange.Parse("~1.2")));
            //Assert.False(SemanticVersion.Satisfies(new SemanticVersion(1, 2), SemanticVersion.Grammar.CaretRange.Parse("^1.2.3")));
            //Assert.True(SemanticVersion.Satisfies(new SemanticVersion(0, 2, 5), SemanticVersion.Grammar.CaretRange.Parse("^0.2.3")));
        }

        [Fact]
        public void CanSatisfyCaretRange()
        {
            Assert.True(Range<SemanticVersion>.Satisfies(new SemanticVersion(1, 3), SemanticVersion.Grammar.CaretRange.Parse("^1.2.3")));
            Assert.True(Range<SemanticVersion>.Satisfies(new SemanticVersion(1, 4, 5), SemanticVersion.Grammar.CaretRange.Parse("^1.2.3")));
            Assert.False(Range<SemanticVersion>.Satisfies(new SemanticVersion(1, 2), SemanticVersion.Grammar.CaretRange.Parse("^1.2.3")));
            Assert.True(Range<SemanticVersion>.Satisfies(new SemanticVersion(0, 2, 5), SemanticVersion.Grammar.CaretRange.Parse("^0.2.3")));
        }
    }
}
