using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Sudoku.SudokuClass;

namespace Sudoku
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SudokuClass sudoku;
        SudokuClass.ON on;
        List<int[,]> Undo_Cache;
        int undo_ind;
        bool clear = false;

        public MainWindow()
        {
            InitializeComponent();
            MainFM.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ShowMainLines();
            on.on = false;
            Undo_Cache = new List<int[,]>();
        }

        public void ShowTable(int[,] table)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (table[i, j] != 0)
                    {
                        TextBlock num = new TextBlock();

                        num = new TextBlock();
                        num.FontSize = 26;
                        num.Text = Convert.ToString(table[i, j]);

                        Canvas.SetTop(num, i * 40);
                        Canvas.SetLeft(num, j * 40 + 10);

                        mainCanvas.Children.Add(num);
                    }
                }
            }

            ShowMainLines();
        }

        private void mainCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            mainCanvas.Background = Brushes.Transparent;
        }

        private void ShowRibbon(int curr_i, int curr_j)
        {
            if ((sudoku != null) && (Hint_cb.IsChecked == true))
            {
                if (sudoku.table[curr_i, curr_j] == 0)
                {
                    ribbonCanvas.Children.Clear();

                    HashSet<int> digs = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                    digs.ExceptWith(sudoku.UsedValues(sudoku.table, curr_i, curr_j));
                    List<int> rem = new List<int>();
                    rem.AddRange(digs);
                    rem.Sort();

                    for (int i = 0; i < digs.Count(); i++)
                    {
                        TextBlock num = new TextBlock();

                        num = new TextBlock();
                        num.FontSize = 26;
                        num.Text = rem[i].ToString();

                        Canvas.SetTop(num, 0);
                        Canvas.SetLeft(num, 40 * i + 10);

                        ribbonCanvas.Children.Add(num);
                    }

                    ShowRibLines(rem.Count());
                }
            }
            else if (Hint_cb.IsChecked == false)
            {
                ribbonCanvas.Children.Clear();
            }
        }

        private void ShowMainLines()
        {
            for (int i = 0; i < 10; i++)
            {
                Line line_h = new Line();
                Line line_v = new Line();
                
                if (i % 3 == 0)
                {
                    line_h.Stroke = Brushes.Black;
                    line_h.StrokeThickness = 2;
                }
                else
                {
                    line_h.Stroke = Brushes.Gray;
                    line_h.StrokeThickness = 1;
                }

                line_h.X1 = 0;
                line_h.Y1 = i * 40;

                line_h.X2 = 360;
                line_h.Y2 = i * 40;

                mainCanvas.Children.Add(line_h);

                line_v.Stroke = line_h.Stroke;
                line_v.StrokeThickness = line_h.StrokeThickness;

                line_v.X1 = i * 40;
                line_v.Y1 = 0;

                line_v.X2 = i * 40;
                line_v.Y2 = 360;

                mainCanvas.Children.Add(line_v);
            }
        }

        private void ShowRibLines(int len)
        {
            for (int i = 0; i <= len; i++)
            {
                Line line_v = new Line();

                line_v.Stroke = Brushes.Gray;
                line_v.StrokeThickness = 1;

                if ((i == 0)  || (i == len))
                {
                    Line line_h = new Line();

                    line_h.Stroke = Brushes.Black;
                    line_h.StrokeThickness = 2;

                    line_h.X1 = 0;
                    line_h.Y1 = (i / len) * 40;

                    line_h.X2 = len * 40;
                    line_h.Y2 = (i / len) * 40;

                    ribbonCanvas.Children.Add(line_h);

                    line_v.Stroke = Brushes.Black;
                    line_v.StrokeThickness = 2;
                }

                line_v.X1 = i * 40;
                line_v.Y1 = 0;

                line_v.X2 = i * 40;
                line_v.Y2 = 40;

                ribbonCanvas.Children.Add(line_v);
            }
        }

        private void TurnSquareOn(int sqr_i, int sqr_j, SolidColorBrush color)
        {
            on.on = true;

            System.Windows.Shapes.Polygon sqr = new System.Windows.Shapes.Polygon();
            sqr.Fill = color;

            int sqr_x = sqr_j * 40 + 1;
            int sqr_y = sqr_i * 40 + 1;

            sqr.Points.Add(new System.Windows.Point(sqr_x, sqr_y));
            sqr.Points.Add(new System.Windows.Point(sqr_x + 38, sqr_y));
            sqr.Points.Add(new System.Windows.Point(sqr_x + 38, sqr_y + 38));
            sqr.Points.Add(new System.Windows.Point(sqr_x, sqr_y + 38));

            mainCanvas.Children.Add(sqr);
        }

        private void MainFM_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            if ((!on.on) && (sudoku != null) &&
                ((e.Key == Key.Up) || (e.Key == Key.Down) ||
                 (e.Key == Key.Left) || (e.Key == Key.Right)))
            {
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if ((!on.on) && (sudoku.table[i,j] == 0))
                        {
                            TurnSquareOn(i,j, System.Windows.Media.Brushes.Yellow);
                            on.on = true;
                            on.i = i;
                            on.j = j;
                        }
                    }
                }
            }
            else if ((on.on) && (sudoku != null) &&
                    ((e.Key == Key.Up) || (e.Key == Key.Down) ||
                     (e.Key == Key.Left) || (e.Key == Key.Right)))
            {
                int[] next_sqr;
                SudokuClass.Next nxt = SudokuClass.Next.undefinded;

                switch (e.Key)
                {
                    case Key.Up:
                        {
                            nxt = SudokuClass.Next.up;
                            break;
                        }
                    case Key.Down:
                        {
                            nxt = SudokuClass.Next.down;
                            break;
                        }
                    case Key.Left:
                        {
                            nxt = SudokuClass.Next.left;
                            break;
                        }
                    case Key.Right:
                        {
                            nxt = SudokuClass.Next.right;
                            break;
                        }
                }

                if (nxt != SudokuClass.Next.undefinded)
                {
                    next_sqr = sudoku.NextZero(nxt, on.i, on.j);
                    if ((next_sqr[0] != 9) && (next_sqr[1] != 9))
                    {
                        mainCanvas.Children.Clear();
                        TurnSquareOn(next_sqr[0], next_sqr[1], System.Windows.Media.Brushes.Yellow);
                        ShowTable(sudoku.table);
                        ShowRibbon(next_sqr[0], next_sqr[1]);
                        on.i = next_sqr[0];
                        on.j = next_sqr[1];
                    }
                }
            }

            int keynum;
            IoCmd_t kc = new IoCmd_t();
            KeyToChar(e.Key, ref kc);

            if ((on.on) && (sudoku != null) &&
                (int.TryParse(kc.character.ToString(), out keynum)))
            {
                HashSet<int> digs = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                digs.ExceptWith(sudoku.UsedValues(sudoku.table, on.i, on.j));

                sudoku.table[on.i, on.j] = keynum;
                mainCanvas.Children.Clear();

                if (Hint_cb.IsChecked == true)
                {
                    if (!digs.Contains(keynum))
                    {
                        TurnSquareOn(on.i, on.j, System.Windows.Media.Brushes.Red);
                    }
                    else
                    {
                        TurnSquareOn(on.i, on.j, System.Windows.Media.Brushes.LightSkyBlue);
                    }
                }
                
                Undo_Redo(sudoku.table);
                ShowTable(sudoku.table);

                if (!sudoku.Unfilled(sudoku.table))
                {
                    Check_btn_Click(sender, e);
                }
            }
        }

        private void mainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {

            System.Windows.Point p = e.GetPosition(mainCanvas);

            int x = (int)p.X;
            int y = (int)p.Y;

            if ((x <= 360) && (y <= 360) && (sudoku != null))
            {
                if (sudoku != null)
                {
                    if (sudoku.table[y / 40, x / 40] == 0)
                    {
                        mainCanvas.Children.Clear();
                        TurnSquareOn(y / 40, x / 40, System.Windows.Media.Brushes.Yellow);
                        ShowTable(sudoku.table);
                        on.on = true;
                        on.i = y / 40;
                        on.j = x / 40;
                        ShowRibbon(y / 40, x / 40);
                    }
                    else if ((sudoku.table[y / 40, x / 40] != 0) && (clear))
                    {
                        sudoku.table[y / 40, x / 40] = 0;
                        Undo_Redo(sudoku.table);
                        mainCanvas.Children.Clear();
                        ShowTable(sudoku.table);
                    }
                    else if ((sudoku.table[y / 40, x / 40] != 0) && (!clear))
                    {
                        mainCanvas.Children.Clear();

                        for (int i = 0; i < 9; i++)
                        {
                            for (int j = 0; j < 9; j++)
                            {
                                if (sudoku.table[i, j] == sudoku.table[y / 40, x / 40])
                                {
                                    TurnSquareOn(i, j, System.Windows.Media.Brushes.LawnGreen);
                                }
                            }
                        }

                        ShowTable(sudoku.table);
                    }
                }
            }
        }

        private void Create_btn_Click(object sender, RoutedEventArgs e)
        {
            sudoku = new SudokuClass();

            listBox1.Items.Add($"{listBox1.Items.Count:D3}-Create tries = {sudoku.tries}");
            listBox1.ScrollIntoView(listBox1.Items[listBox1.Items.Count - 1]);

            sudoku.table = sudoku.Unsolver(sudoku.table);
            Undo_Redo(sudoku.table);

            mainCanvas.Children.Clear();
            ShowTable(sudoku.table);
        }

        private void Harder_btn_Click(object sender, RoutedEventArgs e)
        {
            if (sudoku != null)
            {
                sudoku.table = sudoku.Unsolver(sudoku.table);
                Undo_Redo(sudoku.table);

                mainCanvas.Children.Clear();
                ShowTable(sudoku.table);
            }
        }

        private void Solve_btn_Click(object sender, RoutedEventArgs e)
        {
            int sol_tries = 0;

            int[,] scn = new int[9, 9];

            scn = sudoku.Scan_All(sudoku.table);

            while (!SudokuClass.eq_tbls(scn, sudoku.table))
            {
                Array.Copy(sudoku.Scan_All(sudoku.table), sudoku.table, 81);
                sol_tries++;
                mainCanvas.Children.Clear();
                ShowTable(sudoku.table);
                scn = sudoku.Scan_All(sudoku.table);
            }

            Undo_Redo(scn);

            listBox1.Items.Add($"{listBox1.Items.Count:D3}-Solution passes = {sol_tries}");
            listBox1.ScrollIntoView(listBox1.Items[listBox1.Items.Count - 1]);
        }

        private void Hardest_btn_Click(object sender, RoutedEventArgs e)
        {
            sudoku = new SudokuClass();

            listBox1.Items.Add($"{listBox1.Items.Count:D3}-Create tries = {sudoku.tries}");
            listBox1.ScrollIntoView(listBox1.Items[listBox1.Items.Count - 1]);

            sudoku.table = sudoku.Unsolver(sudoku.table, true);
            Undo_Redo(sudoku.table);

            mainCanvas.Children.Clear();
            ShowTable(sudoku.table);
        }

        private void Check_btn_Click(object sender, RoutedEventArgs e)
        {
            if (sudoku != null)
            {
                if (sudoku.Solved(sudoku.table))
                {
                    MessageBox.Show("The sudoku has been solved!");
                }
                else
                {
                    MessageBox.Show("It's not solved yet!");
                }
            }
        }

        private void Undo_btn_Click(object sender, RoutedEventArgs e)
        {
            if ((sudoku != null) && (undo_ind > 1))
            {
                undo_ind--;
                sudoku.table = Undo_Cache[undo_ind-1];
                mainCanvas.Children.Clear();
                ShowTable(sudoku.table);
            }
        }

        private void Redo_btn_Click(object sender, RoutedEventArgs e)
        {
            if ((sudoku != null) && (undo_ind < Undo_Cache.Count))
            {
                undo_ind++;
                sudoku.table = Undo_Cache[undo_ind-1];
                mainCanvas.Children.Clear();
                ShowTable(sudoku.table);
            }
        }

        private void Undo_Redo(int[,] tbl)
        {
            int[,] tmp = new int[9, 9];
            Array.Copy(tbl, tmp, 81);

            if (undo_ind < Undo_Cache.Count)
            {
                Undo_Cache = Undo_Cache.GetRange(0, undo_ind + 1);
            }

            Undo_Cache.Add(tmp);
            undo_ind++;
        }

        private void Hint_cb_Unchecked(object sender, RoutedEventArgs e)
        {
            ribbonCanvas.Children.Clear();
        }

        private void Hint_cb_Checked(object sender, RoutedEventArgs e)
        {
            if (on.on)
            {
                ShowRibbon(on.i, on.j);
            }
        }

        private void Clear_btn_Click(object sender, RoutedEventArgs e)
        {
            if (sudoku != null)
            {
                if (clear)
                {
                    clear = false;
                    Clear_btn.ClearValue(BackgroundProperty);
                }
                else
                {
                    clear = true;
                    Clear_btn.Background = Brushes.Red;
                }
            }
        }

        private void AboutItem_Click(object sender, RoutedEventArgs e)
        {
            AboutFM af = new AboutFM();

            af.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            af.Show();
        }

        //Thanx to Bimo ---> https://stackoverflow.com/a/46471739/4452714 for KeyToChar and IoCmd_t

        public struct IoCmd_t
        {
            public Key key;
            public bool printable;
            public char character;
            public bool shift;
            public bool ctrl;
            public bool alt;
            public int type; //sideband
            public string s;    //sideband
        };

        public void KeyToChar(Key key, ref IoCmd_t KeyDecode)
        {
            bool iscap;
            bool caplock;
            bool shift;

            KeyDecode.key = key;

            KeyDecode.alt = Keyboard.IsKeyDown(Key.LeftAlt) ||
                              Keyboard.IsKeyDown(Key.RightAlt);

            KeyDecode.ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) ||
                              Keyboard.IsKeyDown(Key.RightCtrl);

            KeyDecode.shift = Keyboard.IsKeyDown(Key.LeftShift) ||
                              Keyboard.IsKeyDown(Key.RightShift);

            if (KeyDecode.alt || KeyDecode.ctrl)
            {
                KeyDecode.printable = false;
                KeyDecode.type = 1;
            }
            else
            {
                KeyDecode.printable = true;
                KeyDecode.type = 0;
            }

            shift = KeyDecode.shift;
            caplock = Console.CapsLock; //Keyboard.IsKeyToggled(Key.CapsLock);
            iscap = (caplock && !shift) || (!caplock && shift);

            switch (key)
            {
                case Key.Enter: KeyDecode.character = '\n'; return;
                case Key.A: KeyDecode.character = (iscap ? 'A' : 'a'); return;
                case Key.B: KeyDecode.character = (iscap ? 'B' : 'b'); return;
                case Key.C: KeyDecode.character = (iscap ? 'C' : 'c'); return;
                case Key.D: KeyDecode.character = (iscap ? 'D' : 'd'); return;
                case Key.E: KeyDecode.character = (iscap ? 'E' : 'e'); return;
                case Key.F: KeyDecode.character = (iscap ? 'F' : 'f'); return;
                case Key.G: KeyDecode.character = (iscap ? 'G' : 'g'); return;
                case Key.H: KeyDecode.character = (iscap ? 'H' : 'h'); return;
                case Key.I: KeyDecode.character = (iscap ? 'I' : 'i'); return;
                case Key.J: KeyDecode.character = (iscap ? 'J' : 'j'); return;
                case Key.K: KeyDecode.character = (iscap ? 'K' : 'k'); return;
                case Key.L: KeyDecode.character = (iscap ? 'L' : 'l'); return;
                case Key.M: KeyDecode.character = (iscap ? 'M' : 'm'); return;
                case Key.N: KeyDecode.character = (iscap ? 'N' : 'n'); return;
                case Key.O: KeyDecode.character = (iscap ? 'O' : 'o'); return;
                case Key.P: KeyDecode.character = (iscap ? 'P' : 'p'); return;
                case Key.Q: KeyDecode.character = (iscap ? 'Q' : 'q'); return;
                case Key.R: KeyDecode.character = (iscap ? 'R' : 'r'); return;
                case Key.S: KeyDecode.character = (iscap ? 'S' : 's'); return;
                case Key.T: KeyDecode.character = (iscap ? 'T' : 't'); return;
                case Key.U: KeyDecode.character = (iscap ? 'U' : 'u'); return;
                case Key.V: KeyDecode.character = (iscap ? 'V' : 'v'); return;
                case Key.W: KeyDecode.character = (iscap ? 'W' : 'w'); return;
                case Key.X: KeyDecode.character = (iscap ? 'X' : 'x'); return;
                case Key.Y: KeyDecode.character = (iscap ? 'Y' : 'y'); return;
                case Key.Z: KeyDecode.character = (iscap ? 'Z' : 'z'); return;
                case Key.D0: KeyDecode.character = (shift ? ')' : '0'); return;
                case Key.D1: KeyDecode.character = (shift ? '!' : '1'); return;
                case Key.D2: KeyDecode.character = (shift ? '@' : '2'); return;
                case Key.D3: KeyDecode.character = (shift ? '#' : '3'); return;
                case Key.D4: KeyDecode.character = (shift ? '$' : '4'); return;
                case Key.D5: KeyDecode.character = (shift ? '%' : '5'); return;
                case Key.D6: KeyDecode.character = (shift ? '^' : '6'); return;
                case Key.D7: KeyDecode.character = (shift ? '&' : '7'); return;
                case Key.D8: KeyDecode.character = (shift ? '*' : '8'); return;
                case Key.D9: KeyDecode.character = (shift ? '(' : '9'); return;
                case Key.OemPlus: KeyDecode.character = (shift ? '+' : '='); return;
                case Key.OemMinus: KeyDecode.character = (shift ? '_' : '-'); return;
                case Key.OemQuestion: KeyDecode.character = (shift ? '?' : '/'); return;
                case Key.OemComma: KeyDecode.character = (shift ? '<' : ','); return;
                case Key.OemPeriod: KeyDecode.character = (shift ? '>' : '.'); return;
                case Key.OemOpenBrackets: KeyDecode.character = (shift ? '{' : '['); return;
                case Key.OemQuotes: KeyDecode.character = (shift ? '"' : '\''); return;
                case Key.Oem1: KeyDecode.character = (shift ? ':' : ';'); return;
                case Key.Oem3: KeyDecode.character = (shift ? '~' : '`'); return;
                case Key.Oem5: KeyDecode.character = (shift ? '|' : '\\'); return;
                case Key.Oem6: KeyDecode.character = (shift ? '}' : ']'); return;
                case Key.Tab: KeyDecode.character = '\t'; return;
                case Key.Space: KeyDecode.character = ' '; return;

                // Number Pad
                case Key.NumPad0: KeyDecode.character = '0'; return;
                case Key.NumPad1: KeyDecode.character = '1'; return;
                case Key.NumPad2: KeyDecode.character = '2'; return;
                case Key.NumPad3: KeyDecode.character = '3'; return;
                case Key.NumPad4: KeyDecode.character = '4'; return;
                case Key.NumPad5: KeyDecode.character = '5'; return;
                case Key.NumPad6: KeyDecode.character = '6'; return;
                case Key.NumPad7: KeyDecode.character = '7'; return;
                case Key.NumPad8: KeyDecode.character = '8'; return;
                case Key.NumPad9: KeyDecode.character = '9'; return;
                case Key.Subtract: KeyDecode.character = '-'; return;
                case Key.Add: KeyDecode.character = '+'; return;
                case Key.Decimal: KeyDecode.character = '.'; return;
                case Key.Divide: KeyDecode.character = '/'; return;
                case Key.Multiply: KeyDecode.character = '*'; return;

                default:
                    KeyDecode.type = 1;
                    KeyDecode.printable = false;
                    KeyDecode.character = '\x00';
                    return;
            }
        }
    }
}
