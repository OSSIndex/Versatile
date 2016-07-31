using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sprache;
namespace Versatile
{
    public interface IVersionFactory<T> where T: Version
    {
        T Construct(List<string> v);
        T Max();
        T Min();
        Parser<T> Parser { get; }
    }
}
