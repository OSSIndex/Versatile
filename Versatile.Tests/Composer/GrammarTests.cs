using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Sprache;
using Xunit;

using Versatile;

namespace Versatile.Tests
{
    public partial class ComposerTests
    {
        [Fact]
        public void GrammarCanParseVersion()
        {
            Composer c1 = Composer.Grammar.ComposerVersion.Parse("3.2.5-dev");
            Composer c2 = Composer.Grammar.ComposerVersion.Parse("v0.1.4.3");
            Assert.Equal(c1.Major, 3);
            Assert.Equal(c2.Major, 0);
            Assert.Equal(c1.PreRelease.ToString(), "dev");
            Assert.Throws<ParseException>(() => NuGetv2.Grammar.NuGetv2Version.Parse("A.2.3"));
            Assert.Throws<ParseException>(() => NuGetv2.Grammar.NuGetv2Version.Parse("3.2.3.X"));
        }

        [Fact]
        public void GrammarCanParseComparator()
        {
            Composer.Comparator re = Composer.Grammar.Comparator.Parse("<10.3.4");
            Assert.Equal(ExpressionType.LessThan, re.Operator);
            Assert.Equal(10, re.Version.Major);
            Assert.Equal(3, re.Version.Minor);
            Assert.Equal(4, re.Version.Patch);
            re = Composer.Grammar.Comparator.Parse("<=0.0.4-alpha");
            Assert.Equal(ExpressionType.LessThanOrEqual, re.Operator);
            Assert.Equal(0, re.Version.Major);
            Assert.Equal(4, re.Version.Patch);
            Assert.Equal("alpha", re.Version.PreRelease.ToString());
        }

        [Fact]
        public void GrammarCanParseLessThan()
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
        public void GrammarCanParseOpenBracketsOpenBrackets()
        {
            NuGetv2.ComparatorSet cs = NuGetv2.Grammar.OpenBracketOpenBracketRange.Parse("(2.0, 3.1.0)");
            Assert.Equal(cs.Count, 2);
        }
    }
}
