using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using Sprache;

using Versatile;

namespace Versatile.Tests
{
    public class AssemblyVersionTests
    {
        [Fact]
        public void CanParseAssemblyVersionNumber()
        {
            Assert.Throws<ParseException>(() => AssemblyVersion.Grammar.AssemblyVersionNumber.Parse("m"));
            Assert.Throws<ParseException>(() => AssemblyVersion.Grammar.AssemblyVersionNumber.Parse("blah"));
            //Assert.Throws<ParseException>(() => AssemblyVersion.Grammar.AssemblyVersionNumber.Parse("11143fff.2"));
            //Assert.Equal("11143", AssemblyVersion.Grammar.AssemblyVersionNumber.Parse("11143fff"));
            Assert.Throws<ParseException>(() => AssemblyVersion.Grammar.AssemblyVersionNumber.Parse(".34"));
            Assert.Throws<ParseException>(() => AssemblyVersion.Grammar.AssemblyVersionNumber.Parse("-67"));
            Assert.Throws<ParseException>(() => AssemblyVersion.Grammar.AssemblyVersionNumber.Parse("67000"));
        }

        [Fact]
        public void CanParseAssemblyVersionCore()
        {
            //Assert.Throws<ParseException>(() => AssemblyVersion.Grammar.VersionCore.Parse("11143fff.2"));
            List<string> v = AssemblyVersion.Grammar.AssemblyVersionCore.Parse("4.3").ToList();
            Assert.Equal(v.Count(), 4);
            Assert.Equal(v[0], "4");
            Assert.Equal(v[1], "3");
            v = AssemblyVersion.Grammar.AssemblyVersionCore.Parse("0.3.3.0").ToList();
            Assert.Equal(v[0], "0");
            Assert.Equal(v[1], "3");
            Assert.Equal(v[2], "3");
            Assert.Equal(v[3], "0");

        }
    }
}