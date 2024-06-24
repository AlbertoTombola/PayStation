using PayStationSW.Devices;
using static PayStationSW.ProtocolBarCodeReader1;

namespace PayStationSW.Protocols.BarCodeReader
{
    public interface InterfaceBarCodeReaderProtocol
    {


        CommandParameter ResetCommand();
        CommandParameter SetUpCommand();
        CommandParameter SetUpExpansionCommand();

        CommandParameter SetUpFeautureCommand();


        CommandParameter InhibitionCommand();
        CommandParameter DisinhibitionCommand();
        CommandParameter StatusCommand();
        CommandParameter ListenerCommand();
        


        Task<bool> setUp();
        Task<bool> setUpFeauture();
        Task<bool> setUpExpansion();
        Task<bool> coinIntroducedLstn();
    }
}
