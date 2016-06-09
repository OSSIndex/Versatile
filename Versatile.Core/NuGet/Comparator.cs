using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Versatile
{
    public partial class NuGetv2
    {
        public class Comparator
        {
            private Tuple<ExpressionType, NuGetv2> _comparator;

            public ExpressionType Operator { get { return _comparator.Item1; } }

            public NuGetv2 Version { get { return _comparator.Item2; } }

            public Comparator(ExpressionType op, NuGetv2 version)
            {
                _comparator = new Tuple<ExpressionType, NuGetv2>(op, version);
            }
        }
    }
}
