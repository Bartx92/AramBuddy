#region

using System;
using System.IO;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;

#endregion

namespace AramBuddy.AutoShop.Sequences
{
    internal class Buy
    {
        /// <summary>
        ///     Attempts to buy the next item, and continues to buy next items until
        ///     it is no longer allowed to do so.
        /// </summary>
        /// <param name="build">The parsed build</param>
        /// <returns></returns>
        public static bool BuyNextItem(Build build)
        {
            try
            {
                // Check if we've reached the end of the build
                if (build.BuildData.Length < GetIndex() + 1)
                {
                    // Notify the user that the build is finished
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(DateTime.Now.ToString("[H:mm:ss - ") +
                                      "AramBuddy Info] Build is finished - Cannot buy any more items!");
                    Console.ResetColor();

                    // Return false because we could not buy items
                    return false;
                }

                // Get the item
                string itemname = build.BuildData.ElementAt(GetIndex());
                var item = Item.ItemData.FirstOrDefault(i => i.Value.Name == itemname);

                // Check if we can buy the item
                if ((item.Value != null) && (item.Key != null) && (item.Key != ItemId.Unknown) &&
                    item.Value.ValidForPlayer && item.Value.InStore && item.Value.Gold.Purchasable &&
                    item.Value.AvailableForMap && (Player.Instance.Gold >= item.Value.Gold.Total))
                {
                    // Buy the actual item from the shop
                    Shop.BuyItem(item.Key);

                    // Increment the static item index
                    IncrementIndex();

                    // Notify the user that the item has been bought and of the value of the item
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(DateTime.Now.ToString("[H:mm:ss - ") + "AramBuddy Info] Item bought: " + item.Value.Name + " - Item Value: " + item.Value.Gold.Total);
                    Console.ResetColor();

                    // Try to buy more than one item if we can afford it
                    BuyNextItem(build);

                    // Success
                    return true;
                }

                // Fail
                return false;
            }
            catch (Exception ex)
            {
                // Exception has been cought; Notify the user of the error and print the exception to the console
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(DateTime.Now.ToString("[H:mm:ss - ") +
                                  "AramBuddy Error] Exception occurred in AutoShop on buying the next item:" +
                                  Environment.NewLine);
                Console.ResetColor();
                Console.Write(ex);

                // Warn the user that AutoShop may not be functioning correctly
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(
                    DateTime.Now.ToString("[H:mm:ss - ") +
                    "AramBuddy Warning] Exception occurred during AutoShop buy sequence. AutoShop will most likely NOT work properly!");
                Console.ResetColor();
                return false;
            }
        }

        /// <summary>
        ///     Creates an index file in AppData location for storing
        ///     the index of the current item being bought.
        /// </summary>
        public static void CreateIndexFile()
        {
            try
            {
                // Create the temporary files path directory
                Directory.CreateDirectory(Setup.TempPath);

                // If the index file already exists, stop running the method as the user probably wants to continue
                // using their build sequence on its current index
                if (File.Exists(Setup.TempPath + "\\buildindex" + Player.Instance.NetworkId + Game.GameId + ".dat"))
                {
                    return;
                }

                // Create the index file
                using (StreamWriter sw = File.AppendText(Setup.TempPath + "\\buildindex" + Player.Instance.NetworkId + Game.GameId + ".dat"))
                {
                    // Write the default value (0) to the index file
                    sw.Write(0);
                }
            }
            catch (Exception ex)
            {
                // Exception has been cought; Notify the user of the error and print the exception to the console
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(DateTime.Now.ToString("[H:mm:ss - ") + "AramBuddy Error] Exception occurred in AutoShop on creating build index file:" + Environment.NewLine);
                Console.Write(ex);
                Console.ResetColor();

                // Warn the user that AutoShop may not be functioning correctly
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(DateTime.Now.ToString("[H:mm:ss - ") + "AramBuddy Warning] Exception occurred during AutoShop buy sequence. AutoShop will most likely NOT work properly!");
                Console.ResetColor();
            }
        }

        /// <summary>
        ///     Increases the index of the index file by one.
        /// </summary>
        private static void IncrementIndex()
        {
            try
            {
                // Create the temporary files path directory
                Directory.CreateDirectory(Setup.TempPath);

                // The contents of the index file
                string data = File.ReadAllText(Setup.TempPath + "\\buildindex" + Player.Instance.NetworkId + Game.GameId + ".dat");

                // The incremented index of the index file
                int index = int.Parse(data) + 1;

                // Delete the index file
                File.Delete(Setup.TempPath + "\\buildindex" + Player.Instance.NetworkId + Game.GameId + ".dat");

                // Re-write the index file
                using (StreamWriter sw = File.AppendText(Setup.TempPath + "\\buildindex" + Player.Instance.NetworkId + Game.GameId + ".dat"))
                {
                    // Write the new, incremented index on the index file
                    sw.Write(index);
                }
            }
            catch (Exception ex)
            {
                // Exception has been cought; Notify the user of the error and print the exception to the console
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(DateTime.Now.ToString("[H:mm:ss - ") + "AramBuddy Error] Exception occurred in AutoShop on increment build index:" + Environment.NewLine);
                Console.Write(ex);
                Console.ResetColor();

                // Warn the user that AutoShop may not be functioning correctly
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(DateTime.Now.ToString("[H:mm:ss - ") + "AramBuddy Warning] Exception occurred during AutoShop buy sequence. AutoShop will most likely NOT work properly!");
                Console.ResetColor();
            }
        }

        /// <summary>
        ///     Retreives the index from the index file as an integer
        /// </summary>
        /// <returns>The integer stored in the index file.</returns>
        private static int GetIndex()
        {
            try
            {
                // Get the data from the index file
                string data = File.ReadAllText(Setup.TempPath + "\\buildindex" + Player.Instance.NetworkId + Game.GameId + ".dat");

                // return the parsed data to an integer
                return int.Parse(data);
            }
            catch (Exception ex)
            {
                // Exception has been cought; Notify the user of the error and print the exception to the console
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(DateTime.Now.ToString("[H:mm:ss - ") + "AramBuddy Error] Exception occurred in AutoShop on get build index:" + Environment.NewLine);
                Console.Write(ex);
                Console.ResetColor();

                // Warn the user that AutoShop may not be functioning correctly
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(DateTime.Now.ToString("[H:mm:ss - ") + "AramBuddy Warning] Exception occurred during AutoShop buy sequence. AutoShop will most likely NOT work properly!");
                Console.ResetColor();
                return 0;
            }
        }

        /// <summary>
        ///     Resets the index file back to its default value.
        /// </summary>
        public static void ResetIndex()
        {
            try
            {
                // Create the temporary files path directory
                Directory.CreateDirectory(Setup.TempPath);

                // Return if the index file does not exist
                if (!File.Exists(Setup.TempPath + "\\buildindex" + Player.Instance.NetworkId + Game.GameId + ".dat"))
                {
                    return;
                }

                // Delete the index file
                File.Delete(Setup.TempPath + "\\buildindex" + Player.Instance.NetworkId + Game.GameId + ".dat");

                // Rewrite to the index file
                using (StreamWriter sw = File.AppendText(Setup.TempPath + "\\buildindex" + Player.Instance.NetworkId + Game.GameId + ".dat"))
                {
                    // Write the default index file value (0) to the index file
                    sw.Write(0);
                }
            }
            catch (Exception ex)
            {
                // Exception has been cought; Notify the user of the error and print the exception to the console
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(DateTime.Now.ToString("[H:mm:ss - ") + "AramBuddy Error] Exception occurred in AutoShop reset build index:" + Environment.NewLine);
                Console.Write(ex);
                Console.ResetColor();

                // Warn the user that AutoShop may not be functioning correctly
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(DateTime.Now.ToString("[H:mm:ss - ") + "AramBuddy Warning] Exception occurred during AutoShop buy sequence. AutoShop will most likely NOT work properly!");
                Console.ResetColor();
            }
        }
    }
}