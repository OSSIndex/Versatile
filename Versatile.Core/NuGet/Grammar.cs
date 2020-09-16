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
        public class Grammar : Grammar<NuGetv2>
        {
        
            public new static Parser<char> NonDigit
            {
                get
                {
                    return Sprache.Parse.Letter.Or(Sprache.Parse.Char('-')).Token();
                }
            }

            public static Parser<char> IdentifierCharacter
            {
                get
                {
                    return Sprache.Parse.Digit.Or(NonDigit).Token();
                }
            }

            public static Parser<string> IdentifierCharacters
            {
                get
                {
                    return IdentifierCharacter.AtLeastOnce().Token().Text();
                }
            }

            public static Parser<char> RangeSymbol
            {
                get
                {
                    return Sprache.Parse.AnyChar.Where(c => "(),<>".Contains(c));
                }
            }

            public static Parser<char> AnyChar
            {
                get
                {
                    return Sprache.Parse.AnyChar;
                }
            }

            public static Parser<int> Build
            {
                get
                {

                    return
                        from dot in Sprache.Parse.Char('.')
                        from m in NumericIdentifier
                        select Int32.Parse(m);
                }
            }

            public static Parser<int> Revision
            {
                get
                {

                    return
                        from dot in Sprache.Parse.Char('.')
                        from m in NumericIdentifier
                        select Int32.Parse(m);
                }
            }

            public static Parser<string> Special
            {
                get
                {
                    return AlphaNumericIdentifier.DelimitedBy(Dot).Select(d => d.Aggregate((d1, d2) => d1 + "." + d2));
                }
            }

            public static Parser<string> SpecialSuffix
            {
                get
                {
                    return 
                        from dash in Sprache.Parse.Char('-')
                        from s in AnyIdentifier
                        select s;
                }
            }

            public static Parser<NuGetv2> NuGetv2Version
            {
                get
                {
                    return
                        Major
                        .Then(major => (Minor.Select(m_ => m_.ToString()).XOr(Sprache.Parse.Return(string.Empty))).Select(minor => major + "|" + minor))
                        .Then(minor => (Build.Select(m_ => m_.ToString()).XOr(Sprache.Parse.Return(string.Empty))).Select(build => minor + "|" + build))
                        .Then(build => (Revision.Select(m_ => m_.ToString()).XOr(Sprache.Parse.Return(string.Empty))).Select(revision => build + "|" + revision))
                        .Then(revision => (SpecialSuffix.XOr(Sprache.Parse.Return(string.Empty))).Select(special => revision + "|" + special))
                        .Select(v => v.Split('|').ToList())
                        .Select(v =>
                        {
                            if (v[3] == "")
                            {
                                return new NuGetv2(v[0] == "" ? 0 : Int32.Parse(v[0]), v[1] == "" ? 0 : Int32.Parse(v[1]), v[2] == "" ? 0 : Int32.Parse(v[2]), v[4]);
                            }
                            else
                            {
                                return new NuGetv2(v[0] == "" ? 0 : Int32.Parse(v[0]), v[1] == "" ? 0 : Int32.Parse(v[1]), v[2] == "" ? 0 : Int32.Parse(v[2]),
                                    v[3] == "" ? 0 : Int32.Parse(v[3]), v[4]);
                            }
                        });
                }
            }

            public static Parser<ExpressionType> Tilde
            {
                get
                {

                    return Sprache.Parse.String("~").Token().Return(ExpressionType.OnesComplement);
                }
            }


            public static Parser<ComparatorSet<NuGetv2>> AllXRange
            {
                get
                {
                    return XIdentifier.Return(new ComparatorSet<NuGetv2>());
                }
            }

            public static Parser<ComparatorSet<NuGetv2>> MajorXRange
            {
                get
                {
                    return
                        from major in Major.Select(m => Int32.Parse(m))
                        from dot in Sprache.Parse.Char('.').Once().Text().Token()
                        from x in XIdentifier
                        select new ComparatorSet<NuGetv2>
                        {
                        new Comparator<NuGetv2>(ExpressionType.GreaterThanOrEqual, new NuGetv2(major, 0, 0, 0)),
                        new Comparator<NuGetv2>(ExpressionType.LessThan, new NuGetv2((major + 1), 0, 0, 0))
                        };
                }
            }

            public static Parser<ComparatorSet<NuGetv2>> MajorMinorXRange
            {
                get
                { 
                    return
                        from major in Major.Select(m => Int32.Parse(m))
                        from minor in Minor.Select(m => Int32.Parse(m))
                        from dot in Sprache.Parse.Char('.').Once().Text().Token()
                        from x in XIdentifier
                        select new ComparatorSet<NuGetv2>
                        {
                            new Comparator<NuGetv2>(ExpressionType.GreaterThanOrEqual, new NuGetv2(major, minor, 0, 0)),
                            new Comparator<NuGetv2>(ExpressionType.LessThan, new NuGetv2(major, minor  + 1, 0, 0))
                        };
                }
            }

            public static Parser<ComparatorSet<NuGetv2>> XRange
            {
                get
                {
                    return MajorMinorXRange.Or(MajorXRange).Or(AllXRange);
                }
            }

            public static Parser<ComparatorSet<NuGetv2>> MajorTildeRange
            {
                get
                {
                    return
                        from tilde in Sprache.Parse.Char('~')
                        from major in Major.Select(m => Int32.Parse(m))
                        select new ComparatorSet<NuGetv2>
                        {
                            new Comparator<NuGetv2>(ExpressionType.GreaterThanOrEqual, new NuGetv2(major, 0, 0, 0)),
                            new Comparator<NuGetv2>(ExpressionType.LessThan, new NuGetv2(major + 1, 0, 0, 0))
                        };

                }
            }

            public static Parser<ComparatorSet<NuGetv2>> MajorMinorTildeRange
            {
                get
                {
                    return
                        from tilde in Sprache.Parse.Char('~')
                        from major in Major.Select(m => Int32.Parse(m))
                        from minor in Minor.Select(m => Int32.Parse(m))
                        select new ComparatorSet<NuGetv2>
                        {
                            new Comparator<NuGetv2>(ExpressionType.GreaterThanOrEqual, new NuGetv2(major, minor, 0, 0)),
                            new Comparator<NuGetv2>(ExpressionType.LessThan, new NuGetv2(major, minor  + 1, 0, 0))
                        };

                }
            }

            public static Parser<ComparatorSet<NuGetv2>> MajorMinorBuildTildeRange
            {
                get
                {
                    return
                        from tilde in Sprache.Parse.Char('~')
                        from major in Major.Select(m => Int32.Parse(m))
                        from minor in Minor.Select(m => Int32.Parse(m))
                        from build in Build
                        select new ComparatorSet<NuGetv2>
                        {
                            new Comparator<NuGetv2>(ExpressionType.GreaterThanOrEqual, new NuGetv2(major, minor, build, 0)),
                            new Comparator<NuGetv2>(ExpressionType.LessThan, new NuGetv2(major, minor  + 1, build, 0))
                        };

                }
            }

            public static Parser<ComparatorSet<NuGetv2>> MajorMinorBuildSpecialTildeRange
            {
                get
                {
                    return
                        from tilde in Sprache.Parse.Char('~')
                        from major in Major.Select(m => Int32.Parse(m))
                        from minor in Minor.Select(m => Int32.Parse(m))
                        from build in Build
                        from special in Special
                        select new ComparatorSet<NuGetv2>
                        {
                            new Comparator<NuGetv2>(ExpressionType.GreaterThanOrEqual, new NuGetv2(major, minor, build, special)),
                            new Comparator<NuGetv2>(ExpressionType.LessThan, new NuGetv2(major, minor  + 1, 0, 0))
                        };

                }
            }

            public static Parser<ComparatorSet<NuGetv2>> TildeRange
            {
                get
                {
                    return MajorMinorBuildSpecialTildeRange.Or(MajorMinorBuildTildeRange).Or(MajorMinorTildeRange).Or(MajorTildeRange);
                }
            }

            public static Parser<ComparatorSet<NuGetv2>> OpenBracketOpenBracketRange
            {
                get
                {
                    return
                        from left_bracket in Sprache.Parse.Char('(').Token()
                        from l in NuGetv2Version.Optional()
                        from c in Sprache.Parse.Char(',').Token()
                        from r in NuGetv2Version.Optional()
                        from right_bracket in Sprache.Parse.Char(')').Token()
                        let x = new ComparatorSet<NuGetv2>
                            {
                                l.IsDefined ? new Comparator<NuGetv2>(ExpressionType.GreaterThan, l.Get()) 
                                : new Comparator<NuGetv2>(ExpressionType.GreaterThan, V.Min()),
                                r.IsDefined ? new Comparator<NuGetv2>(ExpressionType.LessThan, r.Get())
                                : new Comparator<NuGetv2>(ExpressionType.LessThan, V.Max()),
                            }
                        select x;
                }
            }

            public static Parser<ComparatorSet<NuGetv2>> OpenBracketClosedBracketRange
            {
                get
                {
                    return
                        from left_bracket in Sprache.Parse.Char('(').Token()
                        from l in NuGetv2Version.Optional()
                        from c in Sprache.Parse.Char(',').Token()
                        from r in NuGetv2Version.Optional()
                        from right_bracket in Sprache.Parse.Char(']').Token()
                        let x = new ComparatorSet<NuGetv2>
                            {
                                l.IsDefined ? new Comparator<NuGetv2>(ExpressionType.GreaterThan, l.Get())
                                : new Comparator<NuGetv2>(ExpressionType.GreaterThan, V.Min()),
                                r.IsDefined ? new Comparator<NuGetv2>(ExpressionType.LessThanOrEqual, r.Get())
                                : new Comparator<NuGetv2>(ExpressionType.LessThanOrEqual, V.Max()),
                            }
                        select x;
                }
            }

            public static Parser<ComparatorSet<NuGetv2>> ClosedBracketOpenBracketRange
            {
                get
                {
                    return
                        from left_bracket in Sprache.Parse.Char('[').Token()
                        from l in NuGetv2Version.Optional()
                        from c in Sprache.Parse.Char(',').Token()
                        from r in NuGetv2Version.Optional()
                        from right_bracket in Sprache.Parse.Char(')').Token()
                        let x = new ComparatorSet<NuGetv2>
                            {
                                l.IsDefined ? new Comparator<NuGetv2>(ExpressionType.GreaterThanOrEqual, l.Get())
                                : new Comparator<NuGetv2>(ExpressionType.GreaterThanOrEqual, V.Min()),
                                r.IsDefined ? new Comparator<NuGetv2>(ExpressionType.LessThan, r.Get())
                                : new Comparator<NuGetv2>(ExpressionType.LessThan, V.Max()),
                            }
                        select x;
                }
            }

            public static Parser<ComparatorSet<NuGetv2>> ClosedBracketClosedBracketRange
            {
                get
                {
                    return
                        from left_bracket in Sprache.Parse.Char('[').Token()
                        from l in NuGetv2Version.Optional()
                        from c in Sprache.Parse.Char(',').Token()
                        from r in NuGetv2Version.Optional()
                        from right_bracket in Sprache.Parse.Char(']').Token()
                        let x = new ComparatorSet<NuGetv2>
                            {
                                l.IsDefined ? new Comparator<NuGetv2>(ExpressionType.GreaterThanOrEqual, l.Get())
                                : new Comparator<NuGetv2>(ExpressionType.GreaterThanOrEqual, V.Min()),
                                r.IsDefined ? new Comparator<NuGetv2>(ExpressionType.LessThanOrEqual, r.Get())
                                : new Comparator<NuGetv2>(ExpressionType.LessThanOrEqual, V.Max()),
                            }
                        select x;
                }
            }

            public static Parser<ComparatorSet<NuGetv2>> TwoSidedIntervalRange
            {
                get
                {
                    return
                        from le in OneSidedIntervalOperator
                        from l in NuGetv2Version.Token()
                        from a in Sprache.Parse.WhiteSpace.Or(Sprache.Parse.Char(',')).Token().Optional()
                        from re in OneSidedIntervalOperator
                        from r in NuGetv2Version.Token()
                        select new ComparatorSet<NuGetv2>
                        {
                            new Comparator<NuGetv2>(le,  l),
                            new Comparator<NuGetv2>(re, r)
                        };
                }
            }

            public static Parser<ComparatorSet<NuGetv2>> TwoSidedRange
            {
                get
                {
                    return TwoSidedIntervalRange.Or(ClosedBracketClosedBracketRange).Or(ClosedBracketOpenBracketRange).Or(OpenBracketClosedBracketRange).Or(OpenBracketOpenBracketRange);
                }
            }

            public static Parser<ComparatorSet<NuGetv2>> OneOrTwoSidedRange
            {
                get
                {
                    return TwoSidedRange.Or(OneSidedRange);
                }
            }

            public static Parser<List<ComparatorSet<NuGetv2>>> Range
            {
                get
                {
                    return CommaDelimitedRange.End().Or(OneOrTwoSidedRange.DelimitedBy(Sprache.Parse.String("||").Token())).End().Select(r => r.ToList());
                }
            }
        }
    }
}
