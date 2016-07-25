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

        public static Parser<char> Dash
        {
            get
            {
                return Parse.Char('-');
            }
        }

        public static Parser<char> Caret
        {
            get
            {
                return Parse.Char('^');
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
