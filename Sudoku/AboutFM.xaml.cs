using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sudoku
{
    public partial class AboutFM : Window
    {
        public AboutFM()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            FlowDocument flow = new FlowDocument();

            Paragraph par = new Paragraph();
            par.Inlines.Add("Hi, this is a relatively simple Sudoko 1.1.1 game.");
            par.Inlines.Add(" You can find the source and binary(-ies) here:");

            Hyperlink link = new Hyperlink();
            link.Inlines.Add("https://github.com/MojtabaKD/Sudoku");
            link.NavigateUri = new Uri("https://github.com/MojtabaKD/Sudoku");
            link.RequestNavigate += Hyperlink_RequestNavigate;

            par.Inlines.Add("\n");
            par.Inlines.Add("\n");
            par.Inlines.Add((Hyperlink)link);

            flow.Blocks.Add(par);

            MainRTB.Document = flow;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) {UseShellExecute = true,});
            e.Handled = true;
        }
    }
}
