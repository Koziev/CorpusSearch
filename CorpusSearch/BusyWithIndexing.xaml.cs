﻿using System;
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
    /// Interaction logic for BusyWithIndexing.xaml
    /// </summary>
    public partial class BusyWithIndexing : Window
    {
        public BusyWithIndexing(Indexation_ViewModel data_context )
        {
            InitializeComponent();
            DataContext = data_context;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            ((Indexation_ViewModel)DataContext).AbortIndexationWorker();
        }
    }
}
