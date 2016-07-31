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
            if (string.IsNullOrEmpty(a[0])) a[0] = "0";
            if (string.IsNullOrEmpty(b[0])) b[0] = "0";
            for (int i = 0; i < 3; i++)
            {
                string ac = a[i];
                string bc = b[i];
                if (ac.Length > bc.Length)
                {
                    bc = bc.PadRight(ac.Length, char.MinValue);
                }
                
                else if (bc.Length > ac.Length)
                {
                    ac = ac.PadRight(bc.Length, char.MinValue);
                }


                int anum, bnum;
                bool isanum = Int32.TryParse(ac, out anum);
                bool isbnum = Int32.TryParse(bc, out bnum);
                int r = 0;
                if (isanum && isbnum)
                {
                    r = anum.CompareTo(bnum);
                    if (r != 0) return r;
                }
                else
                {
                    for (int j = 0; j < ac.Length;j++)
                    {
                        int s = 0;
                        char acc = ac[j];
                        char bcc = bc[j];
                        if (acc == '~' && bcc != '~')
                        {
                            s = -1;
                        }
                        else if (bcc == '~' && acc != '~')
                        {
                            s = 1;
                        }
                        else if (!char.IsDigit(acc) && !char.IsDigit(bcc))
                        {
                            s = acc.CompareTo(bcc);
                        }
                        else if (char.IsDigit(acc) && !char.IsDigit(bcc))
                        {
                            s = -1;
                        }
                        else if (char.IsDigit(bcc) && !char.IsDigit(acc))
                        {
                            s = 1;
                        }
                        else
                        {
                            string ack = Parse.Number.Parse(ac.Substring(j));
                            string bck = Parse.Number.Parse(bc.Substring(j));
                            s = Int32.Parse(ack).CompareTo(Int32.Parse(bck));
                            j += Math.Max(ack.Length, bck.Length) - 1;
                            
                        }
                        if (s != 0)
                        {
                            if (s >= 1) return 1;
                            else if (s <= -1) return -1;
                        }
                    }                
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
        public Parser<Debian> Parser
        {
            get
            {
                return Grammar.DebianVersion;
            }
        }
        #endregion

        #region Constructors
        public Debian() : base() { }
        public Debian(string epoch, string upstream_version, string debian_revision)
        {
            if (!string.IsNullOrEmpty(epoch))
            {
                this.Epoch = Int32.Parse(epoch);
                this.Add(this.Epoch.ToString());
            }
            else
            {
                this.Add(string.Empty);
            }
            this.UpstreamVersion = upstream_version;
            this.DebianRevision = !string.IsNullOrEmpty(debian_revision) ? debian_revision : "0";
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
            return CompareComponent(left, right) <= -1;
        }


        public static bool operator >(Debian left, Debian right)
        {
            return CompareComponent(left, right) >= 1;
        }


        public static bool operator <=(Debian left, Debian right)
        {
            int c = CompareComponent(left, right);
            return (c == 0) || (c <= -1);
        }

        public static bool operator >=(Debian left, Debian right)
        {
            int c = CompareComponent(left, right);
            return (c == 0) || (c >= 1); ;
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

        #region Public Static methods        
        #endregion
    }
}
