using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Versatile
{
    
    public class ComparatorSet<T> : List<Comparator<T>>
    {
        public ComparatorSet() : base() { }

        public override string ToString()
        {
            return this.Select(cs => cs.Operator.ToString() +  cs.Version.ToString())
                .Aggregate((p, n) => p + " && " + n);
        }
    }
    
}
