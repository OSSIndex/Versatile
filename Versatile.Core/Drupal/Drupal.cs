using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Sprache;
namespace Versatile
{
    public partial class Drupal : Version, IVersionFactory<Drupal>, IEquatable<Drupal>, IComparable, IComparable<Drupal>
    {
        #region Public properties
        public int CoreCompatibility { get; set; }
        #endregion

        #region Constructors
        public Drupal() : base() { }
        public Drupal(List<string> d) : base(d.Skip(1).ToList())
        {
            this.CoreCompatibility = Int32.Parse(d[0]);
            this.Insert(0, d[0]);
            if (d.Count == 5 && !string.IsNullOrEmpty(d[4]))
            {
                IResult<List<string>> pr = Grammar.PreReleaseIdentifier.TryParse("-" + d[4]);
                if (pr.WasSuccessful)
                {
                    this.PreRelease = new PreReleaseVersion(d[4]);
                    this.Add(this.PreRelease.ToNormalizedString());
                }
                else
                {
                    throw new ArgumentOutOfRangeException("The pre-release identifier is invalid.");
                }
            }
        }

        public Drupal(int core, int? major, int? minor, int? patch, string prerelease = "") : base(major, minor, patch, prerelease)
        {
            this.CoreCompatibility = core;
            this.Insert(0, core.ToString());
            if (!string.IsNullOrEmpty(prerelease))
            {
                IResult<List<string>> pr = Grammar.PreReleaseIdentifier.TryParse("-" + prerelease);
                if (pr.WasSuccessful)
                {
                    this.PreRelease = new PreReleaseVersion(prerelease);
                    this.Add(this.PreRelease.ToNormalizedString());
                }
                else
                {
                    throw new ArgumentOutOfRangeException("The pre-release identifier is invalid.");
                }
            }
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
                int result = this.Major.GetHashCode() + this.CoreCompatibility.GetHashCode();
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

        public override int CompareComponent(Version other)
        {

            List<string> a = this.Take(4).ToList();
            List<string> b = other.Take(4).ToList();
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
            if (this.Count == 4 && other.Count == 4)
            {
                return 0;
            }
            if (this.Count <= 4 && other.Count == 5) //b has prerelease so a > b
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
            else if (other.Count <= 4 && this.Count == 5) //a has prerelease so a > b
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
            else return Version.CompareComponent(this[4].Split('.').ToList(), other[4].Split('.').ToList());
        }

        public override string ToString()
        {
            string s = this.CoreCompatibility.ToString() + ".x-";
            s += this.Major.HasValue ? this.Major.Value.ToString() : "0";
            s += "." + (this.Patch.HasValue ? this.Patch.Value.ToString() : "0");
            s += ReferenceEquals(this.PreRelease, null) || this.PreRelease.Count == 0 ? string.Empty : "-" + this.PreRelease.ToString();
            return s;
        }
        #endregion

        #region Public methods
        public bool Equals(Drupal other)
        {
            return base.Equals((Version)other);
        }


        public int CompareTo(Drupal other) 
        {
            if (ReferenceEquals(other, null))
            {
                return 1;
            }
            else return CompareComponent(this, other);
        }

        public Drupal Construct(List<string> s)
        {
            return new Drupal(s);
        }

        public Drupal Max()
        {
            return new Drupal(1000000, 1000000, 0, 0);
        }

        public Drupal Min()
        {
            return new Drupal(0, 0, 0, 0);
        }

        public Parser<Drupal> Parser
        {
            get
            {
                return Grammar.DrupalVersion;
            }
        }

        #endregion

        #region Operators
        public static bool operator == (Drupal left, Drupal right)
        {
            return left.Equals(right);
        }

        public static bool operator != (Drupal left, Drupal right)
        {
            return !left.Equals(right);
        }

        public static bool operator < (Drupal left, Drupal right)
        {
            return CompareComponent(left, right) == -1;
        }


        public static bool operator >(Drupal left, Drupal right)
        {
            return CompareComponent(left, right) == 1;
        }


        public static bool operator <= (Drupal left, Drupal right)
        {
            return (new List<int> { 0, -1 }).Contains(CompareComponent(left, right));
        }

        public static bool operator >= (Drupal left, Drupal right)
        {
            return (new List<int> { 0, 1 }).Contains(CompareComponent(left, right));
        }

        public static Drupal operator ++ (Drupal s)
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

        public static Drupal operator -- (Drupal s)
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

        #region Public Static methods        
        public static bool RangeIntersect(string left, string right, out string exception_message)
        {
            IResult<List<ComparatorSet<Drupal>>> l = Grammar.Range.TryParse(left);
            IResult<List<ComparatorSet<Drupal>>> r = Grammar.Range.TryParse(right);
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
                return Range<Drupal>.Intersect(l.Value, r.Value);
            }
        }
        #endregion
    }
}
