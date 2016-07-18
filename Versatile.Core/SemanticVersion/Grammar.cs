﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Sprache;

namespace Versatile
{
    public partial class SemanticVersion
    {
        public class Grammar : Grammar<SemanticVersion>
        {
            public new static Parser<char> NonDigit
            {
                get
                {
                    return Parse.Letter.Or(Parse.Char('-')).Token();
                }
            }

            public static Parser<string> PreReleaseSuffix
            {
                get
                {

                    return
                        from dot in Parse.Char('-')
                        from m in PreRelease.Select(b => string.Join(".", b.ToArray()))
                        select m;
                }
            }

            public static Parser<string> BuildSuffix
            {
                get
                {

                    return
                        from dot in Parse.Char('+')
                        from b in Build.Select(b => string.Join(".", b.ToArray()))
                        select b;
                }
            }

            public static Parser<char> IdentifierCharacter
            {
                get
                {
                    return Parse.Digit.Or(NonDigit).Token();
                }
            }

            public static Parser<string> IdentifierCharacters
            {
                get
                {
                    return IdentifierCharacter.AtLeastOnce().Token().Text();
                }
            }


            public new static Parser<string> AlphaNumericIdentifier
            {
                get
                {
                    return
                        IdentifierCharacters.Concat(NonDigit.Once()).Concat(IdentifierCharacters)
                        .Or(IdentifierCharacters.Concat(NonDigit.Once()))
                        .Or(NonDigit.Once().Concat(IdentifierCharacters))
                        .Or(NonDigit.Once())
                        .Token()
                        .Text();
                }
            }

            public static Parser<string> BuildIdentifier
            {
                get
                {
                    return AlphaNumericIdentifier.Or(Digits);
                }
            }

            public static Parser<string> PreReleaseIdentifier
            {
                get
                {
                    return AlphaNumericIdentifier.Or(NumericIdentifier);
                }
            }


            public static Parser<IEnumerable<string>> DotSeparatedBuildIdentifier
            {
                get
                {
                    return
                        BuildIdentifier.DelimitedBy(Parse.String("."));
                }
            }


            public static Parser<IEnumerable<string>> Build
            {
                get
                {
                    return DotSeparatedBuildIdentifier;

                }
            }

            public static Parser<IEnumerable<string>> DotSeparatedPreReleaseIdentifiers
            {
                get
                {
                    return
                       PreReleaseIdentifier.DelimitedBy(Parse.String("."));
                }
            }

            public static Parser<IEnumerable<string>> PreRelease
            {
                get
                {
                    return DotSeparatedPreReleaseIdentifiers;
                }
            }

            public static Parser<IEnumerable<string>> VersionCore
            {
                get
                {
                    return
                        Major
                            .Then(major => (Minor.XOr(Parse.Return(string.Empty)))
                            .Select(minor => major + "|" + minor))
                            .Then(minor => (Patch.XOr(Parse.Return(string.Empty)))
                            .Select(patch => (minor + "|" + patch)))
                            .Select(v => v.Split('|').ToList());
                }
            }

            public static Parser<List<string>> SemanticVersionIdentifier
            {
                get
                {
                    return

                        Major
                        .Then(major => (Minor.XOr(Parse.Return(string.Empty)))
                        .Select(minor => major + "|" + minor))
                        .Then(minor => (Patch.XOr(Parse.Return(string.Empty)))
                        .Select(patch => (minor + "|" + patch)))
                        .Then(patch => (PreReleaseSuffix.XOr(Parse.Return(string.Empty)))
                        .Select(prs => (patch + "|" + prs)))
                        .Then(prs => (BuildSuffix.XOr(Parse.Return(string.Empty)))
                        .Select(bs => (prs + "|" + bs)))
                        .Select(v => v.Split('|').ToList());
                }
            } //<valid semver> ::= <version core> | <version core> "-" <pre-release> | <version core> "+" <build> | <version core> "-" <pre-release> "+" <build>

            public static Parser<SemanticVersion> SemanticVersion
            {
                get
                {
                    return SemanticVersionIdentifier.Select(v => new SemanticVersion(v.ToList()));

                }
            }

            public static Parser<ComparatorSet<SemanticVersion>> LessThanRange
            {
                get
                {
                    return
                        from o in LessThan
                        from v in SemanticVersion
                        select new ComparatorSet<SemanticVersion>
                        {
                            new Comparator<SemanticVersion> (ExpressionType.GreaterThan, V.Min()),
                            new Comparator<SemanticVersion> (ExpressionType.LessThan, v)
                        };
                }
            }

            public static Parser<ComparatorSet<SemanticVersion>> LessThanOrEqualRange
            {
                get
                {
                    return
                        from o in LessThanOrEqual
                        from v in SemanticVersion
                        select new ComparatorSet<SemanticVersion>
                        {
                            new Comparator<SemanticVersion> (ExpressionType.GreaterThan, V.Min()),
                            new Comparator<SemanticVersion> (ExpressionType.LessThanOrEqual, v)
                        };
                }
            }

            public static Parser<ComparatorSet<SemanticVersion>> GreaterThanRange
            {
                get
                {
                    return
                        from o in GreaterThan
                        from v in SemanticVersion
                        select new ComparatorSet<SemanticVersion>
                        {
                            new Comparator<SemanticVersion> (ExpressionType.LessThan, V.Max()),
                            new Comparator<SemanticVersion> (ExpressionType.GreaterThan, v)
                        };
                }
            }

            public static Parser<ComparatorSet<SemanticVersion>> GreaterThanOrEqualRange
            {
                get
                {
                    return
                        from o in GreaterThanOrEqual
                        from v in SemanticVersion
                        select new ComparatorSet<SemanticVersion>
                        {
                            new Comparator<SemanticVersion> (ExpressionType.LessThan, V.Max()),
                            new Comparator<SemanticVersion> (ExpressionType.GreaterThanOrEqual, v)
                        };
                }
            }

            public static Parser<ExpressionType> OneSidedRangeOperator
            {
                get
                {
                    return LessThanOrEqual.Or(GreaterThanOrEqual).Or(LessThan).Or(GreaterThan);
                }
            }


            public static Parser<ComparatorSet<SemanticVersion>> OneSidedRange
            {
                get
                {
                    return LessThanRange.Or(LessThanOrEqualRange).Or(GreaterThanRange).Or(GreaterThanOrEqualRange)
                        .Or(SemanticVersion.Select(s => new ComparatorSet<SemanticVersion> { new Comparator<SemanticVersion>(ExpressionType.Equal, s) }));
                }
            }

            public static Parser<ComparatorSet<SemanticVersion>> AllXRange
            {
                get
                {
                    return XIdentifier.Return(new ComparatorSet<SemanticVersion>());
                }
            }

            public static Parser<ComparatorSet<SemanticVersion>> MajorXRange
            {
                get
                {
                    return
                        from major in Major.Select(m =>
                        {
                            int num;
                            Int32.TryParse(m.ToString(), out num);
                            return num;

                        })
                        from dot in Parse.Char('.').Once().Text().Token()
                        from x in XIdentifier
                        select new ComparatorSet<SemanticVersion>
                        {
                            new Comparator<SemanticVersion>(ExpressionType.GreaterThanOrEqual, new SemanticVersion(major)),
                            new Comparator<SemanticVersion>(ExpressionType.LessThan, new SemanticVersion(major + 1))
                        };
                }
            }

            public static Parser<ComparatorSet<SemanticVersion>> MajorMinorXRange
            {
                get
                {
                    return
                        from major in Major.Select(m =>
                        {
                            int num;
                            Int32.TryParse(m.ToString(), out num);
                            return num;

                        })
                        from minor in Minor.Select(m =>
                        {
                            int num;
                            Int32.TryParse(m.ToString(), out num);
                            return num;

                        })
                        from dot in Parse.Char('.').Once().Text().Token()
                        from x in XIdentifier
                        select new ComparatorSet<SemanticVersion>
                        {
                            new Comparator<SemanticVersion>(ExpressionType.GreaterThanOrEqual, new SemanticVersion(major, minor)),
                            new Comparator<SemanticVersion>(ExpressionType.LessThan, new SemanticVersion(major, minor  + 1))
                        };
                }
            }

            public static Parser<ComparatorSet<SemanticVersion>> XRange
            {
                get
                {
                    return MajorMinorXRange.Or(MajorXRange).Or(AllXRange);
                }
            }

            public static Parser<ComparatorSet<SemanticVersion>> MajorTildeRange
            {
                get
                {
                    return
                        from tilde in Parse.Char('~')
                        from major in Major.Select(m =>
                        {
                            int num;
                            Int32.TryParse(m.ToString(), out num);
                            return num;
                        })
                        select new ComparatorSet<SemanticVersion>
                        {
                        new Comparator<SemanticVersion>(ExpressionType.GreaterThanOrEqual, new SemanticVersion(major)),
                        new Comparator<SemanticVersion>(ExpressionType.LessThan, new SemanticVersion(major + 1))
                        };

                }
            }

            public static Parser<ComparatorSet<SemanticVersion>> MajorMinorTildeRange
            {
                get
                {
                    return
                        from tilde in Parse.Char('~')
                        from major in Major.Select(m =>
                        {
                            int num;
                            Int32.TryParse(m.ToString(), out num);
                            return num;
                        })
                        from minor in Minor.Select(m =>
                        {
                            int num;
                            Int32.TryParse(m.ToString(), out num);
                            return num;

                        })
                        select new ComparatorSet<SemanticVersion>
                        {
                            new Comparator<SemanticVersion>(ExpressionType.GreaterThanOrEqual, new SemanticVersion(major, minor)),
                            new Comparator<SemanticVersion>(ExpressionType.LessThan, new SemanticVersion(major, minor  + 1))
                        };

                }
            }

            public static Parser<ComparatorSet<SemanticVersion>> MajorMinorPatchTildeRange
            {
                get
                {
                    return
                        from tilde in Parse.Char('~')
                        from major in Major.Select(m =>
                        {
                            int num;
                            Int32.TryParse(m.ToString(), out num);
                            return num;
                        })
                        from minor in Minor.Select(m =>
                        {
                            int num;
                            Int32.TryParse(m.ToString(), out num);
                            return num;

                        })
                        from patch in Patch.Select(m =>
                        {
                            int num;
                            Int32.TryParse(m.ToString(), out num);
                            return num;

                        })

                        select new ComparatorSet<SemanticVersion>
                        {
                            new Comparator<SemanticVersion>(ExpressionType.GreaterThanOrEqual, new SemanticVersion(major, minor, patch)),
                            new Comparator<SemanticVersion>(ExpressionType.LessThan, new SemanticVersion(major, minor  + 1))
                        };

                }
            }

            public static Parser<ComparatorSet<SemanticVersion>> MajorMinorPatchPreReleaseTildeRange
            {
                get
                {
                    return
                        from tilde in Parse.Char('~')
                        from major in Major.Select(m =>
                        {
                            int num;
                            Int32.TryParse(m.ToString(), out num);
                            return num;
                        })
                        from minor in Minor.Select(m =>
                        {
                            int num;
                            Int32.TryParse(m.ToString(), out num);
                            return num;

                        })
                        from patch in Patch.Select(m =>
                        {
                            int num;
                            Int32.TryParse(m.ToString(), out num);
                            return num;

                        })
                        from prerelease in PreReleaseSuffix
                        select new ComparatorSet<SemanticVersion>
                        {
                            new Comparator<SemanticVersion>(ExpressionType.GreaterThanOrEqual, new SemanticVersion(major, minor, patch, prerelease)),
                            new Comparator<SemanticVersion>(ExpressionType.LessThan, new SemanticVersion(major, minor  + 1))
                        };

                }
            }

            public static Parser<ComparatorSet<SemanticVersion>> TildeRange
            {
                get
                {
                    return MajorMinorPatchPreReleaseTildeRange.Or(MajorMinorPatchTildeRange).Or(MajorMinorTildeRange).Or(MajorTildeRange);
                }
            }

            /*
            public static Parser<ComparatorSet<SemanticVersion>> MajorMinorPatchCaretRange
            {
                get
                {
                    return
                        from caret in Parse.Char('^')
                        from major in Major.Except(Parse.Char('0')).Select(m =>
                        {
                            int num;
                            Int32.TryParse(m.ToString(), out num);
                            return num;
                        })
                        from minor in Minor.Select(m =>
                        {
                            int num;
                            Int32.TryParse(m.ToString(), out num);
                            return num;

                        })
                        from patch in Patch.Select(m =>
                        {
                            int num;
                            Int32.TryParse(m.ToString(), out num);
                            return num;

                        })

                        select new ComparatorSet<SemanticVersion>
                        {
                            new Comparator<SemanticVersion>(ExpressionType.GreaterThanOrEqual, new SemanticVersion(major, minor, patch)),
                            new Comparator<SemanticVersion>(ExpressionType.LessThan, new SemanticVersion(major + 1))
                        };

                }
            }

            public static Parser<ComparatorSet<SemanticVersion>> MinorPatchCaretRange
            {
                get
                {
                    return
                        from caret in Parse.Char('^')
                        from major in Parse.Char('0').Return(0)
                        from minor in Minor.Select(m =>
                        {
                            int num;
                            Int32.TryParse(m.ToString(), out num);
                            return num;

                        })
                        from patch in Patch.Select(m =>
                        {
                            int num;
                            Int32.TryParse(m.ToString(), out num);
                            return num;

                        })
                        select new ComparatorSet<SemanticVersion>
                        {
                            new Comparator<SemanticVersion>(ExpressionType.GreaterThanOrEqual, new SemanticVersion(major, minor, patch)),
                            new Comparator<SemanticVersion>(ExpressionType.LessThan, new SemanticVersion(major, minor  + 1))
                        };

                }
            }

            public static Parser<ComparatorSet<SemanticVersion>> PatchCaretRange
            {
                get
                {
                    return
                        from tilde in Parse.Char('^')
                        from major in Parse.Char('0').Return(0)
                        from d1 in Parse.Char('.')
                        from minor in Parse.Char('0').Return(0)
                        from patch in Patch.Select(m =>
                        {
                            int num;
                            Int32.TryParse(m.ToString(), out num);
                            return num;

                        })
                        select new ComparatorSet<SemanticVersion>
                        {
                            new Comparator<SemanticVersion>(ExpressionType.GreaterThanOrEqual, new SemanticVersion(major, minor, patch)),
                            new Comparator<SemanticVersion>(ExpressionType.LessThan, new SemanticVersion(major, minor, patch + 1))
                        };

                }
            }

            public static Parser<ComparatorSet<SemanticVersion>> CaretRange
            {
                get
                {
                    return MajorMinorPatchCaretRange.Or(MinorPatchCaretRange).Or(PatchCaretRange);
                }
            }
            */

            public static Parser<ComparatorSet<SemanticVersion>> Range
            {
                get
                {
                    return OneSidedRange.Or(XRange).Or(TildeRange).Or(CaretRange);
                }
            }
        }
    }
}
