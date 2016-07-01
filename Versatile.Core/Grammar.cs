using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using Sprache;

namespace Versatile
{
    public abstract class Grammar
    {
        public static Parser<char> Dot
        {
            get
            {
                return Parse.Char('.');
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

    }
}
