using System;
using Unum;
using System.Collections.Generic;
using System.Collections;

namespace posit_csharp
{
    class Program
    {
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
            float x = 11347935.15625f;
            BitLattice fbl = new BitLattice(BitConverter.GetBytes(x));
            fbl.AddField("S", 31, 1);
            fbl.AddField("Exponent", 23, 8);
            fbl.AddField("Fraction", 0, 23);

            Console.WriteLine(fbl);
            Console.WriteLine();

            Posit p = new Posit(x, 3);
            BitLattice pbl = p.Encode();
            Console.WriteLine(pbl);
            Console.WriteLine(string.Format("f = {0} p = {0}", x, p.CalculatedValue()));
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
            positTestFromFloat();
        }
    }
}
