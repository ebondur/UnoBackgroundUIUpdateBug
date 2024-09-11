using BackgroundUIUpdate.Messaging;
using Stark.Messaging;

namespace BackgroundUIUpdate.Presentation;

public sealed partial class SecondPage : Page
{
    public SecondPage()
    {
        this.InitializeComponent();
        var messageBroker = App.Host?.Services.GetRequiredService<IMessageBroker>();

        //DataContext = new BindableSecondModel(messageBroker);

        messageBroker!.Subscribe<TagDiscovered>(message =>
        {
          var vm = (BindableSecondModel)DataContext;
          vm.Save.Execute(message.Data.Uid);
          return Task.CompletedTask;
        });
    }
}
