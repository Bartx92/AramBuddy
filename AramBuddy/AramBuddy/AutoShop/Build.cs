// <summary>
//   The class containing the BuildData used by the interpreter to buy items in order
// </summary>
using System;
using System.IO;
using System.Linq;
using System.Net;
using AramBuddy.MainCore.Utility;
using EloBuddy;

namespace AramBuddy.AutoShop
{
    /// <summary>
    ///     The class containing the BuildData used by the interpreter to buy items in order
    /// </summary>
    public class Build
    {
        /// <summary>
        ///     An array of the item names
        /// </summary>
        public string[] BuildData { get; set; }

        /// <summary>
        /// returns The build name.
        /// </summary>
        public static string BuildName()
        {
            var ChampionName = Player.Instance.ChampionName;

            if (ADC.Contains(ChampionName))
            {
                return "ADC";
            }

            if (AD.Contains(ChampionName))
            {
                return "AD";
            }

            if (AP.Contains(ChampionName))
            {
                return "AP";
            }

            if (ManaAP.Contains(ChampionName))
            {
                return "ManaAP";
            }

            if (Tank.Contains(ChampionName))
            {
                return "Tank";
            }

            Logger.Send("Failed To Detect " + ChampionName, Logger.LogLevel.Warn);
            Logger.Send("Using Default Build !", Logger.LogLevel.Info);
            return "Default";
        }

        /// <summary>
        ///     Creates Builds
        /// </summary>
        public static void Create()
        {
            try
            {
                GetResponse(
                    WebRequest.Create("https://raw.githubusercontent.com/plsfixrito/AramBuddy/master/DefaultBuilds/" + BuildName() + ".json"),
                    response =>
                        {
                            var data = new StreamReader(response.GetResponseStream()).ReadToEnd().ToString();
                            if (data.Contains("data"))
                            {
                                File.WriteAllText(Setup.BuildPath + "\\" + BuildName() + ".json", data);
                                Setup.Builds.Add(BuildName(), File.ReadAllText(Setup.BuildPath + "\\" + BuildName() + ".json"));

                                Logger.Send(BuildName() + " Build Created for " + Player.Instance.ChampionName + " - " + BuildName(), Logger.LogLevel.Info);
                            }
                            else
                            {
                                Logger.Send("Wrong Response, No Champion Build Created", Logger.LogLevel.Warn);
                                Console.WriteLine(data);
                            }
                        });
            }
            catch (Exception ex)
            {
                // if faild to create build terminate the AutoShop
                Logger.Send("Failed to create default build for " + Player.Instance.ChampionName, ex, Logger.LogLevel.Error);
                Logger.Send("No build is currently being used!", Logger.LogLevel.Error);
            }
        }

        /// <summary>
        /// Sends and get response from web
        /// </summary>
        private static void GetResponse(WebRequest Request, Action<HttpWebResponse> ResponseAction)
        {
            try
            {
                Action wrapperAction = () =>
                    {
                        Request.BeginGetResponse(
                            iar =>
                                {
                                    var Response = (HttpWebResponse)((HttpWebRequest)iar.AsyncState).EndGetResponse(iar);
                                    ResponseAction(Response);
                                },
                            Request);
                    };
                wrapperAction.BeginInvoke(
                    iar =>
                        {
                            var Action = (Action)iar.AsyncState;
                            Action.EndInvoke(iar);
                        },
                    wrapperAction);
            }
            catch (Exception ex)
            {
                Logger.Send("Failed to create default build, No Response.", ex, Logger.LogLevel.Error);
            }
        }

        /// <summary>
        ///  ADC Champions.
        /// </summary>
        public static readonly string[] ADC =
            {
                "Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Jhin", "Jinx", "Kalista", "Kindred", "KogMaw", "Lucian", "MissFortune", "Sivir", "Quinn", "Tristana",
                "Twitch", "Urgot", "Varus", "Vayne"
            };

        /// <summary>
        ///  Mana AP Champions.
        /// </summary>
        public static readonly string[] ManaAP =
            {
                "Ahri", "Anivia", "Annie", "AurelioSol", "Azir", "Brand", "Cassiopeia", "Diana", "Elise", "Ekko", "Evelynn", "Fiddlesticks", "Fizz", "Galio",
                "Gragas", "Heimerdinger", "Janna", "Karma", "Karthus", "Kassadin", "Kayle", "Leblanc", "Lissandra", "Lulu", "Lux", "Malzahar", "Morgana", "Nami",
                "Nidalee", "Ryze", "Orianna", "Sona", "Soraka", "Swain", "Syndra", "Taliyah", "Teemo", "TwistedFate", "Veigar", "Viktor", "VelKoz", "Xerath", "Ziggs",
                "Zilean", "Zyra"
            };

        /// <summary>
        ///  AP no Mana Champions.
        /// </summary>
        public static readonly string[] AP = { "Akali", "Katarina", "Kennen", "Mordekaiser", "Rumble", "Vladimir" };

        /// <summary>
        ///  AD Champions.
        /// </summary>
        public static readonly string[] AD =
            {
                "Aatrox", "Darius", "Fiora", "Gangplank", "Jax", "Jayce", "KhaZix", "LeeSin", "MasterYi", "Nocturne", "Olaf", "Pantheon", "RekSai", "Renekton", "Rengar",
                "Riven", "Talon", "Tryndamere", "Wukong", "XinZhao", "Yasuo", "Zed"
            };

        /// <summary>
        ///  Tank Champions.
        /// </summary>
        public static readonly string[] Tank =
            {
                "Alistar", "Amumu", "Blitzcrank", "Bard", "Braum", "ChoGath", "DrMundo", "Garen", "Gnar", "Hecarim", "Illaoi", "Irelia", "JarvanIV", "Leona",
                "Malphite", "Maokai", "Nasus", "Nautilus", "Nunu", "Poppy", "Rammus", "Sejuani", "Shaco", "Shen", "Shyvana", "Singed", "Sion", "Skarner", "TahmKench",
                "Taric", "Thresh", "Trundle", "Udyr", "Vi", "Volibear", "Warwick", "Yorick", "Zac"
            };
    }
}
