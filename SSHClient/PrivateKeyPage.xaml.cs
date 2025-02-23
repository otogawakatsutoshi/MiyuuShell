using Microsoft.Maui.Storage;
using Microsoft.Maui.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.Maui.Controls;
using System.Security.Cryptography;

namespace SSHClient;

public partial class PrivateKeyPage : ContentPage
{
	//public PrivateKeyPage()
	//{
	//	InitializeComponent();
	//}
    private const string PrivateKeyStorageKey = "SSH_PRIVATE_KEYS";
    private Dictionary<string, string> _privateKeys = new();

    public PrivateKeyPage()
    {
        InitializeComponent();
        LoadStoredKeys();
    }

    private async void LoadStoredKeys()
    {
        var storedJson = await SecureStorage.GetAsync(PrivateKeyStorageKey);
        if (!string.IsNullOrEmpty(storedJson))
        {
            _privateKeys = JsonSerializer.Deserialize<Dictionary<string, string>>(storedJson);
            KeyPicker.ItemsSource = _privateKeys.Keys.ToList();
        }
    }

    private async void OnSavePrivateKeyClicked(object sender, EventArgs e)
    {
        string keyName = KeyNameEntry.Text;
        string privateKey = PrivateKeyEditor.Text;

        if (string.IsNullOrWhiteSpace(keyName) || string.IsNullOrWhiteSpace(privateKey))
        {
            OutputLabel.Text = "Key name and private key are required!";
            OutputLabel.TextColor = Colors.Red;
            return;
        }

        _privateKeys[keyName] = privateKey;
        string json = JsonSerializer.Serialize(_privateKeys);
        await SecureStorage.SetAsync(PrivateKeyStorageKey, json);

        KeyPicker.ItemsSource = _privateKeys.Keys.ToList();
        OutputLabel.Text = $"Key '{keyName}' saved!";
    }

    // 鍵を生成（RSA 2048bit）
    private void OnGenerateKeyClicked(object sender, EventArgs e)
    {
        using var rsa = new RSACryptoServiceProvider(2048);
        rsa.PersistKeyInCsp = false;

        // crlfが機種依存で変わるのかわからんのでチェック。
        // SSH プライベートキー形式 (PEM)
        string privateKey = ExportPrivateKeyToPEM(rsa);
        PrivateKeyEditor.Text = privateKey;

        OutputLabel.Text = "New private key generated!";
    }

    private async Task<bool> RequestStoragePermissionAsync()
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            }
            return status == PermissionStatus.Granted;
        }
        return true; // iOS / Windows は特に不要
    }

    // 鍵をPEM形式でエクスポート
    private static string ExportPrivateKeyToPEM(RSACryptoServiceProvider rsa)
    {
        var sb = new StringBuilder();
        sb.AppendLine("-----BEGIN RSA PRIVATE KEY-----");
        sb.AppendLine(Convert.ToBase64String(rsa.ExportRSAPrivateKey(), Base64FormattingOptions.InsertLineBreaks));
        sb.AppendLine("-----END RSA PRIVATE KEY-----");
        return sb.ToString();
    }

    // 鍵をエクスポート
    private async void OnExportKeyClicked(object sender, EventArgs e)
    {
        if (KeyPicker.SelectedItem == null)
        {
            OutputLabel.Text = "Select a key to export.";
            OutputLabel.TextColor = Colors.Red;
            return;
        }

        // android
        if (!await RequestStoragePermissionAsync())
        {
            OutputLabel.Text = "Storage permission is required.";
            OutputLabel.TextColor = Colors.Red; 
            return;
        }

        string selectedKey = KeyPicker.SelectedItem.ToString();
        string privateKey = _privateKeys[selectedKey];

        try
        {
            string fileName = $"{selectedKey}_private_key.pem";
            string filePath = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);

            await File.WriteAllTextAsync(filePath, privateKey);

            // 🔹 Android & iOS の場合、共有アクション（Google Driveなどへエクスポート）
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Export Private Key",
                File = new ShareFile(filePath)
            });

            OutputLabel.Text = $"Exported: {filePath}";
        }
        catch (Exception ex)
        {
            OutputLabel.Text = $"Export failed: {ex.Message}";
        }
    }



    private async void OnLoadPrivateKeysClicked(object sender, EventArgs e)
    {
        LoadStoredKeys();
        OutputLabel.Text = "Keys loaded.";
    }

    private async void OnDeleteSelectedKeyClicked(object sender, EventArgs e)
    {
        if (KeyPicker.SelectedItem == null)
        {
            OutputLabel.Text = "Select a key to delete.";
            return;
        }

        string selectedKey = KeyPicker.SelectedItem.ToString();
        _privateKeys.Remove(selectedKey);

        string json = JsonSerializer.Serialize(_privateKeys);
        await SecureStorage.SetAsync(PrivateKeyStorageKey, json);

        KeyPicker.ItemsSource = _privateKeys.Keys.ToList();
        OutputLabel.Text = $"Key '{selectedKey}' deleted!";
    }
}