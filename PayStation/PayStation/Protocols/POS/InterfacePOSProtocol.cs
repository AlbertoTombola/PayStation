﻿namespace PayStationName.Protocols.POS
{
    public interface InterfacePOSProtocol
    {
        Task<bool> resetDevice();
        Task<bool> enableDevice();
        Task<bool> disableDevice();
    }
}
