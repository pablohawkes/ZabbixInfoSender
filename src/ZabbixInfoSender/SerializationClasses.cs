using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ZabbixInfoSender
{
    [XmlRoot("ZabbixInfoSenderConfiguration")]
    public class ZabbixInfoSenderConfiguration
    {
        [XmlElement("TemperatureSources")]
        public TemperatureSources TemperatureSources { get; set; }
    }

    [XmlRoot("TemperatureSources")]
    public class TemperatureSources
    {
        [XmlElement("TemperatureSource")]
        public List<TemperatureSource> TemperatureSource { get; set; }

        [XmlAttribute("ExecutionTimeSeconds")]
        public int ExecutionTimeSeconds { get; set; }

        [XmlAttribute("CPUEnabled")]
        public bool CPUEnabled { set; get; }

        [XmlAttribute("GPUEnabled")]
        public bool GPUEnabled { set; get; }

        [XmlAttribute("RAMEnabled")]
        public bool RAMEnabled { set; get; }

        [XmlAttribute("FanControllerEnabled")]
        public bool FanControllerEnabled { set; get; }

        [XmlAttribute("HDDEnabled")]
        public bool HDDEnabled { set; get; }

        [XmlAttribute("MainboardEnabled")]
        public bool MainboardEnabled { set; get; }

        [XmlAttribute("NICEnabled")]
        public bool NICEnabled { set; get; }

        public DateTime LastExecutionDatetime { get; set; }
    }

    [XmlRoot("TemperatureSource")]
    public class TemperatureSource
    {
        [XmlAttribute("Name")]
        public string Name { set; get; }

        //[XmlAttribute("HardwareIdentifier")]
        //public string HardwareIdentifier { set; get; } >> Deleted due is part of HardwareSensorIdentifier and is not useful.

        [XmlAttribute("HardwareSensorIdentifier")]
        public string HardwareSensorIdentifier { set; get; }

        [XmlAttribute("ZabbixItem")]
        public string ZabbixItem { set; get; }

        [XmlAttribute("MinLimit")]
        public int MinLimit { set; get; }

        [XmlAttribute("MaxLimit")]
        public int MaxLimit { set; get; }

        [XmlAttribute("MinValueChangeToInformZabbix")]
        public int MinValueChangeToInformZabbix { set; get; }
        
        [XmlAttribute("SendEvenIfEqualToPreviousValue")]
        public bool SendEvenIfEqualToPreviousValue { set; get; }

        [XmlAttribute("Inactive")]
        public bool Inactive { set; get; }

        public int LastTemperatureValue { set; get; }

        public TemperatureSource()
        {
            LastTemperatureValue = OpenHardwareMonitor.ABSOLUTE_ZERO_TEMPERATURE;
        }

        /*
        public int GetTemperature()
        {
            try
            {
                LastTemperatureValue = CurrentTemperatureValue;
                CurrentTemperatureValue = OpenHardwareMonitor.GetTemperature(HardwareIdentifier, HardwareSensorIdentifier);
                LastExecutionTime = DateTime.Now;

                return CurrentTemperatureValue;
            }
            catch
            {
                return OpenHardwareMonitor.ABSOLUTE_ZERO_TEMPERATURE;
            }
        }
        */
    }
}
