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
            string dx = Drupal.Grammar.CoreIdentifierPrefix.Parse("5.x");
            Assert.Equal("5", dx);
            dx = Drupal.Grammar.CoreIdentifierPrefix.Parse("6.x");
            List<string> d;
            Assert.Equal("6", dx);
            Assert.Throws<ParseException>(() => Drupal.Grammar.CoreIdentifierPrefix.Parse("A.x"));
            d = Drupal.Grammar.ContribIdentitifierWithoutCoreIdentifierPrefix.Parse("6.5");
            Assert.Equal(d[0], "6");
            Assert.Equal(d[1], "5");
            d = Drupal.Grammar.ContribIdentitifierWithoutCoreIdentifierPrefix.Parse("4.2.7");
            Assert.Equal(d[0], "4");
            Assert.Equal(d[1], "2");
            Assert.Equal(d[3], "7");
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
            d = Drupal.Grammar.ContribIdentifierWithPreReleaseOnly.Parse("6.x-dev");
            Assert.Equal(d[0], "6");
            d = Drupal.Grammar.ContribIdentitifierWithoutCoreIdentifierPrefix.Parse("5.1_rev1.1");
            Assert.Equal("rev.1.1", d[4]);
            d = Drupal.Grammar.ContribIdentitifierWithNumericCoreIdentifier.Parse("4.6_1.0");
            Assert.Equal(d[0], "4");
            d = Drupal.Grammar.ContribIdentitifierWithoutCoreIdentifierPrefix.Parse("4.5-alpha1");
            Assert.Equal(d[0], "4");
            Assert.Equal(d[1], "5");
            Assert.Equal(d[2], "0");
            d = Drupal.Grammar.ContribIdentitifierWithoutCoreIdentifierPrefix.Parse("6.5.3_rc2");
            Assert.Equal(d[0], "6");
            Assert.Equal(d[1], "5");
            Assert.Equal(d[2], "0");
            Assert.Equal(d[3], "3");
            Assert.Equal(d[4], "rc.2");
            d = Drupal.Grammar.ContribIdentitifierWithoutCoreIdentifierPrefix.Parse("4.7_rev1.1");
            Assert.Equal("rev.1.1", d[4]);
            d = Drupal.Grammar.ContribIdentitifierWithNumericCoreIdentifier.Parse("4.x_1.2");
            Assert.Equal("4", d[0]);
            //v = Drupal.Grammar.DrupalVersion.Parse("-");
            //Assert.Equal(v.CoreCompatibility, 1);
            //Assert.Equal(v.Major, 0);

            Assert.Throws<ParseException>(() => Drupal.Grammar.ContribIdentifier.End().Parse("7.x-1.0-release1"));
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
            //Assert.Throws<ParseException>(() => Drupal.Grammar.DrupalVersion.End().Parse("7.x-1.0-release1"));
            v = Drupal.Grammar.DrupalVersion.Parse("5.x");
            Assert.True(v.CoreCompatibility == 5);
            Assert.Equal(v.Patch, 0);
            v = Drupal.Grammar.DrupalVersion.Parse("8.x-2.x");
            Assert.True(v.CoreCompatibility == 8);
            Assert.Equal(v.Major, 2);
            Assert.Equal(v.Patch, 0);
            v = Drupal.Grammar.DrupalVersion.Parse("4.7.x");
            Assert.Equal(4, v.CoreCompatibility);
            v = Drupal.Grammar.DrupalVersion.Parse("4.7_1.2");
            Assert.Equal(v.Major, 7);
            Assert.Equal(v.CoreCompatibility, 4);
            Assert.Equal(v.Minor, 1);
            Assert.Equal(v.Patch, 2);
            v = Drupal.Grammar.DrupalVersion.Parse("7.x-dev");
            Assert.Equal(v.Major, 0);
            Assert.Equal(v.CoreCompatibility, 7);
            v = Drupal.Grammar.DrupalVersion.Parse("-");
            Assert.Equal(v.Major, 0);
            Assert.Equal(1, v.CoreCompatibility);
            v = Drupal.Grammar.DrupalVersion.Parse("4.7_rev1.15");
            v = Drupal.Grammar.DrupalVersion.Parse("4.7_revision_1.2");
        }

        [Fact]
        public void GrammarCanParseCommaDelimitedRange()
        {
            List<ComparatorSet<Drupal>> r = Drupal.Grammar.CommaDelimitedRange.Parse("4.4.0,4.5.0,4.6-rc1");
            Assert.NotEmpty(r);
            Assert.Equal(r[0].First().Version.ToString(), "4.x-4.0");
            Assert.Equal(r[1].First().Version.ToString(), "4.x-5.0");
            Assert.Equal(r[2].First().Version.ToString(), "4.x-6.0-rc1");
            r = Drupal.Grammar.CommaDelimitedRange.Parse("6,5");
            Assert.Equal(r[0].First().Version.ToString(), "6.x-0.0");
            r = Drupal.Grammar.CommaDelimitedRange.Parse("4.7_");
            r = Drupal.Grammar.CommaDelimitedRange.Parse("6.2,6.0,6.0,5.0,5.7,5,5.2,6.0,5.4,6.0,5.5.,5.0,5.0,6,5.0,5.8,5.0,5.5,5.6,6.0,6.0,5.3,6.0,6.1,5.1,6.0,6.0");
            r = Drupal.Grammar.CommaDelimitedRange.Parse("4.0,4.0.0,4.1.0,4.2.0_rc,4.4,4.4.0,4.4.1,4.4.2,4.4.3,4.5,4.5.0,4.5.1,4.5.2,4.5.3,4.5.4,4.5.5,4.5.6,4.5.7,4.5.8,4.6,4.6.0,4.6.1,4.6.10,4.6.11,4.6.2,4.6.3,4.6.4,4.6.5,4.6.6,4.6.7,4.6.8,4.6.9,4.7,4.7.0,4.7.1,4.7.10,4.7.2,4.7.3,4.7.4,4.7.5,4.7.6,4.7.7,4.7.8,4.7.9,4.7_rev1.15,4.7_revision_1.2,4.7_rev_1.15,4.7_rev_1.2,5.0,5.1,5.10,5.11,5.12,5.13,5.14,5.15,5.16,5.17,5.18,5.19,5.1_rev1.1,5.2,5.20,5.21,5.22,5.23,5.3,5.4,5.5,5.5.,5.6,5.7,5.8,5.9,5.x,6.0,6.1,6.10,6.11,6.12,6.13,6.14,6.15,6.16,6.17,6.18,6.19,6.2,6.20,6.21,6.22,6.23,6.24,6.3,6.4,6.5,6.6,6.7,6.8,6.9,6.x-dev,7.0,7.1,7.10,7.11,7.12,7.2,7.3,7.4,7.5,7.6,7.7,7.8,7.9,7.x-dev, 4.6.7,4.6.8,4.6.9,4.7,4.7.0,4.7.1,4.7.10,4.7.2,4.7.3,4.7.4,4.7.5,4.7.6,4.7.7,4.7.8,4.7.9,4.7_rev1.15,4.7_revision_1.2,4.7_rev_1.15,4.7_rev_1.2,5.0,5.1,5.10,5.11,5.12,5.13,5.14,5.15,5.16,5.17,5.18,5.19,5.1_rev1.1,5.2,5.20,5.21,5.22");
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
        //(>=0) || (>=0)
        [Fact]
        public void GrammarCanParseRangeVersionIdentifier()
        {
            List<ComparatorSet<Drupal>> csl = Drupal.Grammar.Range.Parse("7.x-dev");
            Assert.Equal(1, csl.Count());
            csl = Drupal.Grammar.Range.Parse("4.7_rev1.1");
            Assert.Equal(1, csl.Count());
            csl = Drupal.Grammar.Range.Parse("8.x-2.x");
            Assert.Equal(1, csl.Count());

        }

    }
}
