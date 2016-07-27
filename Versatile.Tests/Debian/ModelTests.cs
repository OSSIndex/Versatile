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
        Debian d1 = Debian.Grammar.DebianVersion.Parse("2:3.6.19-1~bpo70+1+b1");
        Debian d2 = Debian.Grammar.DebianVersion.Parse("3:3.6.19-1~bpo70+1+b1");
        Debian d3 = Debian.Grammar.DebianVersion.Parse("3.6.19-1~bpo70+1+b1");


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
        }
    }
}
