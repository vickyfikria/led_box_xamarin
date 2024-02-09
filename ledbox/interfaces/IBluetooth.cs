using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;



namespace ledbox
{
    public interface IBluetooth
    {
        void InitializeBluetooth();   // Initialises bluetooth adapter settings
        void Connect(BluetoothItem bluetoothDevice, Action<bool> isconnected);
        List<BluetoothItem> getDevices();
        void SendMessage(string message);
        bool isConnected();
        void Disconnect();
        void startDiscovery();
        bool sendFile(string filePath, bool isExist, bool forceUpload = false);


    }
}
