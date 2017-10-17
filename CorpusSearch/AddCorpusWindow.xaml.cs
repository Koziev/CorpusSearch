using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace CorpusSearch
{
    /// <summary>
    /// Interaction logic for AddCorpusWindow.xaml
    /// </summary>
    public partial class AddCorpusWindow : Window
    {
        public AddCorpusWindow()
        {
            InitializeComponent();
            DataContext = new AddCorpus_ViewModel();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btnChooseFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                tbtxtFile.Text = openFileDialog.FileName;
            }
        }
    }
}
