using System;
using System.Collections;

namespace Unum
{

    public struct Posit
    {
        public bool sign;
        public int regime;
        public int exponent;
        public uint fraction;
        
        private int es;
        private int size;
        private bool inexact;
        private uint loss;

        public const string SignField = "Sign";
        public const string RegimeField = "Regime";
        public const string TerminatorField = "Terminator";
        public const string ExponentField = "Exponent";
        public const string FractionField = "Fraction";

        /// <summary>
        /// Inexact flag signals that previous operation lead to a loss of precision
        /// </summary>
        public bool InexactFlag { get { return inexact; } }

        public Posit(int size, int es)
        {
            if (es < 0 || es >= size)
                throw new System.ArgumentException(string.Format("es={0} parameter cannot be less than 1 or more or equal to width={1}", es, size));

            this.es = es;
            this.size = size;

            sign = false;
            regime = 0;
            exponent = 0;
            fraction = 0;
            inexact = false;
            loss = 0;
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
            loss = 0;
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
            loss = 0;
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
                fraction = bitLattice.GetUint(FractionField);
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
            uint floatFraction = fbits.GetUint(FractionField);

            sign = fbits.GetBool(SignField);

            int twoPowES = 1 << es;
            if (floatExp > 0)
                regime = floatExp / twoPowES;
            else if (floatExp < 0)
                regime = -(1 + (-floatExp - 1) / twoPowES);//-((-floatExp + 1) / twoPowES);

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

        public bool Validate()
        {
            if (exponent < 0)
                return false;

            if (RegimeSize + ExponentSize + FractionSize + 2 > Size)
                return false;

            int actualExponentSize = sizeof(uint) * 8 - clz((uint)exponent, sizeof(uint) * 8);
            if (actualExponentSize > ExponentSize)
                return false;

            int actualFractionSize = sizeof(uint) * 8 - clz(fraction, sizeof(uint) * 8);
            
            if (actualFractionSize > FractionSize)
                return false;

            return true;
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
            int floatExp = FullExponent - floatExpBias;

            if (floatExp > (1 << floatExpSize)-1)
            {
                floatExp = (1 << floatExpSize) - 1;
                inexact = true;
            }

            fbits.SetBool(SignField, sign);
            fbits.SetUint(ExponentField, (uint)floatExp);

            int fracResize = floatFracSize - FractionSize;
            uint floatFrac = fraction;
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

        private static void decomposeFullExponent(int fullExp, int es, out int positExp, out int positRegime)
        {
            int twoPowES = 1 << es;
            positRegime = 0;
            if (fullExp > 0)
                positRegime = fullExp / twoPowES;
            else if (fullExp < 0)
                positRegime = -(1 + (-fullExp - 1) / twoPowES);//-(-fullExp + 1) / twoPowES;

            positExp = fullExp - positRegime * twoPowES;
        }

        private static int clz(uint n, int size)
        {
            uint m = 1u << (size - 1);
            int c = 0;
            while ( (m & n) == 0 )
            {
                m >>= 1;
                ++c;
                if (c == size)
                    break;
            }
            return c;
        }

        private static uint shiftRightLoss(uint x, int shift)
        {
            return x & ((1u << shift) - 1);
        }

        private Posit add(Posit p)
        {
            int fullExp1 = FullExponent;
            int fullExp2 = p.FullExponent;

            uint fractionFirstBitMask1 = 1u << FractionSize;
            uint fractionFirstBitMask2 = 1u << p.FractionSize;

            int maxFractionSize = Math.Max(FractionSize, p.FractionSize);
            ulong fraction1 = fractionFirstBitMask1 | fraction;// (fraction >> 1);
            ulong fraction2 = fractionFirstBitMask2 | p.fraction;// (p.fraction >> 1);

            // normalizing fractions
            fraction1 <<= (maxFractionSize - FractionSize);
            fraction2 <<= (maxFractionSize - p.FractionSize);

            Posit r = new Posit(Size, ES);
            int resultFullExp = 0;
            ulong resultFraction = 0;

            if (sign == p.sign)
            {
                if (fullExp1 > fullExp2)
                {
                    resultFullExp = fullExp1;
                    r.loss += shiftRightLoss((uint)fraction2, fullExp1 - fullExp2);
                    r.inexact = true;
                    fraction2 >>= fullExp1 - fullExp2;
                }
                else
                {
                    resultFullExp = fullExp2;
                    r.loss += shiftRightLoss((uint)fraction1, fullExp2 - fullExp1);
                    r.inexact = true;
                    fraction1 >>= fullExp2 - fullExp1;
                }

                resultFraction = (ulong)(fraction1 + fraction2);
                if (resultFraction >> (maxFractionSize+1) > 0)
                {
                    r.loss += shiftRightLoss((uint)resultFraction, 1);
                    r.inexact = true;
                    resultFraction >>= 1;
                    ++resultFullExp;
                }

                resultFraction = ((~(1u << maxFractionSize)) & resultFraction);

                r.sign = sign;
                r.fraction = (uint)resultFraction;
                decomposeFullExponent(resultFullExp, ES, out r.exponent, out r.regime);
                if (maxFractionSize > r.FractionSize)
                {
                    r.loss += shiftRightLoss((uint)resultFraction, maxFractionSize - r.FractionSize);
                    r.inexact = true;
                    resultFraction >>= (maxFractionSize - r.FractionSize);
                }
                else if (r.FractionSize > maxFractionSize)
                    resultFraction <<= (r.FractionSize - maxFractionSize);

                r.fraction = (uint)resultFraction;
            }
            else
            {
                if (fullExp1 > fullExp2 || ((fullExp1 == fullExp2 && fraction1 > fraction2)))
                {
                    resultFullExp = fullExp1;
                    r.sign = sign;
                    fraction2 >>= fullExp1 - fullExp2;
                    resultFraction = (ulong)(fraction1 - fraction2);
                }
                else
                {
                    r.sign = !sign;
                    resultFullExp = fullExp2;
                    fraction1 >>= fullExp2 - fullExp1;
                    resultFraction = (ulong)(fraction2 - fraction1);
                }

                if (resultFraction == 0)
                {
                    r.sign = false;
                    resultFullExp = 0;
                }

                int shift = clz((uint)resultFraction, maxFractionSize+1);
                //if (shift > 0)
                {
                    resultFullExp -= shift;
                    resultFraction <<= shift;
                }
                resultFraction = ((~(1u << maxFractionSize)) & resultFraction);

                decomposeFullExponent(resultFullExp, ES, out r.exponent, out r.regime);
                if (maxFractionSize > r.FractionSize)
                    resultFraction >>= (maxFractionSize - r.FractionSize);
                else if (r.FractionSize > maxFractionSize)
                    resultFraction <<= (r.FractionSize - maxFractionSize);

                r.fraction = (uint)resultFraction;
            }

            return r;
        }

        public static Posit operator+(Posit a, Posit b)
        {
            return a.add(b);
        }

        public static Posit minPos(int size, int es)
        {
            Posit mp = new Posit(size, es);
            mp.regime = 2 - size;
            return mp;
        }

        public static Posit maxPos(int size, int es)
        {
            Posit mp = new Posit(size, es);
            mp.regime = size - 2;
            return mp;
        }

        public static Posit Infinity(int size, int es)
        {
            Posit inf = new Posit(size, es);
            inf.sign = true;
            inf.regime = 1 - size;
            return inf;
        }

        public static Posit Zero(int size, int es)
        {
            Posit zero = new Posit(size, es);
            zero.sign = false;
            zero.regime = 1 - size;
            return zero;
        }

        public Posit BitStep(int step)
        {
            if (sign)
                step = -step;

            if (IsZero)
            {
                if (step > 0)
                    return Posit.minPos(Size, ES);
                else if (step < 0)
                {
                    Posit nmp = minPos(Size, ES);
                    nmp.sign = true;
                    return nmp;
                }    
            }

            long newFraction = fraction + step;
            int carry = 0;

            long maxFraction = 1 << FractionSize;
            if (newFraction >= maxFraction)
            {
                newFraction -= maxFraction;
                carry = 1;
            }
            else if (newFraction < 0)
            {
                newFraction += maxFraction;
                carry = -1;
            }
           
            Posit r = new Posit(Size, ES);
            r.fraction = (uint)newFraction;
            r.sign = sign;
            r.regime = regime;
            /*
            int newExponent = FullExponent + carry;
            decomposeFullExponent(newExponent, ES, out r.exponent, out r.regime);
            */

            int newExponent = exponent + carry;
            int maxExponent = 1 << ExponentSize;
            if (newExponent >= maxExponent)
            {
                newExponent -= maxExponent;
                ++r.regime;
            }
            else if (newExponent < 0)
            {
                newExponent += maxExponent;
                --r.regime;
            }
            r.exponent = newExponent;

            if (r.regime > 0 && r.RegimeSize > r.Size-1)
            {
                return Posit.Infinity(r.size, r.es);
            }
            if (r.regime < 0 && r.RegimeSize > r.Size-1)
            {
                return Posit.Zero(r.size, r.es);
            }
            return r;
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

            double ffrac = 1.0 + fraction * Math.Pow(2.0, -FractionSize);

            return IntSign * Math.Pow(2.0, FullExponent) * ffrac;

            //return 0f;
        }

        public int ES { get { return es; } }
        public int Size { get { return size; } }

        public int FullExponent { get { return exponent + regime * (1 << es); } }
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
            get { return !sign && regime == 1-size && exponent == 0 && fraction == 0; }
        }

        public bool IsInfinity
        {
            get { return sign && regime == 1-size && exponent == 0 && fraction == 0; }
        }

        public override string ToString()
        {
            return string.Format("[({0}) reg={1} exp={2} frac={3}]", sign ? "-" : "+", regime, exponent, fraction);
        }
    }

}