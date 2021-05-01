using System;
using System.Collections;

namespace Unum
{

    public struct Posit
    {
        public bool sign;
        public int regime;
        public int exponent;
        public int fraction;
        
        private int es;
        private int width;

        public const string SignField = "Sign";
        public const string RegimeField = "Regime";
        public const string TerminatorField = "Terminator";
        public const string ExponentField = "Exponent";
        public const string FractionField = "Fraction";

        public Posit(int width, int es)
        {
            if (es < 0 || es >= width)
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

        public Posit(BitArray bits, int es)
        {
            sign = false;
            regime = 0;
            exponent = 0;
            fraction = 0;
            this.es = es;
            this.width = bits.Length;
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

            int regimePosition = width - 1 - regimeSize;
            int regimeTerminatorPosition = regimePosition - 1;

            b.AddField(RegimeField, regimePosition, regimeSize);            
            for (int i = 0; i < regimeSize; ++i) 
            {
                b[regimePosition + i] = regime >= 0; // writing regime
            }
            b.AddField(TerminatorField, regimeTerminatorPosition, 1);
            b[regimeTerminatorPosition] = regime < 0; // writing regime terminator

            // --- Exponent ---
            int exponentSize = Math.Min(es, width - 2 - regimeSize);
            if (exponentSize > 0)
            {
                int exponentPosition = regimeTerminatorPosition - exponentSize;
                b.AddField(ExponentField, exponentPosition, exponentSize);
                b.SetUint(ExponentField, (uint)exponent);
            }

            // --- Fraction ---
            int fractionSize = Math.Max(0, width - 2 - regimeSize - exponentSize);
            if (fractionSize > 0)
            {
                b.AddField(FractionField, 0, fractionSize);
                b.SetUint(FractionField, (uint)fraction);
            }

            return b;
        }

        public void Decode(BitLattice bitLattice)
        {
            sign = bitLattice.GetBool(SignField);
            if (bitLattice.GetBool(RegimeField))
            {
                regime = bitLattice.GetFieldLength(RegimeField) - 1;
            }
            else
            {
                regime = -bitLattice.GetFieldLength(RegimeField);
            }

            if (bitLattice.HasField(ExponentField))
                exponent = (int)bitLattice.GetUint(ExponentField);
            else
                exponent = 0;

            if (bitLattice.HasField(FractionField))
                fraction = (int)bitLattice.GetUint(FractionField);
            else
                fraction = 0;
        }

        public void Decode(BitArray bitArray)
        {
            BitLattice bl = new BitLattice(bitArray);

            bl.AddField(SignField, width - 1, 1);

            int pos = width - 2;
            int regimeSize = 0;
            bool regimeBit = bl[pos];

            while (bl[pos] == regimeBit)
            {
                ++regimeSize;
                --pos;
            }

            int regimePosition = pos+1;
            bl.AddField(RegimeField, regimePosition, regimeSize);

            int regimeTerminatorPosition = pos;
            bl.AddField(TerminatorField, regimeTerminatorPosition, 1);

            int exponentSize = Math.Min(pos, es);
            int exponentPosition = pos - exponentSize;
            if (exponentSize > 0)
                bl.AddField(ExponentField, exponentPosition, exponentSize);
            pos = exponentPosition;

            if (pos > 0)
            {
                bl.AddField(FractionField, 0, pos);
            }

            Decode(bl);
        }

        private int IntPow(int x, int pow)
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
            int useed = 2 << (es - 1);
            useed = 2 << (useed - 1);
            int v = IntPow(useed, regime) * (2 << (exponent - 1));
            return IntSign * v;
            */
            return 0f;
        }

        public float ToFloat()
        {
            return 0f;
        }

        public int ES { get { return ES; } }
        public int Width { get { return width; } }

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