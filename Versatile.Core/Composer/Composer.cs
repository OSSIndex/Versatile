using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Sprache;

namespace Versatile
{
    public partial class Composer : IComparable, IComparable<Composer>, IEquatable<Composer>
    {
        public int? Major { get; set; } = null;
        public int? Minor { get; set; } = null;
        public int? Patch { get; set; } = null;
        public ComposerPreRelease PreRelease { get; set; } = null;

        #region Constructors
        public Composer(int? major, int? minor = null, int? patch = null, string prerelease = "")
        {
            if (!major.HasValue && minor.HasValue) throw new ArgumentNullException("Major component cannot be null if minor is non-null.");
            if (!minor.HasValue && patch.HasValue) throw new ArgumentNullException("Minor component cannot be null if patch is non-null.");
            if (!string.IsNullOrEmpty(prerelease) && !patch.HasValue) throw new ArgumentNullException("Patch component cannot be null if pre-release is non-null.");
    
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
            if (!string.IsNullOrEmpty(prerelease))
            {
                this.PreRelease = (ComposerPreRelease) Grammar.PreRelease.Parse("-" + prerelease);
            }
        }

        public Composer(List<string> v)
        {
            if (v.Count() < 3) throw new ArgumentException("List length must be at least 3.", "v");
            int major, minor, patch;
            if (Int32.TryParse(v[0], out major))
            {
                this.Major = major;
            }
            else
            {
                throw new ArgumentNullException("Could not parse major component or major component cannot be null.");
            }
            if (Int32.TryParse(v[1], out minor))
            {
                this.Minor = minor;
            }
            else
            {
                throw new ArgumentNullException("Could not parse minor component or minor component cannot be null.");
            }
            if (Int32.TryParse(v[2], out patch))
            {
                this.Patch = patch;
            }
            else
            {
                throw new ArgumentNullException("Could not parse major component or major component cannot be null.");
            }
            if (!string.IsNullOrEmpty(v[3]))
            {
                this.PreRelease = (ComposerPreRelease) Grammar.PreRelease.Parse("-" + v[3]);
            }

        }

        public Composer(string v) : this(Grammar.ComposerVersionIdentifier.Parse(v)) { }

        #endregion

        public int CompareTo(object obj)
        {
            if (Object.ReferenceEquals(obj, null))
            {
                return 1;
            }
            Composer other = obj as Composer;
            if (other == null)
            {
                throw new ArgumentException("Must be a Composer Version", "obj");
            }
            return CompareComponent(this.ToNormalizedString(), other.ToNormalizedString());
        }

        public int CompareTo(Composer other)
        {
            if (Object.ReferenceEquals(other, null))
            {
                return 1;
            }
            return CompareComponent(this.ToNormalizedString(), other.ToNormalizedString());
        }

        private string ToNormalizedString()
        {
            StringBuilder s = new StringBuilder(5);
            s.Append(this.Major.HasValue ? this.Major.Value : 0);
            s.Append(".");
            s.Append(this.Minor.HasValue ? this.Minor.Value : 0);
            s.Append(".");
            s.Append(this.Patch.HasValue ? this.Patch.Value : 0);
            if (this.PreRelease != null && this.PreRelease.Count > 0)
            {
                s.Append(".");
                s.Append(this.PreRelease[0]);
                if (this.PreRelease.Count == 2)
                {
                    s.Append(".");
                    s.Append(this.PreRelease[1]);
                }
            }
            return s.ToString();
        }
        public static int CompareComponent(string a, string b, bool lower = false)
        {
            var aEmpty = String.IsNullOrEmpty(a);
            var bEmpty = String.IsNullOrEmpty(b);
            if (aEmpty && bEmpty)
                return 0;

            if (aEmpty)
                return lower ? 1 : -1;
            if (bEmpty)
                return lower ? -1 : 1;

            var aComps = a.Split('.');
            var bComps = b.Split('.');

            var minLen = Math.Min(aComps.Length, bComps.Length);
            for (int i = 0; i < minLen; i++)
            {
                var ac = aComps[i];
                var bc = bComps[i];
                int anum, bnum;
                var isanum = Int32.TryParse(ac, out anum);
                var isbnum = Int32.TryParse(bc, out bnum);
                int r;
                if (isanum && isbnum)
                {
                    r = anum.CompareTo(bnum);
                    if (r != 0) return anum.CompareTo(bnum);
                }
                else
                {
                    if (isanum)
                        return -1;
                    if (isbnum)
                        return 1;
                    r = String.CompareOrdinal(ac, bc);
                    if (r != 0)
                        return r;
                }
            }
            if (aComps.Length == 3 && bComps.Length > 3) // if a and b have equal components but b has a pre-release
            {
                if (bComps[3] != "patch")
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            else if (bComps.Length == 3 && aComps.Length > 3)
            {
                if (aComps[3] != "patch")
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return aComps.Length.CompareTo(bComps.Length);
            }
        }


        public static bool operator == (Composer left, Composer right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Composer left, Composer right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(Composer left, Composer right)
        {
            return CompareComponent (left.ToNormalizedString(), right.ToNormalizedString()) == -1;
        }
       

        public static bool operator >(Composer left, Composer right)
        {
            return CompareComponent(left.ToNormalizedString(), right.ToNormalizedString()) == 1;
        }

             
        public static bool operator <=(Composer left, Composer right)
        {
            return (new List<int> { 0, -1 }).Contains(CompareComponent(left.ToNormalizedString(), right.ToNormalizedString()));
        }

        public static bool operator >=(Composer left, Composer right)
        {
            return (new List<int> { 0, 1 }).Contains(CompareComponent(left.ToNormalizedString(), right.ToNormalizedString()));
        }

        public static Composer operator ++(Composer s)
        {
            if (s.PreRelease != null && s.PreRelease.Count > 0)
            {
                ++s.PreRelease;
                return s;
            }
            else if (s.Patch.HasValue)
            {
                ++s.Patch;
                return s;
            }
            else if (s.Minor.HasValue)
            {
                ++s.Minor;
                return s;
            }
            else
            {
                ++s.Major;
                return s;
            }
        }

        public static Composer operator -- (Composer s)
        {
            if (s.PreRelease != null && s.PreRelease.Count > 0)
            {
                --s.PreRelease;
                return s;
            }
            else if (s.Patch.HasValue)
            {
                --s.Patch;
                return s;
            }
            else if (s.Minor.HasValue)
            {
                --s.Minor;
                return s;
            }
            else
            {
                --s.Major;
                return s;
            }
        }

        public bool Equals(Composer other)
        {
            return !Object.ReferenceEquals(null, other) &&
                CompareComponent(this.ToNormalizedString(), other.ToNormalizedString()) == 0; 
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            Composer other = obj as Composer;
            if (other == null)
            {
                throw new ArgumentException("Must be a Composer Version", "obj");
            }
            return CompareComponent(this.ToNormalizedString(), other.ToNormalizedString()) == 0;
        }

        public override string ToString()
        {
            string result = this.Major.ToString();
            if (this.Minor.HasValue)
            {
                result = result + "." + this.Minor.ToString();
            }
            if (this.Patch.HasValue)
            {
                result = result + "." + this.Patch.ToString();
            }

            if (this.PreRelease != null && this.PreRelease.Count() > 0)
            {
                result = result + "-" + this.PreRelease.ToString();
            }
         
            return result;

        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = this.Major.GetHashCode();
                if (this.Minor.HasValue)
                {
                    result = result * 31 + this.Minor.GetHashCode();
                }
                if (this.Patch.HasValue)
                {
                    result = result * 31 + this.Patch.GetHashCode();
                }

                if (this.PreRelease != null)
                {
                    result = result * 31 + this.PreRelease.GetHashCode();
                }
                return result;
            }
        }

        public static BinaryExpression GetBinaryExpression(ExpressionType et, Composer left, Composer right)
        {
            return Expression.MakeBinary(et, Expression.Constant(left, typeof(Composer)), 
                Expression.Constant(right, typeof(Composer)));
        }

        public static BinaryExpression GetBinaryExpression(Composer left, ComparatorSet right)
        {
            if (right.Count() == 0)
            {
                return GetBinaryExpression(ExpressionType.Equal, left, left);
            }
            else
            {
                BinaryExpression c = null;
                foreach (Comparator r in right)
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
    
        public static bool InvokeBinaryExpression(BinaryExpression be)
        {
            return Expression.Lambda<Func<bool>>(be).Compile().Invoke();
        }

        public static bool RangeIntersect(ExpressionType left_operator, Composer left, ExpressionType right_operator, Composer right)
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
                return right <= left;
            }

            else if ((left_operator == ExpressionType.GreaterThanOrEqual) && (right_operator == ExpressionType.LessThanOrEqual))
            {
                return right >= left;
            }


            else if ((left_operator == ExpressionType.LessThan || left_operator == ExpressionType.LessThanOrEqual)
                && (right_operator == ExpressionType.GreaterThan || right_operator == ExpressionType.GreaterThanOrEqual))
            {
                return right < left;
            }

            else
            {
                return right > left;
            }
        }
       
        public static bool RangeIntersect(string left, string right)
        {
            Comparator l = Grammar.Comparator.Parse(left);
            Comparator r = Grammar.Comparator.Parse(right);
            return RangeIntersect(l.Operator, l.Version, r.Operator, r.Version);
        }

        public static bool Satisfies(Composer v, ComparatorSet s)
        {
            return InvokeBinaryExpression(GetBinaryExpression(v, s));
        }

    }
}
