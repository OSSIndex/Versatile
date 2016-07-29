using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sprache;
using Xunit;

namespace Versatile.Tests
{
    public partial class DebianTests
    {
        Debian d100 = new Debian("", "1.0.0", "");
        Debian d100rc1 = new Debian("", "1.0.0~rc1", "");
        Debian d2_100 = Debian.Grammar.DebianVersion.Parse("2:1.0.0");
        Debian d1 = Debian.Grammar.DebianVersion.Parse("2:3.6.19-1~bpo70+1+b1");
        Debian d2 = Debian.Grammar.DebianVersion.Parse("3:3.6.19-1~bpo70+1+b1");
        Debian d3 = Debian.Grammar.DebianVersion.Parse("3.6.19-1~bpo70+1+b1");
        List<string[]> versions = new List<string[]> { //item 1 is greater than item 2
  new string[] {"1.0.1","1.0.0"}
, new string[] {"2.0.1","1.0.1"}
, new string[] {"2.0.0","2.0.0~rc1"}
, new string[] {"2.0.0~rc2","2.0.0~rc1"}
, new string[] {"2.0.0~rc2+u1","2.0.0~rc2"}
, new string[] {"1.0.3~rc2+b2","1.0.3~rc2+b1"}
, new string[] {"2.0.0","2.0.0~b1"}
, new string[] {"2.0.0+u10","2.0.0+u9"}
, new string[] {"2.21-9","2.19-18+deb8u3"}
, new string[] {"2.21-9","2.19-18+deb8u3"}
, new string[] {"2:1.2498-1","2:1.2492-4"}
};

        [Fact]
        public void CanConstruct()
        {
            Assert.Equal(d1.ToString(), "2:3.6.19-1~bpo70+1+b1");
            Assert.Equal(d1.ToNormalizedString(), "2.3.6.19.1~bpo70+1+b1");
            Assert.Equal(d3.ToString(), "3.6.19-1~bpo70+1+b1");
        }


        [Fact]
        public void CanCompareLessThan()
        {
            Assert.True(d1 < d2);
            Assert.True(d100rc1 < d100);
            Assert.True(d100 < d2_100);
            Assert.True(d3 < d2);
        }


        [Fact]
        public void CanCompareGreaterThan()
        {
            Assert.True(Debian.Grammar.DebianVersion.Parse("2.0.0") > Debian.Grammar.DebianVersion.Parse("2.0.0~rc1"));
            Assert.True(d100 > d100rc1);
            foreach (string[] v in versions)
            {
                Assert.True(Debian.Grammar.DebianVersion.Parse(v[0]) > Debian.Grammar.DebianVersion.Parse(v[1]));
            }
        }
    }
}
