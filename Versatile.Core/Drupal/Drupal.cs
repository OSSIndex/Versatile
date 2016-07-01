using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Versatile
{
    public partial class Drupal : Version
    {
        public override string ToNormalizedString()
        {
            return this.Aggregate((p, n) => p + "." + n);
        }

        public Drupal(int? major, int? minor, int? patch, string prerelease = "") : base(major, minor, patch, prerelease) { }
    }
}
