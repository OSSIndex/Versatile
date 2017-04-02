using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sprache;
using Xunit;


namespace Versatile.Tests
{
    public partial class NuGetv2Tests
    {
        [Fact]
        public void CanIntervalIntersect()
        {
            ComparatorSet<NuGetv2> cs1 = NuGetv2.Grammar.OpenBracketOpenBracketRange.Parse("(3.2, 5.3.1)");
            ComparatorSet<NuGetv2> cs2 = NuGetv2.Grammar.OpenBracketOpenBracketRange.Parse("(4.2.1, 5.6.1)");
            Interval<NuGetv2> i1 = cs1.ToInterval();
            Interval<NuGetv2> i2 = cs2.ToInterval();
            Assert.True(i1.Intersect(i1));
            Assert.True(i1.Intersect(i2));
            Interval<NuGetv2> i3 = NuGetv2.Grammar.Range.Parse("[3.2, 5.3.1)").ToInterval();
            Assert.True(i1.Intersect(i3));
            i3 = NuGetv2.Grammar.Range.Parse("(3.2, 5.3.1)").ToInterval();
            Assert.True(i1.Intersect(i3));
            Interval<NuGetv2> i4 = NuGetv2.Grammar.Range.Parse("( , 5.3.1]").ToInterval();
            Assert.True(i2.Intersect(i4));
            Assert.False(i1.Intersect(NuGetv2.Grammar.Range.Parse("(5.3.1, 6.3.1)").ToInterval()));
            Assert.True(i4.Intersect(NuGetv2.Grammar.Range.Parse("[5.3.1, )").ToInterval()));
            Assert.False(i4.Intersect(NuGetv2.Grammar.Range.Parse("(5.3.1, )").ToInterval()));
            Assert.True(i2.Intersect(NuGetv2.Grammar.Range.Parse("(5.3.1, )").ToInterval()));
        }

        [Fact]
        public void CanRangeIntersect()
        {
            string e;
            Assert.True(NuGetv2.RangeIntersect("4.5.7", "(2.4, 6.1.3-alpha5]", out e));
            Assert.False(NuGetv2.RangeIntersect("4.5.7", "(4.5.7, 6.1.3-alpha5]", out e));
            Assert.True(NuGetv2.RangeIntersect("4.5.7", "[4.5.7, 6.1.3-alpha5]", out e));
            Assert.True(NuGetv2.RangeIntersect("(5.5,]", "(2.4, 6.1.3-alpha5]", out e));
            Assert.True(NuGetv2.RangeIntersect("(11, 11.9)", "(11, 11.3.0-beta7]", out e));
            Assert.True(NuGetv2.RangeIntersect("(11, 13.3.0-beta7]", "12", out e));
            Assert.False(NuGetv2.RangeIntersect("(11, 13.3.0-beta7]", "13.4", out e));
            Assert.True(NuGetv2.RangeIntersect("3.4.0199", ">= 0.0.0", out e));
            Assert.False(NuGetv2.RangeIntersect("1.3.0", ">=1.2.19 <1.2.24", out e));
            Assert.False(NuGetv2.RangeIntersect("1.3.0", ">1.3.0-beta.1 <1.3.0-beta.14", out e));
            Assert.True(NuGetv2.RangeIntersect("1.3.0", ">1.3.0-beta.14.4 <1.4.0-beta.2", out e));

        }
    }
}
