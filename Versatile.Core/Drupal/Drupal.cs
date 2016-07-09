using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Sprache;
namespace Versatile
{
    public partial class Drupal : Version, IVersionFactory<Drupal>, IEquatable<Drupal>, IComparable<Drupal>
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
            if (d.Count == 5) this.PreRelease = new PreReleaseVersion(d[4]);
        }

        public Drupal(int core, int? major, int? minor, int? patch, string prerelease = "") : base(major, minor, patch, prerelease)
        {
            this.CoreCompatibility = core;
            this.Insert(0, core.ToString());
        }
        #endregion

        #region Overriden methods
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        
        public override string ToNormalizedString()
        {
            return this.Aggregate((p, n) => p + "." + n);
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
            else return CompareComponent(this.ToNormalizedString(), other.ToNormalizedString());
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

        #endregion

        #region Public static fields
        
        #endregion

        #region Public Static methods
        
        public static bool RangeIntersect(string left, string right, out string exception_message)
        {
            IResult<ComparatorSet<Drupal>> l = Grammar.Range.TryParse(left);
            IResult<ComparatorSet<Drupal>> r = Grammar.Range.TryParse(right);
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
