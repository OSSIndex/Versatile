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
           public static Parser<List<string>> CoreDrupal5Identifier
            {
                get
                {
                    return
                        from five in Parse.Char('5').Once().Text()
                        from minor in Minor.Or(MinorX)
                        let m = minor != "x" ? minor : string.Empty
                        select new List<string> { five, m, "" };  
                }
            } 
        }
    }
}
