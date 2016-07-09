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
            return CompareComponent(left.ToNormalizedString(), right.ToNormalizedString()) == -1;
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


        #region Constructors
        public Composer() {}
        public Composer(int? major, int? minor = null, int? patch = null, string prerelease = "") : base (major, minor, patch)
        {
            if (!string.IsNullOrEmpty(prerelease))
            {
                this.PreRelease = (ComposerPreRelease) Grammar.PreRelease.Parse("-" + prerelease);
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
                }
                else this.PreRelease = (ComposerPreRelease) Grammar.PreRelease.Parse("-" + v[3]);
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
            else return CompareComponent(this.ToNormalizedString(), other.ToNormalizedString());
        }

        public Composer Construct(List<string> s)
        {
            return new Composer(s);
        }

        public Composer Max()
        {
            return new Composer(1000000, 1000000, 0);
        }

        public Composer Min()
        {
            return new Composer(0, 0, 0);
        }

        #endregion


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
     

        public static Composer MAX
        {
            get
            {
                return new Composer(1000000, 1000000, 1000000);
            }
        }

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

        public enum VersionStringType
        {
            Invalid,
            Version,
            Range,
        }
    }
}
