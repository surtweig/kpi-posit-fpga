using System;
using Posit;

namespace posit_csharp
{
    class Program
    {
        static void Main(string[] args)
        {
            BitLattice bitLattice = new BitLattice(32);

            bitLattice.AddField("test", 12, 8);

            bitLattice.SetUInt("test", 123);

            Console.WriteLine(bitLattice.GetUInt("test"));
            Console.Write(bitLattice);
        }
    }
}
