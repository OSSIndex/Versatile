using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using Sprache;

namespace Versatile
{
    public abstract class Grammar<T> where T: Version, IVersionFactory<T>, IComparable, IComparable<T>, IEquatable<T>, new()
    {
        public static T V = new T();

        public static Parser<char> Dot
        {
            get
            {
                return Parse.Char('.');
            }
        }

        public static Parser<char> Colon
        {
            get
            {
                return Parse.Char(':');
            }
        }

        public static Parser<char> Dash
        {
            get
            {
                return Parse.Char('-');
            }
        }

        public static Parser<char> Underscore
        {
            get
            {
                return Parse.Char('_');
            }
        }

        public static Parser<char> Caret
        {
            get
            {
                return Parse.Char('^');
            }
        }

        public static Parser<char> Comma
        {
            get
            {
                return Parse.Char(',');
            }
        }

        public static Parser<char> Ampersand
        {
            get
            {
                return Parse.Char('&');
            }
        }

        public static Parser<char> OpenBracket
        {
            get
            {
                return Parse.Char('(');
            }
        }

        public static Parser<char> ClosedBracket
        {
            get
            {
                return Parse.Char(')');
            }
        }

        public static Parser<string> Digits
        {
            get
            {
                return Parse.Digit.AtLeastOnce().Text();
            }
        }

        public static Parser<char> NonDigit
        {
            get
            {
                return Parse.Letter;
            }
        }

        public static Parser<char> PositiveDigit
        {
            get
            {
                return Parse.Digit.Except(Parse.Char('0'));
            }
        }

        public static Parser<string> NumericIdentifier
        {
            get
            {
                return Digits;
            }
        }

        public static Parser<string> NumericOnlyIdentifier
        {
            get
            {
                return
                from chars in Parse.Digit.Or(Dot).AtLeastOnce()
                let x = chars.Where(c => char.IsDigit(c)).ToArray()
                select new string(x);
            }
        }

        public static Parser<char> AlphaNumericIdentifierChar
        {
            get
            {
                return Parse.Digit.Or(NonDigit);
            }
        }

        public static Parser<string> AlphaNumericIdentifier
        {
            get
            {
                return AlphaNumericIdentifierChar.AtLeastOnce().Token().Text();
            }
        }

        public static Parser<char> AnyIdentifierChar
        {
            get
            {
                return Parse.Digit.Or(NonDigit).Or(Dot).Or(Dash);
            }
        }

        public static Parser<string> AnyIdentifier
        {
            get
            {
                return AnyIdentifierChar.AtLeastOnce().Token().Text();
            }
        }

        public static Parser<string> XIdentifier
        {
            get
            {
                return
                    Parse.Char('*').XOr(Parse.Char('x')).XOr(Parse.Char('X')).Once().Text();
            }
        }

        public static Parser<IOption<string>> XOptional
        {
            get
            {
                return Dot.Then(d => XIdentifier).Select(x => "x").Optional();
            }
        }


        public static Parser<string> Major
        {
            get
            {
                return NumericIdentifier.Token();
            }
        }

        public static Parser<string> Minor
        {
            get
            {
                return
                    from dot in Parse.Char('.')
                    from m in NumericIdentifier.Token()
                    select m;
            }
        }

        public static Parser<string> MinorX
        {
            get
            {
                return
                    from dot in Parse.Char('.')
                    from x in XIdentifier
                    select x;
            }
        }

        public static Parser<string> Patch
        {
            get
            {
                return
                    from dot in Parse.Char('.')
                    from m in NumericIdentifier.Token()
                    select m;
            }
        }

        public static Parser<ExpressionType> Equal
        {
            get
            {
                return Sprache.Parse.String("=").Once().Token().Return(ExpressionType.Equal);
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

        public static Parser<ComparatorSet<T>> LessThanRange
        {
            get
            {
                return
                    from o in LessThan
                    from v in V.Parser
                    select new ComparatorSet<T>
                    {
                            new Comparator<T> (ExpressionType.GreaterThan, V.Min()),
                            new Comparator<T> (ExpressionType.LessThan, v)
                    };
            }
        }


        public static Parser<ComparatorSet<T>> LessThanOrEqualRange
        {
            get
            {
                return
                    from o in LessThanOrEqual
                    from v in V.Parser
                    select new ComparatorSet<T>
                    {
                            new Comparator<T> (ExpressionType.GreaterThan, V.Min()),
                            new Comparator<T> (ExpressionType.LessThanOrEqual, v)
                    };
            }
        }

        public static Parser<ComparatorSet<T>> GreaterThanRange
        {
            get
            {
                return
                    from o in GreaterThan
                    from v in V.Parser
                    select new ComparatorSet<T>
                    {
                            new Comparator<T> (ExpressionType.LessThan, V.Max()),
                            new Comparator<T> (ExpressionType.GreaterThan, v)
                    };
            }
        }

        public static Parser<ComparatorSet<T>> GreaterThanOrEqualRange
        {
            get
            {
                return
                    from o in GreaterThanOrEqual
                    from v in V.Parser
                    select new ComparatorSet<T>
                    {
                            new Comparator<T> (ExpressionType.LessThan, V.Max()),
                            new Comparator<T> (ExpressionType.GreaterThanOrEqual, v)
                    };
            }
        }

        public static Parser<ComparatorSet<T>> EqualRange
        {
            get
            {
                return
                    from o in Equal
                    from v in V.Parser
                    select new ComparatorSet<T>
                    {
                        new Comparator<T> (ExpressionType.Equal, v)
                    };
            }
        }


        public static Parser<ComparatorSet<T>> OneSidedRange
        {
            get
            {
                return LessThanRange.Or(LessThanOrEqualRange).Or(GreaterThanRange).Or(GreaterThanOrEqualRange)
                    .Or(V.Parser.Select(s => new ComparatorSet<T> { new Comparator<T>(ExpressionType.Equal, s) }));
            }
        }


        public static Parser<ExpressionType> OneSidedIntervalOperator
        {
            get
            {
                return LessThanOrEqual.Or(LessThan).Or(GreaterThanOrEqual).Or(GreaterThan);
            }
        }

        public static Parser<Tuple<int, int, int>> CaretRangeIdentifier
        {
            get
            {
                return
                    from c in Caret
                    from major in Major.Select(m => Int32.Parse(m)).Or(XIdentifier.Return(-1))
                    from minor in Minor.Select(m => Int32.Parse(m)).Or(XIdentifier.Return(-1)).Optional()
                    from patch in Patch.Except(Parse.Char('0')).Select(m => Int32.Parse(m)).Optional()
                    select new Tuple<int, int, int>(major, minor.GetOrElse(-1), patch.GetOrElse(-1));
            }
        }

        public static Parser<ComparatorSet<T>> CaretRange
        {
            get
            {
                return CaretRangeIdentifier.Select(cr => CaretVersionToRange(cr));
            }
        }

        public static Parser<List<ComparatorSet<T>>> CommaDelimitedRange
        {
            get
            {
                return
                    from cd in V.Parser.DelimitedBy(Comma.Token())
                    let x = cd.Select(c => new ComparatorSet<T> { new Comparator<T>(ExpressionType.Equal, c) }) .ToList()
                    select x;
            }
        }

        public static Parser<List<ComparatorSet<T>>> BracketedRange(Parser<List<ComparatorSet<T>>> range)
        {
            return
                from b1 in OpenBracket
                from r in range
                from b2 in ClosedBracket
                select r;

        }
        public static Parser<ComparatorSet<T>> BracketedRange(Parser<ComparatorSet<T>> range)
        {
            return
                from b1 in OpenBracket
                from r in range
                from b2 in ClosedBracket
                select r;
        }


        public static ComparatorSet<T> CaretVersionToRange(Tuple<int, int, int> cv)
        {
            int major = cv.Item1;
            int minor = cv.Item2;
            int patch = cv.Item3;
            if (major != -1 && minor != -1 && patch != -1) //^7.1.9
            {
                if (major > 0) //^7.1.9
                {
                    return new ComparatorSet<T>
                    {
                        new Comparator<T>(ExpressionType.GreaterThanOrEqual,
                            V.Construct(new List<string> { major.ToString(), minor.ToString(), patch.ToString() })),
                        new Comparator<T>(ExpressionType.LessThan,
                            V.Construct(new List<string> { (major + 1).ToString(), "0", "0" }))
                    };
                }
                else if (major == 0 && minor > 0) //^0.1.9
                {
                    return new ComparatorSet<T>
                    {
                        new Comparator<T>(ExpressionType.GreaterThanOrEqual,
                            V.Construct(new List<string> { major.ToString(), minor.ToString(), patch.ToString() })),
                        new Comparator<T>(ExpressionType.LessThan,
                            V.Construct(new List<string> { "0", (minor + 1).ToString(), "0", "0" }))
                    };
                }
                else if (major == 0 && minor == 0 && patch > 0) //^0.0.9
                {
                    return new ComparatorSet<T>
                    {
                        new Comparator<T>(ExpressionType.GreaterThanOrEqual,
                            V.Construct(new List<string> { major.ToString(), minor.ToString(), patch.ToString() })),
                        new Comparator<T>(ExpressionType.LessThan,
                            V.Construct(new List<string> { "0", "0", (patch + 1).ToString() }))
                    };
                }
                else throw new ArgumentOutOfRangeException("cv", "Major, minor, patch can't all be zero.");
            }
            else if (major != -1 && minor != -1 && patch == -1) //^2.4.x
            {
                if (major > 0) //^7.1.x
                {
                    return new ComparatorSet<T>
                    {
                        new Comparator<T>(ExpressionType.GreaterThanOrEqual,
                            V.Construct(new List<string> { major.ToString(), minor.ToString(), "0" })),
                        new Comparator<T>(ExpressionType.LessThan,
                            V.Construct(new List<string> { (major + 1).ToString(), "0", "0" }))
                    };
                }
                else if (major == 0 && minor > 0) //^0.1.x
                {
                    return new ComparatorSet<T>
                    {
                        new Comparator<T>(ExpressionType.GreaterThanOrEqual,
                            V.Construct(new List<string> { major.ToString(), minor.ToString(), patch.ToString() })),
                        new Comparator<T>(ExpressionType.LessThan,
                            V.Construct(new List<string> { "0", (minor + 1).ToString(), "0", "0" }))
                    };
                }
                else if (major == 0 && minor == 0) //0.0.x
                {
                    return new ComparatorSet<T>
                    {
                        new Comparator<T>(ExpressionType.GreaterThanOrEqual, V.Construct(new List<string> { major.ToString(), minor.ToString(), "" })),
                        new Comparator<T>(ExpressionType.LessThan, V.Construct(new List<string> { "0", "1", "" }))
                    };
                }
                else throw new ArgumentOutOfRangeException("cv", "Invalid values for major,minor,patch.");
            }
            else if (major != -1 && minor == -1) //^2.x
            {
                if (major > 0) //^7.x
                {
                    return new ComparatorSet<T>
                    {
                        new Comparator<T>(ExpressionType.GreaterThanOrEqual,
                            V.Construct(new List<string> { major.ToString(), "0", "0" })),
                        new Comparator<T>(ExpressionType.LessThan,
                            V.Construct(new List<string> { (major + 1).ToString(), "0", "0" }))
                    };
                }
                else  //^0.x
                {
                    return new ComparatorSet<T>
                    {
                        new Comparator<T>(ExpressionType.GreaterThanOrEqual,
                            V.Construct(new List<string> { "0", "0", "0" })),
                        new Comparator<T>(ExpressionType.LessThan,
                            V.Construct(new List<string> { "1", "0", "0" }))
                    };
                }
            }
            else throw new ArgumentOutOfRangeException("cv", "Invalid major,minor,match values.");
        }


    }
}
