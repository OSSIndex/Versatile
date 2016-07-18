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
        Drupal d6207 = new Drupal(6, 2, 0, 7);
        Drupal d5207 = new Drupal(5, 2, 0, 7);
        Drupal d6407 = new Drupal(6, 4, 0, 7);
        Drupal d7100 = new Drupal(7, 1, 0, 0);
        Drupal d7100a1 = new Drupal(7, 1, 0, 0, "alpha1");
        Drupal d7201b11 = new Drupal(7, 2, 0, 1, "beta111");

        [Fact]
        public void CanCompareEqual()
        {
            Assert.Equal(d6207, new Drupal(new List<string> { "6", "2", "0", "7" }));
            Assert.True(d6207 == new Drupal(new List<string> { "6", "2", "0", "7" }));
            Assert.NotEqual(d6207, new Drupal(new List<string> { "5", "2", "0", "7" }));
            Assert.NotEqual(d6207, new Drupal(new List<string> { "6", "2", "0", "8" }));
        }


        [Fact]
        public void CanCompareLessThan()
        {
            Assert.True(d5207 < d6207);
            Assert.True(d6207 < d6407);
            Assert.True(d5207 < d6407);
            Assert.True(d7100a1 < d7100);
            Assert.True(d7100 < d7201b11);
            Assert.True(d7100a1 < d7201b11);
        }

        [Fact]
        public void CanRangeIntersect()
        {
            string o;
            Assert.True(Drupal.RangeIntersect("<6.x-2.0", "5.x-3.0", out o));
        }
    }
}
