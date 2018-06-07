using AMICommons;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Xml;

namespace AMISystemManagementUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ServiceHost SystemManagementHost;

        public PlotModel Model { get; private set; }

        public string Filename = "globalStorage.xml";
        public MainWindow()
        {
            InitializeComponent();
            List<string> list = new List<string>() { "Voltage", "Current", "ActivePower", "ReactivePower" };
            comboBoxMeasurementType.ItemsSource = list;
            StartAggregatorService();
        }

        private void StartAggregatorService()
        {
            string SystemManagementPath = String.Format("net.tcp://localhost:{0}/{1}", AggregatorMessage.SysPort, AggregatorMessage.SysEndpointName);
            SystemManagementHost = new ServiceHost(typeof(SystemManagementManager));

            var binding = new NetTcpBinding();
            binding.MaxBufferSize = 20000000;
            binding.MaxBufferPoolSize = 20000000;
            binding.MaxReceivedMessageSize = 20000000;

            SystemManagementHost.AddServiceEndpoint(typeof(IMessageForSystemManagement), binding, SystemManagementPath);
            SystemManagementHost.Open();
        }


        private void buttonDraw1_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateDeviceMeasurement())
                return;

            DateTime date = DateHelper.ParseDate(textBoxDate.Text);

            string deviceCode = textBoxDeviceCode.Text.Trim();

            long minTime = DateHelper.StartOfDay(date);
            long maxTime = DateHelper.EndOfDay(date);

            string type = comboBoxMeasurementType.SelectedItem.ToString(); List<int> vremena = new List<int>();
            List<int> vrednosti = new List<int>();

            var GraphSeries = new LineSeries { Title = type, MarkerType = MarkerType.Circle };
            var Plot = new PlotModel { Title = "" };
            Plot.Axes.Add(new DateTimeAxis { Position = AxisPosition.Bottom, StringFormat = "hh/mm" });

            using (XmlReader reader = XmlReader.Create(Filename))
            {
                while (reader.Read())
                {
                    switch (reader.Name)
                    {
                        case ("DeviceCode"):
                            if (reader.NodeType == XmlNodeType.EndElement)
                                break;
                            if (reader.GetAttribute("value") == deviceCode)
                            {
                                while (reader.Read() && reader.Name != "DeviceCode")
                                {
                                    if (reader.Name == "Timestamp" && reader.NodeType != XmlNodeType.EndElement)
                                    {
                                        reader.Read();
                                        if (long.Parse(reader.Value) >= minTime && long.Parse(reader.Value) <= maxTime)
                                        {
                                            var time = DateHelper.TimeStampToOxyTime(long.Parse(reader.Value));
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
                                                        GraphSeries.Points.Add(new DataPoint(time, value));
                                                        break;
                                                    }
                                                }
                                            }
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

            Plot.Series.Add(SortPointsInSeries(GraphSeries));

            PlotWindow window = new PlotWindow(Plot);
            window.ShowDialog();
        }

        private void buttonDraw2_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateAggregatorMeasurement())
                return;
            Dictionary<long, List<double>> desiredMeasurements = GenericAggregatorPlotPrep();
            string type = comboBoxMeasurementType.SelectedItem.ToString(); List<int> vremena = new List<int>();

            var GraphSeries = new LineSeries { Title = type, MarkerType = MarkerType.Circle };
            var Plot = new PlotModel { Title = "" };
            Plot.Axes.Add(new DateTimeAxis { Position = AxisPosition.Bottom, StringFormat = "hh/mm" });
            foreach (var pair in desiredMeasurements)
            {
                var time = DateHelper.TimeStampToOxyTime(pair.Key);
                var value = pair.Value.Sum();
                GraphSeries.Points.Add(new DataPoint(time, value));
            }

            Plot.Series.Add(SortPointsInSeries(GraphSeries));

            PlotWindow window = new PlotWindow(Plot);
            window.ShowDialog();
        }

        private void buttonDraw4_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateAverageAggregatorMeasurement())
                return;
            Dictionary<long, List<double>> desiredMeasurements = GenericAggregatorPlotPrep();
            string type = comboBoxMeasurementType.SelectedItem.ToString(); List<int> vremena = new List<int>();

            var GraphSeries = new LineSeries { Title = type, MarkerType = MarkerType.Circle };
            var Plot = new PlotModel { Title = "" };
            Plot.Axes.Add(new DateTimeAxis { Position = AxisPosition.Bottom, StringFormat = "hh/mm" });
            foreach (var pair in desiredMeasurements)
            {
                var time = DateHelper.TimeStampToOxyTime(pair.Key);
                var value = pair.Value.Average();
                GraphSeries.Points.Add(new DataPoint(time, value));
            }

            Plot.Series.Add(SortPointsInSeries(GraphSeries));

            PlotWindow window = new PlotWindow(Plot);
            window.ShowDialog();
        }

        Dictionary<long,List<double>> GenericAggregatorPlotPrep()
        {
            DateTime date = DateHelper.ParseDate(textBoxDate.Text);

            string aggregatorCode = textBoxAgregatorCode.Text.Trim();

            long minTime = DateHelper.StartOfDay(date);
            long maxTime = DateHelper.EndOfDay(date);

            string type = comboBoxMeasurementType.SelectedItem.ToString(); List<int> vremena = new List<int>();
            List<int> vrednosti = new List<int>();

            Dictionary<long, List<double>> desiredMeasurements = new Dictionary<long, List<double>>();


            using (XmlReader reader = XmlReader.Create(Filename))
            {
                while (reader.Read())
                {
                    if (reader.Name=="AggregatorCode" && reader.GetAttribute("value") == aggregatorCode)
                    {
                        while (reader.Read() && reader.Name != "AggregatorCode")
                        {
                            if (reader.Name == "Timestamp" && reader.NodeType!=XmlNodeType.EndElement)
                            {
                                reader.Read();
                                var currTimestamp = long.Parse(reader.Value);
                                if (currTimestamp >= minTime && currTimestamp <= maxTime)
                                {
                                    if (!desiredMeasurements.ContainsKey(currTimestamp))
                                        desiredMeasurements.Add(currTimestamp, new List<double>());
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
                                                desiredMeasurements[currTimestamp].Add(value);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return desiredMeasurements;
        }

        private void ButtonEnlistAlarm_Click(object sender, RoutedEventArgs e)
        {
            AlarmWindow al = new AlarmWindow();
            al.ShowDialog();
        }
        /// <summary>
        /// validiraju se vrednost koje moraju biti popunjene za svaki vid grafika
        /// u ovom slucaju-datum
        /// </summary>
        /// <returns></returns>
        private bool ValidateCommon()
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

        private bool ValidateDeviceMeasurement()
        {
            ValidateCommon();
            if (textBoxDeviceCode.Text.Trim().Equals(""))
            {
                labelDeviceCodeError.Content = "Enter device code.";
                return false;
            }
            else
            {
                labelDeviceCodeError.Content = "";
            }
            //if(textBoxDeviceCode.Text.Trim().Length != VREDNOST)    //proveri da li postoji u STORAGE-u
            //{                                                       //takodje proveri da li ima slovo umesto broja
            //      labelDeviceCodeError.Content = "Device code is invalid";
            //      return false;
            //}
            //else
            //{
            //    labelDeviceCodeError.Content = "";
            //}
            //if(textBoxDeviceCode.Text.Trim() not exists )     //proveri da li postoji uredjaj sa tim kodom
            //{
            //    labelDeviceCodeError.Content = "Device with that code does not exist.";
            //    return false;
            //} 
            //else
            //{
            //    labelDeviceCodeError.Content = "";
            //}
            if (comboBoxMeasurementType.SelectedItem == null)
            {
                labelMeasurementTypeError.Content = "Select the measurement type.";
                return false;
            }
            else
            {
                labelMeasurementTypeError.Content = "";
            }
            return ValidateCommon();
        }

        private bool ValidateAggregatorMeasurement()
        {
            ValidateCommon();
            if (comboBoxMeasurementType.SelectedItem == null)
            {
                labelMeasurementTypeError.Content = "Select the measurement type.";
                return false;
            }
            else
            {
                labelMeasurementTypeError.Content = "";
            }
            if (textBoxAgregatorCode.Text.Trim().Equals(""))
            {
                labelAgregatorCodeError.Content = "Enter device code.";
                return false;
            }
            else
            {
                labelAgregatorCodeError.Content = "";
            }
            //if(textBoxAgregatorCode.Text.Trim().Length != VREDNOST)    //proveri da li je unesen string u STORAGEU
            //{ 
            //      labelAgregatorCodeError.Content = "Aggregator code is invalid.";
            //      return false;
            //}
            //else
            //{
            //    labelAgregatorCodeError.Content = "";
            //}
            //if(textBoxAgregatorCode.Text.Trim() not exists )     //proveri da li postoji uredjaj sa tim kodom
            //{
            //    labelAgregatorCodeError.Content = "Agregator with that code does not exist.";
            //    return false;
            //} 
            //else
            //{
            //    labelAgregatorCodeError.Content = "";
            //}
            return ValidateCommon();
        }

        private bool ValidateAverageAggregatorMeasurement()
        {
            ValidateCommon();
            if (comboBoxMeasurementType.SelectedItem == null)
            {
                labelMeasurementTypeError.Content = "Select the measurement type.";
                return false;
            }
            else
            {
                labelMeasurementTypeError.Content = "";
            }
            if (textBoxAgregatorCode.Text.Trim().Equals(""))
            {
                labelAgregatorCodeError.Content = "Enter device code.";
                return false;
            }
            else
            {
                labelAgregatorCodeError.Content = "";
            }
            //if(textBoxAgregatorCode.Text.Trim().Length != VREDNOST)    //proveri da li je unesen string u storage-u
            //{ 
            //      labelAgregatorCodeError.Content = "Aggregator code is invalid.";
            //      return false;
            //}
            //else
            //{
            //    labelAgregatorCodeError.Content = "";
            //}
            //if(textBoxAgregatorCode.Text.Trim() not exists )     //proveri da li postoji uredjaj sa tim kodom
            //{
            //    labelAgregatorCodeError.Content = "Agregator with that code does not exist.";
            //    return false;
            //}
            //else
            //{
            //    labelAgregatorCodeError.Content = "";
            //}
            return ValidateCommon();
        }

        LineSeries SortPointsInSeries(LineSeries GraphSeries)
        {
            List<DataPoint> temp = GraphSeries.Points.OrderBy(i => i.X).ToList();
            var GraphSeriesOrdered = new LineSeries();
            GraphSeriesOrdered.ItemsSource = temp;
            return GraphSeriesOrdered;
        }
    }
}
