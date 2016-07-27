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
            unchecked
            {
                int result = this.Major.GetHashCode();
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

        public override string ToNormalizedString()
        {
            return this.Aggregate((p, n) => p + "." + n);
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
                    if (r != 0) return r;
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
            return new Debian("1", "zzzzzzzzzzzzzzzz", "zzzzzzzzzzzzzzzz");
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
            this.DebianRevision = debian_revision;
            this.Add(epoch);
            this.Add(upstream_version);
            this.Add(debian_revision);
        }
        public Debian(List<string> d)
        {
            if (d.Count < 3) throw new ArgumentOutOfRangeException("d", "The length of the list must be at least 3.");
            this.Epoch = Int32.Parse(d[0]);
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
                this.DebianRevision = d[2];
            }

            this.Add(this.Epoch.ToString());
            this.Add(this.UpstreamVersion);
            this.Add(this.DebianRevision);
        }
        #endregion
    }
}
