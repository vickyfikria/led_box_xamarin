using System;


namespace ledbox
{
    public interface IWifi
    {
       // Socket createSocketOverWifi();
        void ForceWifiOverCellular();
        void ForceCellularOverWifi();

    }
}
