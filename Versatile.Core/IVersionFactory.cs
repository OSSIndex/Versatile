using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Versatile
{
    public interface IVersionFactory<T> where T: Version
    {
        T Construct(List<string> v);
        T Max();
        T Min();
    }
}
