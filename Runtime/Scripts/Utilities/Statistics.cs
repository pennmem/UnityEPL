using System.Numerics;

namespace UnityEPL {

    public class Statistics {
        // https://stackoverflow.com/a/51740258
        public static BigInteger Factorial(BigInteger integer) {
            if(integer < 1) return new BigInteger(1);

            BigInteger result = integer;
            for (BigInteger i = 1; i < integer; i++)
            {
                result = result * i;
            }

            return result;
        }

        // https://stackoverflow.com/a/51740258
        public static BigInteger Permutation(BigInteger n, BigInteger r) {
            return Factorial(n) / Factorial(n-r);
        }

        public static BigInteger Combination(BigInteger n, BigInteger r) {
            return Permutation(n, r) / Factorial(r);
        }
    }
}