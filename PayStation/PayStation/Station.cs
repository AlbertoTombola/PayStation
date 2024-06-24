using Microsoft.AspNetCore.SignalR.Protocol;
using PayStationSW.DataBase;
using PayStationSW.Devices;
using PayStationSW.Protocols.Coin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PayStationSW.RESTAPI;
using static PayStationSW.Devices.Device;
using System.IO.Ports;
//ALBERTO 24-6-24
namespace PayStationSW
{
    public class PayStation
    {
        //Devices is a public dictionary based on elements in DeviceEnum
        //to add new Device add it to enumerato DeviceEnum
        public Dictionary<DeviceEnum, Device> Devices { get; private set; }
        //Flag to enable or disable PayStation
        public bool InAlarm { get; private set; } = true;
        public bool IsEnabled { get; private set; } = true;
        //construct Station Object
        public PayStation()
        {
            Devices = new Dictionary<DeviceEnum, Device>();
        }
        //Initialize PayStation with the ApplicationDbContext for every Device class
        public async Task InitializeAsync(ApplicationDbContext context)
        {
            Devices[DeviceEnum.Cash] = await CashDevice.CreateAsync(context);
            Devices[DeviceEnum.Coin] = await CoinDevice.CreateAsync(context);
            Devices[DeviceEnum.Pos] = await POSDevice.CreateAsync(context);
            Devices[DeviceEnum.Printer] = await PrinterDevice.CreateAsync(context);
            Devices[DeviceEnum.RCModule] = await RCModule.CreateAsync(context);
            Devices[DeviceEnum.TwinModule] = await TwinModule.CreateAsync(context);
            Devices[DeviceEnum.Web2Park] = await Web2ParkDevice.CreateAsync(context);
            Devices[DeviceEnum.BarCodeReader] = await BarCodeReaderDevice.CreateAsync(context);
            var status = ReconfigureDevices(context);
        }
        public async Task<string> ReconfigureDevices(ApplicationDbContext context)
        {
            try
            {
                var configEnDevices = await ConfigureEnablingDevice(context);

                foreach (Device device in Devices.Values)
                {
                    if (device.Config.IsEnabled)
                    {
                        device.ApplyConfig();

                        // Configura le porte seriali se il dispositivo ne richiede una
                        if (device.Config.serialConnectionParameter != null)
                        {
                            await ConfigureSerialPortsForDevice(context, device);

                        }
                    }
                }

                return "Devices reconfigured successfully.";
            }
            catch (Exception ex)
            {
                return $"Error reconfiguring devices: {ex.Message}";
            }
        }
        public async Task<string> ConfigureEnablingDevice(ApplicationDbContext context)
        {
            try
            {
                // Mappatura dei codici numerici ai dispositivi enum
                var deviceTypes = new Dictionary<string, DeviceEnum>
                {
                    {"1", DeviceEnum.Coin},
                    {"2", DeviceEnum.Cash},
                    {"3", DeviceEnum.Pos},
                    {"4", DeviceEnum.Printer},
                    {"5", DeviceEnum.RCModule},
                    {"6", DeviceEnum.TwinModule},
                    {"7", DeviceEnum.Web2Park},
                    {"8", DeviceEnum.BarCodeReader}
                };

                foreach (var entry in deviceTypes)
                {
                    var deviceCode = entry.Key;
                    var deviceEnum = entry.Value;

                    var device = Devices[deviceEnum];  // Ottieni il dispositivo corrispondente dall'enum

                    var deviceSetting = await context.DevicesDB
                                                     .FirstOrDefaultAsync(d => d.DeviceType == deviceCode);

                    if (deviceSetting != null && deviceSetting.Enabled == "1")
                    {
                        device.Config.IsEnabled = true;
                    }
                    else
                    {
                        device.Config.IsEnabled = false;
                    }
                }

                Enable();  // Abilita la stazione
                return "Initialization completed successfully.";
            }
            catch (Exception ex)
            {
                return $"Initialization failed: {ex.Message}";
            }
        }
        private async Task ConfigureSerialPortsForDevice(ApplicationDbContext context, Device device)
        {
            // Recupera le informazioni specifiche della porta seriale per il dispositivo dal database
            var serialParams = await context.SerialConnectionParametersDB
                .FirstOrDefaultAsync(p => p.Device == device.DeviceType);

            if (serialParams != null)
            {
                // Applica le informazioni della porta seriale alla configurazione del dispositivo
                device.Config.serialConnectionParameter = serialParams;
            }
            else
            {
                throw new Exception($"Serial parameters not found for device {device.DeviceType}.");
            }
        }

        //Alarm Paystation
        public void Alarm(bool alarm)
        {
            InAlarm = alarm;
        }

        //Enable Paystation
        public void Enable()
        {
            IsEnabled = true;
        }
        //Disable Paystation
        public void Disable()
        {
            IsEnabled = false;
        }
        //Gives you back list of Devices of Paystation
        public List<Device> GetDevices()
        {
            return Devices.Values.ToList();
        }
        //Gives you back list of enabled devices in Devices of Paystation
        public List<Device> GetEnabledDevices()
        {
            List<Device> ListEnabledDevices;
            ListEnabledDevices = new List<Device>();
            foreach (Device device in Devices.Values)
            {
                if (device.Config.IsEnabled) { 
                    ListEnabledDevices.Add(device);
                }
            }
            return ListEnabledDevices;
        }
        
        /*
        //Connect all enable Device 
        public async Task ConnectDevices()
        {
            foreach (var device in Devices.Values)
            {
                await device.Connect();
            }
        }*/
    }

    public class StationManager
    {
        private static PayStation? _instance;
        private static readonly object _lock = new object();
        private static bool _isInitialized = false;

        public StationManager() { }

        public static async Task<PayStation> GetStationAsync(ApplicationDbContext context)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new PayStation();
                }
            }

            if (!_isInitialized)
            {
                await _instance.InitializeAsync(context);
                _instance.Enable();
                _isInitialized = true;
            }

            return _instance;
        }
    }
    public enum PaymentStatus
    {
        PaymentCancelled,
        PaymentStarted,
        PaymentInProgress,
        PaymentCompleted,
        Error
    }
    public enum DeviceEnum
    {
        Cash,
        Coin,
        Pos,
        Printer,
        RCModule,
        TwinModule,
        IO,
        Web2Park,
        BarCodeReader
    }

    //PaymentObject da rivedere o usare direttamente MovementDB come modello
    public class PaymentObject()
    {
        public int id_movment;
        public DateTime movement_date_req;
        public DateTime movement_date_close;
        public char outcome;
        public string? description;
        public int paid_cents;
        public int pay_amount;
        public int banknotes;
        public int coins;
        public int change;
        public PaymentStatus status;
        public int change_banknotes;
    }


    // ConnectionObj da spostare in serialConnection class
    public class ConnectionObj
    {
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public DeviceEnum? deviceName;
        public SerialConnectionParameterDB? port;
        public bool connectDisconnect = true;
        public bool deviceNameOrSerialData = true;
        public ConnectionObj() { }
    }
}