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
            Interval<NuGetv2> i1 = NuGetv2.ComparatorSetToInterval(cs1);
            Interval<NuGetv2> i2 = NuGetv2.ComparatorSetToInterval(cs2);
            Assert.True(i1.Intersect(i1));
            Assert.True(i1.Intersect(i2));
            Interval<NuGetv2> i3 = NuGetv2.Grammar.BracketRangeInterval.Parse("[3.2, 5.3.1)");
            Assert.True(i1.Intersect(i3));
            i3 = NuGetv2.Grammar.BracketRangeInterval.Parse("(3.2, 5.3.1)");
            Assert.True(i1.Intersect(i3));
            Interval<NuGetv2> i4 = NuGetv2.Grammar.BracketRangeInterval.Parse("( , 5.3.1]");
            Assert.True(i2.Intersect(i4));
            Assert.False(i1.Intersect(NuGetv2.Grammar.BracketRangeInterval.Parse("(5.3.1, 6.3.1)")));
            Assert.True(i4.Intersect(NuGetv2.Grammar.BracketRangeInterval.Parse("[5.3.1, )")));
            Assert.False(i4.Intersect(NuGetv2.Grammar.BracketRangeInterval.Parse("(5.3.1, )")));
            Assert.True(i2.Intersect(NuGetv2.Grammar.BracketRangeInterval.Parse("(5.3.1, )")));
        }
    }
}
