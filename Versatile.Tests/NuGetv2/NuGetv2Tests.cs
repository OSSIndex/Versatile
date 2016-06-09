using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace Versatile.Tests
{
   public class NuGetv2Tests
    {
        [Fact]
        public void CanCompare()
        {
            NuGetv2 v0 = new NuGetv2("0.7");
            NuGetv2 v1 = new NuGetv2("2.3");
            NuGetv2 v2 = new NuGetv2("2.4");
            NuGetv2 v3 = new NuGetv2("2.4.4");
            NuGetv2 v4 = new NuGetv2("3.3.3.4");
            NuGetv2 v4b = new NuGetv2("3.3.3.5");
            Assert.True(v2 > v1);
            Assert.True(v4b > v4);
            Assert.True(v4 < v4b);
            Assert.Equal(v0, NuGetv2.Parse("0.7.0"));
            Assert.True(v3 > v2);
            Assert.True(v1 < v4);
        }

    }
}
