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
        public class Grammar : Grammar<Composer>
        {
   
            public static Parser<List<string>> PreReleaseIdentifier
            {
                get
                {

                    return
                        from dash in Parse.Char('-')
                        from s in Parse.String("dev").Or(Parse.String("patch")).Or(Parse.String("alpha")).Or(Parse.String("beta")).Or(Parse.String("RC")).Token().Text()
                        from d in NumericIdentifier.Optional().Select(o => o.GetOrElse(string.Empty))
                        from v in AlphaNumericIdentifier.DelimitedBy(Parse.Char('-')).Select(vo => string.Join("-", vo))
                        .Optional().Select(vos => vos.GetOrElse(string.Empty))
                        select new List<string> { s, d + v };
                }
            }

            public static Parser<ComposerPreRelease> PreRelease
            {
                get
                {

                    return PreReleaseIdentifier.Select(p => new ComposerPreRelease(p));
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
                        .Then(patch => PreReleaseIdentifier.Select(p => p[0] + "-" + p[1]).XOr(Parse.Return(string.Empty))
                            .Select(prs => patch + "|" + prs))
                        .Select(v => v.Split('|').ToList());
                }
            } //<valid semver> ::= <version core> | <version core> "-" <pre-release> | <version core> "+" <build> | <version core> "-" <pre-release> "+" <build>

            public static Parser<List<string>> CoreVersionIdentifier
            {
                get
                {
                    return
                        Major
                        .Then(major => Minor.XOr(Parse.Return(string.Empty)).Select(minor => major + "|" + minor))
                        .Then(minor => Patch.XOr(Parse.Return(string.Empty)).Select(patch => (minor + "|" + patch)))
                        .Select(v => v.Split('|').ToList());
                }
            }

            public static Parser<List<string>> PreReleaseOnlyIdentifier
            {
                get
                {
                    return
                        from s in Parse.String("dev").Or(Parse.String("patch")).Or(Parse.String("alpha")).Or(Parse.String("beta")).Or(Parse.String("RC")).Token().Text()
                        from dash in Parse.Char('-')
                        from v in AlphaNumericIdentifier.DelimitedBy(Parse.Char('-'))
                        let p = s + dash + v.Aggregate((p, n) => p + "-" + n)
                        select new List<string> { "0", "0", "0", p };
                }
            }

            public static Parser<Composer> ComposerVersion
            {
                get
                {
                    return 
                        from v in Parse.Char('v').Optional()
                        from c in ComposerVersionIdentifier.XOr(PreReleaseOnlyIdentifier)
                        select new Composer(c);
                }
            }

            public static Parser<ComparatorSet<Composer>> LessThanRange
            {
                get
                {
                    return
                        from o in LessThan
                        from v in ComposerVersion
                        select new ComparatorSet<Composer>
                        {
                            new Comparator<Composer> (ExpressionType.GreaterThan, V.Min()),
                            new Comparator<Composer> (ExpressionType.LessThan, v)
                        };
                }
            }

            public static Parser<ComparatorSet<Composer>> LessThanOrEqualRange
            {
                get
                {
                    return
                        from o in LessThanOrEqual
                        from v in ComposerVersion
                        select new ComparatorSet<Composer>
                        {
                            new Comparator<Composer> (ExpressionType.GreaterThan, V.Min()),
                            new Comparator<Composer> (ExpressionType.LessThanOrEqual, v)
                        };
                }
            }

            public static Parser<ComparatorSet<Composer>> GreaterThanRange
            {
                get
                {
                    return
                        from o in GreaterThan
                        from v in ComposerVersion
                        select new ComparatorSet<Composer>
                        {
                            new Comparator<Composer> (ExpressionType.LessThan, Composer.MAX),
                            new Comparator<Composer> (ExpressionType.GreaterThan, v)
                        };
                }
            }

            public static Parser<ComparatorSet<Composer>> GreaterThanOrEqualRange
            {
                get
                {
                    return
                        from o in GreaterThanOrEqual
                        from v in ComposerVersion
                        select new ComparatorSet<Composer>
                        {
                            new Comparator<Composer> (ExpressionType.LessThan, Composer.MAX),
                            new Comparator<Composer> (ExpressionType.GreaterThanOrEqual, v)
                        };
                }
            }

            public static Parser<ComparatorSet<Composer>> OneSidedRange
            {
                get
                {
                    return LessThanOrEqualRange.Or(GreaterThanOrEqualRange).Or(LessThanRange).Or(GreaterThanRange)
                        .Or(ComposerVersion.Select(s => new ComparatorSet<Composer> { new Comparator<Composer>(ExpressionType.Equal, s) }));
                }
            }

            public static Parser<ExpressionType> Tilde
            {
                get
                {

                    return Parse.String("~").Token().Return(ExpressionType.OnesComplement);
                }
            }

            public static Parser<ComparatorSet<Composer>> AllXRange
            {
                get
                {
                    return XIdentifier.Return(new ComparatorSet<Composer>
                    {
                        new Comparator<Composer>(ExpressionType.GreaterThanOrEqual, V.Min()),
                        new Comparator<Composer>(ExpressionType.LessThan, Composer.MAX)
                    });
                }
            }

            public static Parser<ComparatorSet<Composer>> MajorXRange
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
                        select new ComparatorSet<Composer>
                        {
                            new Comparator<Composer>(ExpressionType.GreaterThanOrEqual, new Composer(major)),
                            new Comparator<Composer>(ExpressionType.LessThan, new Composer(major + 1))
                        };
                }
            }

            public static Parser<ComparatorSet<Composer>> MajorMinorXRange
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
                        select new ComparatorSet<Composer>
                        {
                            new Comparator<Composer>(ExpressionType.GreaterThanOrEqual, new Composer(major, minor)),
                            new Comparator<Composer>(ExpressionType.LessThan, new Composer(major, minor  + 1))
                        };
                }
            }

            public static Parser<ComparatorSet<Composer>> XRange
            {
                get
                {
                    return MajorMinorXRange.Or(MajorXRange).Or(AllXRange);
                }
            }

            public static Parser<ComparatorSet<Composer>> MajorTildeRange
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
                        select new ComparatorSet<Composer>
                        {
                            new Comparator<Composer>(ExpressionType.GreaterThanOrEqual, new Composer(major)),
                            new Comparator<Composer>(ExpressionType.LessThan, new Composer(major + 1))
                        };

                }
            }

            public static Parser<ComparatorSet<Composer>> MajorMinorTildeRange
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
                        select new ComparatorSet<Composer>
                        {
                        new Comparator<Composer>(ExpressionType.GreaterThanOrEqual, new Composer(major, minor)),
                        new Comparator<Composer>(ExpressionType.LessThan, new Composer(major, minor  + 1))
                        };

                }
            }

            public static Parser<ComparatorSet<Composer>> MajorMinorPatchTildeRange
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

                        select new ComparatorSet<Composer>
                        {
                        new Comparator<Composer>(ExpressionType.GreaterThanOrEqual, new Composer(major, minor, patch)),
                        new Comparator<Composer>(ExpressionType.LessThan, new Composer(major, minor  + 1))
                        };

                }
            }

            public static Parser<ComparatorSet<Composer>> MajorMinorPatchPreReleaseTildeRange
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
                        select new ComparatorSet<Composer>
                        {
                            new Comparator<Composer>(ExpressionType.GreaterThanOrEqual, new Composer(major, minor, patch, prerelease.ToString())),
                            new Comparator<Composer>(ExpressionType.LessThan, new Composer(major, minor  + 1))
                        };

                }
            }

            public static Parser<ComparatorSet<Composer>> TildeRange
            {
                get
                {
                    return MajorMinorPatchPreReleaseTildeRange.Or(MajorMinorPatchTildeRange).Or(MajorMinorTildeRange).Or(MajorTildeRange);
                }
            }

            /*
            public static Parser<ComparatorSet<Composer>> MajorMinorPatchCaretRange
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

                        select new ComparatorSet<Composer>
                        {
                            new Comparator<Composer>(ExpressionType.GreaterThanOrEqual, new Composer(major, minor, patch)),
                            new Comparator<Composer>(ExpressionType.LessThan, new Composer(major + 1))
                        };

                }
            }

            
            public static Parser<ComparatorSet<Composer>> MinorPatchCaretRange
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
                        select new ComparatorSet<Composer>
                        {
                            new Comparator<Composer>(ExpressionType.GreaterThanOrEqual, new Composer(major, minor, patch)),
                            new Comparator<Composer>(ExpressionType.LessThan, new Composer(major, minor  + 1))
                        };

                }
            }

            public static Parser<ComparatorSet<Composer>> PatchCaretRange
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
                        select new ComparatorSet<Composer>
                        {
                            new Comparator<Composer>(ExpressionType.GreaterThanOrEqual, new Composer(major, minor, patch)),
                            new Comparator<Composer>(ExpressionType.LessThan, new Composer(major, minor, patch + 1))
                        };

                }
            }

            public static Parser<ComparatorSet<Composer>> MajorMinorCaretRange
            {
                get
                {
                    return
                        from caret in Parse.Char('^')
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
                        select new ComparatorSet<Composer>
                        {
                            new Comparator<Composer>(ExpressionType.GreaterThanOrEqual, new Composer(major, minor, 0)),
                            new Comparator<Composer>(ExpressionType.LessThan, new Composer(major + 1))
                        };

                }
            }


            /*
            public static Parser<ComparatorSet<Composer>> PatchCaretRange
            {
                get
                {
                    return
                        from caret in Parse.Char('^')
                        from version in ComposerVersion.Select(v =>
                        {
                            if (v.Major.HasValue  && v.Minor.HasValue && v.Patch.HasValue)
                            {
                                int major = v.Major.Value;
                                int minor = v.Minor.Value;
                                int patch = v.Patch.Value;
                                if (major > 0)
                                {
                                    return new ComparatorSet<Composer>
                                    {
                                        new Comparator<Composer>(ExpressionType.GreaterThanOrEqual, new Composer(v.m)),
                                        new Comparator<Composer>(ExpressionType.LessThan, new Composer(major + 1))
                                    };
                                }
                                else if (minor > 0)
                                {
                                    return new ComparatorSet<Composer>
                                    {
                                        new Comparator<Composer>(ExpressionType.GreaterThanOrEqual,
                                            new Composer(major, v.Minor.Value, v.Patch.Value)),
                                        new Comparator<Composer>(ExpressionType.LessThan, new Composer(0, v.Minor.Value + 1))
                                    };

                                }
                                else if (patch > 0)
                                {
                                    return new ComparatorSet<Composer>
                                    {
                                        new Comparator<Composer>(ExpressionType.GreaterThanOrEqual,
                                            new Composer(major, v.Minor.Value, v.Patch.Value)),
                                        new Comparator<Composer>(ExpressionType.LessThan, new Composer(0, 0, patch + 1))
                                    };

                                }
                                else
                                {
                                    return new ComparatorSet<Composer>
                                    {
                                        new Comparator<Composer>(ExpressionType.GreaterThanOrEqual,
                                            new Composer(major, v.Minor.Value, v.Patch.Value)),
                                        new Comparator<Composer>(ExpressionType.LessThan, new Composer(0,1)
                                    };

                                }
                            }
                            else if (v.Major.HasValue && v.Minor.HasValue && v.Patch.HasValue)
                            {
                                return new ComparatorSet<Composer>
                                {
                                    new Comparator<Composer>(ExpressionType.GreaterThanOrEqual, new Composer(v.Major.Value, v.Minor.Value, v.Patch.Value)),
                                    new Comparator<Composer>(ExpressionType.LessThan, new Composer(v.Major.Value + 1))
                                };
                            }


                            else return new ComparatorSet<Composer>();
                        })
                        select version; 
                    
                }
            }
            */

            public static Parser<ComparatorSet<Composer>> HyphenRange
            {
                get
                {
                    return
                        from l in CoreVersionIdentifier
                        from dash in Parse.Char('-').Token()
                        from r in CoreVersionIdentifier
                        select new ComparatorSet<Composer>
                        {
                            new Comparator<Composer>(ExpressionType.GreaterThanOrEqual,  new Composer(l)),
                            new Comparator<Composer>(ExpressionType.LessThanOrEqual, new Composer(r))
                        };
                }
            }

            public static Parser<ComparatorSet<Composer>> TwoSidedIntervalRange
            {
                get
                {
                    return
                        from le in LessThanOrEqual.Or(GreaterThanOrEqual).Or(LessThan).Or(GreaterThan)
                        from l in ComposerVersion.Token()
                        from a in Parse.Char(',').Optional()
                        from re in LessThan.Or(LessThanOrEqual).Or(GreaterThan).Or(GreaterThanOrEqual)
                        from r in ComposerVersion.Token()
                        select new ComparatorSet<Composer>
                        {
                            new Comparator<Composer>(le,  l),
                            new Comparator<Composer>(re, r)
                        };
                }
            }

            public static Parser<ComparatorSet<Composer>> OneOrTwoSidedRange
            {
                get
                {
                    return TwoSidedIntervalRange.Or(XRange).Or(TildeRange).Or(CaretRange).Or(HyphenRange).Or(OneSidedRange);
                }
            }

            public static Parser<List<ComparatorSet<Composer>>> Range
            {
                get
                {
                    return OneOrTwoSidedRange.Token().DelimitedBy(Parse.String("||").Token()).Select(u => u.ToList());
                }
            }

            public static Parser<List<ComparatorSet<Composer>>> RangeIntersection
            {
                get
                {
                    return OneOrTwoSidedRange.DelimitedBy(Parse.WhiteSpace.Or(Parse.Char(',').Token())).Select(u => u.ToList());
                }
            }

        }
    }
}
