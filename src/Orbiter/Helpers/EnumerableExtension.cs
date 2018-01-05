using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbiter.Helpers
{
    public static class EnumerableExtension
    {
        public static (T item, double value) GetMin<T>(this IEnumerable<T> list, Func<T, double> func)
        {
            var min = default(T);
            double minValue = double.MaxValue;
            foreach (var item in list)
            {
                var value = func(item);
                if (value < minValue)
                {
                    minValue = value;
                    min = item;
                }
            }

            return (min, minValue);
        }
    }
}
