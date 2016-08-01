using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using Sprache;

namespace Versatile.Tests
{
    public partial class DrupalTests
    {
        [Fact]
        public void GrammarCanParseCoreIdentifier()
        {
            string d = Drupal.Grammar.CoreIdentifier.Parse("5.x");
            Assert.Equal("5", d);
            d = Drupal.Grammar.CoreIdentifier.Parse("6.x");
            Assert.Equal("6", d);
            Assert.Throws<ParseException>(() => Drupal.Grammar.CoreIdentifier.Parse("A.x"));
        }

        [Fact]
        public void GrammarCanParseModuleIdentifier()
        {
            List<string> d = Drupal.Grammar.ContribIdentifier.Parse("6.x-2.0");
            Assert.Equal(d.Count, 4);
            d = Drupal.Grammar.ContribIdentifier.Parse("6.x-3.0-alpha1");
            Assert.Equal(d.Count, 5);
            Assert.Equal("6", d[0]);
            Assert.Equal("3", d[1]);
            Assert.Equal("0", d[3]);
            Assert.Equal("alpha.1", d[4]);
            Assert.Throws<ParseException>(() => Drupal.Grammar.ContribIdentifier.End().Parse("7.x-1.0-release1"));
            d = Drupal.Grammar.ContribIdentifierWithoutCoreIdentitifier.Parse("6.5");
            Assert.Equal(d[0], "6");
            Assert.Equal(d[1], "5");
            d = Drupal.Grammar.ContribIdentifierWithoutCoreIdentitifier.Parse("4.2.7");
            Assert.Equal(d[0], "4");
            Assert.Equal(d[1], "2");
            Assert.Equal(d[3], "7");
            d = Drupal.Grammar.ContribIdentifierWithoutCoreIdentitifier.Parse("4.5-alpha1");
            Assert.Equal(d[0], "4");
            Assert.Equal(d[1], "5");
            Assert.Equal(d[2], "0");
            d = Drupal.Grammar.ContribIdentifierWithoutCoreIdentitifier.Parse("6.5.3_rc2");
            Assert.Equal(d[0], "6");
            Assert.Equal(d[1], "5");
            Assert.Equal(d[2], "0");
            Assert.Equal(d[3], "3");
            Assert.Equal(d[4], "rc.2");
        }

        [Fact]
        public void GrammarCanParseDrupalVersion()
        {
            Drupal v = Drupal.Grammar.DrupalVersion.Parse("6.x-3.4-beta2");
            Assert.Equal(6, v.CoreCompatibility);
            Assert.Equal(3, v.Major);
            Assert.Equal(4, v.Patch);
            Assert.Equal("beta.2", v.PreRelease.ToNormalizedString());
            v = Drupal.Grammar.DrupalVersion.Parse("7.x-5.0-dev");
            Assert.Equal(7, v.CoreCompatibility);
            Assert.Equal(5, v.Major);
            Assert.Equal(0, v.Patch);
            Assert.Equal("dev", v.PreRelease.ToString());
            Assert.Throws<ParseException>(() => Drupal.Grammar.DrupalVersion.End().Parse("7.x-1.0-release1"));
            v = Drupal.Grammar.DrupalVersion.Parse("5.x");
            Assert.True(v.CoreCompatibility == 5);
            Assert.Equal(v.Patch, 0);
            v = Drupal.Grammar.DrupalVersion.Parse("8.x-2.x");
            Assert.True(v.CoreCompatibility == 8);
            Assert.Equal(v.Major, 2);
            Assert.Equal(v.Patch, 0);
        }

        [Fact]
        public void GrammarCanParseCommaDelimitedRange()
        {
            List<ComparatorSet<Drupal>> r = Drupal.Grammar.CommaDelimitedRange.Parse("4.4.0,4.5.0,4.6-rc1");
            Assert.NotEmpty(r);
            Assert.Equal(r[0].First().Version.ToString(), "4.x-4.0");
            Assert.Equal(r[1].First().Version.ToString(), "4.x-5.0");
            Assert.Equal(r[2].First().Version.ToString(), "4.x-6.0-rc1");

        }

        [Fact]
        public void GrammarCanParseTwoSidedIntervalRange()
        {
            ComparatorSet<Drupal> cs = Drupal.Grammar.TwoSidedIntervalRange.Parse(">=7.x-3.x & <7.x-3.8");
            Assert.Equal(cs[0].Version, new Drupal(7, 3, 0, 0));
            Assert.Equal(cs[0].Operator, ExpressionType.GreaterThanOrEqual);
            Assert.Equal(cs[1].Version, new Drupal(7, 3, 0, 8));
            Assert.Equal(cs[1].Operator, ExpressionType.LessThan);
            ComparatorSet<Drupal> csb = Drupal.Grammar.BracketedTwoSidedIntervalRange.Parse("(>=7.x-3.x & <7.x-3.8)");
            Assert.Equal(cs[0].Version, csb[0].Version);
        }
    }
}
