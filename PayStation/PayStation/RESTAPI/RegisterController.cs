﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using PayStationSW.DataBase;
using PayStationSW.Devices;
using System.Diagnostics.Eventing.Reader;
using Microsoft.EntityFrameworkCore;

namespace PayStationSW.RESTAPI
{
    [Route("api/Register")]
    [ApiController]
    public class RegisterController : ControllerBase
    {

        private readonly StationManagerWS _stationManagerWS;
        private readonly ApplicationDbContext _context;
        private readonly DeviceService _deviceService;

        public RegisterController(ApplicationDbContext context, DeviceService deviceService, StationManagerWS stationManagerWS)
        {
            _context = context;
            _deviceService = deviceService;
            _stationManagerWS = stationManagerWS;

        }

        [HttpPost("EnableDiasbleEntity")]
        public async Task<IActionResult> EnableDiasbleEntity([FromBody] DeviceEntityDB device)
        {

            try
            {
                // Delega la gestione del database al DeviceService
                if ((device.DeviceType == "5") && (device.Enabled == "1"))
                {
                    var deviceResult1 = await _deviceService.ManageDeviceAsync("6", "", "0");
                }
                else if ((device.DeviceType == "6") && (device.Enabled == "1"))
                {
                    var deviceResult1 = await _deviceService.ManageDeviceAsync("5", "", "0");
                }
                var deviceResult = await _deviceService.ManageDeviceAsync(device.DeviceType ?? "", device.Description ?? "", device.Enabled ?? "0");  
                return Ok("Device parameters updated successfully: " + deviceResult.Description);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("GetEntityStatus/{deviceType}")]
        public async Task<ActionResult<DeviceEntityDto>> GetEntityStatus(string deviceType)
        {
            try
            {
                var device = await _context.DevicesDB.FirstOrDefaultAsync(d => d.DeviceType == deviceType);

                if (device == null)
                {
                    return NotFound("Device not found, check the device type.");
                }

                var response = new DeviceEntityDto
                {
                    Status = $"Device {device.Description}: " + (device.Enabled == "1" ? "enabled." : "disabled."),
                    IsEnabled = device.Enabled == "1"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /*
        [HttpPost("EnableDisableDevice")]
        public async Task<IActionResult> EnableDisableDevice([FromBody] bool enableDevice)
        {
            Console.WriteLine("EnableDisableDevice API called");
            try
            {
                var station = await StationManager.GetStationAsync(_context);
                var coinDevice = station.Devices[DeviceEnum.Coin] as CoinDevice;
                if (!coinDevice.Config.IsConnected)
                {
                    return BadRequest(new { error = "The Coin device is not a connected device." });
                }
                string response = enableDevice ? await coinDevice.Enable() : await coinDevice.DisableCommand();
                return Ok(new { status = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        */








        [HttpPost("AutomaticInit")]
        public async Task<IActionResult> AutomaticInit()
        {
            try
            {
                var station = await StationManager.GetStationAsync(_context);
                string response = await station.ReconfigureDevices(_context);
                return Ok(new { status = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }














        [HttpPost("SetImporto")]
        public async Task<IActionResult> SetImporto([FromBody] int importo)
        {
            try
            {
                var station = await StationManager.GetStationAsync(_context);
                if (!station.IsEnabled)
                {
                    return BadRequest(new { error = "The PayStation is not enable." });
                }
                Console.WriteLine($"Set Importo called {importo}");
                //Her I need to proccess the payment

                // Create a new MovementDB instance
                var movement = new MovementDB
                {
                    Amount = importo,
                    MovementDateOpen = DateTime.Now
                };

                // Process the payment
               // var paymentProcessor = new PaymentProcessor(movement);
                //paymentProcessor.StartPaymentProcess();

                // Add the movement to the database and save changes
                _context.MovementsDB.Add(movement);
                await _context.SaveChangesAsync();

                var coinDevice = station.Devices[DeviceEnum.Coin] as CoinDevice;
                var cashDevice = station.Devices[DeviceEnum.Cash] as CashDevice;
                var posDevice = station.Devices[DeviceEnum.Pos] as POSDevice;

                if (coinDevice.Config.IsConnected)
                {
                    coinDevice.Enable();
                    //coinDevice.StartPolling();
                }
                if (cashDevice.Config.IsConnected)
                {
                    cashDevice.Enable();
                    //cashDevice.StartPolling();
                }
                if (posDevice.Config.IsConnected && posDevice.Config.IsSetUp)
                {
                    await posDevice.SetImportoPos();
                }



                _stationManagerWS.StartPeriodicMessages();
                return Ok(new { message = $"Set importo recived for importo {importo}, id movment is {movement.Id}." });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/Register/GetAlarms
        [HttpGet("GetAlarms")]
        public async Task<IActionResult> GetAlarms()
        {
            try
            {
                var station = await StationManager.GetStationAsync(_context);
                if (!station.InAlarm)
                {
                    return Ok("There is an allarm");
                }
                else
                {
                    return Ok("No allarm");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // GET: api/Register/GetStatus
        [HttpGet("GetStatus")]
        public async Task<IActionResult> GetStaus()
        {
            try
            {
                var station = await StationManager.GetStationAsync(_context);
  
                    return Ok("1,100");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // GET: api/DataBase/GetMovment/{id}
        [HttpGet("GetMovment/{id}")]
        public async Task<ActionResult<MovementDB>> GetMovementById(int? id)
        {
            if (id == null)
            {
                return BadRequest("Id cannot be null");
            }
            try
            {
                var movement = await _context.MovementsDB.FindAsync(id);
                if (movement == null)
                {
                    return NotFound($"Movement with id {id} not found");
                }
                return Ok(movement);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }


    public class DeviceEntityDto
    {
        public string Status { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class ParametriPayment
    {
        public int Amount { get; set; }
        public DateTime DateSend { get; set; }

    }
}
