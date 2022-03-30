using System;
using System.Numerics;

namespace JubJubCrypto
{
    static partial class Poseidon
    {
        public readonly static int Length;

        private readonly static int Rounds;
        private readonly static Field[,] Matrix;
        private readonly static Field[] Constants;

        static Poseidon()
        {
            Length = MATRIX.GetLength(0) - 1;
            Rounds = CONSTANTS.Length / (Length + 1);

            Matrix = new Field[Length + 1, Length + 1];
            for (int i = 0; i <= Length; i++)
                for (int k = 0; k <= Length; k++)
                    Matrix[i, k] = Field.Parse(MATRIX[i, k]);

            Constants = new Field[CONSTANTS.Length];
            for (int i = 0; i < Constants.Length; i++)
                Constants[i] = Field.Parse(CONSTANTS[i]);
        }

        private static void Initialize(BigInteger[] message, Field[] state)
        {
            for (int i = 1; i < state.Length; i++)
                state[i] = message[i - 1];
            state[0] = 0;
        }

        private static void Add(Field[] state, int round)
        {
            int offset = round * state.Length;
            for (int i = 0; i < state.Length; i++)
                state[i] += Constants[offset + i];
        }

        private static void Substitute(Field[] state, bool partial)
        {
            Field Fifth(Field v) { Field r = v * v; return r * r * v; }

            state[0] = Fifth(state[0]);
            if (partial) return;

            for (int i = 1; i < state.Length; i++)
                state[i] = Fifth(state[i]);
        }

        private static void Mix(ref Field[] state, ref Field[] buffer)
        {
            for (int i = 0; i < state.Length; i++)
            {
                Field v = 0;
                for (int k = 0; k < state.Length; k++)
                    v += state[k] * Matrix[i, k];
                buffer[i] = v;
            }

            Field[] s = buffer;
            buffer = state;
            state = s;
        }

        public static BigInteger Hash(BigInteger[] message)
        {
            if (message.Length != Length)
                throw new ArgumentException($"Message length is not {Length}.");

            Field[] state = new Field[message.Length + 1];
            Field[] buffer = new Field[state.Length];
            Initialize(message, state);

            for (int i = 0; i < Rounds; i++)
            {
                Add(state, i);
                Substitute(state, (i > 3) && (i + 4 < Rounds));
                Mix(ref state, ref buffer);
            }

            return state[1].Value;
        }
    }
}
