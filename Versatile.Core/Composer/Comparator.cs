using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Versatile
{
    public partial class Composer
    {
        public class Comparator
        {
            private Tuple<ExpressionType, Composer> _comparator;

            public ExpressionType Operator { get { return _comparator.Item1; } }

            public Composer Version { get { return _comparator.Item2; } }

            public Comparator(ExpressionType op, Composer version)
            {
                _comparator = new Tuple<ExpressionType, Composer>(op, version);
            }
        }
    }
}
