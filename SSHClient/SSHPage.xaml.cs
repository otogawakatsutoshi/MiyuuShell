using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.IO;

namespace SSHClient;

public partial class SSHPage : ContentPage
{
    private SshClient _client;
    public SSHPage()
	{
		InitializeComponent();
	}
    private async void OnConnectButtonClicked(object sender, EventArgs e)
    {
        if (_client != null && _client.IsConnected)
        {
            _client.Disconnect();
            _client.Dispose();
            _client = null;

            if (OperatingSystem.IsIOSVersionAtLeast(12) || OperatingSystem.IsAndroidVersionAtLeast(21))
            {
                OutputLabel.Text = "Disconnected";
                ConnectButton.Text = "🔗Connect";
            } else
            {
                OutputLabel.Text = "Disconnected";
                ConnectButton.Text = "🔗Connect";
            }
               
           

                return;
        }

        string host = HostEntry.Text;
        string username = UsernameEntry.Text;
        string password = PasswordEntry.Text;
        string privateKeyPath = PrivateKeyEditor.Text;

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username))
        {
            OutputLabel.Text = "Please fill in required fields";
            return;
        }

        await Task.Run(() =>
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(privateKeyPath) && File.Exists(privateKeyPath))
                {
                    using var keyFile = new PrivateKeyFile(privateKeyPath);
                    using var keyAuth = new PrivateKeyAuthenticationMethod(username, keyFile);
                    var connectionInfo = new ConnectionInfo(host, username, keyAuth);
                    _client = new SshClient(connectionInfo);
                }
                else
                {
                    _client = new SshClient(host, username, password);
                }

                _client.Connect();

                if (_client.IsConnected)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        OutputLabel.Text = $"Connected to {host}";
                        ConnectButton.Text = "⛓️‍💥Disconnect";
                    });
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() => OutputLabel.Text = "Connection failed");
                }
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() => OutputLabel.Text = $"Error: {ex.Message}");
            }
        });
    }


}