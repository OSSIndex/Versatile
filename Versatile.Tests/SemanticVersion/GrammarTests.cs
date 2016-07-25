using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Sprache;
using Xunit;

using Versatile;

namespace Versatile.Tests
{
    public partial class SemanticVersionTests
    {
        [Fact]
        public void GrammarCanParseDigits()
        {
            Assert.Equal(SemanticVersion.Grammar.Digits.Parse("1"), "1");
            Assert.Equal(SemanticVersion.Grammar.Digits.Parse("01"), "01");
            Assert.Equal(SemanticVersion.Grammar.Digits.Parse("1004"), "1004");
            Assert.Equal(SemanticVersion.Grammar.Digits.Parse("44"), "44");
            Assert.Throws<ParseException>(() => SemanticVersion.Grammar.Digits.Parse("d44"));
        }

        [Fact]
        public void GrammarCanParseNonDigit()
        {
            Assert.True(SemanticVersion.Grammar.NonDigit.Parse("-") == '-');
            Assert.True(SemanticVersion.Grammar.NonDigit.Parse("a") == 'a');
            Assert.Throws<ParseException>(() => SemanticVersion.Grammar.NonDigit.Parse("4"));
        }


        [Fact]
        public void GrammarCanParseIdentifierCharacter()
        {
            Assert.True(SemanticVersion.Grammar.IdentifierCharacter.Parse("-") == '-');
            Assert.True(SemanticVersion.Grammar.IdentifierCharacter.Parse("a") == 'a');
            Assert.True(SemanticVersion.Grammar.IdentifierCharacter.Parse("9") == '9');
            Assert.Throws<ParseException>(() => SemanticVersion.Grammar.NonDigit.Parse("."));
        }

        [Fact]
        public void GrammarCanParseIdentifierCharacters()
        {
            Assert.True(SemanticVersion.Grammar.IdentifierCharacters.Parse("23-") == "23-");
            Assert.True(SemanticVersion.Grammar.IdentifierCharacters.Parse("alpha1") == "alpha1");
        }

        [Fact]
        public void GrammarCanParsePreleaseSuffix()
        {
            string p = SemanticVersion.Grammar.PreReleaseSuffix.Parse("-alpha.1");
        }


        [Fact]
        public void GrammarCanParseDotSeparatedBuildIdentifiers()
        {
            IEnumerable<string> v = SemanticVersion.Grammar.DotSeparatedBuildIdentifier.Parse("2.3.4");
            Assert.True(v.Count() == 3);
            v = SemanticVersion.Grammar.DotSeparatedBuildIdentifier.Parse("1.2.3.4.alpha1");
            Assert.True(v.Count() == 5);
        }

        [Fact]
        public void GrammarCanParseAlphaNumericIdentifier()
        {
            Assert.True(SemanticVersion.Grammar.IdentifierCharacters.Parse("23") == "23");
            Assert.True(SemanticVersion.Grammar.IdentifierCharacters.Parse("23-") == "23-");
            Assert.True(SemanticVersion.Grammar.IdentifierCharacters.Parse("alpha1") == "alpha1");
        }

        [Fact]
        public void GrammarCanParseVersionCore()
        {
            List<string> v = SemanticVersion.Grammar.VersionCore.Parse("2.3.4").ToList();
            Assert.NotEmpty(v);
            Assert.Equal(v[0], "2");
            v = SemanticVersion.Grammar.VersionCore.Parse("4").ToList();
            Assert.Equal(v[0], "4");
            Assert.Equal(v[1], "");
            Assert.Equal(v[2], "");
        }

        [Fact]
        public void GrammarCanParseVersionIdentifier()
        {
            var v = SemanticVersion.Grammar.SemanticVersionIdentifier.Parse("0.0.1+build.12");
            Assert.NotEmpty(v);
        }

        [Fact]
        public void GrammarCanParseOneSidedRange()
        {
            Comparator<SemanticVersion> re = SemanticVersion.Grammar.OneSidedRange.Parse("<10.3.4").First();
            Assert.Equal(ExpressionType.GreaterThan, re.Operator);
            re = SemanticVersion.Grammar.OneSidedRange.Parse("<10.3.4").Last();
            Assert.Equal(10, re.Version.Major);
            Assert.Equal(3, re.Version.Minor);
            Assert.Equal(4, re.Version.Patch);
            re = SemanticVersion.Grammar.OneSidedRange.Parse("<=0.0.4-alpha").Last();
            Assert.Equal(ExpressionType.LessThanOrEqual, re.Operator);
            Assert.Equal(0, re.Version.Major);
            Assert.Equal(4, re.Version.Patch);
            Assert.Equal("alpha.0", re.Version.PreRelease.ToNormalizedString());
            re = SemanticVersion.Grammar.OneSidedRange.Parse(">10.0.100-beta.0").Last();
            Assert.Equal(ExpressionType.GreaterThan, re.Operator);
            Assert.Equal(10, re.Version.Major);
            Assert.Equal(100, re.Version.Patch);
            Assert.Equal("beta.0", re.Version.PreRelease.ToNormalizedString());
            re = SemanticVersion.Grammar.OneSidedRange.Parse("10.6").First();
            Assert.Equal(ExpressionType.Equal, re.Operator);
            Assert.Equal(10, re.Version.Major);
            Assert.Equal(6, re.Version.Minor);
            Assert.Equal(null, re.Version.PreRelease);
            Comparator<SemanticVersion> c = SemanticVersion.Grammar.OneSidedRange.Parse("<1.5.4").Last();
            Assert.Equal(c.Operator, ExpressionType.LessThan);
            Assert.Equal(c.Version.Major, 1);
            Assert.Equal(c.Version.Minor, 5);
            c = SemanticVersion.Grammar.OneSidedRange.Parse("<1.0").Last();
            Assert.Equal(c.Operator, ExpressionType.LessThan);
            Assert.Equal(c.Version.Major, 1);
            Assert.Equal(c.Version.Minor, 0);
            c = SemanticVersion.Grammar.OneSidedRange.Parse("<1.0.0-alpha.1.0").Last();
            Assert.Equal(c.Operator, ExpressionType.LessThan);
            Assert.Equal(c.Version.Major, 1);
            Assert.Equal(c.Version.Minor, 0);
            Assert.Equal(c.Version.PreRelease.ToNormalizedString(), "alpha.1.0");
        }

        [Fact]
        public void GrammarCanParseXRangeExpression()
        {
            ComparatorSet<SemanticVersion> xr1 = SemanticVersion.Grammar.MajorXRange.Parse("4.x");
            Assert.NotNull(xr1);
            Assert.Equal(xr1[0].Operator, ExpressionType.GreaterThanOrEqual);
            Assert.Equal(xr1[0].Version, new SemanticVersion(4));
            Assert.Equal(xr1[1].Operator, ExpressionType.LessThan);
            Assert.Equal(xr1[1].Version, new SemanticVersion(5));
            ComparatorSet<SemanticVersion> xr2 = SemanticVersion.Grammar.MajorMinorXRange.Parse("4.3.x");
            Assert.NotNull(xr1);
            Assert.Equal(xr1[0].Operator, ExpressionType.GreaterThanOrEqual);
            Assert.Throws(typeof(Sprache.ParseException), () => SemanticVersion.Grammar.MajorXRange.Parse("*"));
            Assert.Throws(typeof(Sprache.ParseException), () => SemanticVersion.Grammar.MajorXRange.Parse("4.3.x"));
        }

        [Fact]
        public void GrammarCanParseTildeRangeExpression()
        {
            ComparatorSet<SemanticVersion> tr1 = SemanticVersion.Grammar.MajorTildeRange.Parse("~4");
            ComparatorSet<SemanticVersion> tr2 = SemanticVersion.Grammar.MajorTildeRange.Parse("~14.4");
            ComparatorSet<SemanticVersion> tr3 = SemanticVersion.Grammar.MajorTildeRange.Parse("~7.0.1");
            Assert.NotNull(tr1);
            Assert.NotNull(tr2);
            Assert.NotNull(tr3);
        }

        [Fact]
        public void GrammarCanParseTwoSidedIntervalRange()
        {
            ComparatorSet<SemanticVersion> cs = SemanticVersion.Grammar.TwoSidedIntervalRange.Parse("<3 >=5");
        }

        [Fact]
        public void GrammarCanParseOneSidedRangeExpression()
        {
            List<ComparatorSet<SemanticVersion>> r = SemanticVersion.Grammar.Range.Parse("<6 || >=4.5");
            Assert.Equal(r.Count, 2);
        }

        [Fact]
        public void GrammarCanParseRange()
        {
            List<ComparatorSet<SemanticVersion>> lcs = SemanticVersion.Grammar.Range.Parse("<8 >5 || > 13 || 47.3.6-alpha1");
            Assert.False(Range<SemanticVersion>.Intersect(lcs, SemanticVersion.Grammar.Range.Parse("5")));
            Assert.True(Range<SemanticVersion>.Intersect(lcs, SemanticVersion.Grammar.Range.Parse("6")));
        }
    }
}
