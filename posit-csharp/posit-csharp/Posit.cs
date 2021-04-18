using System;
using System.Collections;

namespace Posit
{

    public struct Posit
    {
        public bool sign;
        public uint regime;
        public uint exponent;
        public uint fraction;
        
        private uint es;
        private uint width;

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

        public Posit(BitArray bits, uint es)
        {
            Decode(bits);
        }
        */

        public BitArray Encode()
        {
            return null;
        }

        public void Decode(BitArray bits)
        {

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
            int useed = 2 << ((int)es - 1);
            useed = 2 << (useed - 1);
            int v = IntPow(useed, regime) * (2 << ((int)exponent - 1));
            return IntSign * v;
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