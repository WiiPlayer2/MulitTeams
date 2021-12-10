using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTeams
{
    static class Helper
    {
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> sequence) => sequence.Where(o => o != null).Select(o => o!);
    }
}
