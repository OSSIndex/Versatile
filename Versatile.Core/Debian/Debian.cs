using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sprache;

namespace Versatile
{
    public partial class Debian : Version, IVersionFactory<Debian>, IEquatable<Debian>, IComparable, IComparable<Debian>
    {
        #region Overriden methods
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ((List<string>)this).GetHashCode();
        }

        public override string ToString()
        {
            string e = this.Epoch.HasValue ? this.Epoch.ToString() + ":" : "";
            string uv = this.UpstreamVersion;
            string dr = !string.IsNullOrEmpty(this.DebianRevision) && this.DebianRevision != "0" ? "-" + this.DebianRevision : string.Empty;
            return e + uv + dr;
        }


        public override int CompareComponent(Version other)
        {

            List<string> a = this.ToList();
            List<string> b = other.ToList();
            for (int i = 0; i < 3; i++)
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
            return 0;
        }
        #endregion

        #region Public methods
        public bool Equals(Debian other)
        {
            return base.Equals((Version)other);
        }


        public int CompareTo(Debian other)
        {
            if (ReferenceEquals(other, null))
            {
                return 1;
            }
            else return CompareComponent(this, other);
        }

        public Debian Construct(List<string> s)
        {
            return new Debian(s);
        }

        public Debian Max()
        {
            return new Debian("1", "999999", "999999999");
        }

        public Debian Min()
        {
            return new Debian("0", "0", "0");
        }
        #endregion

        #region Public properties
        public int? Epoch { get; set; }
        public string UpstreamVersion { get; set; }
        public string DebianRevision { get; set; }
        #endregion

        #region Constructors
        public Debian() : base() { }
        public Debian(string epoch, string upstream_version, string debian_revision)
        {
            this.Epoch = Int32.Parse(epoch);
            this.UpstreamVersion = upstream_version;
            this.DebianRevision = !string.IsNullOrEmpty(debian_revision) ? debian_revision : "0";
            this.Add(epoch);
            this.Add(this.UpstreamVersion);
            this.Add(this.DebianRevision);
        }
        public Debian(List<string> d)
        {
            if (d.Count < 3) throw new ArgumentOutOfRangeException("d", "The length of the list must be at least 3.");
            if (!string.IsNullOrEmpty(d[0]))
            {
                this.Epoch = Int32.Parse(d[0]);
                this.Add(this.Epoch.ToString());
            }
            else
            {
                this.Add(string.Empty);
            }
            if (!string.IsNullOrEmpty(d[1]))
            {
                this.UpstreamVersion = char.IsDigit(d[1][0]) ? d[1] : "0." + d[1];
            }
            else
            {
                this.UpstreamVersion = d[1];
            }
            if (!string.IsNullOrEmpty(d[2]))
            {
                this.DebianRevision = char.IsDigit(d[2][0]) ? d[2] : "0." + d[2];
            }
            else
            {
                this.DebianRevision = "0";
            }
            this.Add(this.UpstreamVersion);
            this.Add(this.DebianRevision);
        }
        #endregion

        #region Operators
        public static bool operator ==(Debian left, Debian right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Debian left, Debian right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(Debian left, Debian right)
        {
            return CompareComponent(left, right) == -1;
        }


        public static bool operator >(Debian left, Debian right)
        {
            return CompareComponent(left, right) == 1;
        }


        public static bool operator <=(Debian left, Debian right)
        {
            return (new List<int> { 0, -1 }).Contains(CompareComponent(left, right));
        }

        public static bool operator >=(Debian left, Debian right)
        {
            return (new List<int> { 0, 1 }).Contains(CompareComponent(left, right));
        }

        public static Debian operator ++(Debian s)
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

        public static Debian operator --(Debian s)
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
    }
}
