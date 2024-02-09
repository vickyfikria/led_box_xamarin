using System;
namespace ledbox
{
    public interface IMessage
    {
        void LongAlert(string message);
        void ShortAlert(string message);
        void PromptYesNo(string message, Action<bool> result, string yes_button = "Yes", string no_button = "No");
        void DisplayAlert(string message);

    }
}
