﻿using System;
using System.Collections.Generic;

namespace Posit
{
    public class BitLattice
    {
        public struct Field
        {
            public int position;
            public int length;

            public Field(uint position, uint length)
            {
                this.position = (int)position;
                this.length = (int)length;
            }
        }

        private byte[] data;
        private uint size;
        private Dictionary<string, Field> fields;

        public BitLattice(uint size)
        {
            this.size = size;
            data = new byte[(size + 8 - (size - 1) % 8 - 1)/8]; // least number of bytes that accomodate required (size) bits
            fields = new Dictionary<string, Field>();
        }

        public bool this[int i]
        {
            get
            {
                int di = getDataIndex(i);
                byte mask = getBitMask(i);
                return (data[di] & mask) > 0;
            }
            set
            {
                int di = getDataIndex(i);
                byte mask = getBitMask(i);
                if (value)
                    data[di] |= mask;
                else
                    data[di] &= (byte)(~mask);
            }
        }

        public void AddField(string name, uint position, uint length)
        {
            if (position + length <= size)
                fields[name] = new Field(position, length);
            else
                throw new System.IndexOutOfRangeException(string.Format("Field {0} [{1}..{2}] does not fit to the lattice.", name, position, position+length-1));
        }

        private int getDataIndex(int pos)
        {
            return pos / 8;
        }

        private byte getBitMask(int pos)
        {
            return (byte)(1 << (pos % 8));
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = (int)size-1; i >= 0; --i)
                sb.Append(i + "  " + (i>9 ? "" : " "));
            sb.Append("\n");
            for (int i = (int)size - 1; i >= 0; --i)
                sb.Append(this[i] ? "1   " : "0   ");
            sb.Append("\n");

            int tab = 4;
            //string f = new string(' ', 5 * (int)size);
            char[] f = new char[tab * (int)size];
            for (int i = 0; i < f.Length; ++i)
                f[i] = ' ';


            //
            //   L2S(bit_i) = (size-1-bit_i)*tab
            //   S2L(char_i) = 
            //

            

            foreach (string fname in fields.Keys)
            {
                /*
                for (int i = 0; i < fields[fname].length; ++i)
                {
                    int bit_i = fields[fname].position + i;
                    int char_i = ((int)size - 1 - bit_i) * tab;
                    f[char_i] = (i < fname.Length ? fname[i] : '-');
                }
                */

                int bit_i_0 = fields[fname].position;
                int bit_i_1 = fields[fname].position + fields[fname].length - 1;
                int char_i_0 = ((int)size - 1 - bit_i_0) * tab;
                int char_i_1 = ((int)size - 1 - bit_i_1) * tab;

                int ci0 = Math.Min(char_i_0, char_i_1);
                int ci1 = Math.Max(char_i_0, char_i_1);

                for (int ci = ci0; ci <= ci1; ++ci)
                {
                    if (ci == ci0)
                        f[ci] = '[';
                    else if (ci == ci1)
                        f[ci] = ']';
                    else if (ci >= ci0 + 2 && ci < ci0 + 2 + fname.Length)
                        f[ci] = fname[ci - ci0 - 2];
                    else
                        f[ci] = '-';
                }

            }

            sb.Append(new string(f));
            return sb.ToString();
        }

        public void SetBool(string fieldName, bool value)
        {
            this[fields[fieldName].position] = value;
        }

        public bool GetBool(string fieldName)
        {
            return this[fields[fieldName].position];
        }

        public void SetUInt(string fieldName, uint value)
        {
            uint svalue = value;
            uint firstBitMask = 1;

            for (int i = 0; i < fields[fieldName].length; ++i)
            {
                int bitpos = fields[fieldName].position + i;
                this[bitpos] = (svalue & firstBitMask) > 0;
                svalue >>= 1;
            }
        }

        public uint GetUInt(string fieldName)
        {
            uint svalue = 0;
            uint firstBitMask = 1;

            for (int i = 0; i < fields[fieldName].length; ++i)
            {
                svalue <<= 1;
                int bitpos = fields[fieldName].position + fields[fieldName].length - i - 1;
                if (this[bitpos])
                    svalue |= firstBitMask;
            }

            return svalue;
        }

        public void SetULong(string fieldName, ulong value)
        {
            ulong svalue = value;
            ulong firstBitMask = 1;

            for (int i = 0; i < fields[fieldName].length; ++i)
            {
                int bitpos = fields[fieldName].position + i;
                this[bitpos] = (svalue & firstBitMask) > 0;
                svalue >>= 1;
            }
        }

        public ulong GetULong(string fieldName)
        {
            ulong svalue = 0;
            ulong firstBitMask = 1;

            for (int i = 0; i < fields[fieldName].length; ++i)
            {
                svalue <<= 1;
                int bitpos = fields[fieldName].position + fields[fieldName].length - i - 1;
                if (this[bitpos])
                    svalue |= firstBitMask;
            }

            return svalue;
        }
    }
}