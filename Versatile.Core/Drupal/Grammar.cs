using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Sprache;

namespace Versatile
{
    public partial class Drupal
    {
        public class Grammar : Versatile.Grammar<Drupal>
        {
            public static Parser<string> CoreIdentifierPrefix
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
                        from dash in Dash.Or(Underscore)
                        from s in Parse.String("dev")
                            .Or(Parse.String("unstable"))
                            .Or(Parse.String("alpha"))
                            .Or(Parse.String("beta"))
                            .Or(Parse.String("rc"))
                            .Or(Parse.String("revision_").Return("rev"))
                            .Or(Parse.String("rev_").Return("rev"))
                            .Or(Parse.String("rev"))
                        .Text()
                        from d in NumericIdentifier.DelimitedBy(Dot).Optional().Select(o => o.GetOrElse(s == "dev" ? null : new List<string> { "0" }))
                        let has_number = !ReferenceEquals(null, d)
                        select has_number ? new List<string> {s}.Concat(d).ToList() : new List<string> {s};
                }
            }

            public static Parser<List<string>> CoreIdentifier
            {
                get
                {
                    return
                        from xdl in NumericIdentifier.Or(XIdentifier).DelimitedBy(Dot).Select(nid => nid.ToList())
                        let d = xdl.Select(d => d == "x" ? "0" : d).ToList()
                        let has1 = d.Count() == 1
                        let d1 = has1 ? new List<string> { d[0], "0", "0" } : null
                        let has2 = d.Count() == 2
                        let d2 = has2 ? new List<string> { d[0], d[1], "0" } : null
                        let has3 = d.Count() == 3
                        let d3 = has3 ? new List<string> { d[0], d[1], "0", d[2] } : null
                        select has1 ? d1 : has2 ? d2 : has3 ? d3 : d.Take(5).ToList();
                }
            }

            public static Parser<List<string>> ContribIdentifier
            {
                get
                {
                    return
                        from c in CoreIdentifierPrefix
                        from dash in Dash.Or(Underscore)
                        from major in Major
                        from patch in Patch
                        from pre in PreReleaseIdentifier.Optional().Select(p => p.GetOrElse(null))
                        let has_pre = pre is List<string> 
                        select has_pre ? new List<string> {c, major, "", patch, string.Join(".", pre) } : new List<string> {c, major, "", patch };
                }
            }

            public static Parser<List<string>> ContribIdentifierWithPatchXIdentifier
            {
                get
                {
                    return
                        from c in CoreIdentifierPrefix
                        from dash in Dash
                        from major in Major
                        from dot in Dot
                        from x in XIdentifier
                        from pre in PreReleaseIdentifier.Optional().Select(p => p.GetOrElse(null))
                        let has_pre = pre is List<string>
                        select has_pre ? new List<string> { c, major, "", "0", string.Join(".", pre) } : new List<string> { c, major, "", "0" };
                }
            }

            public static Parser<List<string>> ContribIdentitifierWithoutCoreIdentifierPrefix
            {
                get
                {
                    return
                        from xdl in NumericIdentifier.Or(XIdentifier).DelimitedBy(Dot).Select(nid => nid.ToList())
                        let d = xdl.Select(d => d == "x" ? "0" : d).ToList()
                        from prerelease in PreReleaseIdentifier.Optional()
                        let has1 = d.Count() == 1
                        let d1 = has1 ? new List<string> { d[0], "0", "0", "0", prerelease.IsDefined ? string.Join(".", prerelease.Get()) : string.Empty } : null
                        let has2 = d.Count() == 2
                        let d2 = has2 ? new List<string> { d[0], d[1], "0", "0", prerelease.IsDefined ? string.Join(".", prerelease.Get()) : string.Empty } : null
                        let has3 = d.Count() == 3
                        let d3 = has3 ? new List<string> { d[0], d[1], "0", d[2], prerelease.IsDefined ? string.Join(".", prerelease.Get()) : string.Empty } : null
                        let has4 = d.Count() == 4
                        let d4 = has4 ? new List<string> { d[0], d[1], d[2], d[3], prerelease.IsDefined ? string.Join(".", prerelease.Get()) : string.Empty } : null
                        select has1 ? d1 : has2 ? d2 : has3 ? d3 : has4 ? d4 : d.Take(5).ToList();
                }
            }

            public static Parser<List<string>> ContribIdentitifierWithNumericCoreIdentifier
            {
                get
                {
                    return
                        from xdl in NumericIdentifier.Or(XIdentifier).DelimitedBy(Dot).Select(nid => nid.ToList())
                        let c = xdl.Select(d => d == "x" ? "0" : d).ToList()
                        from dash in Dash.Or(Underscore)
                        from contrib in NumericIdentifier.Or(XIdentifier).DelimitedBy(Dot).Select(nid => nid.ToList())
                        let has2 = c.Count() >= 2
                        let core = has2 ? new List<string> { c[0], c[1] } : new List<string> { c[0], "0" }
                        select core.Concat(contrib).ToList();
                }
            }

            public static Parser<List<string>> CoreIdentitifierPrefixOnly
            {
                get
                {
                    return
                        from c in CoreIdentifierPrefix
                        select new List<string> { c, "0", "0", "0" }; 
                    
                }
            }

            public static Parser<List<string>> ContribIdentifierWithDashOnly
            {
                get
                {
                    return
                        from d in Dash
                        select new List<string> { "1", "0", "0", "0" };

                }
            }

            public static Parser<List<string>> ContribIdentifierWithPreReleaseOnly
            {
                get
                {
                    return
                        from m in Major
                        from x in MinorX
                        from p in PreReleaseIdentifier
                        select new List<string> { m, "0", "0", "0", string.Join(".", p) };
                }
            }

            public static Parser<Drupal> DrupalVersion
            {
                get
                {
                    return
                        from dv in ContribIdentifierWithPreReleaseOnly
                            .Or(ContribIdentifier)
                            .Or(ContribIdentifierWithPatchXIdentifier)
                            .Or(ContribIdentitifierWithNumericCoreIdentifier)
                            .Or(ContribIdentitifierWithoutCoreIdentifierPrefix)
                            .Or(ContribIdentifierWithDashOnly)
                            .Or(CoreIdentitifierPrefixOnly)
                        select new Drupal(dv);
                }
            }

            public static Parser<Composer> DrupalToComposerVersion
            {
                get
                {
                    return
                        from d in DrupalVersion
                        select new Composer(d.Major, d.Minor, d.Patch, d.PreRelease.ToString());
                }
            }

            public static Parser<ComparatorSet<Drupal>> TwoSidedIntervalRange
            {
                get
                {
                    return
                        from le in OneSidedIntervalOperator
                        from l in DrupalVersion.Token()
                        from a in Parse.WhiteSpace.Or(Ampersand).Token().Optional()
                        from re in OneSidedIntervalOperator
                        from r in DrupalVersion.Token()
                        select new ComparatorSet<Drupal>
                        {
                            new Comparator<Drupal>(le,  l),
                            new Comparator<Drupal>(re, r)
                        };
                }
            }

            public static Parser<ComparatorSet<Drupal>> BracketedTwoSidedIntervalRange
            {
                get
                {
                    return
                    from b1 in OpenBracket
                    from d in TwoSidedIntervalRange
                    from b2 in ClosedBracket
                    select d;
                }
            }

            public static Parser<ComparatorSet<Drupal>> BracketedOneSidedIntervalRange
            {
                get
                {
                    return
                    from b1 in OpenBracket
                    from d in OneSidedRange
                    from b2 in ClosedBracket
                    select d;
                }
            }

            public static Parser<ComparatorSet<Drupal>> OneOrTwoSidedRange
            {
                get
                {
                    return BracketedTwoSidedIntervalRange.Or(TwoSidedIntervalRange).Or(BracketedOneSidedIntervalRange).Or(OneSidedRange);
                }
            }

            public static Parser<List<ComparatorSet<Drupal>>> Range
            {
                get
                {
                    return CommaDelimitedRange.End().Or(OneOrTwoSidedRange.DelimitedBy(Parse.String("||").Token())).End().Select(r => r.ToList());
                }
            }



        }
    }
}
