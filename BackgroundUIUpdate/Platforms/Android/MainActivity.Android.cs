using System.Text;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Nfc;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using BackgroundUIUpdate.Messaging;
using Stark.Messaging;
using Plugin.CurrentActivity;
using Plugin.Permissions.Abstractions;

namespace BackgroundUIUpdate.Droid;
[Activity(
    MainLauncher = true,
    ConfigurationChanges = global::Uno.UI.ActivityHelper.AllConfigChanges,
    WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.StateHidden
)]
public class MainActivity : Microsoft.UI.Xaml.ApplicationActivity
{
    /// <summary>
    /// Private event to properly handle transition to async context
    /// </summary>
    private event EventHandler<NfcTagDiscoveredEventArgs> OnNfcTagIntent = delegate { };

    private class NfcTagDiscoveredEventArgs : EventArgs
    {
        public TagDiscovered NfcTagDiscovered { get; set; }
    }

    /// <summary>
    /// NFC Adapter
    /// </summary>
    private NfcAdapter _nfcAdapter;


    /// <summary>
    /// Message Broker
    /// </summary>
    private IMessageBroker _nfcMessageBroker;

    /// <summary>
    /// App reference
    /// </summary>
    private App _app;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        OnNfcTagIntent += HandleOnNfcTagIntent;
        _nfcAdapter = NfcAdapter.GetDefaultAdapter(this);


        CheckAppPermissions();
    }
    /// <summary>
    /// </summary>
    /// <param name="requestCode"></param>
    /// <param name="permissions"></param>
    /// <param name="grantResults"></param>
    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
    {
        Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }
    /// <summary>
    /// Requests permissions from user if necessary
    /// </summary>
    private void CheckAppPermissions()
    {
        if ((int)Build.VERSION.SdkInt < 23)
        {
        }
        else
        {
            if (PackageManager?.CheckPermission(Android.Manifest.Permission.ReadExternalStorage, PackageName) != Android.Content.PM.Permission.Granted
                && PackageManager?.CheckPermission(Android.Manifest.Permission.WriteExternalStorage, PackageName) != Android.Content.PM.Permission.Granted)
            {
                var permissions = new[] { Android.Manifest.Permission.ReadExternalStorage, Android.Manifest.Permission.WriteExternalStorage };
                RequestPermissions(permissions, 1);
            }
        }
    }

    /// <summary>
    /// Called when the application is resumed
    /// </summary>
    protected override void OnResume()
    {
        base.OnResume();

        if (_nfcAdapter == null)
        {
            var alert = new AlertDialog.Builder(this).Create();
            alert?.SetMessage("NFC is not supported on this device.");
            alert?.SetTitle("NFC Unavailable");
            alert?.Show();
        }
        else
        {
            var filters = new[] { new IntentFilter(NfcAdapter.ActionTagDiscovered) };
            var intent = new Intent(this, GetType()).AddFlags(ActivityFlags.SingleTop);
            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Mutable);

            _nfcAdapter.EnableForegroundDispatch(this, pendingIntent, filters, null);
        }
    }

    /// <summary>
    /// Intercepts intents from the Android OS
    /// </summary>
    /// <param name="intent"></param>
    protected override void OnNewIntent(Intent intent)
    {
        base.OnNewIntent(intent);

        if (intent.Action != NfcAdapter.ActionTagDiscovered) return;
        if (intent.GetParcelableExtra(NfcAdapter.ExtraTag) is not Tag tag) return;

        var uid = ByteArrayToHexString(tag.GetId()).ToUpper();
        string ndef = null;

        // First get all the NdefMessage
        var rawMessages = intent.GetParcelableArrayExtra(NfcAdapter.ExtraNdefMessages);
        if (rawMessages != null)
        {
            var msg = (NdefMessage)rawMessages[0];

            // Get NdefRecord which contains the actual data
            var record = msg.GetRecords()?[0];
            if (record != null)
            {
                if (record.Tnf == NdefRecord.TnfWellKnown) // The data is defined by the Record Type Definition (RTD) specification available from http://members.nfc-forum.org/specs/spec_list/
                {
                    // Get the transferred data
                    ndef = Encoding.UTF8.GetString(record.GetPayload() ?? Array.Empty<byte>());

                    // Split the payload at the first occurrence of "{"
                    int jsonStartIndex = ndef.IndexOf('{');
                    if (jsonStartIndex != -1)
                    {
                        ndef = ndef.Substring(jsonStartIndex);
                    }
                }
            }
        }

        OnNfcTagIntent(this, new NfcTagDiscoveredEventArgs { NfcTagDiscovered = new TagDiscovered(uid, ndef) });
    }

    /// <summary>
    /// Handles OnNfcTagIntent events to allow for asynchronous processing
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    private async void HandleOnNfcTagIntent(object? sender, NfcTagDiscoveredEventArgs eventArgs)
    {
        try
        {
            await ShellModel.HandleIncomingNfcTag(eventArgs.NfcTagDiscovered);
        }
        catch
        {
            // Ignore
        }
    }

    /// <summary>
    /// Converts Byte Array to Hex String
    /// </summary>
    /// <param name="ba"></param>
    /// <returns></returns>
    private static string ByteArrayToHexString(byte[] ba)
    {
        var hex = new StringBuilder(ba.Length * 2);
        foreach (var b in ba) hex.AppendFormat("{0:x2}", b);
        return hex.ToString();
    }
}
