using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sprache;

namespace Versatile
{
    public partial class Drupal
    {
        public class Grammar : Versatile.Grammar
        {
            public static Parser<string> CoreIdentifier
            {
                get
                {
                    return
                        from digits in Digits
                        from minor in MinorX
                        select digits;
                          
                }
            }

            public static Parser<List<string>> PreReleaseIdentifier
            {
                get
                {

                    return
                        from dash in Dash
                        from s in Parse.String("dev").Or(Parse.String("unstable")).Or(Parse.String("alpha")).Or(Parse.String("beta")).Or(Parse.String("rc")).Text()
                        from d in NumericIdentifier
                        select new List<string> { s, d };
                }
            }

            public static Parser<List<string>> ContribIdentifier
            {
                get
                {
                    return
                        from c in CoreIdentifier
                        from dash in Dash
                        from major in Major
                        from patch in Patch
                        from pre in PreReleaseIdentifier.Optional().Select(p => p.GetOrElse(null))
                        let has_pre = pre is List<string> 
                        select has_pre ? new List<string> {c, major, "", patch, string.Join(".", pre) } : new List<string> {c, major, "", patch };
                }
            }

            public static Parser<Drupal> DrupalVersion
            {
                get
                {
                    return
                        from dv in ContribIdentifier
                        select new Drupal(dv);
                }
            }
             
        }
    }
}
