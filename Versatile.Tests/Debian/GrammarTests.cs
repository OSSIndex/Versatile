using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using Sprache;

namespace Versatile.Tests
{
    public partial class DebianTests
    {
        [Fact]
        public void GrammarCanParseVersion()
        {
            IResult<List<string>> r = Debian.Grammar.DebianVersionIdentifier.TryParse("2:0.cvs20060823-8+etch1");
            Assert.True(r.WasSuccessful);
            Assert.Equal(r.Value.Count, 3);
            Assert.Equal(r.Value[0], "2");
            Assert.Equal(r.Value[1], "0.cvs20060823");
            Assert.Equal(r.Value[2], "8+etch1");
            r = Debian.Grammar.DebianVersionIdentifier.TryParse("0.svn20060823");
            Assert.True(r.WasSuccessful);
            Assert.True(string.IsNullOrEmpty(r.Value[0]));
            Assert.Equal(r.Value[1], "0.svn20060823");
            r = Debian.Grammar.DebianVersionIdentifier.TryParse("3:0.vers123-1.foosvn20060823-car-8+etch1");
            Assert.False(string.IsNullOrEmpty(r.Value[2]));
            Assert.Equal(r.Value[0], "3");
            Assert.Equal(r.Value[1], "0.vers123-1.foosvn20060823-car");
            Assert.Equal(r.Value[2], "8+etch1");
            Debian d = Debian.Grammar.DebianVersion.Parse("3:0.vers123-1.foosvn20060823-car-8+etch1");
            Assert.Equal(d.Epoch, 3);
            Assert.Equal(d.DebianRevision, "8+etch1");
            Assert.Equal(d.Count, 3);
            Assert.Equal(d[1], "0.vers123-1.foosvn20060823-car");
        }
    }
}
