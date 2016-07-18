using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace Versatile.Tests
{
    public partial class ComposerTests
    {
        Composer v1 = new Composer(1, 0, 0);
        Composer v11 = new Composer(1, 1, 0);
        Composer v2 = new Composer(2, 0, 0);
        Composer v202 = new Composer(2, 0, 2);
        Composer v202a = new Composer(2, 0, 2, "alpha");
        Composer v310p234 = new Composer(3, 1, 0, "patch234");
        Composer v000a1 = new Composer(0, 0, 0, "alpha1");
        Composer v000b1 = new Composer(0, 0, 0, "beta1");
        Composer v000a2 = new Composer(0, 0, 0, "alpha2");
        Composer v000a0 = new Composer(0, 0, 0, "alpha");
        Composer v090 = new Composer(0, 9, 0);
        Composer v186 = new Composer(1, 8, 6);
        Composer v090p1 = new Composer(0, 9, 0, "patch1");
        Composer v010p1 = new Composer(0, 10, 0, "patch11");
        Composer v090a2 = new Composer(0, 9, 0, "alpha2");
        Composer v090b1 = new Composer(0, 9, 0, "beta1");
        Composer v090b2 = new Composer(0, 9, 0, "beta2");

        [Fact]
        public void CanConstruct()
        {
            Composer c1 = new Composer(0, 2, 3, "alpha11");
            Composer c2 = new Composer(new List<string> { "0", "2", "3", "alpha1" });
            Composer c3 = new Composer("0.2.3");
            Assert.Equal(c1.Patch, 3);
        }

        [Fact]
        public void CanCompare()
        {
            Composer c023a11 = new Composer(0, 2, 3, "alpha11");
            Composer c023a11_ = new Composer(new List<string> { "0", "2", "3", "alpha11" });
            Composer c023 = new Composer("0.2.3");
            Composer c4 = new Composer(0, 3);
            Assert.Equal(c023a11, c023a11_);
            Assert.True(c023a11 < c023);
            Assert.True(c4 > c023);
        }

        [Fact]
        public void CanGetVersionType()
        {
            Composer.VersionStringType t = Composer.GetVersionType("2.6");
            t = Composer.GetVersionType("^2.6");
            Assert.Equal(t, Composer.VersionStringType.Range);
            t = Composer.GetVersionType(">3.6 || 6");
            Assert.Equal(t, Composer.VersionStringType.Range);
            t = Composer.GetVersionType("foo.6.bar");
            Assert.Equal(t, Composer.VersionStringType.Invalid);

        }
    }
}
