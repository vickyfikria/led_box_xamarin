using System;
namespace ledbox
{
    public interface IActivity
    {
        void sendMessageToPlay();
        void sendMessageToStop();
        void sendMessageToPause();
        int getStatus();
        string getTitle();


    }
}
