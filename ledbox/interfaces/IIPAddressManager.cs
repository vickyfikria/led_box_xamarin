using System;
namespace ledbox
{
    public interface IIPAddressManager
    {
        String GetIPAddress();
        bool CheckWifiConnection();
        string CurrentWifiConnection();
        bool CheckDataMobile();

    }

    
}
