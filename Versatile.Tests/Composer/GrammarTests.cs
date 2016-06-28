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
            //Assert.Throws<ParseException>(() => Composer.Grammar.ComposerVersion.Parse("A.2.3"));
            //Assert.Throws<ParseException>(() => Composer.Grammar.ComposerVersion.Parse("3.2.3.X"));
            //Assert.Throws<ParseException>(() => Composer.Grammar.ComposerVersion.Parse("1.2.3-foo"));
        }

        [Fact]
        public void GrammarCanParseOneSidedRange()
        {
            Comparator<Composer> re = Composer.Grammar.OneSidedRange.Parse("<10.3.4").Last();
            Assert.Equal(ExpressionType.LessThan, re.Operator);
            Assert.Equal(10, re.Version.Major);
            Assert.Equal(3, re.Version.Minor);
            Assert.Equal(4, re.Version.Patch);
            re = Composer.Grammar.OneSidedRange.Parse("<=0.0.4-alpha").Last();
            Assert.Equal(ExpressionType.LessThanOrEqual, re.Operator);
            Assert.Equal(0, re.Version.Major);
            Assert.Equal(4, re.Version.Patch);
            Assert.Equal("alpha", re.Version.PreRelease.ToString());
            ComparatorSet<Composer> r = Composer.Grammar.OneSidedRange.Parse(">1.13.4-beta7");
            Assert.Equal(r.Count, 2);
            Assert.Equal(r[0].Operator, ExpressionType.LessThan);
            Assert.Equal(r[0].Version, Composer.MAX);
        }

        [Fact]
        public void GrammarCanParseXRange()
        {
            ComparatorSet<Composer> cs = Composer.Grammar.XRange.Parse("1.*");
            Assert.Equal(cs[0].Operator, ExpressionType.GreaterThanOrEqual);
            Assert.Equal(cs[0].Version, new Composer(1));
            Assert.Equal(cs[1].Operator, ExpressionType.LessThan);
            Assert.Equal(cs[1].Version, new Composer(2));
            cs = Composer.Grammar.XRange.Parse("1.4.*");
            Assert.Equal(cs[0].Operator, ExpressionType.GreaterThanOrEqual);
            Assert.Equal(cs[0].Version, new Composer(1, 4, 0));
            Assert.Equal(cs[1].Operator, ExpressionType.LessThan);
            Assert.Equal(cs[1].Version, new Composer(1, 5, 0));
        }

        [Fact]
        public void GrammarCanParseHyphenRange()
        {
            ComparatorSet<Composer> cs = Composer.Grammar.HyphenRange.Parse("11.3 - 20");
            Assert.Equal(cs[0].Operator, ExpressionType.GreaterThanOrEqual);
            Assert.Equal(cs[0].Version, new Composer(11, 3));
            Assert.Equal(cs[1].Operator, ExpressionType.LessThanOrEqual);
            Assert.Equal(cs[1].Version, new Composer(20));
        }
    }
}
