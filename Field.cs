using System;
using System.Numerics;

namespace JubJubCrypto
{
    struct Field
    {
        public readonly static BigInteger Characteristic =
            BigInteger.Parse("21888242871839275222246405745257275088548364400416034343698204186575808495617");

        public readonly BigInteger Value;

        private Field(BigInteger v)
        {
            v %= Characteristic;
            Value = v.Sign < 0 ? (Characteristic + v) : v;
        }

        public static implicit operator Field(long v) => new Field(v);

        public static implicit operator Field(BigInteger v) => new Field(v);
        public static Field operator +(Field v) => v;
        public static Field operator -(Field v) => new Field(-v.Value);
        public static bool operator ==(Field a, Field b) => a.Value == b.Value;
        public static bool operator !=(Field a, Field b) => a.Value != b.Value;
        public static Field operator +(Field a, Field b) => new Field(a.Value + b.Value);
        public static Field operator -(Field a, Field b) => new Field(a.Value - b.Value);
        public static Field operator *(Field a, Field b) => new Field(a.Value * b.Value);
        public static Field operator /(Field a, Field b) => a * b.Inverse();
        public override string ToString() => Value.ToString();
        public override int GetHashCode() => Value.GetHashCode();
        public override bool Equals(object e) => (e is Field v) && (v == this);
        public bool IsZero() => Value.IsZero;
        public bool IsOne() => Value.IsOne;
        public static Field Parse(string s) => new Field(BigInteger.Parse(s));

        public static bool TryParse(string s, out Field v)
        {
            bool r = BigInteger.TryParse(s, out BigInteger n);
            v = new Field(n);
            return r;
        }

        public Field Inverse()
        {
            if (Value.IsZero) throw new DivideByZeroException();

            BigInteger x = Value, y = Characteristic,
                a = BigInteger.One, c = BigInteger.Zero;

            while (true)
            {
                c -= a * BigInteger.DivRem(y, x, out y);
                if (y.IsZero) return new Field(a);
                a -= c * BigInteger.DivRem(x, y, out x);
                if (x.IsZero) return new Field(c);
            }
        }
    }
}
