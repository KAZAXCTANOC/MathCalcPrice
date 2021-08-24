using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathCalcPrice.Entity
{
    public enum RPKShipherCompEnum
    {
        Equal = 1,
        SemiEqual = 0,
        NotEqual = -1
    }

    public class RPKShipher
    {
        public string S { get; set; }
        public string F { get; set; }
        public string C { get; set; }
        public string M { get; set; }
        public string X_M { get; set; }
        public string P { get; set; }

        public string Code
        {
            get
            {
                return $"{S}.{F}.{C}.{M}.{X_M}.{P}";
            }
        }
        public string ShortCode
        {
            get
            {
                return $"{C}.{M}.{X_M}.{P}";
            }
        }
        public RPKShipher() { }

        public RPKShipher(string shipher)
        {
            var elems = shipher.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (elems.Length != 6)
                throw new ArgumentException("Шифр должен быть следующего вида [*.*.*.*.*.*]");
            S = elems[0];
            F = elems[1];
            C = elems[2];
            M = elems[3];
            X_M = elems[4];
            P = elems[5];
        }
        private bool QuickCheck(string a, string b, bool nonstrict)
            => a != null &&
              (a != "*" || nonstrict) &&
               b != null &&
              (a == b || b == "*");

        public RPKShipherCompEnum CompareTo(RPKShipher other, bool wt = false, bool nonstrict = false)
        {
            int sum = 0;
            if (QuickCheck(S, other.S, nonstrict)) sum++;
            if (QuickCheck(F, other.F, nonstrict)) sum++;
            if (QuickCheck(C, other.C, nonstrict)) sum++;
            if (QuickCheck(M, other.M, nonstrict)) sum++;
            if (QuickCheck(X_M, other.X_M, nonstrict)) sum++;
            if (QuickCheck(P, other.P, nonstrict) || (wt && P.Contains("С"))) sum++;

            if (sum == 6) return RPKShipherCompEnum.Equal; // 1
            else if (sum == 4 || (M == "ВБ" && other.M == "ВБ")) return RPKShipherCompEnum.SemiEqual; // 0
            return RPKShipherCompEnum.NotEqual; // -1
        }

        public override string ToString()
        {
            return Code;
        }
    }
}
