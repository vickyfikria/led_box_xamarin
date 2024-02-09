using System;
namespace ledbox
{
    public interface ConnectionInterface:IDisposable
    {


       



        /// <summary>
        /// Restituisce il tipo di connessione (lan, bluetooth, usb)
        /// </summary>
        /// <returns></returns>
        string getType();

        /// <summary>
        /// Avvvia il processo di connessione al LEDbox (ricerca e connessione)
        /// </summary>
        /// <param name="isconnected"></param>
        void Connect(Action<bool> isconnected);

        /// <summary>
        /// Si connette direttamente ad un LEDbox (senza ricerca)
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        bool ConnectToLedbox(string ip="");

        /// <summary>
        /// Si disconnette dal LEDbox corrente
        /// </summary>
        void DisconnectToLedbox();

        /// <summary>
        /// Verifica se il LEDbox è connesso
        /// </summary>
        /// <returns></returns>
        bool isConnected();

        /// <summary>
        /// Invia un file al LEDbox
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="isExist"></param>
        /// <param name="forceUpload"></param>
        void sendFile(string filePath, bool isExist, bool forceUpload = false);
        
        
        /// <summary>
        /// Carica un file sul LEDbox
        /// </summary>
        /// <param name="filename">Nome del file che verrà salvato sul ledbox</param>
        /// <param name="filePath">Percorso del file locale che dovrà essere uploadato sul Ledbox</param>
        /// <param name="alias">Alias dell'utente corrente</param>
        /// <param name="isFinish">Restituisce il path del file sul LEDbox</param>
        /// <param name="type">cartella del ledbox in cui verrà decompresso il file</param>
        /// <param name="forceUpload">consente di forzare l'upload anche se il file è già presente sul ledbox</param>
        void startUploadFile(string filename, string filePath,string alias, Action<string> isFinish,string type,bool forceUpload=false,string customMessageApiUpload = "");
        void startUploadFile(string customMessageApiUpload, Action<string> isFinish, string type, bool forceUpload = false);

        /// <summary>
        /// Invia un messaggio al LEDbox
        /// </summary>
        /// <param name="message"></param>
        void SendMessage(string message);

        /// <summary>
        /// Imposta l'indirizzo del LEDbox
        /// </summary>
        /// <param name="address"></param>
        void setAddress(string address);
        /// <summary>
        /// Restituisce l'indirizzo del LEDbox
        /// </summary>
        /// <returns></returns>
        string getAddress();
        /// <summary>
        /// Imposta la modalità di ascolto
        /// </summary>
        /// <param name="mode"></param>
        void setModeListener(int mode=0);


        /// <summary>
        /// Verifica se tutti i permessi per la connessione sono stati attivati (altrimenti li attiva o ne chiede l'autorizzazione)
        /// </summary>
        /// <returns></returns>
        bool checkPermission(int action);
   
    }
}
