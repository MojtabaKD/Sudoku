using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sudoku
{
    public class SudokuClass
    {
        public int[,] curr_cells = new int[9,9];
        public int[,] solv_cells = new int[9,9];
        public int[,] table = new int[9,9];
        public Int64 tries = 1;
        public bool good_sudoku = false;
        private static readonly Random x = new Random();
        public enum Next
        {
            undefinded,
            up,
            down,
            left,
            right
        }
        public struct ON
        {
            public bool on;
            public int i;
            public int j;
        }

        public SudokuClass()
        {
            while (!good_sudoku)
            {
                solv_cells = SudokuGenerator();
                table = solv_cells;
                tries++;
            }
        }

        public int[,] SudokuGenerator()
        {
            int[,] cells = new int[9,9];

            good_sudoku = true;

            for (int i = 0; i < 9; i++)
            {
                for (int j=0; j < 9; j++)
                {
                    HashSet<int> digs = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                    HashSet<int> used;

                    used = UsedValues(cells, i, j);

                    HashSet<int> available_set = new HashSet<int>(digs);

                    available_set.ExceptWith(used);

                    List<int> available_list = new List<int>(available_set.ToList());

                    if (available_list.Count > 0)
                    {
                        cells[i, j] = available_list[x.Next(available_list.Count())];
                    }
                    else
                    {
                        good_sudoku = false;
                        return cells;
                    }
                }
            }

            return cells;
        }

        public HashSet<int> UsedValues(int[,] table, int curr_i, int curr_j)
        {
            List<int> used_col = new List<int>();
            List<int> used_row = new List<int>();
            List<int> used_sqr = new List<int>();

            int sqr_i;
            int sqr_j;

            HashSet<int> result = new HashSet<int>();

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (table[i, j] != 0)
                    {
                        if (j == curr_j)
                        {
                            used_col.Add(table[i, j]);
                        }
                        if (i == curr_i)
                        {
                            used_row.Add(table[i, j]);
                        }
                    }
                }
            }

            sqr_i = curr_i / 3;
            sqr_j = curr_j / 3;

            for (int ii = sqr_i * 3; ii < (sqr_i + 1) * 3; ii++)
            {
                for (int jj = sqr_j * 3; jj < (sqr_j + 1) * 3; jj++)
                {
                    if (table[ii, jj] != 0)
                    {
                        used_sqr.Add(table[ii, jj]);
                    }
                }
            }

            result.UnionWith(new HashSet<int>(used_row));
            result.UnionWith(new HashSet<int>(used_col));
            result.UnionWith(new HashSet<int>(used_sqr));

            return result;
        }

        public int[,] Scan_All(int[,] table)
        {
            HashSet<int> digs;
            HashSet<int> used;
            HashSet<int> remaining;

            int[,] tmp = new int[9, 9];

            Array.Copy(table, tmp, 81);

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (table[i, j] == 0)
                    {
                        digs = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

                        used = UsedValues(tmp, i, j);
                        remaining = digs;
                        remaining.ExceptWith(used);

                        if (remaining.Count == 1)
                        {
                            tmp[i, j] = remaining.ToArray()[0];
                        }
                    }
                }
            }

            return tmp;
        }

        public int[,] Unsolver(int[,] in_tbl, bool hardest=false)
        {
            int[,] test;
            int[,] tmp;
            int[,] solvable = new int[9,9];
            
            test = in_tbl;
            tmp = in_tbl;

            bool unsolved = true;

            List<Tuple<int, int>> rem_itms = new List<Tuple<int, int>>();

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (in_tbl[i, j] != 0)
                    {
                        rem_itms.Add(Tuple.Create(i, j));
                    }
                }
            }

            while (rem_itms.Count() > 0)
            {
                Tuple<int,int> int_tup = rem_itms[x.Next(rem_itms.Count())];
                rem_itms.Remove(int_tup);

                int i = int_tup.Item1;
                int j = int_tup.Item2;

                Array.Copy(test, solvable, 81);

                test[i, j] = 0;

                Array.Copy(test, tmp, 81);

                do
                {
                    tmp = Scan_All(tmp);
                }
                while (!eq_tbls(tmp, Scan_All(tmp)));

                unsolved = Unfilled(tmp);

                if ((unsolved) && (!hardest))
                {
                    return solvable;
                }
                else if ((unsolved) && (hardest))
                {
                    Array.Copy(solvable, test, 81);
                }
            }

            return solvable;
        }

        public bool Unfilled(int[,] tbl)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (tbl[i, j] == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool eq_tbls(int[,] a, int[,] b)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (a[i,j] != b[i,j])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool Solved(int[,] curr_tbl)
        {
            if (Unfilled(curr_tbl))
            {
                return false;
            }

            HashSet<int> container;

            for (int i = 0; i < 9; i++)
            {
                container = new HashSet<int>();

                for (int j = 0; j < 9; j++)
                {
                    container.Add(curr_tbl[i,j]);
                }

                if (container.Count() < 9)
                {
                    return false;
                }
            }

            for (int j = 0; j < 9; j++)
            {
                container = new HashSet<int>();

                for (int i = 0; i < 9; i++)
                {
                    container.Add(curr_tbl[i, j]);
                }

                if (container.Count() < 9)
                {
                    return false;
                }
            }

            for (int k = 0; k < 9; k++)
            {
                container = new HashSet<int>();

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        container.Add(curr_tbl[(k/3)*3 + i, (k%3)*3 + j]);
                    }
                }

                if (container.Count() < 9)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
