using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace Versatile.Tests
{
    public partial class SemanticVersionTests
    {
        [Fact]
        public void CanNodeSemverGt()
        {
            List<string[]> ranges = new List<string[]>
            {
                new string[] {"~1.2.2", "1.3.0" },
                new string[] {"~0.6.1-1", "0.7.1-1"},
                new string[] {"1.0.0 - 2.0.0", "2.0.1"},
                new string[] {"1.0.0", "1.0.1-beta1"},
                new string[] {"1.0.0", "2.0.0"},
                new string[] {"<=2.0.0", "2.1.1"},
                new string[] {"<=2.0.0", "3.2.9"},
                new string[] {"<2.0.0", "2.0.0"},
                new string[] {"0.1.20 || 1.2.4", "1.2.5"},
                new string[] {"2.x.x", "3.0.0"},
                new string[] {"1.2.x", "1.3.0"},
                new string[] {"1.2.x || 2.x", "3.0.0"},
                new string[] {"2.*.*", "5.0.1"},
                new string[] {"1.2.*", "1.3.3"},
                new string[] {"1.2.* || 2.*", "4.0.0"},
                new string[] {"2", "3.0.0"},
                new string[] {"2.3", "2.4.2"},
                new string[] {"~2.4", "2.5.0"}, // >=2.4.0 <2.5.0
                new string[] {"~2.4", "2.5.5"},
                new string[] {"~3.2.1", "3.3.0"}, // >=3.2.1 <3.3.0
                new string[] {"~1", "2.2.3"}, // >=1.0.0 <2.0.0
                new string[] {"~1", "2.2.4"},
                new string[] {"~ 1", "3.2.3"},
                new string[] {"~1.0", "1.1.2"}, // >=1.0.0 <1.1.0
                new string[] {"~ 1.0", "1.1.0"},
                new string[] {"<1.2", "1.2.0"},
                new string[] {"< 1.2", "1.2.1"},
                new string[] {"1", "2.0.0beta"},
                new string[] {"~v0.5.4-pre", "0.6.0"},
                new string[] {"~v0.5.4-pre", "0.6.1-pre"},
                new string[] {"0.7.x", "0.8.0"},
                //new string[] {"0.7.x", "0.8.0-asdf"},
                new string[] {"0.7.x", "0.7.0"},
                new string[] {"~1.2.2", "1.3.0"},
                new string[] {"1.0.0 - 2.0.0", "2.2.3"},
                new string[] {"1.0.0", "1.0.1"},
                new string[] {"<=2.0.0", "3.0.0"},
                new string[] {"<=2.0.0", "2.9999.9999"},
                new string[] {"<=2.0.0", "2.2.9"},
                new string[] {"<2.0.0", "2.9999.9999"},
                new string[] {"<2.0.0", "2.2.9"},
                new string[] {"2.x.x", "3.1.3"},
                new string[] {"1.2.x", "1.3.3"},
                new string[] {"1.2.x || 2.x", "3.1.3"},
                new string[] {"2.*.*", "3.1.3"},
                new string[] {"1.2.*", "1.3.3"},
                new string[] {"1.2.* || 2.*", "3.1.3"},
                new string[] {"2", "3.1.2"},
                new string[] {"2.3", "2.4.1"},
                new string[] {"~2.4", "2.5.0"}, // >=2.4.0 <2.5.0
                new string[] {"~3.2.1", "3.3.2"}, // >=3.2.1 <3.3.0
                new string[] {"~1", "2.2.3"}, // >=1.0.0 <2.0.0
                new string[] {"~1", "2.2.3"},
                new string[] {"~1.0", "1.1.0"}, // >=1.0.0 <1.1.0
                new string[] {"<1", "1.0.0"},
                new string[] {"1", "2.0.0beta"},
                new string[] {"<1", "1.0.0beta"},
                new string[] {"< 1", "1.0.0beta"},
                new string[] {"0.7.x", "0.8.2"},
                new string[] {"0.7.x", "0.7.2"}
            };
            foreach (string[] r in ranges)
            {
                string e;
                //Assert.False(SemanticVersion.RangeIntersect("0.7.x", "0.8.0-asdf", out e));
                Assert.False(SemanticVersion.RangeIntersect(r[0], r[1], out e));
                Assert.True(string.IsNullOrEmpty(e));
            }

        }
    }
}
