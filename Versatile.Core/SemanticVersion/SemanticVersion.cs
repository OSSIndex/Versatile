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
        public System.Version Version { get; set; } 
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
            this.Version = new System.Version(this.Major.HasValue ? this.Major.Value : 0, this.Minor.HasValue ? this.Minor.Value : 0
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
            this.Version = new System.Version(this.Major.HasValue ? this.Major.Value : 0, this.Minor.HasValue ? this.Minor.Value : 0
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
            this.Version = new System.Version(this.Major.HasValue ? this.Major.Value : 0, this.Minor.HasValue ? this.Minor.Value : 0, 
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

        public static SemanticVersion MIN
        {
            get
            {
                return new SemanticVersion(0, 0, 0);
            }
        }

        public static SemanticVersion MAX
        {
            get
            {
                return new SemanticVersion(100000, 100000, 100000);
            }
        }

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
    }
}
