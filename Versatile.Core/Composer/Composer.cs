using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Sprache;

namespace Versatile
{
    public partial class Composer : Version, IVersionFactory<Composer>, IComparable, IComparable<Composer>, IEquatable<Composer>
    {
        #region Public properties
        public int? Build { get; set; }
        #endregion

        #region Constructors
        public Composer() {}
        public Composer(int? major, int? minor = null, int? patch = null, string prerelease = "") : base (major, minor, patch)
        {
            if (!string.IsNullOrEmpty(prerelease))
            {
                this.PreRelease = Grammar.PreRelease.Parse("-" + prerelease);
                this.Add(this.PreRelease.ToNormalizedString());
            }
        }

        public Composer(List<string> v) : base(v)
        {
            if (v.Count() > 3 && !string.IsNullOrEmpty(v[3]))
            {
                IResult<List<string>> pro = Grammar.PreReleaseOnlyIdentifier.TryParse(v[3]);
                if (pro.WasSuccessful)
                {
                    this.PreRelease = new ComposerPreRelease(v[3]);
                    this.Add(this.PreRelease.ToNormalizedString());
                }
                else
                {
                    pro = Grammar.PreRelease.TryParse("-" + v[3]);
                    if (pro.WasSuccessful)
                    {
                        this.PreRelease = new ComposerPreRelease(pro.Value);
                        this.Add(this.PreRelease.ToNormalizedString());
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("Invalid pre-release string.");
                    }
                }
            }
        }

        public Composer(string v) : this(Grammar.ComposerVersionIdentifier.Parse(v)) { }

        #endregion

        #region Public methods
        public bool Equals(Composer other)
        {
            return base.Equals((Version)other);
        }


        public int CompareTo(Composer other)
        {
            if (ReferenceEquals(other, null))
            {
                return 1;
            }
            else return CompareComponent(this, other);
        }

        public Composer Construct(List<string> s)
        {
            return new Composer(s);
        }

        public Composer Max()
        {
            return new Composer(1000000, 1000000, 1000000);
        }

        public Composer Min()
        {
            return new Composer(0, 0, 0);
        }

        #endregion

        #region Overriden methods
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
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
                if (this.Build.HasValue)
                {
                    result = result * 31 + this.Build.GetHashCode();
                }
                if (this.PreRelease != null)
                {
                    result = result * 31 + this.PreRelease.GetHashCode();
                }
               
                return result;
            }

        }

        public override int CompareComponent(Version other)
        {
            List<string> a = this.Take(3).ToList();
            List<string> b = other.Take(3).ToList();
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
                    if (r != 0)
                    {
                        if (r >= 1)
                        {
                            return 1;
                        }
                        else if (r <= -1)
                        {
                            return -1;
                        }
                    }
                }
            }
            if (this.Count == 3 && other.Count == 3)
            {
                return 0;
            }
            if (this.Count <= 3 && other.Count == 4) //b has prerelease so a > b
            {
                if (other[3].StartsWith("patch"))
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            else if (other.Count <= 3 && this.Count == 4) //a has prerelease so a > b
            {
                if (this[3].StartsWith("patch"))
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            else return Version.CompareComponent(this[3].Split('.').ToList(), other[3].Split('.').ToList());
        }

        public override string ToNormalizedString()
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
        #endregion

        #region Operators
        public static bool operator ==(Composer left, Composer right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Composer left, Composer right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(Composer left, Composer right)
        {
            return CompareComponent(left, right) == -1;
        }


        public static bool operator >(Composer left, Composer right)
        {
            return CompareComponent(left, right) == 1;
        }


        public static bool operator <=(Composer left, Composer right)
        {
            return (new List<int> { 0, -1 }).Contains(CompareComponent(left, right));
        }

        public static bool operator >=(Composer left, Composer right)
        {
            return (new List<int> { 0, 1 }).Contains(CompareComponent(left, right));
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

        public static Composer operator --(Composer s)
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
        public static bool RangeIntersect(string left, string right, out string exception_message)
        {
            IResult<List<ComparatorSet<Composer>>> l = Grammar.Range.TryParse(left);
            IResult<List<ComparatorSet<Composer>>> r = Grammar.Range.TryParse(right);
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

        public static VersionStringType GetVersionType(string version)
        {
            IResult<Composer> v = Grammar.ComposerVersion.TryParse(version);
            if (v.WasSuccessful)
            {
                return VersionStringType.Version;
            }
            IResult<List<ComparatorSet<Composer>>> r = Grammar.Range.TryParse(version);
            if (r.WasSuccessful)
            {
                return VersionStringType.Range;
            }
            else return VersionStringType.Invalid;
        }
        #endregion

        #region Public enumerations
        public enum VersionStringType
        {
            Invalid,
            Version,
            Range,
        }
        #endregion
    }
}
