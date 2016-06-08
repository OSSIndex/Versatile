using System;
using System.Collections.Generic;
using System.Linq;

using Sprache;

namespace Versatile
{
    public partial class AssemblyVersion
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
                    return Parse.Letter.Or(Parse.Char('-')).Token();
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

            public static Parser<string> AssemblyVersionNumber
            {
                get
                {
                    return
                        from n in NumericIdentifier.Token()
                        let i = Int32.Parse(n)
                        where (i >= 0 && i <= 65534)
                        select n;

                }
            }

            public static Parser<string> Major
            {
                get
                {
                    return AssemblyVersionNumber;
                }
            }


            public static Parser<string> Minor
            {
                get
                {

                    return
                        from dot in Parse.Char('.')
                        from m in AssemblyVersionNumber
                        select m;
                }
            }

            public static Parser<string> BuildNumber
            {
                get
                {

                    return
                        from dot in Parse.Char('.')
                        from m in AssemblyVersionNumber
                        select m;
                }
            }

            public static Parser<string> Revision
            {
                get
                {

                    return
                        from dot in Parse.Char('.')
                        from m in AssemblyVersionNumber
                        select m;
                }
            }


            public static Parser<string> SpecialVersionSuffix
            {
                get
                {

                    return
                        from dot in Parse.Char('-')
                        from m in SpecialVersion.Select(b => string.Join(".", b.ToArray()))
                        select m;
                }
            }

            public static Parser<string> SpecialVersionIdentifier
            {
                get
                {
                    return AlphaNumericIdentifier.Or(NumericIdentifier);
                }
            }


            public static Parser<IEnumerable<string>> DotSeparatedSpecialVersionIdentifiers
            {
                get
                {
                    return
                       SpecialVersionIdentifier.DelimitedBy(Parse.String("."));
                }
            }

            public static Parser<IEnumerable<string>> SpecialVersion
            {
                get
                {
                    return DotSeparatedSpecialVersionIdentifiers;
                }
            }



            public static Parser<IEnumerable<string>> AssemblyVersionCore
            {
                get
                {
                    return
                        Major
                        .Then(major => (Minor.XOr(Parse.Return(string.Empty))).Select(minor => major + "|" + minor))
                        .Then(minor => (BuildNumber.XOr(Parse.Return(string.Empty))).Select(build => minor + "|" + build))
                        .Then(build => (Revision.XOr(Parse.Return(string.Empty))).Select(revision => build + "|" + revision))
                        .Select(v => v.Split('|').ToList());
                }
            }
        }
    }
}
