

using Google.Protobuf.WellKnownTypes;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;

namespace PayStationSW
{

    public partial class ProtocolBarCodeReader1
    {
        //this extention of ProtocolVega100 class wil contain the constant and enumerator as the converter
        //in order to keep the protocol code light and readble

        //Commands
        public readonly byte SyncByte = 0xFC;

        //Status of Cash device have different possible response(ResponseStatusCashDevice/PowerUpStatusResponseCashDevice) or ErrorStatusResponseCashDevice
        public enum StatusCommandAcceptor : byte
        {
            StatusRequest = 0X11,
            ENQ = 0x05
        }
    }
}