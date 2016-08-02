using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Versatile
{
    public class PreReleaseVersion : List<string>
    {
        #region Virtual methods
        public virtual string ToNormalizedString()
        {
            if (this.Count == 0)
            {
                return string.Empty;
            }
            else if (this.Count == 1)
            {
                return this[0];
            }
            else
            {
                return this.Aggregate((p, v) => p + "." + v);
            }
        }
        #endregion

        #region Constructors
        public PreReleaseVersion(string prerelease)
        {
            this.AddRange(prerelease.Split('.'));
        }

        public PreReleaseVersion(List<string> prerelease)
        {
            this.AddRange(prerelease.Where(p => !string.IsNullOrEmpty(p)));
        }
        #endregion

        #region Overriden methods
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            PreReleaseVersion other = (PreReleaseVersion)obj;
            return ComparePreRelease(this, other) == 0;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return base.GetHashCode();
            }
        }

        public override string ToString()
        {
            if (this.Count == 0)
            {
                return string.Empty;
            }
            else if (this.Count == 1)
            {
                return this[0];
            }
            else if (this.Count == 2)
            {
                return this.Aggregate((p, v) => p + "" + v);
            }
            else
            {
                return this.Aggregate((p, v) => p + "." + v);
            }


        }

        #endregion

        #region Operators
        public static bool operator ==(PreReleaseVersion left, PreReleaseVersion right)
        {
            return ComparePreRelease(left, right) == 0;
        }

        public static bool operator !=(PreReleaseVersion left, PreReleaseVersion right)
        {
            return ComparePreRelease(left, right) != 0;
        }

        public static bool operator <(PreReleaseVersion left, PreReleaseVersion right)
        {
            return ComparePreRelease(left, right) == -1;
        }

        public static bool operator >(PreReleaseVersion left, PreReleaseVersion right)
        {

            return ComparePreRelease(left, right) == 1;
        }

        public static bool operator <=(PreReleaseVersion left, PreReleaseVersion right)
        {
            return (left < right) || (left == right);
        }

        public static bool operator >=(PreReleaseVersion left, PreReleaseVersion right)
        {
            return (left > right) || (left == right);
        }

        //Read up to the first integer component then increment
        public static PreReleaseVersion operator ++(PreReleaseVersion p)
        {
            for (int i = p.Count - 1; i >= 0; i--)
            {
                string c = p[i];
                int num;
                bool isnum = Int32.TryParse(c, out num);
                if (!isnum)
                {
                    p.Add("1");
                    return p;

                }
                else
                {
                    if (i == p.Count - 1 && num == 0)
                    {
                        p.RemoveAt(i);
                        continue;

                    }
                    else
                    {
                        p[i] = (++num).ToString();

                        return p;
                    }

                }
            }
            p.RemoveAt(p.Count - 1);
            return p;

        }

        //Read up to the first integer component then increment
        public static PreReleaseVersion operator --(PreReleaseVersion p)
        {
            for (int i = p.Count - 1; i >= 0; i--)
            {
                string c = p[i];
                int num;
                bool isnum = Int32.TryParse(c, out num);
                if (!isnum)
                {
                    p.RemoveAt(i);
                    return p;

                }
                else
                {
                    if (i == p.Count - 1 && num == 0)
                    {
                        p.RemoveAt(i);
                        continue;

                    }
                    else
                    {
                        p[i] = (--num).ToString();
                        if (i == p.Count - 1 && num == 0)
                        {
                            p.RemoveAt(i);
                        }
                        return p;
                    }

                }
            }
            p.RemoveAt(p.Count - 1);
            return p;
        }
        #endregion

        #region Public methods
        public static int ComparePreRelease(PreReleaseVersion left, PreReleaseVersion right)
        {
            //if (ReferenceEquals(left, null)) throw new ArgumentNullException("Left operand can't be null.");
            //if (ReferenceEquals(right, null)) throw new ArgumentNullException("Right operand can't be null.");
            if (ReferenceEquals(left, right))
                return 0;
            else if (ReferenceEquals(left, null) && !ReferenceEquals(right, null) && right.Count == 0)
                return 0;
            else if (ReferenceEquals(left, null) && !ReferenceEquals(right, null) && right.Count > 0)
                return 1;
            else if (ReferenceEquals(right, null) && !ReferenceEquals(left, null) && left.Count == 0)
                return 0;
            else if (ReferenceEquals(right, null) && !ReferenceEquals(left, null) && left.Count > 0)
                return -1;

            for (int i = 0; i < Math.Min(left.Count, right.Count); i++)
            {
                var ac = left[i];
                var bc = right[i];
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
            return left.Count.CompareTo(right.Count);
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

            return aComps.Length.CompareTo(bComps.Length);
        }
        #endregion
    }
}
