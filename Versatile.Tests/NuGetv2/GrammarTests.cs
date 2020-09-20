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
            NuGetv2 n;
            NuGetv2 f = NuGetv2.Grammar.NuGetv2Version.Parse("3.2.5-foo");
            NuGetv2 f2 = NuGetv2.Grammar.NuGetv2Version.Parse("3.1.0.3-foo");
            NuGetv2 f3 = NuGetv2.Grammar.NuGetv2Version.Parse("3.1.0.3-foo-bar");
            NuGetv2 f4 = NuGetv2.Grammar.NuGetv2Version.Parse("3.1.0.3-foo-v1-20200911.23");
            
            n = NuGetv2.Parse("3.2.5-foo");
            Assert.Equal(n, NuGetv2.Grammar.NuGetv2Version.Parse("3.2.5-foo"));
            Assert.Equal(3, n.Version.Major);
            Assert.Equal(2, n.Version.Minor);
            Assert.Equal(5, n.Version.Build);
            Assert.Equal("foo", n.SpecialVersion);
            
            n = NuGetv2.Parse("1.2.3.4-foo-v1-20200911.23");
            Assert.Equal(n, NuGetv2.Grammar.NuGetv2Version.Parse("1.2.3.4-foo-v1-20200911.23"));
            Assert.Equal(1, n.Version.Major);
            Assert.Equal(2, n.Version.Minor);
            Assert.Equal(3, n.Version.Build);
            Assert.Equal(3, n.Version.Build);
            Assert.Equal("foo-v1-20200911.23", n.SpecialVersion);
            
            n = NuGetv2.Grammar.NuGetv2Version.Parse("3.4.0199");

            //Assert.Throws<ParseException>(() => NuGetv2.Grammar.NuGetv2Version.Parse("A.2.3"));
            //Assert.Throws<ParseException>(() => NuGetv2.Grammar.NuGetv2Version.Parse("3.2.3.X"));
        }

        [Fact]
        public void GrammarCanParseRange()
        {
            var re = NuGetv2.Grammar.Range.Parse("<1.2.3.4-foo-v1-20200911.23");
            Assert.NotNull(re);
            
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
            Assert.Equal(ExpressionType.LessThan, c.Operator);
            Assert.Equal(1, c.Version.Version.Major);
            Assert.Equal(5, c.Version.Version.Minor);
            c = NuGetv2.Grammar.OneSidedRange.Parse("<1.0").Last();
            Assert.Equal(ExpressionType.LessThan, c.Operator );
            Assert.Equal(1, c.Version.Version.Major);
            Assert.Equal(0, c.Version.Version.Minor);
            c = NuGetv2.Grammar.OneSidedRange.Parse("<1.0.0-alpha").Last();
            Assert.Equal(ExpressionType.LessThan, c.Operator);
            Assert.Equal(1, c.Version.Version.Major);
            Assert.Equal(0, c.Version.Version.Minor);
            Assert.Equal("alpha", c.Version.SpecialVersion.ToString());
            c = NuGetv2.Grammar.OneSidedRange.Parse(">=0.0.0").Last();
            c = NuGetv2.Grammar.OneSidedRange.Parse("<3.4.0199").Last();
            c = NuGetv2.Grammar.OneSidedRange.Parse("<1.0.0-alpha-v1-20200911.23").Last();
            Assert.Equal(ExpressionType.LessThan, c.Operator);
            Assert.Equal(1, c.Version.Version.Major);
            Assert.Equal(0, c.Version.Version.Minor);
            Assert.Equal("alpha-v1-20200911.23", c.Version.SpecialVersion.ToString() );
        }

        [Fact]
        public void GrammarCanParseOpenBracketsOpenBrackets()
        {
            ComparatorSet<NuGetv2> cs = NuGetv2.Grammar.OpenBracketOpenBracketRange.Parse("(2.0, 3.1.0)");
            Assert.Equal(2, cs.Count);
            Assert.Equal(ExpressionType.GreaterThan, cs[0].Operator);
            Assert.Equal(ExpressionType.LessThan, cs[1].Operator);
            cs = NuGetv2.Grammar.OpenBracketOpenBracketRange.Parse("(, 3.1)");
            Assert.Equal(2, cs.Count);
            Assert.Equal(ExpressionType.GreaterThan, cs[0].Operator);
        }

        [Fact]
        public void GrammarCanParseOpenBracketsClosedBrackets()
        {
            ComparatorSet<NuGetv2> cs = NuGetv2.Grammar.OpenBracketClosedBracketRange.Parse("(2.3, 3.3.0-beta6]");
            Assert.Equal(cs.Count, 2);
            Assert.Equal(ExpressionType.GreaterThan, cs[0].Operator);
            Assert.Equal(ExpressionType.LessThanOrEqual, cs[1].Operator);
            cs = NuGetv2.Grammar.OpenBracketClosedBracketRange.Parse("(, 3.1]");
            Assert.Equal(2, cs.Count);
            Assert.Equal(ExpressionType.LessThanOrEqual, cs[1].Operator);
        }
    }
}
