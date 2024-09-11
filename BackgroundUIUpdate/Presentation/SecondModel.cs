using System.ComponentModel;
using System.Runtime.CompilerServices;
using BackgroundUIUpdate.Messaging;
using Stark.Messaging;
using Xamarin.Essentials;

namespace BackgroundUIUpdate.Presentation;

public partial record SecondModel : INotifyPropertyChanged
{
    public string Uid { get; set; } = "Test";

    private readonly IMessageBroker _messageBroker;

    public SecondModel(IMessageBroker messageBroker)
    {
      _messageBroker = messageBroker;
    }

    /// <summary>
    /// Simulates an NFC tag read
    /// </summary>
    /// <returns></returns>
    public async Task SimulateReadAsync()
    {
        await ShellModel.HandleIncomingNfcTag(new TagDiscovered(Guid.NewGuid().ToString(), string.Empty));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Save(string uid)
    {
        Uid = uid;
        OnPropertyChanged(nameof(Uid));
    }
}
