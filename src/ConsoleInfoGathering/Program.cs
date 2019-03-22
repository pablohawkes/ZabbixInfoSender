using OpenHardwareMonitor.Hardware;
using System;

namespace ConsoleInfoGathering
{
    class Program
    {
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
            const int C_H_N_LENGTH = 25;                //ComputerHardwareName
            const int C_H_S_N_LENGTH = 20;              //ComputerHardwareSensorName
            const int C_H_S_T_N_LENGTH = 8;             //ComputerHardwareSensorTypeName

            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();
            computer.Open();

            computer.CPUEnabled = true;
            computer.GPUEnabled = true;
            computer.RAMEnabled = true;
            computer.FanControllerEnabled = true;
            computer.HDDEnabled = true;
            computer.MainboardEnabled = true;
            computer.NICEnabled = true;

            computer.Accept(updateVisitor);
            for (int i = 0; i < computer.Hardware.Length; i++)
            {
                for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                {
                    if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
                    {
                        //var ComputerHardwareName = computer.Hardware[i].Name;
                        //var ComputerHardwareName = computer.Hardware[i].Identifier + " >>> " + computer.Hardware[i].Name;
                        var ComputerHardwareName = computer.Hardware[i].Identifier.ToString();
                        if (ComputerHardwareName.Length > C_H_N_LENGTH)
                        {
                            ComputerHardwareName = ComputerHardwareName.Substring(1, C_H_N_LENGTH);
                        }
                        else
                        {
                            ComputerHardwareName = ComputerHardwareName.PadRight(C_H_N_LENGTH);
                        }

                        //var ComputerHardwareSensorName = computer.Hardware[i].Sensors[j].Name;
                        //var ComputerHardwareSensorName = computer.Hardware[i].Sensors[j].Identifier + " >>> " + computer.Hardware[i].Sensors[j].Name;
                        var ComputerHardwareSensorName = computer.Hardware[i].Sensors[j].Identifier.ToString();
                        if (ComputerHardwareSensorName.Length > C_H_S_N_LENGTH)
                        {
                            ComputerHardwareSensorName = ComputerHardwareSensorName.Substring(1, C_H_S_N_LENGTH);
                        }
                        else
                        {
                            ComputerHardwareSensorName = ComputerHardwareSensorName.PadRight(C_H_S_N_LENGTH);
                        }

                        var ComputerHardwareSensorTypeName = computer.Hardware[i].Sensors[j].SensorType.ToString();
                        if (ComputerHardwareSensorTypeName.Length > C_H_S_T_N_LENGTH)
                        {
                            ComputerHardwareSensorTypeName = ComputerHardwareSensorTypeName.Substring(0, C_H_S_T_N_LENGTH);
                        }
                        else
                        {
                            ComputerHardwareSensorTypeName = ComputerHardwareSensorTypeName.PadRight(C_H_S_T_N_LENGTH);
                        }

                        var value = Math.Round((decimal)computer.Hardware[i].Sensors[j].Value, 1).ToString();
                        value = value.Replace(",", ".");
                        if (!(value.Contains(".")))
                        {
                            value = (value + ".0").ToString();
                        }
                        value = value.PadLeft(10);

                        Console.WriteLine(ComputerHardwareName + " | " + ComputerHardwareSensorName + " | " + ComputerHardwareSensorTypeName + " | " + value + "\r");
                    }
                }
            }
            computer.Close();
        }

        static int Main(string[] args)
        {
            try
            {
                GetSystemInfo();

                return 0;
            }

            catch (Exception exc)
            {
                Console.WriteLine(Environment.NewLine + "Error: " + exc.Message);
                Console.WriteLine(Environment.NewLine + "Application must be executed as Administrator");
                return 1;
            }
        }
    }
}
