using System;
using Unum;
using System.Collections.Generic;
using System.Collections;

namespace posit_csharp
{
    class Program
    {
        static void testDiv()
        {
            for (int i = -32; i < 33; ++i)
            {
                int j = i/8;
                if (i < 0)
                {
                    j = - (1 + (- i - 1) / 8);
                }
                Console.WriteLine(string.Format("{0}/{1} = {2}", i, 8, j));
            }
        }
        static void addTest()        
        {
            float af = 5.23f;//-5.23f;//0.15625f;
            float bf = 49.11f;//49.1f;//-0.153f;
            float tf = 54.34f;

            Posit a = new Posit(af, 3);
            //a.fraction = 1;
            //a.exponent = 3;
            Posit b = new Posit(bf, 3);
            //b.fraction = 2;
            //b.exponent = 3;

            
            Posit t = new Posit(tf, 3); //[(+) reg=-1 exp=5 frac=125292250]
            //t.regime = -1;
            //t.exponent = 5;
            //t.fraction = 
            BitLattice blt = t.Encode();
            Console.WriteLine(string.Format("test={0} calc={1} tofloat={2} posit={3}", tf, t.CalculatedValue(), t.ToFloat(), t));
            Console.WriteLine(blt);

            /*
            blt[1] = true;
            t.Decode(blt);
            //t.regime = -1;
            //t.exponent = 5;
            //t.fraction = 125292250;
            blt = t.Encode();
            Console.WriteLine(string.Format("\ntest={0} calc={1} tofloat={2} posit={3}", tf, t.CalculatedValue(), t.ToFloat(), t));
            Console.WriteLine(blt);
            */

            /*
            t.regime = -1;
            t.exponent = 5;
            t.fraction = 125292250;
            blt = t.Encode();
            Console.WriteLine(string.Format("\ntest={0} calc={1} tofloat={2} posit={3}", tf, t.CalculatedValue(), t.ToFloat(), t));
            Console.WriteLine(blt);
            */

            Posit c = a + b;

            BitLattice bla = a.Encode();
            BitLattice blb = b.Encode();
            BitLattice blc = c.Encode();

            Console.WriteLine(string.Format("\n\nfloat   {0} + {1} = {2}", af, bf, af+bf));
            Console.WriteLine(string.Format("calc    {0} + {1} = {2}", a.CalculatedValue(), b.CalculatedValue(), c.CalculatedValue()));
            Console.WriteLine(string.Format("tofloat {0} + {1} = {2}", a.ToFloat(), b.ToFloat(), c.ToFloat()));
            Console.WriteLine(string.Format("posit   {0} + {1} = {2}", a, b, c));

            Console.WriteLine("\na = " + a.CalculatedValue());
            Console.WriteLine(bla);
            Console.WriteLine("\nb = " + b.CalculatedValue());
            Console.WriteLine(blb);
            Console.WriteLine("\na + b = " + c.CalculatedValue());
            Console.WriteLine(blc);
        }


        static void positTestCalcValue()
        {
            //BitLattice bl = new BitLattice(16);
            //bl.AddField(Posit.SignField, bl.Size - 1, 1);
            //bl.AddField(Posit.RegimeField)
            BitArray br = new BitArray(16);
            byte[] bits = new byte[16] { 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 0, 0, 0, 0 };
            for (int i = 0; i < bits.Length; ++i)
                br.Set(i, bits[i] > 0);

            Posit p = new Posit(br, 3);
            BitLattice bl = p.Encode();
            Console.WriteLine(string.Format("Value = {0}", p.CalculatedValue()));
            Console.WriteLine(bl);
        }

        static void positTestFromFloat()
        {
            //float x = 0.15625f;
            float x = -27.413f;
            BitLattice fbl = new BitLattice(BitConverter.GetBytes(x));
            fbl.AddField("S", 31, 1);
            fbl.AddField("Exponent", 23, 8);
            fbl.AddField("Fraction", 0, 23);

            Console.WriteLine(fbl);
            Console.WriteLine();

            Posit p = new Posit(x, 3);
            BitLattice pbl = p.Encode();
            Console.WriteLine(pbl);
            Console.WriteLine();
            BitLattice p2fbl;
            float x2 = p.ToFloat(out p2fbl);
            Console.WriteLine(p2fbl);
            Console.WriteLine(string.Format("\nf = {0} p = {1} x2 = {2}", x, p.CalculatedValue(), x2));

            
        }

        static void Main(string[] args)
        {
            /*
            BitLattice bitLattice = new BitLattice(32);

            bitLattice.AddField("test", 12, 8);

            bitLattice.SetUInt("test", 123);

            Console.WriteLine(bitLattice.GetUInt("test"));
            Console.Write(bitLattice);
            */

            /*
            float x = 0.15625f;
            BitLattice bitLattice = new BitLattice(BitConverter.GetBytes(x));
            bitLattice.AddField("S", 31, 1);
            bitLattice.AddField("Exponent", 23, 8);
            bitLattice.AddField("Fraction", 0, 23);

            Console.WriteLine(bitLattice);
            Console.WriteLine();
            int es = 0;
            Posit p = new Posit(32, es);
            p.IntSign = 1;
            p.regime = 4;
            p.exponent = 17;
            p.fraction = 123;

            BitLattice pbl = p.Encode();
            Console.WriteLine(pbl);
            BitArray pbr = pbl.ToBitArray();

            Posit p2 = new Posit(pbr, es);
            BitLattice pbl2 = p2.Encode();
            Console.WriteLine(pbl2);
            */

            //positTestCalcValue();
            //positTestFromFloat();
            addTest();
            //testDiv();
        }
    }
}
