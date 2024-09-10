using BackgroundUIUpdate.Messaging;
using Microsoft.VisualBasic;
using Stark.Messaging;

namespace BackgroundUIUpdate.Presentation;

public class ShellModel
{
    private readonly INavigator _navigator;

    private static IMessageBroker _messageBroker;

    public ShellModel(
        INavigator navigator, IMessageBroker messageBroker)
    {
        _navigator = navigator;
        _messageBroker = messageBroker;

        _ = Start();
    }

    /// <summary>
    /// Navigates to the main view model
    /// </summary>
    /// <returns></returns>
    public async Task Start()
    {
        await _navigator.NavigateViewModelAsync<MainModel>(this);
    }

    /// <summary>
    /// Handles incoming NFC tag reads
    /// </summary>
    /// <param name="tag"></param>
    public static async Task HandleIncomingNfcTag(TagDiscovered tag)
    {
        await _messageBroker.PostAsync(tag);
    }
}
