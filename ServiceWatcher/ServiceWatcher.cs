using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml.Linq;
using System.IO;
using System.Timers;

namespace ServiceWatcher
{
    public partial class ServiceWatcher : ServiceBase
    {
        private Timer timer;
        private string pathToConfigFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath; //path to app.Config

        public ServiceWatcher()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            this.WriteToFile("Service Watcher started {0}");
            XDocument doc = XDocument.Load(pathToConfigFile);
            int intervalNode = doc.Elements().Select(x => int.Parse(x.Element("interval").Value)).Sum();
            this.StartServices();
            this.timer = new System.Timers.Timer(intervalNode);  // 5000D milliseconds = 5 seconds
            this.timer.AutoReset = true;
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_Elapsed);
            this.timer.Start();
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.StartServices();
        }

        protected override void OnStop()
        {
            this.timer.Stop();
            this.timer = null;
        }

        private void WriteToFile(string text)
        {
            XDocument doc = XDocument.Load(pathToConfigFile);
            var logFilePath = doc.Descendants().Where(n => n.Name == "logFilePath").FirstOrDefault();

            using (StreamWriter writer = new StreamWriter(logFilePath.Value, true))
            {
                writer.WriteLine(string.Format(text, DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt")));
                writer.Close();
            }
        }

        private void StartServices()
        {
            XDocument doc = XDocument.Load(pathToConfigFile);
            //Console.WriteLine("Path: " + pathToConfigFile); // \bin\Debug\ServiceWatcher.exe.Config
            var servicesInConfigFile = doc.Descendants("service").Select(s => s.Value);

            foreach (ServiceController sc in ServiceController.GetServices())
            {
                foreach (var serviceInConfigFile in servicesInConfigFile)
                {
                    if (sc.ServiceName == serviceInConfigFile)
                    {
                        try
                        {
                            var manualRestartTimeNode = doc.Descendants("manualRestartTime").Select(s => s.Value);
                            TimeSpan manualRestartTime = TimeSpan.Parse(manualRestartTimeNode.ElementAt(0));
                            TimeSpan currentTime = DateTime.Now.TimeOfDay;
                           /* this.WriteToFile("Man: " + manualRestartTime + " current: " + currentTime);
                            this.WriteToFile("ManMin: " + manualRestartTime.TotalMinutes + " currentMin: " + currentTime.TotalMinutes);
                            this.WriteToFile("ManSec: " + manualRestartTime.TotalSeconds + " currentSec: " + currentTime.TotalSeconds);*/
                            if (((currentTime.Hours == manualRestartTime.Hours) && (currentTime.Minutes == manualRestartTime.Minutes) && (currentTime.TotalSeconds < manualRestartTime.TotalSeconds)))
                            {
                                this.WriteToFile("Manually stopping: " + serviceInConfigFile + " at: " + currentTime);
                                sc.Stop();

                                //this.WriteToFile("Manually starting: " + serviceInConfigFile + " at: " + currentTime);
                                //sc.Start();
                            }

                            if (sc.Status != ServiceControllerStatus.Running || sc.Status != ServiceControllerStatus.StartPending)
                            {
                                sc.Start();
                                this.WriteToFile("Services restarted: " + serviceInConfigFile + " at {0}");
                            }
                        }
                        catch (Exception ex)
                        {
                            //WriteToFile("Service Watcher Error on: {0} " + ex.Message + ex.StackTrace);
                        }
                    }
                }
            }
        }
    }
}