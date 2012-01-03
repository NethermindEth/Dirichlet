﻿using System.Collections;
using System.Collections.Generic;
using Word = System.Int32;

namespace Decompose.Numerics
{
    public class Word32BitMatrix : IBitMatrix
    {
        private const int wordShift = 5;
        private const int wordLength = 1 << wordShift;
        private const int wordMask = wordLength - 1;

        private int rows;
        private int cols;
        private int words;
        private Word[][] bits;

        public int Rows
        {
            get { return rows; }
        }

        public int Cols
        {
            get { return cols; }
        }

        public Word32BitMatrix(int rows, int cols)
        {
            this.rows = rows;
            this.cols = cols;
            words = (cols + wordLength - 1) / wordLength;
            bits = new Word[rows][];
            for (int i = 0; i < rows; i++)
                bits[i] = new Word[words];
        }

        public bool this[int i, int j]
        {
            get
            {
                return (bits[i][j >> wordShift] & (Word)1 << (j & wordMask)) != 0;
            }
            set
            {
                if (value)
                    bits[i][j >> wordShift] |= (Word)1 << (j & wordMask);
                else
                    bits[i][j >> wordShift] &= ~((Word)1 << (j & wordMask));
            }
        }

        public void XorRows(int dst, int src)
        {
            var dstRow = bits[dst];
            var srcRow = bits[src];
            for (int j = 0; j < words; j++)
                dstRow[j] ^= srcRow[j];
        }

        public bool IsRowEmpty(int i)
        {
            var row = bits[i];
            for (int j = 0; j < words; j++)
            {
                if (row[j] != 0)
                    return false;
            }
            return true;
        }

        public void RemoveRow(int i)
        {
            for (int row = i + 1; row < rows; row++)
                bits[row - 1] = bits[row];
            --rows;
        }
    }
}