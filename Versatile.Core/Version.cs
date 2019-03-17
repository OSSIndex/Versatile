using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sprache;
namespace Versatile
{
    public abstract class Version : List<string>, IComparable, IComparable<Version>, IEquatable<Version>
    {
        #region Abstract methods
        public abstract int CompareComponent(Version other);
        #endregion

        #region Virtual methods
        public virtual string ToNormalizedString()
        {
            return this.Aggregate((p, n) => p + "." + n);
        }
        #endregion

        #region Overriden methods
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            Version other = obj as Version;
            if (other == null)
            {
                throw new ArgumentException("Must be a Version.", "obj");
            }
            return CompareComponent(this, other) == 0;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #region Public methods
        public virtual int CompareTo(object obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 1;
            }
            Version other = obj as Version;
            if (other == null)
            {
                throw new ArgumentException("Must be a Version", "obj");
            }
            else return CompareComponent(this, other);
        }
        
        public int CompareTo(Version other)
        {
            if (ReferenceEquals(other, null))
            {
                return 1;
            }
            else return CompareComponent(this, other);
        }

        public bool Equals(Version other)
        {
            return !ReferenceEquals(null, other) &&
                CompareComponent(this, other) == 0;
        }
        #endregion

        #region Public properties
        public int? Major { get; set; } = null;
        public int? Minor { get; set; } = null;
        public int? Patch { get; set; } = null;
 
        public System.Version SystemVersion { get; set; }
        public PreReleaseVersion PreRelease { get; set; }
        #endregion

        #region Constructors
        public Version() { }
        public Version(int? major, int? minor = null, int? patch = null, string prerelease = "")
        {
            if (!major.HasValue && minor.HasValue) throw new ArgumentNullException("Major component cannot be null if minor is non-null.");
            if (!minor.HasValue && patch.HasValue) throw new ArgumentNullException("Minor component cannot be null if patch is non-null.");
            if (!string.IsNullOrEmpty(prerelease) && !patch.HasValue) throw new ArgumentNullException("Patch component cannot be null if pre-release is non-null.");
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
            this.SystemVersion = new System.Version(this.Major.HasValue ? this.Major.Value : 0, this.Minor.HasValue ? this.Minor.Value : 0
            ,this.Patch.HasValue ? this.Patch.Value : 0);
            this.Add(major.HasValue ? major.Value.ToString() : "0");
            this.Add(minor.HasValue ? minor.Value.ToString() : "0");
            this.Add(patch.HasValue ? patch.Value.ToString() : "0");
        }

        public Version(List<string> v)
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
            this.SystemVersion = new System.Version(this.Major.HasValue ? this.Major.Value : 0, this.Minor.HasValue ? this.Minor.Value : 0
            , this.Patch.HasValue ? this.Patch.Value : 0);
            this.Add(this.Major.HasValue ? this.Major.Value.ToString() : "0");
            this.Add(this.Minor.HasValue ? this.Minor.Value.ToString() : "0");
            this.Add(this.Patch.HasValue ? this.Patch.Value.ToString() : "0");
        }
        #endregion

        #region Operators
        public static bool operator == (Version left, Version right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Version left, Version right)
        {
            return !left.Equals(right);
        }

        public static bool operator < (Version left, Version right)
        {
            return CompareComponent(left, right) == -1;
        }


        public static bool operator >(Version left, Version right)
        {
            return CompareComponent(left, right) == 1;
        }


        public static bool operator <=(Version left, Version right)
        {
            return (new List<int> { 0, -1 }).Contains(CompareComponent(left, right));
        }

        public static bool operator >=(Version left, Version right)
        {
            return (new List<int> { 0, 1 }).Contains(CompareComponent(left, right));
        }

        public static Version operator ++(Version s)
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

        public static Version operator --(Version s)
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
        #endregion

        #region Public static methods 
        public static int CompareComponent(Version a, Version b, bool lower = false)
        {
            var aEmpty = ReferenceEquals(a, null) || a.Count == 0;
            var bEmpty = ReferenceEquals(b, null) || b.Count == 0;
            if (aEmpty && bEmpty)
                return 0;
            if (aEmpty)
                return lower ? 1 : -1;
            if (bEmpty)
                return lower ? -1 : 1;
            return a.CompareComponent(b);
            /*
            int min = Math.Min(a.Count, b.Count);
            for (int i = 0; i < min; i++)
            {
                var ac = a[i];
                var bc = b[i];
                int anum, bnum;
                bool isanum = Int32.TryParse(ac, out anum);
                bool isbnum = Int32.TryParse(bc, out bnum);
                int r;
                if (isanum && isbnum)
                {
                    r = anum.CompareTo(bnum);
                    if (r != 0) return r;
                }
                else
                {
                    if (isanum)
                        return -1;
                    if (isbnum)
                        return 1;
                    r = String.CompareOrdinal(ac, bc);
                    if (r != 0) return r;
                }
            }
            return a.Count.CompareTo(b.Count);
            */
        }
        public static int CompareComponent(List<string> a, List<string> b, bool lower = false)
        {
            var aEmpty = ReferenceEquals(a, null) || a.Count == 0;
            var bEmpty = ReferenceEquals(b, null) || b.Count == 0;
            if (aEmpty && bEmpty)
                return 0;
            if (aEmpty)
                return lower ? 1 : -1;
            if (bEmpty)
                return lower ? -1 : 1;
            int min = Math.Min(a.Count, b.Count);
            for (int i = 0; i < min; i++)
            {
                var ac = a[i];
                var bc = b[i];
                int anum, bnum;
                bool isanum = Int32.TryParse(ac, out anum);
                bool isbnum = Int32.TryParse(bc, out bnum);
                int r;
                if (isanum && isbnum)
                {
                    r = anum.CompareTo(bnum);
                    if (r != 0) return r;
                }
                else
                {
                    if (isanum)
                        return -1;
                    if (isbnum)
                        return 1;
                    r = String.CompareOrdinal(ac, bc);
                    if (r != 0) return r;
                }
            }
            return a.Count.CompareTo(b.Count);
        }
        #endregion
    }
}
