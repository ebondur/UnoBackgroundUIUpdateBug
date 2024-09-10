using System.ComponentModel;
using System.Runtime.CompilerServices;
using BackgroundUIUpdate.Messaging;
using Stark.Messaging;
using Xamarin.Essentials;

namespace BackgroundUIUpdate.Presentation;

public partial record SecondModel : INotifyPropertyChanged
{
    public string Uid { get; set; } = "No tag detected";

    private readonly IMessageBroker _messageBroker;

    public SecondModel(IMessageBroker messageBroker)
    {
        _messageBroker = messageBroker;
        var subscriptionName = $"{nameof(SecondModel)}_{nameof(TagDiscovered)}";
        if (!messageBroker.CheckIfExists(subscriptionName))
        {
            messageBroker.Subscribe<TagDiscovered>(async message => {
                await OnTagDiscoveredAsync(message.Data.Uid);
            }, subscriptionName);
        }
    }

    /// <summary>
    /// Simulates an NFC tag read
    /// </summary>
    /// <returns></returns>
    public async Task SimulateReadAsync()
    {
        await _messageBroker.PostAsync(new TagDiscovered(Guid.NewGuid().ToString(), string.Empty));
    }


    /// <summary>
    /// Handles an incoming NFC tag read
    /// </summary>
    /// <param name="uid"></param>
    private async Task OnTagDiscoveredAsync(string uid)
    {
       Uid = uid;

       if (MainThread.IsMainThread)
       {
            OnPropertyChanged(nameof(Uid));
       }
       else
       {
           await MainThread.InvokeOnMainThreadAsync(() =>
               OnPropertyChanged(nameof(Uid)));
       }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
