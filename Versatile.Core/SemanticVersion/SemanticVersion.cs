using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Sprache;

namespace Versatile
{
    public partial class SemanticVersion : IComparable, IComparable<SemanticVersion>, IEquatable<SemanticVersion>
    {
        public int? Major { get; set; } = null;
        public int? Minor { get; set; } = null;
        public int? Patch { get; set; } = null;
        public Version Version { get; set; } 
        public PreReleaseVersion PreRelease { get; set; } = null;
        public IEnumerable<string> Build { get; set; } = null;

        #region Constructors
        public SemanticVersion(int? major, int? minor = null, int? patch = null, string prerelease = "", string build = "")
        {
            if (!major.HasValue && minor.HasValue) throw new ArgumentNullException("Major component cannot be null if minor is non-null.");
            if (!minor.HasValue && patch.HasValue) throw new ArgumentNullException("Minor component cannot be null if patch is non-null.");
            if (!string.IsNullOrEmpty(prerelease) && !patch.HasValue) throw new ArgumentNullException("Patch component cannot be null if pre-release is non-null.");
            if (!string.IsNullOrEmpty(build) && !patch.HasValue) throw new ArgumentNullException("Patch component cannot be null if pre-release is non-null.");
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
            this.Version = new Version(this.Major.HasValue ? this.Major.Value : 0, this.Minor.HasValue ? this.Minor.Value : 0
                , this.Patch.HasValue ? this.Patch.Value : 0);
            if (!string.IsNullOrEmpty(prerelease))
            {                
                if (Grammar.PreRelease.Parse(prerelease).Count() > 0)
                {
                    this.PreRelease = new PreReleaseVersion();
                    IEnumerable<string> p = Grammar.PreRelease.Parse(prerelease);
                    for (int i = 0; i < p.Count(); i++)
                    {
                        this.PreRelease.Add(i, p.ElementAt(i));
                    }
                    int last;
                    if (!Int32.TryParse(this.PreRelease[this.PreRelease.Count - 1], out last) || last < 0)
                    {
                        this.PreRelease.Add(this.PreRelease.Count, "0");
                    }                  
                }
                else throw new ArgumentException("The prerelease identifier is not valid: " + prerelease + ".");
            }

            if (!string.IsNullOrEmpty(build))
            {
                if (Grammar.Build.Parse(build).Count() > 0)
                {
                    this.Build = Grammar.Build.Parse(build);
                }
                else throw new ArgumentException("The build identifier is not valid: " + build + ".");
            }

        }

        public SemanticVersion(List<string> v)
        {
            if (v.Count() != 5) throw new ArgumentException("List length is not 5.");
            this.Major = null;
            this.Minor = null;
            this.Patch = null;
            int major, minor, patch;
            if (Int32.TryParse(v[0], out major))
            {
                this.Major = major;
            }
            else
            {
                throw new ArgumentNullException("Could not parse major component or major component cannot be null.");
            }
            if (Int32.TryParse(v[1], out minor)) this.Minor = minor;
            if (Int32.TryParse(v[2], out patch)) this.Patch = patch;
            this.Version = new Version(this.Major.HasValue ? this.Major.Value : 0, this.Minor.HasValue ? this.Minor.Value : 0
                , this.Patch.HasValue ? this.Patch.Value : 0);
            if (!string.IsNullOrEmpty(v[3]))
            {                
                if (Grammar.PreRelease.Parse(v[3]).Count() > 0)
                {
                    this.PreRelease = new PreReleaseVersion();
                    IEnumerable<string> p = Grammar.PreRelease.Parse(v[3]);
                    for (int i = 0; i < p.Count(); i++)
                    {
                        this.PreRelease.Add(i, p.ElementAt(i));
                    }
                    int last;
                    if (!Int32.TryParse(this.PreRelease[this.PreRelease.Count - 1], out last) || last < 0)
                    {
                        this.PreRelease.Add(this.PreRelease.Count, "0");
                    }
                }
                else throw new ArgumentException("The prerelease identifier is not valid: " + v[3] + ".");
            }

            if (!string.IsNullOrEmpty(v[4]))
            {
                if (Grammar.Build.Parse(v[4]).Count() > 0)
                {
                    this.Build = Grammar.Build.Parse(v[4]);
                }
                else throw new ArgumentException("The build identifier is not valid: " + v[4] + ".");
            }

        }
        
        public SemanticVersion(string v) : this(Grammar.SemanticVersionIdentifier.Parse(v))
        {
            this.Version = new Version(this.Major.HasValue ? this.Major.Value : 0, this.Minor.HasValue ? this.Minor.Value : 0, 
                this.Patch.HasValue ? this.Patch.Value : 0);
        }

        #endregion

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

            return aComps.Length.CompareTo(bComps.Length);
        }

        public int CompareTo(object obj)
        {
            if (Object.ReferenceEquals(obj, null))
            {
                return 1;
            }
            SemanticVersion other = obj as SemanticVersion;
            if (other == null)
            {
                throw new ArgumentException("Must be a Semantic Version.", "obj");
            }
            return CompareTo(other);
        }

        public int CompareTo(SemanticVersion other)
        {
            if (Object.ReferenceEquals(other, null))
            {
                return 1;
            }


            int result = this.Version.CompareTo(other.Version);

            if (result != 0)
            {
                return result;
            }

            bool empty = ReferenceEquals(PreRelease, null);
            bool otherEmpty = ReferenceEquals(other.PreRelease, null); ;
            if (empty && otherEmpty)
            {
                return 0;
            }
            else if (empty)
            {
                return 1;
            }
            else if (otherEmpty)
            {
                return -1;
            }
            else return PreReleaseVersion.ComparePreRelease(PreRelease, other.PreRelease);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return this.CompareTo((SemanticVersion) obj) == 0;
        }

        public bool Equals(SemanticVersion other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) || this.CompareTo(other) == 0);
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
                result = result + "." + this.PreRelease.ToString();
            }
            if (this.Build != null)
            {
                result = result + "." + this.Build.ToString();
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
                if (this.Build == null)
                {
                    result = result * 31 + this.Build.GetHashCode();
                }

                return result;
            }
        }
        public static bool operator == (SemanticVersion left, SemanticVersion right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SemanticVersion left, SemanticVersion right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(SemanticVersion left, SemanticVersion right)
        {
            return left.CompareTo(right) == -1;
        }
       

        public static bool operator >(SemanticVersion left, SemanticVersion right)
        {
            return left.CompareTo(right) > 0;
        }

             
        public static bool operator <=(SemanticVersion left, SemanticVersion right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(SemanticVersion left, SemanticVersion right)
        {
            return left.CompareTo(right) >= 0; ;
        }

        public static SemanticVersion operator ++(SemanticVersion s)
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

        public static SemanticVersion operator -- (SemanticVersion s)
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

        public static BinaryExpression GetBinaryExpression(ExpressionType et, SemanticVersion left, SemanticVersion right)
        {
            return Expression.MakeBinary(et, Expression.Constant(left, typeof(SemanticVersion)), Expression.Constant(right, typeof(SemanticVersion)));
        }

        public static BinaryExpression GetBinaryExpression(SemanticVersion left, ComparatorSet<SemanticVersion> right)
        {
            if (right.Count() == 0)
            {
                return GetBinaryExpression(ExpressionType.Equal, left, left);
            }
            else
            {
                BinaryExpression c = null;
                foreach (Comparator<SemanticVersion> r in right)
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

        public static bool RangeIntersect(ExpressionType left_operator, SemanticVersion left, ExpressionType right_operator, SemanticVersion right)
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
            Comparator<SemanticVersion> l = Grammar.Comparator.Parse(left);
            Comparator<SemanticVersion> r = Grammar.Comparator.Parse(right);
            return RangeIntersect(l.Operator, l.Version, r.Operator, r.Version);
        }

        public static bool Satisfies(SemanticVersion v, ComparatorSet<SemanticVersion> s)
        {
            return InvokeBinaryExpression(GetBinaryExpression(v, s));
        }

    }
}
