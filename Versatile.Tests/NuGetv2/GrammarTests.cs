using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Sprache;
using Xunit;

using Versatile;

namespace Versatile.Tests
{
    public class NuGetv2GrammarTests
    {
        [Fact]
        public void CanParseVersion()
        {
            var f = NuGetv2.Grammar.NuGetv2Version.Parse("3.2.5-foo");
            var f2 = NuGetv2.Grammar.NuGetv2Version.Parse("3.1.0.3-foo");
            NuGetv2 n = NuGetv2.Parse("3.2.5-foo");
            Assert.Equal(n, NuGetv2.Grammar.NuGetv2Version.Parse("3.2.5-foo"));
            Assert.Equal(n.Version.Major, 3);
            Assert.Equal(n.SpecialVersion, "foo");
            Assert.Equal(n.Version.Minor, 2);
            Assert.Equal(n.Version.Build, 5);
        }

        [Fact]
        public void CanParseComparator()
        {
            NuGetv2.Comparator re = NuGetv2.Grammar.Comparator.Parse("<10.3.4");
            Assert.Equal(ExpressionType.LessThan, re.Operator);
            Assert.Equal(10, re.Version.Version.Major);
            Assert.Equal(3, re.Version.Version.Minor);
            Assert.Equal(4, re.Version.Version.Build);
            re = NuGetv2.Grammar.Comparator.Parse("<=0.0.4-alpha");
            Assert.Equal(ExpressionType.LessThanOrEqual, re.Operator);
            Assert.Equal(0, re.Version.Version.Major);
            Assert.Equal(4, re.Version.Version.Build);
            Assert.Equal("alpha", re.Version.SpecialVersion.ToString());
            re = NuGetv2.Grammar.Comparator.Parse(">10.0.100-beta");
            Assert.Equal(ExpressionType.GreaterThan, re.Operator);
            Assert.Equal(10, re.Version.Version.Major);
            Assert.Equal(100, re.Version.Version.Build);
            Assert.Equal("beta", re.Version.SpecialVersion.ToString());
            re = NuGetv2.Grammar.Comparator.Parse("10.6");
            Assert.Equal(ExpressionType.Equal, re.Operator);
            Assert.Equal(10, re.Version.Version.Major);
            Assert.Equal(6, re.Version.Version.Minor);
            Assert.Equal(string.Empty, re.Version.SpecialVersion);
        }

        [Fact]
        public void CanParseLessThan()
        {
            NuGetv2.Comparator c = NuGetv2.Grammar.Comparator.Parse("<1.5.4");
            Assert.Equal(c.Operator, ExpressionType.LessThan);
            Assert.Equal(c.Version.Version.Major, 1);
            Assert.Equal(c.Version.Version.Minor, 5);
            c = NuGetv2.Grammar.Comparator.Parse("<1.0");
            Assert.Equal(c.Operator, ExpressionType.LessThan);
            Assert.Equal(c.Version.Version.Major, 1);
            Assert.Equal(c.Version.Version.Minor, 0);
            c = NuGetv2.Grammar.Comparator.Parse("<1.0.0-alpha");
            Assert.Equal(c.Operator, ExpressionType.LessThan);
            Assert.Equal(c.Version.Version.Major, 1);
            Assert.Equal(c.Version.Version.Minor, 0);
            Assert.Equal(c.Version.SpecialVersion.ToString(), "alpha");
        }

        [Fact]
        public void CanParseOpenBracketsOpenBrackets()
        {
            NuGetv2.ComparatorSet cs = NuGetv2.Grammar.OpenBracketOpenBracketRange.Parse("(2.0, 3.1.0)");
            Assert.Equal(cs.Count, 2);
        }
    }
}
