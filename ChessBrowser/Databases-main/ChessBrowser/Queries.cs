using Microsoft.Maui.Controls;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage.Search;
using System.Diagnostics;

/*
  Author: Daniel Kopta and ...
  Chess browser backend 
*/

namespace ChessBrowser
{
    internal class Queries
    {

        /// <summary>
        /// This function runs when the upload button is pressed.
        /// Given a filename, parses the PGN file, and uploads
        /// each chess game to the user's database.
        /// </summary>
        /// <param name="PGNfilename">The path to the PGN file</param>
        internal static async Task InsertGameData(string PGNfilename, MainPage mainPage)
        {
            // This will build a connection string to your user's database on atr,
            // assuimg you've typed a user and password in the GUI
            string connection = mainPage.GetConnectionString();

            // TODO:
            //       Load and parse the PGN file
            //       We recommend creating separate libraries to represent chess data and load the file
            PngReader read = new PngReader();
            read.readFile(PGNfilename);
            List<ChessGame> games = read.getGames();

            // TODO:
            //       Use this to tell the GUI's progress bar how many total work steps there are
            //       For example, one iteration of your main upload loop could be one work step
            //mainPage.SetNumWorkItems( ... );
            mainPage.SetNumWorkItems(games.Count);


            using (MySqlConnection conn = new MySqlConnection(connection))
            {
                try
                {
                    // Open a connection
                    conn.Open();

                    // Insertion for Player
                    // insert into Players(Name, Elo) values(whitePlayer, Elo)
                    //    on duplicate key update elo = if (current elo > database elo, current elo, database elo);
                    MySqlCommand CommandPlayer = conn.CreateCommand();
                    CommandPlayer.CommandText = "insert into Players(Name, Elo) values(@val1, @val2) "
                                                    + "on duplicate key update Elo = if(@val2 > Elo, @val2, Elo); "
                                                    + "insert into Players(Name, Elo) values(@val3, @val4) "
                                                    + "on duplicate key update Elo = if(@val4 > Elo, @val2, Elo)";
                    CommandPlayer.Parameters.AddWithValue("@val1", "");
                    CommandPlayer.Parameters.AddWithValue("@val2", 0);
                    CommandPlayer.Parameters.AddWithValue("@val3", "");
                    CommandPlayer.Parameters.AddWithValue("@val4", 0);
                    CommandPlayer.Prepare();

                    // Insertion for Event
                    // insert ignore into Events(Name, Site, Date) values(x, y, z)
                    MySqlCommand CommandEvent = conn.CreateCommand();
                    CommandEvent.CommandText = "insert ignore into Events(Name, Site, Date) values(@val1, @val2, @val3);";
                    CommandEvent.Parameters.AddWithValue("@val1", "");
                    CommandEvent.Parameters.AddWithValue("@val2", "");
                    CommandEvent.Parameters.AddWithValue("@val3", "0000-00-00");
                    CommandEvent.Prepare();

                    // Insertion for Games: Round, Result, Moves, BlackPlayer, WhitePlayer, eID
                    // insert ignore into Games values(@val1, @val2, @val3, (select pID from Player where Name = @val4),
                    //      (select pID from Player where Name = @val5),
                    //      (select eID from Events where Name = @val6 and Site = @val7 and Date = @val8));
                    MySqlCommand CommandGames = conn.CreateCommand();
                    CommandGames.CommandText = "insert ignore into Games values(@val1, @val2, @val3, (select pID from Players where Name = @val4), "
                        + "(select pID from Players where Name = @val5), " 
                        + "(select eID from Events where Name = @val6 and Site = @val7 and Date = @val8));";
                    CommandGames.Parameters.AddWithValue("@val1", "");
                    CommandGames.Parameters.AddWithValue("@val2", "");
                    CommandGames.Parameters.AddWithValue("@val3", "");
                    CommandGames.Parameters.AddWithValue("@val4", "");
                    CommandGames.Parameters.AddWithValue("@val5", "");
                    CommandGames.Parameters.AddWithValue("@val6", "");
                    CommandGames.Parameters.AddWithValue("@val7", "");
                    CommandGames.Parameters.AddWithValue("@val8", "");
                    CommandGames.Prepare();
                    // TODO:
                    //       iterate through your data and generate appropriate insert commands
                    foreach (ChessGame x in games)
                    {
                        //Insert into database both players
                        CommandPlayer.Parameters["@val1"].Value = x.getWhitePlayer();
                        CommandPlayer.Parameters["@val2"].Value = x.getWhiteElo();
                        CommandPlayer.Parameters["@val3"].Value = x.getBlackPlayer();
                        CommandPlayer.Parameters["@val4"].Value = x.getBlackElo();
                        CommandPlayer.ExecuteNonQuery();

                        //Insert into database Event
                        CommandEvent.Parameters["@val1"].Value = x.getEventName();
                        CommandEvent.Parameters["@val2"].Value = x.getEventSite();
                        CommandEvent.Parameters["@val3"].Value = x.getEventDate();
                        CommandEvent.ExecuteNonQuery();

                        //Insert into database Games
                        CommandGames.Parameters["@val1"].Value = x.getRound();
                        CommandGames.Parameters["@val2"].Value = x.getResult();
                        CommandGames.Parameters["@val3"].Value = x.getMoves();
                        CommandGames.Parameters["@val4"].Value = x.getBlackPlayer();
                        CommandGames.Parameters["@val5"].Value = x.getWhitePlayer();
                        CommandGames.Parameters["@val6"].Value = x.getEventName();
                        CommandGames.Parameters["@val7"].Value = x.getEventSite();
                        CommandGames.Parameters["@val8"].Value = x.getEventDate();
                        CommandGames.ExecuteNonQuery();

                        // TODO:
                        //       Use this inside a loop to tell the GUI that one work step has completed:
                        await mainPage.NotifyWorkItemCompleted();
                    }

                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }

        }


        /// <summary>
        /// Queries the database for games that match all the given filters.
        /// The filters are taken from the various controls in the GUI.
        /// </summary>
        /// <param name="white">The white player, or null if none</param>
        /// <param name="black">The black player, or null if none</param>
        /// <param name="opening">The first move, e.g. "1.e4", or null if none</param>
        /// <param name="winner">The winner as "W", "B", "D", or null if none</param>
        /// <param name="useDate">True if the filter includes a date range, False otherwise</param>
        /// <param name="start">The start of the date range</param>
        /// <param name="end">The end of the date range</param>
        /// <param name="showMoves">True if the returned data should include the PGN moves</param>
        /// <returns>A string separated by newlines containing the filtered games</returns>
        internal static string PerformQuery(string white, string black, string opening,
          string winner, bool useDate, DateTime start, DateTime end, bool showMoves,
          MainPage mainPage)
        {
            // This will build a connection string to your user's database on atr,
            // assuimg you've typed a user and password in the GUI
            string connection = mainPage.GetConnectionString();

            //Added this
            //connection += "ConvertZeroDateTime=True;";

            // Build up this string containing the results from your query
            string parsedResult = "";

            // Use this to count the number of rows returned by your query
            // (see below return statement)
            int numRows = 0;

            using (MySqlConnection conn = new MySqlConnection(connection))
            {
                try
                {
                    // Open a connection
                    conn.Open();

                    // TODO:
                    //       Generate and execute an SQL command,
                    //       then parse the results into an appropriate string and return it.

                    MySqlCommand querey = conn.CreateCommand();

                    //SQL command 
                    //select Events.Name, Site, Events.Date, whiteName, whiteElo, blackName, blackElo, Result
                    //from Games join(select white.Name as whiteName, white.Elo as whiteElo, white.pID as whiteID, black.Name as blackName,
                    //black.Elo as blackElo, black.pID as blackID from Players as white join Players as black where white.pid != black.pid)as pairs
                    //natural join Events where whiteID = Games.WhitePlayer and blackID = Games.BlackPlayer;

                    querey.CommandText = "select Events.Name, Site, Events.Date, whiteName, whiteElo, blackName, blackElo, Result";

                    //Build querey based on what is wanted
                    //Check for Moves
                    if(showMoves == true)
                    {
                        querey.CommandText += ", Moves";
                    }

                    // Build rows of data from tables
                    querey.CommandText += " from Games join(select white.Name as whiteName, white.Elo as whiteElo, white.pID as whiteID, "
                                            + "black.Name as blackName, black.Elo as blackElo, black.pID as blackID from Players as white "
                                            + "join Players as black where white.pid != black.pid)as pairs natural join Events where whiteID = "
                                            + "Games.WhitePlayer and blackID = Games.BlackPlayer";

                    // Add in User specifications
                    if(white != null) //if user specifies white player
                    {
                        querey.CommandText += " and whiteName = @val1";
                        querey.Parameters.AddWithValue("@val1", "");
                    }
                    if(black != null)
                    {
                        querey.CommandText += " and blackName = @val2";
                        querey.Parameters.AddWithValue("@val2", "");
                    }
                    if (opening != null)
                    {
                        querey.CommandText += " and Moves like @val3";
                        querey.Parameters.AddWithValue("@val3", "");
                    }
                    if(winner != null)
                    {
                        querey.CommandText += " and Result = @val4";
                        querey.Parameters.AddWithValue("@val4", "");
                    }
                    if(useDate == true)
                    {
                        querey.CommandText += " and Date between @val5 and @val6";
                        querey.Parameters.AddWithValue("@val5", "");
                        querey.Parameters.AddWithValue("@val6", "");
                    }

                    //jclose out the querey
                    querey.CommandText += ";";

                    // Fill in User querey
                    if(white != null)
                    {
                        querey.Parameters["@val1"].Value = white;
                    }
                    if(black != null)
                    {
                        querey.Parameters["@val2"].Value = black;
                    }
                    if(opening != null)
                    {
                        querey.Parameters["@val3"].Value = opening + "%";
                    }
                    if(winner != null)
                    {
                        querey.Parameters["@val4"].Value = winner;
                    }
                    if(useDate == true)
                    {
                        querey.Parameters["@val5"].Value = start;
                        querey.Parameters["@val6"].Value = end;
                    }

                    Debug.WriteLine(querey.CommandText);

                    //Execute user querey
                    using(MySqlDataReader reader = querey.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Format output
                            parsedResult += "Events: " + reader["Name"] + "\n";
                            parsedResult += "Site: " + reader["Site"] + "\n";
                            try {
                                parsedResult += "Date: " + reader["Date"] + "\n";
                            }
                            catch(Exception e)
                            {
                                parsedResult += "Date: " + "00/00/0000 12:00:00 AM" + "\n";
                            }
                            
                            parsedResult += "White: " + reader["whiteName"] + " (" + reader["whiteElo"] + ")\n";
                            parsedResult += "Black: " + reader["blackName"] + " (" + reader["blackElo"] + ")\n";
                            parsedResult += "Result: " + reader["Result"] + "\n";
                            parsedResult += "\n";

                            //If moves are requested
                            if(showMoves == true)
                            {
                                parsedResult += reader["Moves"] + "\n\n";
                            }

                            numRows++;
                            
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }

            return numRows + " results\n\n" + parsedResult;
        }

    }
}
