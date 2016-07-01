using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using Sprache;

namespace Versatile.Tests
{
    public partial class DrupalTests
    {
        [Fact]
        public void CanParseDrupal5Identifier()
        {
            List<string> d5 = Drupal.Grammar.CoreDrupal5Identifier.Parse("5.x");
            Assert.Equal(3, d5.Count);
            Assert.Equal("5", d5[0]);
            d5 = Drupal.Grammar.CoreDrupal5Identifier.Parse("5.6");
            Assert.Equal("6", d5[1]);
        }
    }
}
