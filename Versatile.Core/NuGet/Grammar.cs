using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Sprache;

namespace Versatile
{
    public partial class NuGetv2
    {
        public class Grammar
        {
            public static Parser<NuGetv2> NuGetv2Version
            {
                get
                {
                    return Sprache.Parse.AnyChar.Many().Token().Text().Select(s =>
                    {
                        NuGetv2 v;
                        if (NuGetv2.TryParse(s, out v))
                        {
                            return v;
                        }
                        else
                        {
                            throw new ParseException(string.Format("Could not parse {0} as NuGetv2 version.", s));
                        }
                    });
                }
            }

            public static Parser<int> Major
            {
                get
                {
                    return NuGetv2Version.Select(n => n.Version.Major);
                }
            }

            public static Parser<int> Minor
            {
                get
                {
                    return NuGetv2Version.Select(n => n.Version.Minor);
                }
            }

            public static Parser<int> Build
            {
                get
                {
                    return NuGetv2Version.Select(n => n.Version.Build);
                }
            }

            public static Parser<int> Revision
            {
                get
                {
                    return NuGetv2Version.Select(n => n.Version.Revision);
                }
            }

            public static Parser<string> Special
            {
                get
                {
                    return NuGetv2Version.Select(n => n.SpecialVersion);
                }
            }


            public static Parser<ExpressionType> LessThan
            {
                get
                {
                    return Sprache.Parse.String("<").Once().Token().Return(ExpressionType.LessThan);
                }
            }

            public static Parser<ExpressionType> LessThanOrEqual
            {
                get
                {

                    return Sprache.Parse.String("<=").Once().Token().Return(ExpressionType.LessThanOrEqual);
                }
            }

            public static Parser<ExpressionType> GreaterThan
            {
                get
                {
                    return Sprache.Parse.String(">").Once().Token().Return(ExpressionType.GreaterThan);
                }
            }

            public static Parser<ExpressionType> GreaterThanOrEqual
            {
                get
                {

                    return Sprache.Parse.String(">=").Once().Token().Return(ExpressionType.GreaterThanOrEqual);
                }
            }

            public static Parser<ExpressionType> Equal
            {
                get
                {

                    return Sprache.Parse.String("=").Once().Token().Return(ExpressionType.LessThanOrEqual);
                }
            }

            public static Parser<ExpressionType> Tilde
            {
                get
                {

                    return Sprache.Parse.String("~").Token().Return(ExpressionType.OnesComplement);
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
                        NuGetv2Version.Select(version
                        => new Comparator(o, version)))
                        .Or(NuGetv2Version.Select(s => new Comparator(ExpressionType.Equal, s)));
                }
            }

            public static Parser<string> XIdentifier
            {
                get
                {
                    return
                        Sprache.Parse.Char('*').XOr(Sprache.Parse.Char('x')).XOr(Sprache.Parse.Char('X')).Once().Text().Token();
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
                        from major in Major
                        from dot in Sprache.Parse.Char('.').Once().Text().Token()
                        from x in XIdentifier
                        select new ComparatorSet
                        {
                        new Comparator(ExpressionType.GreaterThanOrEqual, new NuGetv2(major, 0, 0, 0)),
                        new Comparator(ExpressionType.LessThan, new NuGetv2((major + 1), 0, 0, 0))
                        };
                }
            }

            public static Parser<ComparatorSet> MajorMinorXRange
            {
                get
                { 
                    return
                        from major in Major
                        from minor in Minor
                        from dot in Sprache.Parse.Char('.').Once().Text().Token()
                        from x in XIdentifier
                        select new ComparatorSet
                        {
                            new Comparator(ExpressionType.GreaterThanOrEqual, new NuGetv2(major, minor, 0, 0)),
                            new Comparator(ExpressionType.LessThan, new NuGetv2(major, minor  + 1, 0, 0))
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
                        from tilde in Sprache.Parse.Char('~')
                        from major in Major
                        select new ComparatorSet
                        {
                            new Comparator(ExpressionType.GreaterThanOrEqual, new NuGetv2(major, 0, 0, 0)),
                            new Comparator(ExpressionType.LessThan, new NuGetv2(major + 1, 0, 0, 0))
                        };

                }
            }

            public static Parser<ComparatorSet> MajorMinorTildeRange
            {
                get
                {
                    return
                        from tilde in Sprache.Parse.Char('~')
                        from major in Major
                        from minor in Minor
                        select new ComparatorSet
                        {
                            new Comparator(ExpressionType.GreaterThanOrEqual, new NuGetv2(major, minor, 0, 0)),
                            new Comparator(ExpressionType.LessThan, new NuGetv2(major, minor  + 1, 0, 0))
                        };

                }
            }

            public static Parser<ComparatorSet> MajorMinorBuildTildeRange
            {
                get
                {
                    return
                        from tilde in Sprache.Parse.Char('~')
                        from major in Major
                        from minor in Minor
                        from build in Build
                        select new ComparatorSet
                        {
                            new Comparator(ExpressionType.GreaterThanOrEqual, new NuGetv2(major, minor, build, 0)),
                            new Comparator(ExpressionType.LessThan, new NuGetv2(major, minor  + 1, build, 0))
                        };

                }
            }

            public static Parser<ComparatorSet> MajorMinorBuildSpecialTildeRange
            {
                get
                {
                    return
                        from tilde in Sprache.Parse.Char('~')
                        from major in Major
                        from minor in Minor
                        from build in Build
                        from special in Special
                        select new ComparatorSet
                        {
                            new Comparator(ExpressionType.GreaterThanOrEqual, new NuGetv2(major, minor, build, special)),
                            new Comparator(ExpressionType.LessThan, new NuGetv2(major, minor  + 1, 0, 0))
                        };

                }
            }

            public static Parser<ComparatorSet> TildeRange
            {
                get
                {
                    return MajorMinorBuildSpecialTildeRange.Or(MajorMinorBuildTildeRange).Or(MajorMinorTildeRange).Or(MajorTildeRange);
                }
            }

            /*
            public static Parser<ComparatorSet> MajorMinorBuildCaretRange
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
                        new Comparator(ExpressionType.GreaterThanOrEqual, new NuGetv2(major, minor, patch)),
                        new Comparator(ExpressionType.LessThan, new NuGetv2(major + 1))
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
                        new Comparator(ExpressionType.GreaterThanOrEqual, new NuGetv2(major, minor, patch)),
                        new Comparator(ExpressionType.LessThan, new NuGetv2(major, minor  + 1))
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
                        new Comparator(ExpressionType.GreaterThanOrEqual, new NuGetv2(major, minor, patch)),
                        new Comparator(ExpressionType.LessThan, new NuGetv2(major, minor, patch + 1))
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
            */
        }
    }
 
}
