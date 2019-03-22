using OpenHardwareMonitor.Hardware;
using System;

namespace ZabbixInfoSender
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

    public class OpenHardwareMonitor
    {
        public const int ABSOLUTE_ZERO_TEMPERATURE = -273;
        readonly UpdateVisitor updateVisitor;
        Computer computer;

        public OpenHardwareMonitor(bool CPUEnabled = false,
                                   bool GPUEnabled = false,
                                   bool RAMEnabled = false,
                                   bool FanControllerEnabled = false,
                                   bool HDDEnabled = false,
                                   bool MainboardEnabled = false,
                                   bool NICEnabled = false)
        {
            updateVisitor = new UpdateVisitor();
            computer = new Computer();

            computer.Open();

            computer.CPUEnabled = CPUEnabled;
            computer.GPUEnabled = GPUEnabled;
            computer.RAMEnabled = RAMEnabled;
            computer.FanControllerEnabled = FanControllerEnabled;
            computer.HDDEnabled = HDDEnabled;
            computer.MainboardEnabled = MainboardEnabled;
            computer.NICEnabled = NICEnabled;

            computer.Accept(updateVisitor);
        }


        public void CloseHardwareMonitor()
        {
            if (computer != null)
            {
                computer.Close();
            }
        }

        //public int GetIntValue(string HardwareIdentifier, string HardwareSensorIdentifier)
        public int GetIntValue(string HardwareSensorIdentifier)
        {
            try
            {
                for (int i = 0; i < computer.Hardware.Length; i++)
                {
                    //if (computer.Hardware[i].Identifier.ToString().Trim() == HardwareIdentifier.Trim())
                    //{
                        for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                        {
                            if (computer.Hardware[i].Sensors[j].Identifier.ToString().Trim() == HardwareSensorIdentifier.Trim())
                            {
                                return (int)Math.Round((decimal)computer.Hardware[i].Sensors[j].Value, 0);
                            }
                        }
                    //}
                }

                return ABSOLUTE_ZERO_TEMPERATURE;
            }
            catch
            {
                return ABSOLUTE_ZERO_TEMPERATURE;
            }
        }
    }
}