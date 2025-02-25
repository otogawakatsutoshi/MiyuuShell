using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace SSHClient
{
    public partial class NetworkPage : ContentPage
    {
        public NetworkPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await UpdateNetworkInfo();
        }

        private async void OnUpdateButtonClicked(object sender, EventArgs e)
        {
            await UpdateNetworkInfo();
        }

        private async Task UpdateNetworkInfo()
        {
            // NIC名, MAC, ローカル IP, Subnet Mask, IPv6 をセットで取得
            string networkInfo = GetNetworkInterfacesInfo();
            NetworkInfoLabel.Text = string.IsNullOrEmpty(networkInfo) ? "No active network adapters found." : networkInfo;

            // グローバル IP を取得
            string publicIP = await GetPublicIPAddress();
            PublicIPLabel.Text = string.IsNullOrEmpty(publicIP) ? "Not found" : publicIP;
        }

        private string GetNetworkInterfacesInfo()
        {
            StringBuilder sb = new StringBuilder();
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    // NIC 名
                    sb.AppendLine($"NIC: {ni.Name}");

                    // MAC アドレス
                    string mac = string.Join(":", ni.GetPhysicalAddress().GetAddressBytes().Select(b => b.ToString("X2")));
                    sb.AppendLine($"  MAC: {mac}");

                    // ローカル IPv4 アドレス と サブネットマスク
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork) // IPv4
                        {
                            sb.AppendLine($"  IPv4: {ip.Address}");
                            sb.AppendLine($"  Subnet Mask: {ip.IPv4Mask}");
                        }
                        else if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6) // IPv6
                        {
                            sb.AppendLine($"  IPv6: {ip.Address}");
                        }
                    }
                    sb.AppendLine(); // 改行で区切る
                }
            }
            return sb.Length > 0 ? sb.ToString().Trim() : "No active network adapters found.";
        }

        private async Task<string> GetPublicIPAddress()
        {
            try
            {
                using HttpClient client = new HttpClient();
                return await client.GetStringAsync("https://api64.ipify.org");
            }
            catch
            {
                return "Unable to retrieve public IP.";
            }
        }
    }
}
