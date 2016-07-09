using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Versatile
{
    public class Range<T> where T : IComparable, IComparable<T>, IEquatable<T>
    {
        public static List<ExpressionType> ValidRangeOperators =
            new List<ExpressionType> { ExpressionType.LessThan, ExpressionType.LessThanOrEqual, ExpressionType.GreaterThan, ExpressionType.GreaterThanOrEqual };

        public static bool InvokeBinaryExpression(BinaryExpression be)
        {
            return Expression.Lambda<Func<bool>>(be).Compile().Invoke();
        }

        public static BinaryExpression GetBinaryExpression(ExpressionType et, T left, T right)
        {
            return Expression.MakeBinary(et, Expression.Constant(left, typeof(T)), Expression.Constant(right, typeof(T)));
        }

        public static BinaryExpression GetBinaryExpression(T left, ComparatorSet<T> right)
        {
            if (right.Count == 0)
            {
                return GetBinaryExpression(ExpressionType.Equal, left, left);
            }
            else
            {
                BinaryExpression c = null;
                foreach (Comparator<T> r in right)
                {
                    if (c == null)
                    {
                        c = GetBinaryExpression(r.Operator, left, r.Version);
                    }
                    else
                    {
                        c = Expression.AndAlso(c, GetBinaryExpression(r.Operator, left, r.Version));
                    }
                }
                return c;
            }
        }

        public static bool Intersect(ExpressionType left_operator, T left, ExpressionType right_operator, T right)
        {
            if (left_operator != ExpressionType.LessThan && left_operator != ExpressionType.LessThanOrEqual &&
                    left_operator != ExpressionType.GreaterThan && left_operator != ExpressionType.GreaterThanOrEqual
                    && left_operator != ExpressionType.Equal)
                throw new ArgumentException("Unsupported left operator expression type " + left_operator.ToString() + ".");
            if (right_operator != ExpressionType.LessThan && right_operator != ExpressionType.LessThanOrEqual &&
                   right_operator != ExpressionType.GreaterThan && right_operator != ExpressionType.GreaterThanOrEqual
                   && right_operator != ExpressionType.Equal)
                throw new ArgumentException("Unsupported left operator expression type " + left_operator.ToString() + ".");

            if (left_operator == ExpressionType.Equal)
            {
                return InvokeBinaryExpression(GetBinaryExpression(right_operator, left, right));
            }
            else if (right_operator == ExpressionType.Equal)
            {
                return InvokeBinaryExpression(GetBinaryExpression(left_operator, right, left));
            }

            if ((left_operator == ExpressionType.LessThan || left_operator == ExpressionType.LessThanOrEqual)
                && (right_operator == ExpressionType.LessThan || right_operator == ExpressionType.LessThanOrEqual))
            {
                return true;
            }
            else if ((left_operator == ExpressionType.GreaterThan || left_operator == ExpressionType.GreaterThanOrEqual)
                && (right_operator == ExpressionType.GreaterThan || right_operator == ExpressionType.GreaterThanOrEqual))
            {
                return true;
            }

            else if ((left_operator == ExpressionType.LessThanOrEqual) && (right_operator == ExpressionType.GreaterThanOrEqual))
            {
                return right.CompareTo(left) <= 0; // right <= left;
            }

            else if ((left_operator == ExpressionType.GreaterThanOrEqual) && (right_operator == ExpressionType.LessThanOrEqual))
            {
                return right.CompareTo(left) >= 0;// right >= left;
            }


            else if ((left_operator == ExpressionType.LessThan || left_operator == ExpressionType.LessThanOrEqual)
                && (right_operator == ExpressionType.GreaterThan || right_operator == ExpressionType.GreaterThanOrEqual))
            {
                return right.CompareTo(left) < 0; // right < left;
            }

            else
            {
                return right.CompareTo(left) > 0; // right > left;
            }
        }

        public static bool Intersect(Comparator<T> left, Comparator<T> right)
        {
            return Intersect(left.Operator, left.Version, right.Operator, right.Version);
        }

        public static bool Intersect(ComparatorSet<T> cs1, ComparatorSet<T> cs2)
        {
            if (!ComparatorSetIsValidRange(cs1)) throw new ArgumentException("Invalid comparator set for range.", "cs1");
            if (!ComparatorSetIsValidRange(cs2)) throw new ArgumentException("Invalid comparator set for range.", "cs2");
            if (cs1.Count == 1 && cs2.Count == 1)
            {
                return Intersect(cs1[0], cs2[0]);
            }
            else if (cs1.Count == 1 && cs2.Count == 2)
            {
                return Satisfies(cs1[0].Version, cs2);
            }
            else if (cs1.Count == 2 && cs2.Count == 1)
            {
                return Satisfies(cs2[0].Version, cs1);
            }

            else if (cs2.Count == 2 && cs2.Count == 2)
            {
                return cs1.ToInterval().Intersect(cs2.ToInterval());
            }
            else
            {
                throw new ArgumentOutOfRangeException("Can't convert comparator sets to range.");
            }

        }

        public static bool Intersect(ComparatorSet<T> left, List<ComparatorSet<T>> right)
        {
            bool result = false;
            right.ForEach(r => result |= Intersect(left, r));
            return result;
        }

        public static bool Intersect(List<ComparatorSet<T>> left, List<ComparatorSet<T>> right)
        {
            bool result = false;
            left.ForEach(l => right.ForEach(r => result |= Intersect(l, r)));
            return result;
        }

        public static bool Satisfies(T v, ComparatorSet<T> s)
        {
            return InvokeBinaryExpression(GetBinaryExpression(v, s));
        }

        public static bool ComparatorSetIsValidRange(ComparatorSet<T> cs)
        {

            if ((cs.Count == 1) && cs[0].Operator == ExpressionType.Equal) return true;
            else if (cs.Count == 2)
            {
                foreach (Comparator<T> c in cs)
                {
                    if (!ValidRangeOperators.Contains(c.Operator)) return false;
                }
                return true;
            }
            else return false;
        }

        
    }
}
