using System;
using System.Numerics;

namespace JubJubCrypto
{
    static class EdDSA
    {     
        public readonly static int Length = Poseidon.Length - 2;
        
        public static ECPoint GetPublicKey(BigInteger privateKey)
        {
            if (privateKey.Sign <= 0)
                throw new ArgumentException("Private key is not positive.");
            return privateKey * ECPoint.G;
        }

        public static void Sign(BigInteger privateKey, BigInteger[] message, out ECPoint R, out BigInteger S)
        {
            CheckMessage(message);
            ECPoint publicKey = GetPublicKey(privateKey);

            BigInteger[] buffer = new BigInteger[message.Length + 2];
            buffer[0] = privateKey; buffer[0] = Poseidon.Hash(buffer);
            Array.Copy(message, 0, buffer, 2, message.Length);

            BigInteger r = Poseidon.Hash(buffer) % ECPoint.Order;
            R = r * ECPoint.G;

            buffer[0] = R.X.Value; buffer[1] = publicKey.X.Value;
            BigInteger h = Poseidon.Hash(buffer) % ECPoint.Order;
            S = (r + h * privateKey) % ECPoint.Order;
        }

        public static bool Verify(ECPoint publicKey, BigInteger[] message, ECPoint R, BigInteger S)
        {
            CheckMessage(message);

            BigInteger[] buffer = new BigInteger[message.Length + 2];
            Array.Copy(message, 0, buffer, 2, message.Length);
            buffer[0] = R.X.Value; buffer[1] = publicKey.X.Value;

            BigInteger h = Poseidon.Hash(buffer) % ECPoint.Order;
            return S * ECPoint.G == R + h * publicKey;
        }

        private static void CheckMessage(BigInteger[] message)
        {
            if (message.Length != Length)
                throw new ArgumentException($"Message length is not {Length}.");
        }
    }
}
