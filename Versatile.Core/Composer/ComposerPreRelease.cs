using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Versatile
{
    public class ComposerPreRelease : PreReleaseVersion
    {
        public ComposerPreRelease(string prerelease) : base(prerelease) { }

        public ComposerPreRelease(List<string> p) : base(p) { }
    } 
}
