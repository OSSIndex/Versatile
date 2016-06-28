using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Versatile
{
    
    public class ComparatorSet<T> : List<Comparator<T>> where T : IComparable, IComparable<T>, IEquatable<T>
    {
        public ComparatorSet() : base() { }

        public Interval<T> ToInterval()
        {
            if (this.Count != 2) throw new ArgumentOutOfRangeException("this", "The comparator set size must be 2.");
            if ((this[0].Operator == ExpressionType.LessThan || this[0].Operator == ExpressionType.LessThanOrEqual) &&
                (this[1].Operator == ExpressionType.GreaterThan || this[1].Operator == ExpressionType.GreaterThanOrEqual))
            {// first comparator has the endpoint, 2nd the startpoint
                return new Interval<T>(this[1].Version, this[1].Operator == ExpressionType.GreaterThan ? false : true,
                    this[0].Version, this[0].Operator == ExpressionType.LessThan ? false : true);
            }
            else if ((this[0].Operator == ExpressionType.GreaterThan || this[0].Operator == ExpressionType.GreaterThanOrEqual) &&
                        (this[1].Operator == ExpressionType.LessThan || this[1].Operator == ExpressionType.LessThanOrEqual))
            {// 2nd comparator has the endpoint, first the sta
                return new Interval<T>(this[0].Version, this[0].Operator == ExpressionType.GreaterThan ? false : true,
                    this[1].Version, this[1].Operator == ExpressionType.LessThan ? false : true);
            }
            else throw new ArgumentOutOfRangeException("this", "Cannot convert comparator expressions: + " + this.ToString() + ".");
        }

        public override string ToString()
        {
            return this.Select(cs => cs.Operator.ToString() +  cs.Version.ToString())
                .Aggregate((p, n) => p + " && " + n);
        }
    }
    
}
