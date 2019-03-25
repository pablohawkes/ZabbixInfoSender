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
            const int C_H_N_LENGTH = 20;                                //ComputerHardwareName
            const int C_H_S_N_LENGTH = 30;                              //ComputerHardwareSensorName
            const int C_H_S_T_N_LENGTH = 12;                            //ComputerHardwareSensorTypeName
            const int V_LENGTH = 6;                                     //Value

            const string C_H_N_NAME = "Hardware Identifier";            //ComputerHardwareName
            const string C_H_S_N_NAME = "Hardware Sensor Identifier";   //ComputerHardwareSensorName
            const string C_H_S_T_N_NAME = "Sensor Type";                //ComputerHardwareSensorTypeName
            const string V_NAME = "Value";                              //Value

            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();
            computer.Open();

            computer.CPUEnabled = true;
            computer.GPUEnabled = true;
            computer.RAMEnabled = true;
            //computer.FanControllerEnabled = true;                 //Gets a weird exception: "Error: No se puede cargar el archivo o ensamblado 'HidLibrary, Version=3.2.46.0, Culture=neutral, PublicKeyToken=null' ni una de sus dependencias. El sistema no puede encontrar el archivo especificado."
            computer.HDDEnabled = true;
            computer.MainboardEnabled = true;
            //computer.NICEnabled = true;                           //Shows Too much info from NIC that is not useful for me

            computer.Accept(updateVisitor);

            Console.WriteLine((C_H_N_NAME.Length > C_H_N_LENGTH ? C_H_N_NAME.Substring(0, C_H_N_LENGTH): C_H_N_NAME.PadRight(C_H_N_LENGTH)) + " | " +
                              (C_H_S_N_NAME.Length > C_H_S_N_LENGTH ? C_H_S_N_NAME.Substring(0, C_H_S_N_LENGTH) : C_H_S_N_NAME.PadRight(C_H_S_N_LENGTH)) + " | " + 
                              (C_H_S_T_N_NAME.Length > C_H_S_T_N_LENGTH ? C_H_S_T_N_NAME.Substring(0, C_H_S_T_N_LENGTH) : C_H_S_T_N_NAME.PadRight(C_H_S_T_N_LENGTH)).PadRight(C_H_S_T_N_LENGTH) + " | " +
                              (V_NAME.Length > V_LENGTH ? V_NAME.Substring(0, V_LENGTH) : V_NAME.PadLeft(V_LENGTH)));
            Console.WriteLine(("-").PadLeft(C_H_N_LENGTH + C_H_S_N_LENGTH + C_H_S_T_N_LENGTH + V_LENGTH + 9, '-'));


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
                        /*
                        value = value.Replace(",", ".");
                        if (!(value.Contains(".")))
                        {
                            value = (value + ".0").ToString();
                        }
                        */
                        value = value.PadLeft(V_LENGTH);

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
                Console.WriteLine(Environment.NewLine + "Getting info from OpenHardwareMonitorLib.dll (Sensor Type = \"Temperature\"):" + Environment.NewLine);

                GetSystemInfo();

                Console.WriteLine(Environment.NewLine + "Finished OK.");
                //Console.WriteLine(Environment.NewLine + "Press any key to End...");
                //Console.ReadKey();
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
