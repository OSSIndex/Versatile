using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Sprache;

namespace Versatile
{
    public partial class SemanticVersion : Version, IVersionFactory<SemanticVersion>, IComparable, IComparable<SemanticVersion>, IEquatable<SemanticVersion>
    {
        #region Public properties
        public IEnumerable<string> Build { get; set; } = null;
        #endregion

        #region Constructors
        public SemanticVersion() {}
        public SemanticVersion(int? major, int? minor = null, int? patch = null, string prerelease = "", string build = "") : base(major, minor, patch, prerelease)
        {
            if (!string.IsNullOrEmpty(prerelease))
            {
                if (Grammar.PreRelease.Parse(prerelease).Count() > 0)
                {
                    this.PreRelease = new PreReleaseVersion(prerelease);
                  
                    int last;
                    if (!Int32.TryParse(this.PreRelease[this.PreRelease.Count - 1], out last) || last < 0)
                    {
                        this.PreRelease.Add("0");
                    }
                    this.Add(this.PreRelease.ToNormalizedString());
                }
                else throw new ArgumentException("The prerelease identifier is not valid: " + prerelease + ".");
            }
            if (!string.IsNullOrEmpty(build))
            {
                if (Grammar.Build.Parse(build).Count() > 0)
                {
                    this.Build = Grammar.Build.Parse(build);
                    this.Add(string.Join(".", this.Build));
                }
                else throw new ArgumentException("The build identifier is not valid: " + build + ".");
            }
        }

        public SemanticVersion(List<string> v) : base(v)
        {
            if (v.Count > 3 && !string.IsNullOrEmpty(v[3]))
            {
                if (Grammar.PreRelease.Parse(v[3]).Count() > 0)
                {
                    this.PreRelease = new PreReleaseVersion(v[3]);
                    int last;
                    if (!Int32.TryParse(this.PreRelease[this.PreRelease.Count - 1], out last) || last < 0)
                    {
                        this.PreRelease.Add("0");
                    }
                    this.Add(this.PreRelease.ToNormalizedString());
                }
                else throw new ArgumentException("The prerelease identifier is not valid: " + v[3] + ".");
            }

            if (v.Count > 4 && !string.IsNullOrEmpty(v[4]))
            {
                if (Grammar.Build.Parse(v[4]).Count() > 0)
                {
                    this.Build = Grammar.Build.Parse(v[4]);
                    this.Add(string.Join(".", this.Build));
                }
                else throw new ArgumentException("The build identifier is not valid: " + v[4] + ".");
            }
        }
        
        public SemanticVersion(string v) : this(Grammar.SemanticVersionIdentifier.Parse(v)) {}
        #endregion

        #region Public methods
        public int CompareTo(SemanticVersion other)
        {
            if (Object.ReferenceEquals(other, null))
            {
                return 1;
            }

            int result = this.SystemVersion.CompareTo(other.SystemVersion);

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

        public bool Equals(SemanticVersion other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) || this.CompareTo(other) == 0);
        }


        public SemanticVersion Construct(List<string> s)
        {
            return new SemanticVersion(s);
        }

        public SemanticVersion Max()
        {
            return new SemanticVersion(1000000, 1000000, 0);
        }

        public SemanticVersion Min()
        {
            return new SemanticVersion(0, 0, 0);
        }




        #endregion

        #region Overriden methods
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return this.CompareTo((SemanticVersion) obj) == 0;
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
                result = result + "." + this.PreRelease.ToNormalizedString();
            }
            if (this.Build != null)
            {
                result = result + "." + this.Build.ToString();
            }

            return result;

        }

        public override string ToNormalizedString()
        {
            return this.ToString();
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
                    if (r != 0) return r;
                }
            }
            if (this.Count == 3 && other.Count == 3) 
            {
                return 0;
            }
            if (this.Count <= 3 && other.Count == 4) //b has prerelease so a > b
            {
                return 1;
            }
            else if (other.Count <= 3 && this.Count == 4) //a has prerelease so a > b
            {
                return -1;
            }
            else return Version.CompareComponent(this[3].Split('.').ToList(), other[3].Split('.').ToList()); 
        }
        #endregion

        #region Operators
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
            return CompareComponent(left, right) == -1;
        }


        public static bool operator >(SemanticVersion left, SemanticVersion right)
        {
            return CompareComponent(left, right) == 1;
        }


        public static bool operator <=(SemanticVersion left, SemanticVersion right)
        {
            return (new List<int> { 0, -1 }).Contains(CompareComponent(left, right));
        }

        public static bool operator >=(SemanticVersion left, SemanticVersion right)
        {
            return (new List<int> { 0, 1 }).Contains(CompareComponent(left, right));
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
        #endregion

        #region Public static methods
        public static bool RangeIntersect(string left, string right, out string exception_message)
        {
            IResult<ComparatorSet<SemanticVersion>> l = Grammar.Range.TryParse(left);
            IResult<ComparatorSet<SemanticVersion>> r = Grammar.Range.TryParse(right);
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
                return Range<SemanticVersion>.Intersect(l.Value, r.Value);
            }
        }
        #endregion
    }
}
