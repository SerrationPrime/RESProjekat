using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace AMISystemManagementUI
{
    /// <summary>
    /// Interaction logic for AlarmWindow.xaml
    /// </summary>
    public partial class AlarmWindow : Window
    {
        private string Filename = "globalStorage.xml";
        private bool IsGreaterLimit = false;    //onda hoces da gledas manje vrednosti
       
        private List<Tuple<DateTimeOffset, double>> measurement = new List<Tuple<DateTimeOffset, double>>();


        public AlarmWindow()
        {
            InitializeComponent();
            List<string> list = new List<string>() { "Voltage", "Current", "ActivePower", "ReactivePower" };
            comboBoxMeasurementType.ItemsSource = list;
        }

        public DateTime GetDate(TextBox dateTextBox)
        {
            string[] dateTime = dateTextBox.Text.Trim().Split('.');
            DateTime date = new DateTime(Int32.Parse(dateTime[2]), Int32.Parse(dateTime[1]), Int32.Parse(dateTime[0]));
            return date;
        }

        /// <summary>
        /// Poziv alarma: Sva logika vezana za citanje podataka se vrši ovde, i rezultujuci model se prosleđuje na AlarmTableWindow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonDrawAlarm_Click(object sender, RoutedEventArgs e)
        {
           
            if (Validate())
            {
                long minTime = DateHelper.StartOfDay(GetDate(StartDateTextBox));
                long maxTime = DateHelper.EndOfDay(GetDate(EndDateTextBox));

                string type = comboBoxMeasurementType.SelectedItem.ToString();
                double zadataVrednost = Double.Parse(ValueTextBox.Text.Trim());
                using (XmlReader reader = XmlReader.Create(Filename))
                {
                    while (reader.Read())
                    {
                        switch (reader.Name)
                        {
                            case ("Timestamp"):
                                if (reader.NodeType == XmlNodeType.EndElement)
                                    break;
                                reader.Read();
                                if (long.Parse(reader.Value) > minTime && long.Parse(reader.Value) < maxTime)//ako je timestamp u granicama izmedu pocetnog i krajnjeg datuma
                                {
                                    var time = (long.Parse(reader.Value));
                                    while (reader.Read())
                                    {
                                        if (reader.Name == "Type")
                                        {
                                            reader.Read();
                                            if (reader.Value == type)
                                            {
                                                reader.ReadToFollowing("Value");
                                                reader.Read();
                                                var value = (Double.Parse(reader.Value));
                                                if (IsGreaterLimit && value > zadataVrednost || !IsGreaterLimit && value < zadataVrednost)
                                                {
                                                    measurement.Add(new Tuple<DateTimeOffset, double>(DateTimeOffset.FromUnixTimeSeconds(time), value));
                                                    break;
                                                }
                                                break;
                                            }
                                        }
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                var AlarmTableWindow = new AlarmTableWindow(measurement);
                AlarmTableWindow.ShowDialog();
            }

            else return;
        }
    private bool Validate()
    {
        if (!ValidateDate(StartDateTextBox, labelStartDateError))
                return false;
            if (!ValidateDate(EndDateTextBox, labelEndDateError))
                return false;
        if (ValueTextBox.Text.Trim().Equals(""))
        {
            labelValueError.Content = "Enter the value.";
                return false;
        }
        else
        {
            labelValueError.Content = "";
        }
        double val;
        if (!double.TryParse(ValueTextBox.Text, out val))
        {
            //moze da izabere negativnu vrednost za alarm
            labelValueError.Content = "Invalid value.";
            return false;
        }
        else
        {
            labelValueError.Content = "";
        }

        if (RadioButtonLowerLimit.IsChecked == true)
        {
            IsGreaterLimit = false;
        }
        else
        {
            IsGreaterLimit = true;
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
            labelDateError.Content = "Date format is invalid. ";
            return false;
        }
        else
        {
            labelDateError.Content = "";
        }
        string[] date = textBoxDate.Text.Split('.');
        if (date.Length != 3)
        {
            labelDateError.Content = "Date format is invalid. ";
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

