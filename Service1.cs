using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CityWeatherReporting_Service
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer();
        private  DeviceClient s_deviceClient;
        //string connectionString = "HostName=AzureIOTHub1.azure-devices.net;DeviceId=DEVICE001;SharedAccessKey=9mE5UuxwZKq2Na9vV6sg7G8ktvj92U36ifnsSBt2fA8=";
        string connectionString = "HostName=iottesthub2.azure-devices.net;DeviceId=DEVICE0001;SharedAccessKey=oxBl9hRKJbw+R2yzq+LhW9CoI0RU03xAmRiGg3wyyoI=";
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //WriteToFile("Service is started at " + DateTime.Now);
            //GetData("Bangalore");
            s_deviceClient = DeviceClient.CreateFromConnectionString(connectionString, Microsoft.Azure.Devices.Client.TransportType.Mqtt);
            SendDeviceToCloudMessagesAsync("Bangalore");
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 60000; //number in milisecinds  
            timer.Enabled = true;
        }

        protected override void OnStop()
        {
           // GetData("Bangalore");
            //WriteToFile("Service is stopped at " + DateTime.Now);
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            //WriteToFile("Service is recall at " + DateTime.Now);
            //GetData("Bangalore");
            SendDeviceToCloudMessagesAsync("Bangalore");
        }

        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }

        public void GetData(string city)
        {
            try
            {
                using (var client = new WebClient()) //WebClient  
                {
                    client.Headers.Add("Content-Type:application/json"); //Content-Type  
                    client.Headers.Add("Accept:application/json");
                    string struri = string.Format("http://api.openweathermap.org/data/2.5/weather?q={0}&APPID=42a4cce506cfafcab674c665303d376d", city);
                    var result = client.DownloadString(struri); //URI  
                    JObject o = JObject.Parse(result.ToString());
                    string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
                    if (!File.Exists(filepath))
                    {
                        // Create a file to write to.   
                        using (StreamWriter sw = File.CreateText(filepath))
                        {
                            sw.WriteLine(System.DateTime.Now.ToString());
                            // Console.WriteLine(o.ToString());
                            JToken token = (o["weather"] as JArray);
                            sw.WriteLine("Main:" + token[0]["main"].ToString());
                            sw.WriteLine("Description:" + token[0]["description"].ToString());
                            sw.WriteLine("Icon:" + token[0]["icon"].ToString());
                            sw.WriteLine("Temperature:" + o["main"]["temp"].ToString());
                            sw.WriteLine("Pressure:" + o["main"]["pressure"].ToString());
                            sw.WriteLine("Humidity:" + o["main"]["humidity"].ToString());
                            sw.WriteLine("Min Temperature:" + o["main"]["temp_min"].ToString());
                            sw.WriteLine("Max Temperature:" + o["main"]["temp_max"].ToString());
                            sw.WriteLine("Max Temperature:" + o["visibility"].ToString());
                            sw.WriteLine("Wind Speed:" + o["wind"]["speed"].ToString());
                            sw.WriteLine("Wind Degree:" + o["wind"]["deg"].ToString());
                            sw.WriteLine("Clouds:" + o["clouds"]["all"].ToString());
                            sw.WriteLine("Sunrise:" + o["sys"]["sunrise"].ToString());
                            sw.WriteLine("Sunset:" + o["sys"]["sunset"].ToString());
                        }
                    }
                    else
                    {
                        using (StreamWriter sw = File.AppendText(filepath))
                        {
                            sw.WriteLine(System.DateTime.Now.ToString());
                            // Console.WriteLine(o.ToString());
                            JToken token = (o["weather"] as JArray);
                            sw.WriteLine("Main:" + token[0]["main"].ToString());
                            sw.WriteLine("Description:" + token[0]["description"].ToString());
                            sw.WriteLine("Icon:" + token[0]["icon"].ToString());
                            sw.WriteLine("Temperature:" + o["main"]["temp"].ToString());
                            sw.WriteLine("Pressure:" + o["main"]["pressure"].ToString());
                            sw.WriteLine("Humidity:" + o["main"]["humidity"].ToString());
                            sw.WriteLine("Min Temperature:" + o["main"]["temp_min"].ToString());
                            sw.WriteLine("Max Temperature:" + o["main"]["temp_max"].ToString());
                            sw.WriteLine("Max Temperature:" + o["visibility"].ToString());
                            sw.WriteLine("Wind Speed:" + o["wind"]["speed"].ToString());
                            sw.WriteLine("Wind Degree:" + o["wind"]["deg"].ToString());
                            sw.WriteLine("Clouds:" + o["clouds"]["all"].ToString());
                            sw.WriteLine("Sunrise:" + o["sys"]["sunrise"].ToString());
                            sw.WriteLine("Sunset:" + o["sys"]["sunset"].ToString());
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
                if (!File.Exists(filepath))
                {
                    // Create a file to write to.   
                    using (StreamWriter sw = File.CreateText(filepath))
                    {
                        sw.WriteLine(ex.Message.ToString());
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(filepath))
                    {
                        sw.WriteLine(ex.Message.ToString());
                    }
                }
            }//catch
        }

        public  async void SendDeviceToCloudMessagesAsync(string city)
        {
            var error = new
            {
               error = "Error in processing the request"
       
            };
            var messageString = JsonConvert.SerializeObject(error);
            var message= new Message(Encoding.ASCII.GetBytes(messageString.ToString()));
            using (var client = new WebClient()) //WebClient  
            {
                client.Headers.Add("Content-Type:application/json"); //Content-Type  
                client.Headers.Add("Accept:application/json");
                string struri = string.Format("http://api.openweathermap.org/data/2.5/weather?q={0}&APPID=42a4cce506cfafcab674c665303d376d", city);
                var result = client.DownloadString(struri); //URI  
                message = new Message(Encoding.ASCII.GetBytes(result.ToString()));
            }
            await s_deviceClient.SendEventAsync(message);
           // await Task.Delay(1000);
            }
        }

    }


