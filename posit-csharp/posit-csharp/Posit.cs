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
        private int size;
        private bool inexact;

        public const string SignField = "Sign";
        public const string RegimeField = "Regime";
        public const string TerminatorField = "Terminator";
        public const string ExponentField = "Exponent";
        public const string FractionField = "Fraction";

        /// <summary>
        /// Inexact flag signals that previous operation lead to a loss of precision
        /// </summary>
        public bool InexactFlag { get { return inexact; } }

        public Posit(int width, int es)
        {
            if (es < 0 || es >= width)
                throw new System.ArgumentException(string.Format("es={0} parameter cannot be less than 1 or more or equal to width={1}", es, width));

            this.es = es;
            this.size = width;

            sign = false;
            regime = 0;
            exponent = 0;
            fraction = 0;
            inexact = false;
        }

        public Posit(BitArray bits, int es)
        {
            sign = false;
            regime = 0;
            exponent = 0;
            fraction = 0;
            this.es = es;
            this.size = bits.Length;
            inexact = false;
            Decode(bits);
        }

        public Posit(float value, int es)
        {
            sign = false;
            regime = 0;
            exponent = 0;
            fraction = 0;
            this.es = es;
            size = sizeof(float)*8;
            inexact = false;
            fromFloat(value);
        }

        public BitLattice Encode()
        {
            // --- Sign ---
            BitLattice b = new BitLattice(size);
            b.AddField(SignField, size - 1, 1);
            b.SetBool(SignField, sign); // writing sign

            // --- Regime ---
            int regimeSize;

            if (regime >= 0)
                regimeSize = regime + 1;
            else
                regimeSize = -regime;

            int regimePosition = size - 1 - regimeSize;
            int regimeTerminatorPosition = regimePosition - 1;

            b.AddField(RegimeField, regimePosition, regimeSize);            
            for (int i = 0; i < regimeSize; ++i) 
            {
                b[regimePosition + i] = regime >= 0; // writing regime
            }
            b.AddField(TerminatorField, regimeTerminatorPosition, 1);
            b[regimeTerminatorPosition] = regime < 0; // writing regime terminator

            // --- Exponent ---
            int exponentSize = Math.Min(es, size - 2 - regimeSize);
            if (exponentSize > 0)
            {
                int exponentPosition = regimeTerminatorPosition - exponentSize;
                b.AddField(ExponentField, exponentPosition, exponentSize);
                b.SetUint(ExponentField, (uint)exponent);
            }

            // --- Fraction ---
            int fractionSize = Math.Max(0, size - 2 - regimeSize - exponentSize);
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

            bl.AddField(SignField, size - 1, 1);

            int pos = size - 2;
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

        private void fromFloat(float number)
        {
            inexact = false;
            byte[] data = BitConverter.GetBytes(number);
            BitLattice fbits = new BitLattice(data);

            // IEEE 754
            int floatExpSize = 8;
            int floatFracSize = 23;
            fbits.AddField(SignField, fbits.Size - 1, 1);
            fbits.AddField(ExponentField, fbits.Size - floatExpSize - 1, floatExpSize);
            fbits.AddField(FractionField, 0, floatFracSize);

            //int floatSign = fbits.GetBool(SignField) ? 1 : -1;
            int floatExpBias = 1 - (2 << (fbits.GetFieldLength(ExponentField) - 2)); // -127
            int floatExp = (int)fbits.GetUint(ExponentField) + floatExpBias;
            int floatFraction = (int)fbits.GetUint(FractionField);

            sign = fbits.GetBool(SignField);

            int twoPowES = 1 << es;
            if (floatExp > 0)
                regime = floatExp / twoPowES;
            else if (floatExp < 0)
                regime = -(-floatExp + 1) / twoPowES;

            exponent = floatExp - regime * twoPowES;
            fraction = floatFraction;

            int fracResize = FractionSize - floatFracSize;
            if (fracResize < 0)
            {
                inexact = true;
                fraction >>= (-fracResize);
            }
            else if (fracResize > 0)
                fraction <<= fracResize;
            /*
            while (fraction > (1 << FractionSize))
            {
                fraction >>= 1;
                inexact = true;
            }
            */

            //BitLattice pbits = new BitLattice(fbits.Size);

            //pbits.AddField(SignField, fbits.Size - 1, 1);
            //pbits.AddField(RegimeField, fbits.Size - 2, )
        }

        public float ToFloat()
        {
            BitLattice fbits;
            return ToFloat(out fbits);
        }

        public float ToFloat(out BitLattice fbits)
        {
            inexact = false;
            fbits = new BitLattice(32);

            // IEEE 754
            int floatExpSize = 8;
            int floatFracSize = 23;
            fbits.AddField(SignField, fbits.Size - 1, 1);
            fbits.AddField(ExponentField, fbits.Size - floatExpSize - 1, floatExpSize);
            fbits.AddField(FractionField, 0, floatFracSize);

            int floatExpBias = 1 - (2 << (fbits.GetFieldLength(ExponentField) - 2)); // -127
            int floatExp = exponent + regime * (1 << es) - floatExpBias;

            if (floatExp > (1 << floatExpSize)-1)
            {
                floatExp = (1 << floatExpSize) - 1;
                inexact = true;
            }

            fbits.SetBool(SignField, sign);
            fbits.SetUint(ExponentField, (uint)floatExp);

            int fracResize = floatFracSize - FractionSize;
            int floatFrac = fraction;
            if (fracResize < 0)
            {
                inexact = true;
                floatFrac >>= (-fracResize);
            }
            else if (fracResize > 0)
                floatFrac <<= fracResize;

            fbits.SetUint(FractionField, (uint)floatFrac);

            return BitConverter.ToSingle(fbits.ToBytes());
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

        public double CalculatedValue()
        {
            /*
            int useed = 2 << (es - 1);
            useed = 2 << (useed - 1);
            int v = IntPow(useed, regime) * (2 << (exponent - 1));
            return IntSign * v;
            */

            if (IsZero)
                return 0.0;

            if (IsInfinity)
                return double.PositiveInfinity;

            int twoPowES = 1 << es;

            double ffrac = 1.0 + fraction * Math.Pow(2.0, -FractionSize);

            return IntSign * Math.Pow(2.0, regime * twoPowES + exponent) * ffrac;

            //return 0f;
        }

        public int ES { get { return es; } }
        public int Size { get { return size; } }

        public int ExponentSize { get { return Math.Clamp(Size - 2 - RegimeSize, 0, ES); } }

        public int RegimeSize
        { 
            get
            {
                if (regime >= 0)
                    return regime + 1;
                else
                    return -regime;
            }
        }

        public int FractionSize
        {
            get { return Math.Max(Size - 2 - RegimeSize - ExponentSize, 0); }
        }

        public int IntSign
        {
            get { return sign ? -1 : 1; }
            set { sign = value < 0; }
        }

        public bool IsZero
        {
            get { return !sign && regime == 0 && exponent == 0 && fraction == 0; }
        }

        public bool IsInfinity
        {
            get { return sign && regime == 0 && exponent == 0 && fraction == 0; }
        }
    }

}