using Jarvis.Core.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Jarvis.Core.Service
{
    public class SystemSpeechOutput(ILogger<SystemSpeechOutput> logger) : ISpeechOutput
    {
        public async Task SpeakAsync(string text, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    await SpeakWindowsAsync(text, ct);
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    await RunAsync("say", [text], ct);
                else
                    await SpeakLinuxAsync(text, ct);
            }
            catch (Exception ex)
            {
                // Voice is a nice-to-have. Never let it break the conversation.
                logger.LogWarning(ex, "Text-to-speech unavailable, continuing silently");
            }
        }

        private static async Task SpeakWindowsAsync(string text, CancellationToken ct)
        {
            var tmp = Path.Combine(Path.GetTempPath(), $"jarvis-tts-{Guid.NewGuid():N}.txt");
            await File.WriteAllTextAsync(tmp, text, ct);

            try
            {
                var script =
                    "Add-Type -AssemblyName System.Speech; " +
                    "$s = New-Object System.Speech.Synthesis.SpeechSynthesizer; " +
                    $"$s.Speak([IO.File]::ReadAllText('{tmp}'))";

                await RunAsync("powershell", ["-NoProfile", "-NonInteractive", "-Command", script], ct);
            }
            finally
            {
                try { File.Delete(tmp); } catch { /* best effort */ }
            }
        }

        private static async Task SpeakLinuxAsync(string text, CancellationToken ct)
        {
            try
            {
                await RunAsync("spd-say", ["--wait", text], ct);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                await RunAsync("espeak-ng", [text], ct);
            }
        }

        private static async Task RunAsync(string fileName, string[] args, CancellationToken ct)
        {
            var psi = new ProcessStartInfo(fileName)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };

            // ArgumentList escapes each argument properly - never build one big string.
            foreach (var a in args) psi.ArgumentList.Add(a);

            using var process = Process.Start(psi)
                ?? throw new InvalidOperationException($"Could not start {fileName}");

            await process.WaitForExitAsync(ct);
        }
    }
}
