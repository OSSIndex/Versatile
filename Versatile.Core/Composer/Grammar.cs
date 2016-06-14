using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Sprache;

namespace Versatile
{
    public partial class Composer
    {
        public class Grammar
        {
            public static Parser<string> Digits
            {
                get
                {
                    return Parse.Digit.AtLeastOnce().Text().Token();
                }
            }

            public static Parser<char> NonDigit
            {
                get
                {
                    return Parse.Letter.Token();
                }
            }

            public static Parser<char> PositiveDigit
            {
                get
                {
                    return Parse.Digit.Except(Parse.Char('0')).Token();
                }
            }

            public static Parser<string> NumericIdentifier
            {
                get
                {
                    return Digits;
                }
            }

            public static Parser<char> AlphaNumericIdentifierChar
            {
                get
                {
                    return Parse.Digit.Or(NonDigit).Token();
                }
            }

            public static Parser<string> AlphaNumericIdentifier
            {
                get
                {
                    return AlphaNumericIdentifierChar.AtLeastOnce().Token().Text();
                }
            }


            public static Parser<string> Major
            {
                get
                {
                    return NumericIdentifier;
                }
            }

            public static Parser<string> Minor
            {
                get
                {
                    return
                        from dot in Parse.Char('.')
                        from m in NumericIdentifier
                        select m;
                }
            }

            public static Parser<string> Patch
            {
                get
                {
                    return
                        from dot in Parse.Char('.')
                        from m in NumericIdentifier
                        select m;
                }
            }

            public static Parser<ComposerPreRelease> PreRelease
            {
                get
                {

                    return
                        from dash in Parse.Char('-')
                        from s in Parse.String("dev").Or(Parse.String("patch")).Or(Parse.String("alpha")).Or(Parse.String("beta")).Or(Parse.String("RC")).Token().Text()
                        from d in NumericIdentifier.Optional().Select(o => o.GetOrElse(string.Empty))
                        select new ComposerPreRelease(s.ToString(), d);
                }
            }

            public static Parser<List<string>> ComposerVersionIdentifier
            {
                get
                {
                    return

                        Major
                        .Then(major => Minor.XOr(Parse.Return(string.Empty)).Select(minor => major + "|" + minor))
                        .Then(minor => Patch.XOr(Parse.Return(string.Empty)).Select(patch => (minor + "|" + patch)))
                        .Then(patch => PreRelease.XOr(Parse.Return(new ComposerPreRelease("", "")))
                            .Select(prs => patch + "|" + prs.ToString()))
                            .Select(v => v.Split('|').ToList());
                }
            } //<valid semver> ::= <version core> | <version core> "-" <pre-release> | <version core> "+" <build> | <version core> "-" <pre-release> "+" <build>

            public static Parser<Composer> ComposerVersion
            {
                get
                {
                    return ComposerVersionIdentifier.Select(v => new Composer(v));

                }
            }

            public static Parser<ExpressionType> LessThan
            {
                get
                {
                    return Parse.String("<").Once().Token().Return(ExpressionType.LessThan);
                }
            }

            public static Parser<ExpressionType> LessThanOrEqual
            {
                get
                {

                    return Parse.String("<=").Once().Token().Return(ExpressionType.LessThanOrEqual);
                }
            }

            public static Parser<ExpressionType> GreaterThan
            {
                get
                {
                    return Parse.String(">").Once().Token().Return(ExpressionType.GreaterThan);
                }
            }

            public static Parser<ExpressionType> GreaterThanOrEqual
            {
                get
                {

                    return Parse.String(">=").Once().Token().Return(ExpressionType.GreaterThanOrEqual);
                }
            }

            public static Parser<ExpressionType> Equal
            {
                get
                {

                    return Parse.String("=").Once().Token().Return(ExpressionType.LessThanOrEqual);
                }
            }

            public static Parser<ExpressionType> Tilde
            {
                get
                {

                    return Parse.String("~").Token().Return(ExpressionType.OnesComplement);
                }
            }

            public static Parser<ExpressionType> VersionOperator
            {
                get
                {
                    return LessThanOrEqual.Or(GreaterThanOrEqual).Or(LessThan).Or(GreaterThan).Or(Equal).Or(Tilde);
                }
            }


            public static Parser<Comparator> Comparator
            {
                get
                {
                    return VersionOperator.Then(o =>
                        ComposerVersion.Select(version
                        => new Comparator(o, version)))
                        .Or(ComposerVersion.Select(s => new Comparator(ExpressionType.Equal, s)));
                }
            }

            public static Parser<string> XIdentifier
            {
                get
                {
                    return
                        Parse.Char('*').XOr(Parse.Char('x')).XOr(Parse.Char('X')).Once().Text().Token();
                }
            }


            public static Parser<ComparatorSet> AllXRange
            {
                get
                {
                    return XIdentifier.Return(new ComparatorSet());
                }
            }

            public static Parser<ComparatorSet> MajorXRange
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
                        select new ComparatorSet
                        {
                            new Comparator(ExpressionType.GreaterThanOrEqual, new Composer(major)),
                            new Comparator(ExpressionType.LessThan, new Composer(major + 1))
                        };
                }
            }

            public static Parser<ComparatorSet> MajorMinorXRange
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
                        select new ComparatorSet
                        {
                            new Comparator(ExpressionType.GreaterThanOrEqual, new Composer(major, minor)),
                            new Comparator(ExpressionType.LessThan, new Composer(major, minor  + 1))
                        };
                }
            }

            public static Parser<ComparatorSet> XRange
            {
                get
                {
                    return MajorMinorXRange.Or(MajorXRange).Or(AllXRange);
                }
            }

            public static Parser<ComparatorSet> MajorTildeRange
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
                        select new ComparatorSet
                        {
                        new Comparator(ExpressionType.GreaterThanOrEqual, new Composer(major)),
                        new Comparator(ExpressionType.LessThan, new Composer(major + 1))
                        };

                }
            }

            public static Parser<ComparatorSet> MajorMinorTildeRange
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
                        select new ComparatorSet
                        {
                        new Comparator(ExpressionType.GreaterThanOrEqual, new Composer(major, minor)),
                        new Comparator(ExpressionType.LessThan, new Composer(major, minor  + 1))
                        };

                }
            }

            public static Parser<ComparatorSet> MajorMinorPatchTildeRange
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

                        select new ComparatorSet
                        {
                        new Comparator(ExpressionType.GreaterThanOrEqual, new Composer(major, minor, patch)),
                        new Comparator(ExpressionType.LessThan, new Composer(major, minor  + 1))
                        };

                }
            }

            public static Parser<ComparatorSet> MajorMinorPatchPreReleaseTildeRange
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
                        from prerelease in PreRelease
                        select new ComparatorSet
                        {
                            new Comparator(ExpressionType.GreaterThanOrEqual, new Composer(major, minor, patch, prerelease.ToString())),
                            new Comparator(ExpressionType.LessThan, new Composer(major, minor  + 1))
                        };

                }
            }

            public static Parser<ComparatorSet> TildeRange
            {
                get
                {
                    return MajorMinorPatchPreReleaseTildeRange.Or(MajorMinorPatchTildeRange).Or(MajorMinorTildeRange).Or(MajorTildeRange);
                }
            }

            public static Parser<ComparatorSet> MajorMinorPatchCaretRange
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

                        select new ComparatorSet
                        {
                        new Comparator(ExpressionType.GreaterThanOrEqual, new Composer(major, minor, patch)),
                        new Comparator(ExpressionType.LessThan, new Composer(major + 1))
                        };

                }
            }

            public static Parser<ComparatorSet> MinorPatchCaretRange
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
                        select new ComparatorSet
                        {
                        new Comparator(ExpressionType.GreaterThanOrEqual, new Composer(major, minor, patch)),
                        new Comparator(ExpressionType.LessThan, new Composer(major, minor  + 1))
                        };

                }
            }

            public static Parser<ComparatorSet> PatchCaretRange
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
                        select new ComparatorSet
                        {
                        new Comparator(ExpressionType.GreaterThanOrEqual, new Composer(major, minor, patch)),
                        new Comparator(ExpressionType.LessThan, new Composer(major, minor, patch + 1))
                        };

                }
            }

            public static Parser<ComparatorSet> CaretRange
            {
                get
                {
                    return MajorMinorPatchCaretRange.Or(MinorPatchCaretRange).Or(PatchCaretRange);
                }
            }
        }
    }
}
