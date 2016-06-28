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
            public static Parser<string> Digits
            {
                get
                {
                    return Sprache.Parse.Digit.AtLeastOnce().Text().Token();
                }
            }

            public static Parser<char> NonDigit
            {
                get
                {
                    return Sprache.Parse.Letter.Or(Sprache.Parse.Char('-')).Token();
                }
            }

            public static Parser<char> PositiveDigit
            {
                get
                {
                    return Sprache.Parse.Digit.Except(Sprache.Parse.Char('0')).Token();
                }
            }

            public static Parser<string> NumericIdentifier
            {
                get
                {
                    return Digits;
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

            public static Parser<string> AlphaNumericIdentifier
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

            public static Parser<char> RangeSymbol
            {
                get
                {
                    return Sprache.Parse.AnyChar.Where(c => "(),<>".Contains(c));
                }
            }

            public static Parser<int> Major
            {
                get
                {
                    return
                        from m in NumericIdentifier
                        select Int32.Parse(m);
                }
            }

            public static Parser<int> Minor
            {
                get
                {

                    return
                        from dot in Sprache.Parse.Char('.')
                        from m in NumericIdentifier
                        select Int32.Parse(m);
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
                    return AlphaNumericIdentifier;
                }
            }

            public static Parser<string> SpecialSuffix
            {
                get
                {

                    return
                        from dot in Sprache.Parse.Char('-')
                        from s in Special
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
                                    v[3] == "" ? 0 : Int32.Parse(v[3]));
                            }
                        });
                }
            }

            public static Parser<ExpressionType> LessThan
            {
                get
                {
                    return Sprache.Parse.String("<").Once().Token().Return(ExpressionType.LessThan);
                }
            }

            public static Parser<ComparatorSet<NuGetv2>> LessThanRange
            {
                get
                {
                    return
                        from o in LessThan
                        from v in NuGetv2Version
                        select new ComparatorSet<NuGetv2>
                        {
                            new Comparator<NuGetv2> (ExpressionType.GreaterThan, NuGetv2.MIN),
                            new Comparator<NuGetv2> (ExpressionType.LessThan, v)
                        };
                }
            }
     

            public static Parser<ExpressionType> LessThanOrEqual
            {
                get
                {

                    return Sprache.Parse.String("<=").Once().Token().Return(ExpressionType.LessThanOrEqual);
                }
            }

            public static Parser<ComparatorSet<NuGetv2>> LessThanOrEqualRange
            {
                get
                {
                    return
                        from o in LessThanOrEqual
                        from v in NuGetv2Version
                        select new ComparatorSet<NuGetv2>
                        {
                            new Comparator<NuGetv2> (ExpressionType.GreaterThan, NuGetv2.MIN),
                            new Comparator<NuGetv2> (ExpressionType.LessThanOrEqual, v)
                        };
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

            public static Parser<ComparatorSet<NuGetv2>> GreaterThanRange
            {
                get
                {
                    return
                        from o in GreaterThan
                        from v in NuGetv2Version
                        select new ComparatorSet<NuGetv2>
                        {
                            new Comparator<NuGetv2> (ExpressionType.LessThan, NuGetv2.MAX),
                            new Comparator<NuGetv2> (ExpressionType.GreaterThan, v)
                        };
                }
            }

            public static Parser<ComparatorSet<NuGetv2>> GreaterThanOrEqualRange
            {
                get
                {
                    return
                        from o in GreaterThanOrEqual
                        from v in NuGetv2Version
                        select new ComparatorSet<NuGetv2>
                        {
                            new Comparator<NuGetv2> (ExpressionType.LessThan, NuGetv2.MAX),
                            new Comparator<NuGetv2> (ExpressionType.GreaterThanOrEqual, v)
                        };
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

            public static Parser<ExpressionType> OneSidedRangeOperator
            {
                get
                {
                    return LessThanOrEqual.Or(GreaterThanOrEqual).Or(LessThan).Or(GreaterThan).Or(Equal);
                }
            }


            public static Parser<ComparatorSet<NuGetv2>> OneSidedRange
            {
                get
                {
                    return LessThanRange.Or(LessThanOrEqualRange).Or(GreaterThanRange).Or(GreaterThanOrEqualRange)
                        .Or(NuGetv2Version.Select(s => new ComparatorSet<NuGetv2> { new Comparator<NuGetv2>(ExpressionType.Equal, s) }));
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
                        from major in Major
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
                        from major in Major
                        from minor in Minor
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
                        from major in Major
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
                        from major in Major
                        from minor in Minor
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
                        from major in Major
                        from minor in Minor
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
                        from major in Major
                        from minor in Minor
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
                                : new Comparator<NuGetv2>(ExpressionType.GreaterThan, new NuGetv2(0, 0, 0, 0)),
                                r.IsDefined ? new Comparator<NuGetv2>(ExpressionType.LessThan, r.Get())
                                : new Comparator<NuGetv2>(ExpressionType.LessThan, new NuGetv2(100000, 100000, 100000, 100000)),
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
                                : new Comparator<NuGetv2>(ExpressionType.GreaterThan, new NuGetv2(0, 0, 0, 0)),
                                r.IsDefined ? new Comparator<NuGetv2>(ExpressionType.LessThanOrEqual, r.Get())
                                : new Comparator<NuGetv2>(ExpressionType.LessThanOrEqual, new NuGetv2(100000, 100000, 100000, 100000)),
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
                                : new Comparator<NuGetv2>(ExpressionType.GreaterThanOrEqual, new NuGetv2(0, 0, 0, 0)),
                                r.IsDefined ? new Comparator<NuGetv2>(ExpressionType.LessThan, r.Get())
                                : new Comparator<NuGetv2>(ExpressionType.LessThan, new NuGetv2(100000, 100000, 100000, 100000)),
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
                                : new Comparator<NuGetv2>(ExpressionType.GreaterThanOrEqual, new NuGetv2(0, 0, 0, 0)),
                                r.IsDefined ? new Comparator<NuGetv2>(ExpressionType.LessThanOrEqual, r.Get())
                                : new Comparator<NuGetv2>(ExpressionType.LessThanOrEqual, new NuGetv2(100000, 100000, 100000, 100000)),
                            }
                        select x;
                }
            }

            public static Parser<ComparatorSet<NuGetv2>> Range
            {
                get
                {
                    return OneSidedRange.Or(ClosedBracketClosedBracketRange).Or(ClosedBracketOpenBracketRange).Or(OpenBracketClosedBracketRange).Or(OpenBracketOpenBracketRange);
                }
            }
        }
    }
}
