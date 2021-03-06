﻿using Arcadia.Scraper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Arcadia.Emulator.Identifier
{
    public class VBA : IIdentifier
    {
        /// <summary>
        /// Gets all games for Visual Boy Advanced
        /// </summary>
        /// <param name="emulator">The data for the VBA emu</param>
        /// <returns>A list of games for VBA</returns>
        public List<Game> GetGames(Emu emulator)
        {
            List<Game> games = new List<Game>();

            //Create a JSON serializer to read/write the game data json files.
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            //Get all ROM files within the specified directory
            var RomFiles = Directory.GetFiles(Path.Combine(emulator.Location, emulator.RomLocation), emulator.SearchPattern);

            //Go through each rom within the rom directory
            foreach (var rom in RomFiles)
            {
                string gameTitle = "";
                string gameCode = "";

                //Open up each ROM to be read
                using (BinaryReader reader = new BinaryReader(File.Open(rom, FileMode.Open)))
                {
                    if (Path.GetExtension(rom) == ".gba")
                    {
                        //Skip to position 0xA0
                        reader.ReadBytes(0xA0);

                        //Read the Game Title (0xA0 - 0xAB)
                        gameTitle = Encoding.ASCII.GetString(reader.ReadBytes(12)).Replace("\0", "").Replace(" ", "");
                        //Read the Game Code (0xAC - 0xAF)
                        gameCode = Encoding.ASCII.GetString(reader.ReadBytes(4));
                    }
                    else if (Path.GetExtension(rom) == ".gbc")
                    {
                        reader.ReadBytes(0x134);
                        
                        gameTitle = Encoding.ASCII.GetString(reader.ReadBytes(11)).Replace("\0", "").Replace(" ", "");
                        gameCode = Encoding.ASCII.GetString(reader.ReadBytes(4));
                    }
                }

                //Create the gameName for the directory
                string gameName = gameTitle + "-" + gameCode;

                //Find the data for the rom within the data directory
                string infoJsonPath = Path.Combine("Games", "Data", gameName, "data.json");

                //If the data doesn't exist, we need to look it up online
                if (!File.Exists(infoJsonPath))
                {
                    //Create the directory for the game data to be held in
                    if (!Directory.Exists(Path.GetDirectoryName(infoJsonPath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(infoJsonPath));

                    //Create a new Game object to hold all the game data
                    Game romGame = new Game();

                    //The data directory is located where we specified it
                    romGame.DataDirectory = Path.GetDirectoryName(infoJsonPath);
                    romGame.Location = rom;
                    
                    //Get the name of the rom through Nintendo Age
                    romGame.Name = NintendoAgeScraper.IdentifyGameByGameCode(NintendoAgeConsole.GameboyAdvance, gameCode);

                    Log.Write("Adding information for game \"" + romGame.Name + "\"");

                    //Get the game data from GameDB
                    romGame = GameDBScraper.UpdateGameData(romGame);
                    
                    //Write the game data to the data directory
                    File.WriteAllText(infoJsonPath, serializer.Serialize(romGame));
                }

                Game game = serializer.Deserialize<Game>(File.ReadAllText(infoJsonPath));

                //Add the game to the game list
                if (!game.Skip)
                    games.Add(game);
            }

            return games;
        }
     }
}
