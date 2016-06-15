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
            Composer c3 = new Composer("0.2.3");
            Composer c4 = new Composer(0, 3);
            Assert.Equal(c023a11, c023a11_);
            Assert.True(c023a11 < c3);
            Assert.True(c4 > c3);
        }
    }
}
