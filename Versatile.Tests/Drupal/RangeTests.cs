using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace Versatile.Tests
{
    public partial class DrupalTests
    {
        [Fact]
        public void CanRangeIntersectCommaDelimitedRange()
        {
            string e;
            Assert.True(Drupal.RangeIntersect("6.5.3, 6.5.2, 6.5", "6.x-5.0", out e));
            Assert.True(Drupal.RangeIntersect("6.5.3, 6.5.2, 6.5", ">=6.x", out e));
            Assert.False(Drupal.RangeIntersect("6.5.3, 6.5.2, 6.5", "<6.x", out e));
            Assert.False(Drupal.RangeIntersect("6.5.3, 6.5.2, 6.5", "6.x-4.0", out e));
        }
    }
}
