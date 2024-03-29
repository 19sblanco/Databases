﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Text.RegularExpressions;

namespace ChessBrowser
{
    internal class PngReader
    {
        List<ChessGame> games;

        PngReader()
        {
            games = new List<ChessGame>();
        }

        /// <summary>
        /// read a file from pngPath, parse it and create a list of ChessGames objects
        /// </summary>
        /// <param name="pngPath"></param>
        public void readFile(String pngPath)
        {
            using (StreamReader reader = new StreamReader(pngPath))
            {
                string pattern = "\"([^\"]*)\"";
                string line;
                string eventTag = "";
                string site = "";
                string round = "";
                string whitePlayer = "";
                string blackPlayer = "";
                string result = "";
                uint whiteElo = 0;
                uint blackElo = 0;
                string eventDate = "";
                string moves = "";


                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("[Event "))
                    {
                        Match match = Regex.Match(line, pattern);
                        eventTag = match.Groups[1].Value;
                    }
                    if (line.StartsWith("[Site"))
                    {
                        Match match = Regex.Match(line, pattern);
                        site = match.Groups[1].Value;
                        if (site.Contains("?"))
                        {
                            site = "?";

                        }
                    }
                    if (line.StartsWith("[Round"))
                    {
                        Match match = Regex.Match(line, pattern);
                        round = match.Groups[1].Value;
                    }
                    if (line.StartsWith("[White "))
                    {
                        Match match = Regex.Match(line, pattern);
                        whitePlayer = match.Groups[1].Value;
                    }
                    if (line.StartsWith("[Black "))
                    {
                        Match match = Regex.Match(line, pattern);
                        blackPlayer = match.Groups[1].Value;
                    }
                    if (line.StartsWith("[Result"))
                    {
                        Match match = Regex.Match(line, pattern);
                        result = match.Groups[1].Value;
                        if (result == "1/2-1/2")
                        {
                            result = "D";
                        }
                        else if (result == "1-0")
                        {
                            result = "W";
                        }
                        else if (result == "0-1")
                        {
                            result = "B";
                        }
                    }
                    if (line.StartsWith("[WhiteElo"))
                    {
                        Match match = Regex.Match(line, pattern);
                        whiteElo = uint.Parse(match.Groups[1].Value);
                    }
                    if (line.StartsWith("[BlackElo"))
                    {
                        Match match = Regex.Match(line, pattern);
                        blackElo = uint.Parse(match.Groups[1].Value);
                    }
                    if (line.StartsWith("[EventDate"))
                    {
                        Match match = Regex.Match(line, pattern);
                        eventDate = match.Groups[1].Value;
                        eventDate = eventDate.Replace(".", "-");
                        if (eventDate.Contains("?"))
                        {
                            eventDate = "0000-00-00";
                            Console.WriteLine(eventDate);
                        }
                    }
                    if (line.StartsWith("1."))
                    {
                        moves += line;
                        while ((line = reader.ReadLine()) != "" && line != null)
                        {
                            moves += line;
                        }

                        ChessGame game = new ChessGame(eventTag, site, round, whitePlayer, blackPlayer, whiteElo, blackElo, result, eventDate, moves);
                        this.games.Add(game);

                        moves = "";
                    }
                }
            }
        }

        /// <summary>
        /// return the list of games
        /// </summary>
        /// <returns></returns>
        List<ChessGame> getGames()
        {
            return games;
        }


    }
}
