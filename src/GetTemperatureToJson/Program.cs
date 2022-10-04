using Newtonsoft.Json;
using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ConsoleInfoGathering
{
    class Program
    {
        public class HardwareSensorValue
        {
            //[JsonPropertyAttribute("HardwareIdertifier")]
            //public string HardwareIdertifier { get; set; }

            //[JsonPropertyAttribute("HardwareType")]
            //public string HardwareType { get; set; }

            [JsonPropertyAttribute("SensorIdertifier")]
            public string SensorIdertifier { get; set; }

            //[JsonPropertyAttribute("SensorType")]
            //public string SensorType { get; set; }

            [JsonPropertyAttribute("Value")]
            public int Value { get; set; }
        }

        public class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }

            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware)
                {
                    subHardware.Accept(this);
                }
            }

            public void VisitSensor(ISensor sensor) { }

            public void VisitParameter(IParameter parameter) { }
        }

        static void GetSystemInfo()
        {

            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();

            //computer.IsBatteryEnabled = true;
            //computer.IsControllerEnabled = true;
            computer.IsCpuEnabled = true;
            //computer.IsGpuEnabled = true;
            //computer.IsMemoryEnabled = true;
            //computer.IsMotherboardEnabled = true;
            //computer.IsNetworkEnabled = true;
            //computer.IsPsuEnabled = true;
            computer.IsStorageEnabled = true;

            computer.Open();
            computer.Accept(updateVisitor);

            var lstHardwareSensorValue = new List<HardwareSensorValue>();

            foreach (var hardware in computer.Hardware)
            {
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        var objHardwareSensorValue = new HardwareSensorValue();

                        //objHardwareSensorValue.HardwareIdertifier = hardware.Identifier.ToString();
                        //objHardwareSensorValue.HardwareType = hardware.HardwareType.ToString();
                        objHardwareSensorValue.SensorIdertifier = sensor.Identifier.ToString();
                        //objHardwareSensorValue.SensorType = sensor.SensorType.ToString();
                        objHardwareSensorValue.Value = Convert.ToInt32(sensor.Value);

                        lstHardwareSensorValue.Add(objHardwareSensorValue);
                    }
                };
            }


            string json;
            if (lstHardwareSensorValue.Count > 0)
                json = JsonConvert.SerializeObject(lstHardwareSensorValue);
            else
                json = "[]";

            Console.WriteLine(json);

            computer.Close();
        }

        static int Main()
        {
            try
            {
                GetSystemInfo();
                return 0;
            }
            catch
            {
                Console.WriteLine("[]");
                return 1;
            }
        }
    }
}
