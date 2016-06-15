using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Versatile
{
    public class ComposerPreRelease : List<string>
    {
        public ComposerPreRelease(string s, string d)
        {
            this.Add(s);
            if (!string.IsNullOrEmpty(d))
            {
                this.Add(s);
            }
        }

        public ComposerPreRelease(List<string> p)
        {
            this.Add(p[0]);
            if (p.Count == 2 && !string.IsNullOrEmpty(p[1]))
            {
                this.Add(p[1]);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            ComposerPreRelease other = (ComposerPreRelease)obj;
            return ComparePreRelease(this, other) == 0;

        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ToString().GetHashCode();
            }
        }

        public static bool operator ==(ComposerPreRelease left, ComposerPreRelease right)
        {
            return ComparePreRelease(left, right) == 0;
        }

        public static bool operator !=(ComposerPreRelease left, ComposerPreRelease right)
        {
            return ComparePreRelease(left, right) != 0;
        }

        public static bool operator <(ComposerPreRelease left, ComposerPreRelease right)
        {
            return ComparePreRelease(left, right) == -1;
        }

        public static bool operator >(ComposerPreRelease left, ComposerPreRelease right)
        {

            return ComparePreRelease(left, right) == 1;
        }

        public static bool operator <=(ComposerPreRelease left, ComposerPreRelease right)
        {
            return (left < right) || (left == right);
        }

        public static bool operator >=(ComposerPreRelease left, ComposerPreRelease right)
        {
            return (left > right) || (left == right);
        }

        //Read up to the first integer component then increment
        public static ComposerPreRelease operator ++(ComposerPreRelease p)
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
        public static ComposerPreRelease operator --(ComposerPreRelease p)
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

        public static int ComparePreRelease(ComposerPreRelease left, ComposerPreRelease right)
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


        public override string ToString()
        {
            if ((this.Count == 2) && !string.IsNullOrEmpty(this[1]))
            {
                return this[0] + "-" + this[1];
            }
            else
            {
                return this[0];
            }
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
    }
}
