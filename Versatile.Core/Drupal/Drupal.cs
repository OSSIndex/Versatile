using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Versatile
{
    public partial class Drupal : Version
    {
        #region Public properties
        public int CoreCompatibility { get; set; }
        #endregion

        #region Constructors
        public Drupal(List<string> d) : base(d.Skip(1).ToList())
        {
            this.CoreCompatibility = Int32.Parse(d[0]);
            this.Insert(0, d[0]);
            if (d.Count == 5) this.PreRelease = new PreReleaseVersion(d[4]);
        }

        public Drupal(int core, int? major, int? minor, int? patch, string prerelease = "") : base(major, minor, patch, prerelease)
        {
            this.CoreCompatibility = core;
        }
        #endregion

        #region Overriden methods
        public override string ToNormalizedString()
        {
            return this.Aggregate((p, n) => p + "." + n);
        }
        #endregion
    }
}
