using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Util
{
    /// <summary>
    /// Utility methods for accumulating hash codes
    /// </summary>
    public static class HashHelper
    {
        /// Constant multiplier applied with each.
        static int mult = 37;
        static int startVal = 19;

        /// Begin a new hash with the base value.
        public static int Start(int code)
        {
            return Hash(startVal, code);
        }

        public static int Start<T>(T obj)
        {
            return Hash(startVal, obj);
        }

        /// Accumulate an existing hash.
        public static int Hash(int cur, int code)
        {
            return (cur *= mult) + code;
        }

        public static int Hash<T>(int cur, T obj)
        {
            return (cur *= mult) + (obj == null ? 0 : obj.GetHashCode());
        }

        public static int Hash<T>(params T[] args)
        {
            int hash = Start(args[0]);
            for (int i = 1; i < args.Length; ++i)
                hash = Hash(hash, args[i]);
            return hash;
        }
    }

    public static class HashExt
    {
        public static int Hash<T>(this List<T> args)
        {
            int hash = HashHelper.Start(args[0]);
            for (int i = 1; i < args.Count; ++i)
                hash = HashHelper.Hash(hash, args[i]);
            return hash;
        }
    }
}
