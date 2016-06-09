using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Versatile
{
    public class Comparator<T>
    {
        private Tuple<ExpressionType, T> _comparator;

        public ExpressionType Operator { get { return _comparator.Item1; } }

        public T Version { get { return _comparator.Item2; } }

        public Comparator(ExpressionType op, T version)
        {
            _comparator = new Tuple<ExpressionType, T>(op, version);
        }
    }   
}
