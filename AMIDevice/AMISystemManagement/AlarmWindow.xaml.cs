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

namespace AMISystemManagement
{
    /// <summary>
    /// Interaction logic for AlarmWindow.xaml
    /// </summary>
    public partial class AlarmWindow : Window
    {
        public AlarmWindow()
        {
            InitializeComponent();
            List<string> list = new List<string>() { "Voltage", "Current", "ActivePower", "ReactivePower" };
            comboBoxMeasurementType.ItemsSource = list;
        }

        private void buttonDrawAlarm_Click(object sender, RoutedEventArgs e)
        {
            if(Validate())
            {
                //do work: enlist alarmStates
            }
        }
        private bool Validate()
        {
            ValidateDate(StartDateTextBox, labelStartDateError);
            ValidateDate(EndDateTextBox, labelEndDateError);
            if(ValueTextBox.Text.Trim().Equals(""))
            {
                labelValueError.Content = "Enter the value.";
            }
            else
            {
                labelValueError.Content = "";
            }
            double val;   
            if(!double.TryParse(ValueTextBox.Text, out val))
            {
                //moze da izabere negativnu vrednost za alarm
                labelValueError.Content = "Invalid value.";
            }
            else
            {
                labelValueError.Content = "";
            }
            return true;
        }

        private bool ValidateDate(TextBox textBoxDate, Label labelDateError)
        {
            if (textBoxDate.Text.Trim().Equals("") || textBoxDate.Text.Trim().Equals(".."))
            {
                labelDateError.Content = "Enter the date.";
                return false;
            }
            else
            {
                labelDateError.Content = "";
            }
            if (!textBoxDate.Text.Contains('.'))
            {
                labelDateError.Content = "Date format is invalid. 1";
                return false;
            }
            else
            {
                labelDateError.Content = "";
            }
            string[] date = textBoxDate.Text.Split('.');
            if (date.Length != 3)
            {
                labelDateError.Content = "Date format is invalid. kk";
                return false;
            }
            else
            {
                labelDateError.Content = "";
            }
            int[] dates = new int[3];
            for (int i = 0; i < date.Length; i++)    //proveri da li je unesen broj/karakter
            {
                if (!int.TryParse(date[i], out dates[i]))
                    labelDateError.Content = "Date format is invalid.";
                else
                {
                    labelDateError.Content = "";
                }
            }
            if (int.Parse(date[0]) > 31 || int.Parse(date[0]) < 1 ||
                int.Parse(date[1]) > 12 || int.Parse(date[1]) < 1 ||
                int.Parse(date[2]) < 2018 ||
                int.Parse(date[1]) == 2 && int.Parse(date[0]) > 28)
            {
                labelDateError.Content = "Entered date does not exist.";
                return false;
            }
            else
            {
                labelDateError.Content = "";
            }

            return true;
        }
    }
}
