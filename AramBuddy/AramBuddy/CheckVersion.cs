using System;
using System.Drawing;
using System.Net;
using AramBuddy.MainCore.Utility;
using EloBuddy;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;
using Version = System.Version;

namespace AramBuddy
{
    internal class CheckVersion
    {
        private static Text text;
        private static string UpdateMsg = string.Empty;
        private const string UpdateMsgPath = "https://raw.githubusercontent.com/plsfixrito/AramBuddy/master/AramBuddy/AramBuddy/msg.txt";
        private const string WebVersionPath = "https://raw.githubusercontent.com/plsfixrito/AramBuddy/master/AramBuddy/AramBuddy/Properties/AssemblyInfo.cs";
        private static readonly Version CurrentVersion = typeof(CheckVersion).Assembly.GetName().Version;
        public static bool Outdated;
        private static bool Sent;
        public static void Init()
        {
            try
            {
                var size = Drawing.Width <= 400 || Drawing.Height <= 400 ? 10F : 40F;
                text = new Text("YOUR ARAMBUDDY IS OUTDATED", new Font("Euphemia", size, FontStyle.Bold)) { Color = Color.White };
                var WebClient = new WebClient();
                WebClient.DownloadStringCompleted += delegate(object sender, DownloadStringCompletedEventArgs args) { UpdateMsg = args.Result; };
                WebClient.DownloadStringTaskAsync(UpdateMsgPath);

                var WebClient2 = new WebClient();
                WebClient2.DownloadStringCompleted += delegate(object sender, DownloadStringCompletedEventArgs args)
                    {
                        if (!args.Result.Contains(CurrentVersion.ToString()))
                        {
                            Drawing.OnEndScene += delegate
                                {
                                    text.Position = new Vector2(Camera.ScreenPosition.X, Camera.ScreenPosition.Y + 75);
                                    text.Draw();
                                };
                            Outdated = true;
                            Logger.Send("There is a new Update Available for AramBuddy!", Logger.LogLevel.Warn);
                            Logger.Send(UpdateMsg, Logger.LogLevel.Info);
                        }
                        else
                        {
                            Logger.Send("Your AramBuddy is updated !", Logger.LogLevel.Info);
                        }
                    };
                WebClient2.DownloadStringTaskAsync(WebVersionPath);

                Game.OnTick += delegate
                {
                    if (UpdateMsg != string.Empty && !Sent && Outdated)
                    {
                        Chat.Print("<b>AramBuddy: There is a new Update Available for AramBuddy !</b>");
                        Chat.Print("<b>AramBuddy: " + UpdateMsg + "</b>");
                        Sent = true;
                    }
                };
            }
            catch (Exception ex)
            {
                Logger.Send("Failed To Check for Updates !", ex, Logger.LogLevel.Error);
            }
        }
    }
}
