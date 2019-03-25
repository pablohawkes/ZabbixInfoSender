using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace ZabbixInfoSender
{
    public class ZabbixInfoSenderInformation
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public int ControlIntervalSeconds { get; set; }
        public string ZabbixServerAddress { get; set; }
        public int ZabbixServerPort { get; set; }
        public bool DoNotSendDataToZabbix { get; set; }

        public string ZabbixClientHostName { get; set; }
        public ZabbixInfoSenderConfiguration ZabbixInfoSenderConfiguration { get; set; }

        public ZabbixInfoSenderInformation()
        {
            try
            {
                ControlIntervalSeconds = int.Parse(ConfigurationManager.AppSettings["ControlIntervalSeconds"]);
                log.Info("Parameter ControlIntervalSeconds: " + ControlIntervalSeconds.ToString());
            }
            catch (Exception exc)
            {
                log.Error("Error on parameter read: ControlIntervalSeconds" + Environment.NewLine, exc);
                throw exc;
            }

            try
            {
                ZabbixServerAddress = ConfigurationManager.AppSettings["ZabbixServerAddress"].ToString().Trim();
                log.Info("Parameter ZabbixServerAddress: " + ZabbixServerAddress);
            }
            catch (Exception exc)
            {
                log.Error("Error on parameter read: ZabbixServerAddress" + Environment.NewLine, exc);
                throw exc;
            }

            try
            {
                ZabbixServerPort = int.Parse(ConfigurationManager.AppSettings["ZabbixServerPort"]);
                log.Info("Parameter ZabbixServerPort: " + ZabbixServerPort.ToString());
            }
            catch (Exception exc)
            {
                log.Error("Error on parameter read: ZabbixServerPort" + Environment.NewLine, exc);
                throw exc;
            }

            try
            {
                ZabbixClientHostName = ConfigurationManager.AppSettings["ZabbixClientHostName"].Trim();

                if (ZabbixClientHostName == "")
                {
                    ZabbixClientHostName = System.Environment.MachineName;
                }

                log.Info("Parameter ZabbixClientHostName: " + ZabbixClientHostName.ToString());
            }
            catch (Exception exc)
            {
                log.Error("Error on parameter read: ZabbixClientHostName" + Environment.NewLine, exc);
                throw exc;
            }


            try
            {
                DoNotSendDataToZabbix = bool.Parse(ConfigurationManager.AppSettings["DoNotSendDataToZabbix"]);
                log.Info("Parameter DoNotSendDataToZabbix: " + DoNotSendDataToZabbix.ToString());
            }
            catch (Exception exc)
            {
                log.Warn("Error on parameter read: DoNotSendDataToZabbix - Set value to false" + Environment.NewLine, exc);
                DoNotSendDataToZabbix = false;
            }

            //Get configuration file location:
            var directoryName = System.IO.Path.GetDirectoryName(new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            var path = directoryName + @"\Configuration.xml";

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ZabbixInfoSenderConfiguration));

                using (StreamReader reader = new StreamReader(path))
                {
                    ZabbixInfoSenderConfiguration = (ZabbixInfoSenderConfiguration)serializer.Deserialize(reader);
                    reader.Close();

                    log.Info("Configuration Read OK");
                }
            }
            catch (Exception exc)
            {
                log.Error("Error on AndonSystem Deserializer" + Environment.NewLine, exc);
                throw exc;
            }
        }

        internal void ExecuteInfoGatheringAndSendToZabbix()
        {
            List<ZabbixMessage> messagesToSend = new List<ZabbixMessage>();

            try
            {
                var temps = GetTemperatures();
                messagesToSend.AddRange(temps);
            }
            catch (Exception exc)
            {
                log.Error("Error on ProcessAndSendMessagesToZabbix - GetTemperatures" + Environment.NewLine, exc);
            }


            //TODO: Add other messages to send

            //Send Messages
            if (!DoNotSendDataToZabbix)
            {
                try
                {
                    SendMessagesToZabbix(messagesToSend);
                }
                catch (Exception exc)
                {
                    log.Error("Error on SendMessagesToZabbix - Information not sent" + Environment.NewLine, exc);
                }
            }
        }

        internal void SendMessagesToZabbix(List<ZabbixMessage> lstMessages)
        {

            if (lstMessages.Count == 0)
            {
                log.Info("SendMessagesToZabbix: NO messages to send to Zabbix");
                return;
            }
            /*
             * NO IDEA WHY THIS NOT WORKS :(
            object message =
                (
                    new
                    {
                        request = "sender data",
                        data = lstMessages
                    }
                );

            string json = JsonConvert.SerializeObject(message);
            */

            var json = "{\"request\":\"sender data\",\"data\":[";
            var isFirstValue = true;
            foreach (var a in lstMessages)
            {
                if (isFirstValue)
                {
                    isFirstValue = false;
                }
                else
                {
                    json += ",";
                }
                json += "{\"host\":\"" + a.Host + "\",\"key\":\"" + a.Key + "\",\"value\":\"" + a.Value + "\"}";
            }
            json += "]}";

            log.Info("Message to Zabbix: " + json.ToString());

            byte[] header = Encoding.ASCII.GetBytes("ZBXD\x01");
            byte[] length = BitConverter.GetBytes((long)json.Length); // Little-Endian
            byte[] data = Encoding.ASCII.GetBytes(json);

            byte[] all = new byte[header.Length + length.Length + data.Length];

            //Just for verify how lenght data is sent to zabbix:
            //log.Debug(length[0].ToString() + " " + length[1].ToString() + " " + length[2].ToString() + " " + length[3].ToString());

            System.Buffer.BlockCopy(header, 0, all, 0, header.Length);
            System.Buffer.BlockCopy(length, 0, all, header.Length, length.Length);
            System.Buffer.BlockCopy(data, 0, all, header.Length + length.Length, data.Length);

            using (var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                log.Debug("Socket: " + ZabbixServerAddress + ":" + ZabbixServerPort);
                client.Connect(ZabbixServerAddress, ZabbixServerPort);
                log.Debug("Socket is open? " + client.Connected.ToString());

                client.Send(all);
                log.Debug("Socket Send OK");

                // header
                byte[] buffer = new byte[5];
                _Receive(client, buffer, 0, buffer.Length, 10000);

                if (Encoding.ASCII.GetString(buffer, 0, buffer.Length) != "ZBXD\x01")
                {
                    throw new Exception("Invalid Header Responde from Zabbix (\"ZBXD\")");
                }


                // Message length
                buffer = new byte[8];
                _Receive(client, buffer, 0, buffer.Length, 10000);
                int dataLength = BitConverter.ToInt32(buffer, 0);

                if (dataLength == 0)
                {
                    throw new Exception("Invalid response from Zabbix");
                }


                // Message
                buffer = new byte[dataLength];
                _Receive(client, buffer, 0, buffer.Length, 10000);

                //Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, buffer.Length));
                log.Info("Response From Zabbix: " + Encoding.ASCII.GetString(buffer, 0, buffer.Length));
            }
        }

        internal void _Receive(Socket socket, byte[] buffer, int offset, int size, int timeout)
        {
            int startTickCount = Environment.TickCount;
            int received = 0;
            do
            {
                if (Environment.TickCount > startTickCount + timeout)
                {
                    throw new Exception("Timeout.");
                }

                try
                {
                    received += socket.Receive(buffer, offset + received, size - received, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably empty, wait and try again
                        Thread.Sleep(5);
                    }
                    else
                    {
                        throw ex;  // any serious error occurr
                    }
                }
            } while (received < size);
        }


        private List<ZabbixMessage> GetTemperatures()
        {
            List<ZabbixMessage> zabbixMessages = new List<ZabbixMessage>();

            if (DateTime.Now >= ZabbixInfoSenderConfiguration.TemperatureSources.LastExecutionDatetime.AddSeconds(ZabbixInfoSenderConfiguration.TemperatureSources.ExecutionTimeSeconds))
            {
                try
                {
                    log.Debug("Starting temperature gathering");

                    int temperatureValue;
                    var OpenHardMonitor = new OpenHardwareMonitor(ZabbixInfoSenderConfiguration.TemperatureSources.CPUEnabled,
                                                                  ZabbixInfoSenderConfiguration.TemperatureSources.GPUEnabled,
                                                                  ZabbixInfoSenderConfiguration.TemperatureSources.RAMEnabled,
                                                                  ZabbixInfoSenderConfiguration.TemperatureSources.FanControllerEnabled,
                                                                  ZabbixInfoSenderConfiguration.TemperatureSources.HDDEnabled,
                                                                  ZabbixInfoSenderConfiguration.TemperatureSources.MainboardEnabled,
                                                                  ZabbixInfoSenderConfiguration.TemperatureSources.NICEnabled);

                    foreach (var temp in ZabbixInfoSenderConfiguration.TemperatureSources.TemperatureSource)
                    {
                        if (!temp.Inactive)
                        {
                            //Get temperatures
                            //temperatureValue = OpenHardMonitor.GetIntValue(temp.HardwareIdentifier, temp.HardwareSensorIdentifier);
                            temperatureValue = OpenHardMonitor.GetIntValue(temp.HardwareSensorIdentifier);

                            if (temperatureValue < temp.MinLimit)
                            {
                                log.Info("Temperature below limit - Message Not Sent -  NAME: " + temp.Name + " - Sensor: " + temp.HardwareSensorIdentifier +
                                         " -  Value: " + temperatureValue + "° - Min Limit: " + temp.MinLimit + "°");
                                continue;
                            }

                            if (temperatureValue > temp.MaxLimit)
                            {
                                log.Info("Temperature above limit - Message Not Sent -  NAME: " + temp.Name + " - Sensor: " + temp.HardwareSensorIdentifier +
                                         " -  Value: " + temperatureValue + "° - Max Limit: " + temp.MaxLimit + "°");
                                continue;
                            }
                            
                            if (!temp.SendEvenIfEqualToPreviousValue) //Fix #1
                            {
                                if (temp.LastTemperatureValue == temperatureValue)
                                {
                                    log.Info("Temperature equal to Previous - NOT SENT - Name: " + temp.Name + " - Sensor: " + temp.HardwareSensorIdentifier + " -  Value: " + temperatureValue.ToString() + "°");
                                    continue;
                                }

                                if ((Math.Abs(temp.LastTemperatureValue - temperatureValue)) <= temp.MinValueChangeToInformZabbix)
                                {
                                    log.Info("Temperature change below MinValueChangeToInformZabbix limit - Message Not Sent -  NAME: " + temp.Name + " - Sensor: " + temp.HardwareSensorIdentifier +
                                             " -  Current Value: " + temperatureValue + "° - Last Value: " + temp.LastTemperatureValue + "°");
                                    continue;
                                }
                            }

                            //Everything is OK: Send Data
                            temp.LastTemperatureValue = temperatureValue;

                            zabbixMessages.Add(new ZabbixMessage(ZabbixClientHostName, temp.ZabbixItem, temperatureValue));

                            log.Info("Temperature Gathered: NAME: " + temp.Name + " - Sensor: " + temp.HardwareSensorIdentifier + " -  Value: " + temperatureValue.ToString() + "°");

                        }
                    }

                    OpenHardMonitor.CloseHardwareMonitor();

                    log.Debug("End temperature gathering");

                    return zabbixMessages;
                }
                catch (Exception exc)
                {
                    //OpenHardMonitor.CloseHardwareMonitor();

                    log.Error("Error on Processing Temperatures - Information not sent" + Environment.NewLine, exc);
                    return zabbixMessages;
                }
                finally
                {
                    ZabbixInfoSenderConfiguration.TemperatureSources.LastExecutionDatetime = DateTime.Now; //Set last execution to now
                }
            }
            else
            {
                log.Debug("TemperatureSources not executed due time is not up yet");
                return zabbixMessages;
            }
        }
    }
}
