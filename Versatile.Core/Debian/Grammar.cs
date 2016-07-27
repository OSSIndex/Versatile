using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sprache;

namespace Versatile
{
    public partial class Debian
    {
        public class Grammar : Grammar<Debian>
        {
            public static Parser<string> Epoch
            {
                get
                {
                    return
                        from d in Parse.Digit.Once().Text()
                        from c in Colon
                        select d;
                }
            }

            public static Parser<string> UpstreamVersion
            {
                get
                {
                    return Parse.Digit.Or(Parse.Letter).Or(Parse.Chars(".+:~")).AtLeastOnce().Text();
                }
            }

            public static Parser<string> DebianRevision
            {
                get
                {
                    return
                        from dash in Dash
                        from an in Parse.Digit.Or(Parse.Letter).Or(Parse.Chars(".+~")).AtLeastOnce().Text().End()
                        select an;
                }
            }

            public static Parser<List<string>> DebianVersionIdentifier
            {
                get
                {

                    return
                        from e in Epoch.Optional()
                        let uv_parser = e.IsDefined ? UpstreamVersion.Except(Colon) : UpstreamVersion
                        from v in uv_parser.DelimitedBy(Dash)
                        let uv = v.Count() > 1 ?  string.Join("-", v.Take(v.Count() - 1)) : v.First()
                        let dr = v.Count() == 1 ? string.Empty : v.Last()
                        select new List<string> { e.GetOrDefault(), !string.IsNullOrEmpty(uv) && char.IsDigit(uv[0]) ? uv : "0." + uv,
                            !string.IsNullOrEmpty(dr) && char.IsDigit(dr[0]) ? dr : "1." + dr };
                }
            }

            public static Parser<Debian> DebianVersion
            {
                get
                {
                    return DebianVersionIdentifier.Select(dvi => new Debian(dvi));
                }
            }

        }
    }
}
