using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO.Ports;
using System.IO;
using System.Text;

namespace Ground_Control_Software__GCS_
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        StreamWriter swLog;

        Guid RecordingRun;
        int seconds = 0;

        private DispatcherTimer timer;
        private SerialPort MyCOM4;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (StartStopBtn.Content.ToString() == "Start")
            {
                seconds = 0;
                RecordingRun = Guid.NewGuid();

                //
                // Create a log file
                //
                string strLogFile = "c:\\LOGS\\" + RecordingRun.ToString() +".txt";
                if (!File.Exists(strLogFile))
                {
                    swLog = new StreamWriter(strLogFile);
                }
                else
                {
                    swLog = File.AppendText(strLogFile);
                }

                //
                // Set the button to be a stop button
                //
                StartStopBtn.Content = "Stop";

                //
                // Set the timer interval.
                //
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(10);
                timer.Tick += timer1_Tick;

                try
                {
                    //
                    // Attempt to open the port
                    //
                    MyCOM4 = new SerialPort("COM7", 115200, Parity.None, 8, StopBits.One);
                    MyCOM4.Open();
                }
                catch
                {
                }

                //
                // Start the timer running
                //
                timer.Start();
            }
            else
            {
                StartStopBtn.Content = "Start";
                swLog.Close(); 
                timer.Stop();
                MyCOM4.Close();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //MyCOM4.ReadTimeout = 1000;
            try
            {
                //
                // Read a line from the computer. NOTE: Temporarily commented out so we generate text ourselves.
                //
                string message = MyCOM4.ReadLine();

                //
                // Instead of the above, generate a test line
                //
                /*         Random randNum = new Random();
                         randNum.Next(35);
                         string message = "Temperature:" + randNum.Next(35).ToString() +
                                          "$$BMP085 Pascal:" + randNum.Next(200000).ToString() +
                                          "$$BMP085 InchesMercury:" + randNum.Next(40).ToString() +
                                          "$$BMP085 Temp*C:" + randNum.Next(25).ToString() +
                                          "$$Gyro Axis (X):" + randNum.Next(360).ToString() +
                                          "$$Gyro Axis (Y):" + randNum.Next(360).ToString() +
                                          "$$Gyro Axis (Z):" + randNum.Next(360).ToString() +
                                          "$$Accelerometer Axis (X):" + randNum.Next(360).ToString() +
                                          "$$Accelerometer Axis (Y):" + randNum.Next(360).ToString() +
                                          "$$Accelerometer Axis (Z):" + randNum.Next(360).ToString() +
                                          "\r\n";
                         */

                ReadingPane.Text += message;

                swLog.Write(message);
                swLog.Flush();

                string[] readingsSeparator = {"$$"};
                string[] variables = message.Split(readingsSeparator, StringSplitOptions.RemoveEmptyEntries);

                // Open a connection to the web service
     //           DTSService.DTSServiceClient thisClient = new DTSService.DTSServiceClient();

                // Prepare a new recordingSet entry
                DTSService.RecordingSet newRecordingSet = new DTSService.RecordingSet();
                newRecordingSet.RecordingSetDateTime = DateTime.Now;
                newRecordingSet.RecordingRunId = RecordingRun;
                newRecordingSet.RecordingSetId = seconds;

                newRecordingSet.Recordings = new DTSService.Recording[variables.Count()];

                int counter = 0;
                foreach(string singleReading in variables)
                {
                    string[] partSeparator = {":"};
                    string[] variableParts = singleReading.Split(partSeparator, StringSplitOptions.RemoveEmptyEntries);

                    // Create new individual recordings
                    DTSService.Recording newRecording = new DTSService.Recording();
                    newRecording.Type = DTSService.ValueType.Float;
                    newRecording.Key = variableParts[0];
                    newRecording.Value = float.Parse(variableParts[1]);

                    newRecordingSet.Recordings[counter] = newRecording;
                    counter++;
                }
                
       //         thisClient.AddRecordingSet(newRecordingSet);
        //        DTSService.Recording thisRecording = new DTSService.Recording();

            }
            catch
            {
            }
            TimerText.Text = seconds.ToString() + " ticks";
            seconds++;
        }
    }
}
