using Google.Protobuf;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.IdentityModel.Tokens;
using Mysqlx.Session;
using PayStationSW.Devices;
using PayStationSW.Protocols.BarCodeReader;
using System.Collections;
using System.Linq;
using System.Reflection.Metadata;

namespace PayStationSW
{
    public partial class ProtocolBarCodeReader : InterfaceBarCodeReaderProtocol
    {
        private readonly Device _device;

        public ProtocolBarCodeReader(Device device)
        {
            _device = device;
        }

        //Response
        // ACK is the the constant send by Coin device to notice you the acknowledgement of your command
        private readonly byte[] ACK = [0x30, 0x30, 0x20, 0x0D, 0x0A];
        private readonly byte[] CoinRejected = [0x30, 0x38, 0x20, 0x32, 0x31, 0x0D, 0x0A];
        private readonly byte[] DeviceReseted = [0x30, 0x38, 0x20, 0x30, 0x42, 0x0D, 0x0A];
        //The value recived for device setUp command can change, but the lenght is a constant.
        //Based on how the message is interpretated there are two lenght possible(HEX or ASCII)
        private const int DeviceSetUpHexResponseLenght = 74;
        private const int DeviceSetUpASCIIIResponseLenght = 26;
        //The value recived for device setUp command can change, but the lenght is a constant.
        //Based on how the message is interpretated there are two lenght possible(HEX or ASCII)
        private const int DeviceSetUpExpansionHexResponseLenght = 104;
        private const int DeviceSetUpExpansionASCIIIResponseLenght = 36;
        //The value recived for device setUp command can change, but the lenght is a constant.
        //Based on how the message is interpretated there are two lenght possible(HEX or ASCII)
        private const int DeviceSetUpFeautureHexResponseLenght = 104;

        private int timeOutTask = 10;

        //Commands
        private readonly byte[] resetRequest = [0x08];
        private readonly byte[] setUpRequest = [0x09];
        private readonly byte[] setUpExpansionRequest = [0x0F, 0X00];
        private readonly byte[] setUpFeatureRequest = [0x0F, 0x01, 0x00, 0x00, 0x00, 0x00];

        private readonly byte[] coinIntroducedRequest = [0x0B];
        private readonly byte[] inhibitionRequest = [0x0C, 0x00, 0x1F, 0x00, 0x00];
        private readonly byte[] disinhibitionRequest = [0x0C, 0x00, 0x00, 0x00, 0x00];

        /*
        public async Task<bool> reset()
        {
            
            _device.command();


            var timeout = TimeSpan.FromSeconds(timeOutTask);
            _device.expectingStringResponse = false;
            _device.nbrMessageExpected = 2;
            //minimuLenghtEndingBytes true if the message has a minimu lenght expected
            //false if the message expecte has ending bytes
            _device.minimuLenghtEndingBytes = false;
            _device.endingBytes = [0X0D,0x0A];

            var tcs = new TaskCompletionSource<bool>();

            EventHandler dataReceivedHandler = (sender, args) =>
            {
                //If reset command is recived correctly, resetResponse will contain 2 array of byte
                //The first array is that ACK from coin device, the second one is the message  
                byte[][] resetResponse = _device.response;
                // Complete the task when the response is received
                tcs.TrySetResult(resetResponse.Count() == _device.nbrMessageExpected && resetResponse[0].SequenceEqual(ACK) && resetResponse[1].SequenceEqual(DeviceReseted));
            };

            _device.DataReceived += dataReceivedHandler; // Subscribe to the DataReceived event

            // Send the reset message to the device
            // Assume SendMessage is a method to send data in your device class
            _device.SendMessage(resetRequest);

            // Wait for either the response to be received or for the timeout
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeout));

            // Unsubscribe from the DataReceived event to avoid memory leaks
            _device.DataReceived -= dataReceivedHandler;

            // return the result of task if is done ore not, and false if task time excede
            return completedTask == tcs.Task && await tcs.Task;
            
            return true;
        }*/
        public async Task<bool> setUp()
        {/*
            var timeout = TimeSpan.FromSeconds(timeOutTask);
            _device.expectingStringResponse = false;
            var tcs = new TaskCompletionSource<bool>();
            _device.nbrMessageExpected = 1;
            //minimuLenghtEndingBytes true if the message has a minimu lenght expected
            //false if the message expecte has ending bytes
            _device.minimuLenghtEndingBytes = false;
            _device.endingBytes = [0X0D, 0x0A];

            EventHandler dataReceivedHandler = (sender, args) =>
            {
                //If reset command is recived correctly, resetResponse will contain 2 array of byte
                //The first array is that ACK from coin device, the second one is the message  
                byte[][] resetResponse = _device.response;
                // Complete the task when the response is received
                tcs.TrySetResult(resetResponse.Count() == _device.nbrMessageExpected && resetResponse[0].Length == DeviceSetUpHexResponseLenght);
            };

            _device.DataReceived += dataReceivedHandler; // Subscribe to the DataReceived event

            // Send the reset message to the device
            // Assume SendMessage is a method to send data in your device class
            _device.SendMessage(setupDataRequest);

            // Wait for either the response to be received or for the timeout
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeout));

            // Unsubscribe from the DataReceived event to avoid memory leaks
            _device.DataReceived -= dataReceivedHandler;

            // return the result of task if is done ore not, and false if task time excede
            return completedTask == tcs.Task && await tcs.Task;
            */
            return true;
        }
        public async Task<bool> setUpFeauture()
        {
            /*
            var timeout = TimeSpan.FromSeconds(timeOutTask);
            _device.expectingStringResponse = false;
            var tcs = new TaskCompletionSource<bool>();
            _device.nbrMessageExpected = 1;
            //minimuLenghtEndingBytes true if the message has a minimu lenght expected
            //false if the message expecte has ending bytes
            _device.minimuLenghtEndingBytes = false;
            _device.endingBytes = [0X0D, 0x0A];

            EventHandler dataReceivedHandler = (sender, args) =>
            {
                //If reset command is recived correctly, resetResponse will contain 2 array of byte
                //The first array is that ACK from coin device, the second one is the message  
                byte[][] resetResponse = _device.response;
                // Complete the task when the response is received
                tcs.TrySetResult(resetResponse.Count() == _device.nbrMessageExpected && resetResponse[0].SequenceEqual(ACK));
            };

            _device.DataReceived += dataReceivedHandler; // Subscribe to the DataReceived event

            // Send the reset message to the device
            // Assume SendMessage is a method to send data in your device class
            _device.SendMessage(setupMessageFeatureRequest);

            // Wait for either the response to be received or for the timeout
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeout));

            // Unsubscribe from the DataReceived event to avoid memory leaks
            _device.DataReceived -= dataReceivedHandler;

            // return the result of task if is done ore not, and false if task time excede
            return completedTask == tcs.Task && await tcs.Task;
            */
            return true;
        }
        public async Task<bool> setUpExpansion()
        {
            /*
            var timeout = TimeSpan.FromSeconds(timeOutTask);
            _device.expectingStringResponse = false;
            var tcs = new TaskCompletionSource<bool>();
            _device.nbrMessageExpected = 1;
            //minimuLenghtEndingBytes true if the message has a minimu lenght expected
            //false if the message expecte has ending bytes
            _device.minimuLenghtEndingBytes = false;
            _device.endingBytes = [0X0D, 0x0A];

            EventHandler dataReceivedHandler = (sender, args) =>
            {
                //If reset command is recived correctly, resetResponse will contain 2 array of byte
                //The first array is that ACK from coin device, the second one is the message  
                byte[][] resetResponse = _device.response;
                // Complete the task when the response is received
                tcs.TrySetResult(resetResponse.Count() == _device.nbrMessageExpected && resetResponse[0].Length == DeviceSetUpExpansionHexResponseLenght);
            };

            _device.DataReceived += dataReceivedHandler; // Subscribe to the DataReceived event

            // Send the reset message to the device
            // Assume SendMessage is a method to send data in your device class
            _device.SendMessage(espansionSetUpRequest);

            // Wait for either the response to be received or for the timeout
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeout));

            // Unsubscribe from the DataReceived event to avoid memory leaks
            _device.DataReceived -= dataReceivedHandler;

            // return the result of task if is done ore not, and false if task time excede
            return completedTask == tcs.Task && await tcs.Task;
            */
            return true;
        }

        /*
        public async Task<bool> enable()
        {
           
            var timeout = TimeSpan.FromSeconds(timeOutTask);
            _device.expectingStringResponse = false;
            _device.nbrMessageExpected = 1;
            //minimuLenghtEndingBytes true if the message has a minimu lenght expected
            //false if the message expecte has ending bytes
            _device.minimuLenghtEndingBytes = false;
            _device.endingBytes = [0X0D, 0x0A];
            var tcs = new TaskCompletionSource<bool>();

            EventHandler dataReceivedHandler = (sender, args) =>
            {
                //If reset command is recived correctly, resetResponse will contain 2 array of byte
                //The first array is that ACK from coin device, the second one is the message  
                byte[][] resetResponse = _device.response;
                // Complete the task when the response is received
                tcs.TrySetResult(resetResponse.Count() == _device.nbrMessageExpected && resetResponse[0].SequenceEqual(ACK));
            };

            _device.DataReceived += dataReceivedHandler; // Subscribe to the DataReceived event

            // Send the reset message to the device
            // Assume SendMessage is a method to send data in your device class
            _device.SendMessage(enableRequest);

            // Wait for either the response to be received or for the timeout
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeout));

            // Unsubscribe from the DataReceived event to avoid memory leaks
            _device.DataReceived -= dataReceivedHandler;

            // return the result of task if is done ore not, and false if task time excede
            return completedTask == tcs.Task && await tcs.Task;
            
            return true;
        }
        public async Task<bool> disable()
        {
            
            var timeout = TimeSpan.FromSeconds(timeOutTask);
            _device.expectingStringResponse = false;
            _device.nbrMessageExpected = 1;
            //minimuLenghtEndingBytes true if the message has a minimu lenght expected
            //false if the message expecte has ending bytes
            _device.minimuLenghtEndingBytes = false;
            _device.endingBytes = [0X0D, 0x0A];
            var tcs = new TaskCompletionSource<bool>();

            EventHandler dataReceivedHandler = (sender, args) =>
            {
                //If reset command is recived correctly, resetResponse will contain 2 array of byte
                //The first array is that ACK from coin device, the second one is the message  
                byte[][] resetResponse = _device.response;
                // Complete the task when the response is received
                tcs.TrySetResult(resetResponse.Count() == _device.nbrMessageExpected && resetResponse[0].SequenceEqual(ACK));
            };

            _device.DataReceived += dataReceivedHandler; // Subscribe to the DataReceived event

            // Send the reset message to the device
            // Assume SendMessage is a method to send data in your device class
            _device.SendMessage(disableRequest);

            // Wait for either the response to be received or for the timeout
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeout));

            // Unsubscribe from the DataReceived event to avoid memory leaks
            _device.DataReceived -= dataReceivedHandler;

            // return the result of task if is done ore not, and false if task time excede
            return completedTask == tcs.Task && await tcs.Task;
            
            return true;
        }*/

        public async Task<bool> coinIntroducedLstn()
        {
            /*
            var timeout = TimeSpan.FromSeconds(timeOutTask);
            _device.expectingStringResponse = false;
            _device.nbrMessageExpected = 1;
            //minimuLenghtEndingBytes true if the message has a minimu lenght expected
            //false if the message expecte has ending bytes
            _device.minimuLenghtEndingBytes = false;
            _device.endingBytes = [0X0D, 0x0A];
            var tcs = new TaskCompletionSource<bool>();

            EventHandler dataReceivedHandler = (sender, args) =>
            {
                //If reset command is recived correctly, resetResponse will contain 2 array of byte
                //The first array is that ACK from coin device, the second one is the message  
                byte[][] resetResponse = _device.response;
                // Complete the task when the response is received
                if (resetResponse.Count() == _device.nbrMessageExpected && resetResponse[0].SequenceEqual(ACK)) {
                    //No coin introduced
                    Console.WriteLine("nessuna moneta introdotta");
                    tcs.TrySetResult(true);
                }
                else if (resetResponse.Count() == _device.nbrMessageExpected && resetResponse[0].SequenceEqual(CoinRejected))
                {
                    //coin rejected
                    Console.WriteLine("moneta rigettata");
                    tcs.TrySetResult(true);
                } else if (resetResponse.Count() == _device.nbrMessageExpected && resetResponse[0].Count() > 5) {
                    Console.WriteLine(resetResponse[0].ToString());
                    tcs.TrySetResult(true);
                }else
                {
                    tcs.TrySetResult(false);
                }
            };

            _device.DataReceived += dataReceivedHandler; // Subscribe to the DataReceived event

            // Send the reset message to the device
            // Assume SendMessage is a method to send data in your device class
            _device.SendMessage(coinIntroducedRequest);

            // Wait for either the response to be received or for the timeout
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeout));

            // Unsubscribe from the DataReceived event to avoid memory leaks
            _device.DataReceived -= dataReceivedHandler;

            // return the result of task if is done ore not, and false if task time excede

            if (completedTask == tcs.Task && await tcs.Task)
            {
                //handel response : ACK mean no coin introduce, rejectedCoin o handal value of coin

                return true;
            }
            else
            {
                //something went wrong, device dosen't answare
                return false;
            }

            */
            return true;
        }

    }

}