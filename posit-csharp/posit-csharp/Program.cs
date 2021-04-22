using System;
using Unum;

namespace posit_csharp
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            BitLattice bitLattice = new BitLattice(32);

            bitLattice.AddField("test", 12, 8);

            bitLattice.SetUInt("test", 123);

            Console.WriteLine(bitLattice.GetUInt("test"));
            Console.Write(bitLattice);
            */

            float x = 0.15625f;
            BitLattice bitLattice = new BitLattice(BitConverter.GetBytes(x));
            bitLattice.AddField("S", 31, 1);
            bitLattice.AddField("Exponent", 23, 8);
            bitLattice.AddField("Fraction", 0, 23);

            Console.WriteLine(bitLattice);
            Console.WriteLine();

            Posit p = new Posit(32, 8);
            p.IntSign = 1;
            p.regime = 4;
            p.exponent = 17;
            p.fraction = 123;
            Console.WriteLine(p.Encode());
        }
    }
}
