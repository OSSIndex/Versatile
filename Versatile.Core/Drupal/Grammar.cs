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
                        from d in NumericIdentifier.Optional().Select(o => o.GetOrElse(s == "dev"? string.Empty : "0"))
                        let has_number = !string.IsNullOrEmpty(d)
                        select has_number ? new List<string> { s, d } : new List<string> { s };
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

            public static Parser<ComparatorSet<Drupal>> LessThanRange
            {
                get
                {
                    return
                        from o in LessThan
                        from v in DrupalVersion
                        select new ComparatorSet<Drupal>
                        {
                            new Comparator<Drupal> (ExpressionType.GreaterThan, V.Min()),
                            new Comparator<Drupal> (ExpressionType.LessThan, v)
                        };
                }
            }


            public static Parser<ComparatorSet<Drupal>> LessThanOrEqualRange
            {
                get
                {
                    return
                        from o in LessThanOrEqual
                        from v in DrupalVersion
                        select new ComparatorSet<Drupal>
                        {
                            new Comparator<Drupal> (ExpressionType.GreaterThan, V.Min()),
                            new Comparator<Drupal> (ExpressionType.LessThanOrEqual, v)
                        };
                }
            }

            public static Parser<ComparatorSet<Drupal>> GreaterThanRange
            {
                get
                {
                    return
                        from o in GreaterThan
                        from v in DrupalVersion
                        select new ComparatorSet<Drupal>
                        {
                            new Comparator<Drupal> (ExpressionType.LessThan, V.Max()),
                            new Comparator<Drupal> (ExpressionType.GreaterThan, v)
                        };
                }
            }

            public static Parser<ComparatorSet<Drupal>> GreaterThanOrEqualRange
            {
                get
                {
                    return
                        from o in GreaterThanOrEqual
                        from v in DrupalVersion
                        select new ComparatorSet<Drupal>
                        {
                            new Comparator<Drupal> (ExpressionType.LessThan, V.Max()),
                            new Comparator<Drupal> (ExpressionType.GreaterThanOrEqual, v)
                        };
                }
            }

            public static Parser<ComparatorSet<Drupal>> OneSidedRange
            {
                get
                {
                    return LessThanRange.Or(LessThanOrEqualRange).Or(GreaterThanRange).Or(GreaterThanOrEqualRange)
                        .Or(DrupalVersion.Select(s => new ComparatorSet<Drupal> { new Comparator<Drupal>(ExpressionType.Equal, s) }));
                }
            }

            public static Parser<ComparatorSet<Drupal>> Range
            {
                get
                {
                    return OneSidedRange;
                }
            }



        }
    }
}
