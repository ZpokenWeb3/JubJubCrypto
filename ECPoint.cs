using System;
using System.Numerics;

namespace JubJubCrypto
{
    struct ECPoint
    {
        public readonly static Field A = 168700;
        public readonly static Field D = 168696;

        public readonly static int Cofactor = 8;

        public readonly static BigInteger Order = BigInteger.Parse(
            "21888242871839275222246405745257275088614511777268538073601725287587578984328");

        public readonly static ECPoint G = new ECPoint(
                Field.Parse("16540640123574156134436876038791482806971768689494387082833631921987005038935"),
                Field.Parse("20819045374670962167435360035096875258406992893633759881276124905556507972311"));

        public readonly static ECPoint I = new ECPoint(0, 1);

        public readonly Field X;
        public readonly Field Y;

        private ECPoint(Field x, Field y) { X = x; Y = y; }

        public static explicit operator ECPoint(ValueTuple<Field, Field> p)
        {
            Field f = p.Item1 * p.Item1, s = p.Item2 * p.Item2;
            if (A * f + s != 1 + D * f * s)
                throw new InvalidCastException("The point is not on the curve.");
            return new ECPoint(p.Item1, p.Item2);
        }

        public static ECPoint operator +(ECPoint p) => p;
        public static ECPoint operator -(ECPoint p) => new ECPoint(-p.X, p.Y);
        public static bool operator ==(ECPoint a, ECPoint b) => (a.X == b.X) && (a.Y == b.Y);
        public static bool operator !=(ECPoint a, ECPoint b) => (a.X != b.X) || (a.Y != b.Y);
        public override bool Equals(object e) => (e is ECPoint p) && (p == this);
        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();
        public static ECPoint operator +(ECPoint a, ECPoint b)
        {
            Field c = D * a.X * b.X * a.Y * b.Y;
            Field x = (a.X * b.Y + b.X * a.Y) / (1 + c);
            Field y = (a.Y * b.Y - A * a.X * b.X) / (1 - c);
            return new ECPoint(x, y);
        }
        public static ECPoint operator -(ECPoint a, ECPoint b) => -b + a;
        public static ECPoint operator *(BigInteger a, ECPoint p)
        {
            ECPoint r = I;
            if (a.Sign < 0) { p = -p; a = -a; }
            byte[] s = a.ToByteArray();

            for (int i = 0; i < s.Length; i++)
                for (int k = 0, d = s[i]; k < 8; k++)
                {
                    if ((d & 1) > 0) r += p;
                    p += p; d >>= 1;
                }

            return r;
        }
    }
}
