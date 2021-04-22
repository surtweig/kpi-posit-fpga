using System;
using System.Collections;

namespace Unum
{

    public struct Posit
    {
        public bool sign;
        public int regime;
        public uint exponent;
        public uint fraction;
        
        private uint es;
        private uint width;

        public const string SignField = "Sign";
        public const string RegimeField = "Regime";
        public const string TerminatorField = "Terminator";
        public const string ExponentField = "Exponent";
        public const string FractionField = "Fraction";

        public Posit(uint width, uint es)
        {
            if (es < 1 || es >= width)
                throw new System.ArgumentException(string.Format("es={0} parameter cannot be less than 1 or more or equal to width={1}", es, width));

            this.es = es;
            this.width = width;

            sign = false;
            regime = 0;
            exponent = 0;
            fraction = 0;
        }

        /*
        public Posit(float number)
        {
            byte[] data = BitConverter.GetBytes(number);
            BitArray bits = new BitArray(data);
        }
        */

        public Posit(BitArray bits, uint es)
        {
            sign = false;
            regime = 0;
            exponent = 0;
            fraction = 0;
            this.es = es;
            this.width = (uint)bits.Length;
            Decode(bits);
        }

        public BitLattice Encode()
        {
            // --- Sign ---
            BitLattice b = new BitLattice(width);
            b.AddField(SignField, width - 1, 1);
            b.SetBool(SignField, sign); // writing sign

            // --- Regime ---
            int regimeSize;

            if (regime >= 0)
                regimeSize = regime + 1;
            else
                regimeSize = -regime;

            int regimePosition = (int)width - 1 - regimeSize;
            int regimeTerminatorPosition = regimePosition - 1;

            b.AddField(RegimeField, (uint)regimePosition, (uint)regimeSize);            
            for (int i = 0; i < regimeSize; ++i) 
            {
                b[regimePosition + i] = regime >= 0; // writing regime
            }
            b.AddField(TerminatorField, (uint)regimeTerminatorPosition, 1);
            b[regimeTerminatorPosition] = regime < 0; // writing regime terminator

            // --- Exponent ---
            int exponentSize = Math.Min((int)es, (int)width - 2 - regimeSize);
            if (exponentSize > 0)
            {
                int exponentPosition = regimeTerminatorPosition - exponentSize;
                b.AddField(ExponentField, (uint)exponentPosition, (uint)exponentSize);
                b.SetUInt(ExponentField, exponent);
            }

            // --- Fraction ---
            int fractionSize = Math.Max(0, (int)width - 2 - regimeSize - exponentSize);
            if (fractionSize > 0)
            {
                b.AddField(FractionField, 0, (uint)fractionSize);
                b.SetUInt(FractionField, fraction);
            }

            return b;
        }

        public void Decode(BitLattice bitLattice)
        {
            
        }

        public void Decode(BitArray bitArray)
        {
            BitLattice bl = new BitLattice(bitArray);

            bl.AddField(SignField, width - 1, 1);

            int pos = (int)width - 2;
            bool regimeBit = bl[(int)width - 2];

            Decode(bl);
        }

        private int IntPow(int x, uint pow)
        {
            int ret = 1;
            while (pow != 0)
            {
                if ((pow & 1) == 1)
                    ret *= x;
                x *= x;
                pow >>= 1;
            }
            return ret;
        }

        public float CalculatedValue()
        {
            /*
            int useed = 2 << ((int)es - 1);
            useed = 2 << (useed - 1);
            int v = IntPow(useed, regime) * (2 << ((int)exponent - 1));
            return IntSign * v;
            */
            return 0f;
        }

        public float ToFloat()
        {
            return 0f;
        }

        public uint ES { get { return ES; } }
        public uint Width { get { return width; } }

        public int IntSign
        {
            get { return sign ? -1 : 1; }
            set { sign = value < 0; }
        }

        public bool IsZero
        {
            get { return !sign && exponent == 0 && fraction == 0; }
        }

        public bool IsInfinity
        {
            get { return sign && exponent == 0 && fraction == 0; }
        }
    }

}