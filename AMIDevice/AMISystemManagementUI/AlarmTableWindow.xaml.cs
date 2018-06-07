using System;
using System.Collections.Generic;
using System.Windows;

namespace AMISystemManagementUI
{
    /// <summary>
    /// Interaction logic for AlarmTableWindow.xaml
    /// </summary>
    public partial class AlarmTableWindow : Window
    {
        public List<Tuple<DateTimeOffset, double>> Measurement { get; set; }
        public AlarmTableWindow(List<Tuple<DateTimeOffset,double>> measurement)
        {
            Measurement = measurement;
            DataContext = this;
            InitializeComponent();
        }
    }
}
