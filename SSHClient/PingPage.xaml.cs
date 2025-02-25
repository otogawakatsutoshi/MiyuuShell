using System;
using System.Net;
using System.Net.NetworkInformation;
using Microsoft.Maui.Controls;

namespace SSHClient
{
    public partial class PingPage : ContentPage
    {
        public PingPage()
        {
            InitializeComponent();
        }

        // Ping ボタンがクリックされた時のイベントハンドラー
        private async void OnPingButtonClicked(object sender, EventArgs e)
        {
            var host = HostEntry.Text;
            var pingCountStr = PingCountEntry.Text;
            var timeoutStr = TimeoutEntry.Text;

            // 入力が不正な場合
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(pingCountStr) || string.IsNullOrWhiteSpace(timeoutStr))
            {
                ResultLabel.Text = "Please fill in all fields.";
                return;
            }

            // 数値入力のバリデーション
            if (!int.TryParse(pingCountStr, out int pingCount) || !int.TryParse(timeoutStr, out int timeout))
            {
                ResultLabel.Text = "Please enter valid numeric values for Ping Count and Timeout.";
                return;
            }

            // Ping 操作の実行
            var ping = new Ping();
            try
            {
                // 結果表示用の文字列
                string resultText = "";
                int transmittedPackets = 0;
                int receivedPackets = 0;
                long totalRoundTripTime = 0;
                long minRoundTripTime = long.MaxValue;
                long maxRoundTripTime = long.MinValue;

                for (int i = 0; i < pingCount; i++)
                {
                    var reply = await ping.SendPingAsync(host, timeout);
                    transmittedPackets++;

                    if (reply.Status == IPStatus.Success)
                    {
                        receivedPackets++;
                        long roundTripTime = reply.RoundtripTime;
                        totalRoundTripTime += roundTripTime;

                        // RTTの最小値と最大値を更新
                        if (roundTripTime < minRoundTripTime)
                            minRoundTripTime = roundTripTime;

                        if (roundTripTime > maxRoundTripTime)
                            maxRoundTripTime = roundTripTime;
                        // 結果のフォーマット
                        //resultText += $"64 bytes from {reply.Address}: icmp_seq={i + 1} ttl={reply.Options.Ttl} time={reply.RoundtripTime} ms\n";

                        resultText += $"64 bytes from {reply.Address}: icmp_seq={i + 1} time={reply.RoundtripTime} ms\n";
                    }
                    else
                    {
                        resultText += $"Ping {i + 1}: Failed. Status: {reply.Status}\n";
                    }
                    Dispatcher.Dispatch(() => ResultLabel.Text = resultText);
                    // 次のPingまで少し待機（ユーザーがすぐに結果を確認できるように）
                    await Task.Delay(500);
                }
                double packetLoss = 100.0 * (transmittedPackets - receivedPackets) / transmittedPackets;
                double avgRoundTripTime = receivedPackets > 0 ? (double)totalRoundTripTime / receivedPackets : 0;
                double mdev = 0;  // mdev（平均偏差）の計算は別途実装が必要ですが、簡単な方法で計算できます。

                // 統計情報のフォーマット
                string statsText = $"\n--- {host} ping statistics ---\n" +
                                   $"{transmittedPackets} packets transmitted, {receivedPackets} received, " +
                                   $"{packetLoss:F0}% packet loss, time {totalRoundTripTime}ms\n" +
                                   $"rtt min/avg/max/mdev = {minRoundTripTime / 1000.0:F3}/{avgRoundTripTime / 1000.0:F3}/{maxRoundTripTime / 1000.0:F3}/{mdev / 1000.0:F3} ms";

                resultText += statsText;
                // 最終的な結果をラベルに表示
                ResultLabel.Text = resultText;
            }
            catch (Exception ex)
            {
                ResultLabel.Text = $"Error: {ex.Message}";
            }
        }
    }
}
