using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Util
{
    // https://stackoverflow.com/questions/3130922/sortedsett-and-anonymous-icomparert-in-the-constructor-is-not-working
    public class FuncComparer<T> : IComparer<T>
    {
        private readonly Comparison<T> comparison;

        public FuncComparer(Comparison<T> comparison)
        {
            this.comparison = comparison;
        }
        public int Compare(T x, T y)
        {
            return comparison(x, y);
        }
    }

}
