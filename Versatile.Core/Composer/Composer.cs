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
        public Version Version { get; set; } = null;
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
            this.Version = new Version(this.Major.HasValue ? this.Major.Value : 0, this.Minor.HasValue ? this.Minor.Value : 0
            , this.Patch.HasValue ? this.Patch.Value : 0);
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
            if (!string.IsNullOrEmpty(v[1]) && Int32.TryParse(v[1], out minor))
            {
                this.Minor = minor;
            }
            if (!string.IsNullOrEmpty(v[2]) && Int32.TryParse(v[2], out patch))
            {
                this.Patch = patch;
            }
            if (v.Count() > 3 && !string.IsNullOrEmpty(v[3]))
            {
                this.PreRelease = (ComposerPreRelease) Grammar.PreRelease.Parse("-" + v[3]);
            }
            this.Version = new Version(this.Major.HasValue ? this.Major.Value : 0, this.Minor.HasValue ? this.Minor.Value : 0
            , this.Patch.HasValue ? this.Patch.Value : 0);
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

        public static Composer MIN
        {
            get
            {
                return new Composer(0, 0, 0);
            }
        }

        public static Composer MAX
        {
            get
            {
                return new Composer(1000000, 1000000, 1000000);
            }
        }

        public static bool RangeIntersect(string left, string right, out string exception_message)
        {
            IResult<ComparatorSet<Composer>> l = Grammar.Range.TryParse(left);
            IResult<ComparatorSet<Composer>> r = Grammar.Range.TryParse(right);
            if (!l.WasSuccessful)
            {
                exception_message = string.Format("Failed parsing version string {0}: {1}. ", left, l.Message);
                return false; 
            }
            else if (!r.WasSuccessful)
            {
                exception_message = string.Format("Failed parsing version string {0}: {1}.", right, r.Message);
                return false;
            }
            else
            {
                exception_message = string.Empty;
                return Range<Composer>.Intersect(l.Value, r.Value);
            }
        }
    }
}
