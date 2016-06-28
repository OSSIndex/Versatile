using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Sprache;
using Xunit;

using Versatile;

namespace Versatile.Tests
{
    public partial class NuGetv2Tests
    {
        [Fact]
        public void GrammarCanParseVersion()
        {
            NuGetv2 f = NuGetv2.Grammar.NuGetv2Version.Parse("3.2.5-foo");
            NuGetv2 f2 = NuGetv2.Grammar.NuGetv2Version.Parse("3.1.0.3-foo");
            NuGetv2 n = NuGetv2.Parse("3.2.5-foo");
            Assert.Equal(n, NuGetv2.Grammar.NuGetv2Version.Parse("3.2.5-foo"));
            Assert.Equal(n.Version.Major, 3);
            Assert.Equal(n.SpecialVersion, "foo");
            Assert.Equal(n.Version.Minor, 2);
            Assert.Equal(n.Version.Build, 5);
            //Assert.Throws<ParseException>(() => NuGetv2.Grammar.NuGetv2Version.Parse("A.2.3"));
            //Assert.Throws<ParseException>(() => NuGetv2.Grammar.NuGetv2Version.Parse("3.2.3.X"));
        }

        [Fact]
        public void GrammarCanParseOneSidedRange()
        {
            Comparator<NuGetv2> re = NuGetv2.Grammar.OneSidedRange.Parse("<10.3.4").Last();
            Assert.Equal(ExpressionType.LessThan, re.Operator);
            Assert.Equal(10, re.Version.Version.Major);
            Assert.Equal(3, re.Version.Version.Minor);
            Assert.Equal(4, re.Version.Version.Build);
            re = NuGetv2.Grammar.OneSidedRange.Parse("<=0.0.4-alpha").Last();
            Assert.Equal(ExpressionType.LessThanOrEqual, re.Operator);
            Assert.Equal(0, re.Version.Version.Major);
            Assert.Equal(4, re.Version.Version.Build);
            Assert.Equal("alpha", re.Version.SpecialVersion.ToString());
            re = NuGetv2.Grammar.OneSidedRange.Parse(">10.0.100-beta").Last();
            Assert.Equal(ExpressionType.GreaterThan, re.Operator);
            Assert.Equal(10, re.Version.Version.Major);
            Assert.Equal(100, re.Version.Version.Build);
            Assert.Equal("beta", re.Version.SpecialVersion.ToString());
            re = NuGetv2.Grammar.OneSidedRange.Parse("10.6").Last();
            Assert.Equal(ExpressionType.Equal, re.Operator);
            Assert.Equal(10, re.Version.Version.Major);
            Assert.Equal(6, re.Version.Version.Minor);
            Assert.Equal(string.Empty, re.Version.SpecialVersion);
            Comparator<NuGetv2> c = NuGetv2.Grammar.OneSidedRange.Parse("<1.5.4").Last();
            Assert.Equal(c.Operator, ExpressionType.LessThan);
            Assert.Equal(c.Version.Version.Major, 1);
            Assert.Equal(c.Version.Version.Minor, 5);
            c = NuGetv2.Grammar.OneSidedRange.Parse("<1.0").Last();
            Assert.Equal(c.Operator, ExpressionType.LessThan);
            Assert.Equal(c.Version.Version.Major, 1);
            Assert.Equal(c.Version.Version.Minor, 0);
            c = NuGetv2.Grammar.OneSidedRange.Parse("<1.0.0-alpha").Last();
            Assert.Equal(c.Operator, ExpressionType.LessThan);
            Assert.Equal(c.Version.Version.Major, 1);
            Assert.Equal(c.Version.Version.Minor, 0);
            Assert.Equal(c.Version.SpecialVersion.ToString(), "alpha");
        }

        [Fact]
        public void GrammarCanParseOpenBracketsOpenBrackets()
        {
            ComparatorSet<NuGetv2> cs = NuGetv2.Grammar.OpenBracketOpenBracketRange.Parse("(2.0, 3.1.0)");
            Assert.Equal(cs.Count, 2);
            Assert.Equal(cs[0].Operator, ExpressionType.GreaterThan);
            Assert.Equal(cs[1].Operator, ExpressionType.LessThan);
            cs = NuGetv2.Grammar.OpenBracketOpenBracketRange.Parse("(, 3.1)");
            Assert.Equal(cs.Count, 2);
            Assert.Equal(cs[0].Operator, ExpressionType.GreaterThan);
        }

        [Fact]
        public void GrammarCanParseOpenBracketsClosedBrackets()
        {
            ComparatorSet<NuGetv2> cs = NuGetv2.Grammar.OpenBracketClosedBracketRange.Parse("(2.3, 3.3.0-beta6]");
            Assert.Equal(cs.Count, 2);
            Assert.Equal(cs[0].Operator, ExpressionType.GreaterThan);
            Assert.Equal(cs[1].Operator, ExpressionType.LessThanOrEqual);
            cs = NuGetv2.Grammar.OpenBracketClosedBracketRange.Parse("(, 3.1]");
            Assert.Equal(cs.Count, 2);
            Assert.Equal(cs[1].Operator, ExpressionType.LessThanOrEqual);
        }
    }
}
