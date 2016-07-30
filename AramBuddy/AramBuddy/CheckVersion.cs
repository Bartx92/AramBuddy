using System;
using System.Net;
using AramBuddy.MainCore.Utility;
using EloBuddy;
using Version = System.Version;

namespace AramBuddy
{
    internal class CheckVersion
    {
        private static string UpdateMsg = string.Empty;

        private const string UpdateMsgPath = "https://raw.githubusercontent.com/plsfixrito/AramBuddy/master/AramBuddy/AramBuddy/msg.txt";

        private const string WebVersionPath = "https://raw.githubusercontent.com/plsfixrito/AramBuddy/master/AramBuddy/AramBuddy/Properties/AssemblyInfo.cs";

        private static readonly Version CurrentVersion = typeof(CheckVersion).Assembly.GetName().Version;

        public static void Init()
        {
            try
            {
                var WebClient = new WebClient();
                WebClient.DownloadStringCompleted += delegate(object sender, DownloadStringCompletedEventArgs args) { UpdateMsg = args.Result; };
                WebClient.DownloadStringTaskAsync(UpdateMsgPath);

                var WebClient2 = new WebClient();
                WebClient2.DownloadStringCompleted += delegate(object sender, DownloadStringCompletedEventArgs args)
                    {
                        if (!args.Result.Contains(CurrentVersion.ToString()))
                        {
                            Logger.Send("There is a new Update Available for AramBuddy!", Logger.LogLevel.Warn);
                            Chat.Print("<b>AramBuddy: There is a new Update Available for AramBuddy !</b>");
                            Logger.Send(UpdateMsg, Logger.LogLevel.Info);
                            Chat.Print("<b>AramBuddy: " + UpdateMsg + "</b>");
                        }
                        else
                        {
                            Logger.Send("Your AramBuddy is updated !", Logger.LogLevel.Info);
                        }
                    };
                WebClient2.DownloadStringTaskAsync(WebVersionPath);
            }
            catch (Exception ex)
            {
                Logger.Send("Failed To Check for Updates !", ex, Logger.LogLevel.Error);
            }
        }
    }
}
