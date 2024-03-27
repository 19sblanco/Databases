using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ChessBrowser
{
    internal class ChessGame
    {
        String eventName = "";
        String eventSite = "";
        String round = "";
        String whitePlayer = "";
        String blackPlayer = "";
        uint whiteElo = 0;
        uint blackElo = 0;
        String result = "";
        String eventDate = "";
        String moves = "";

        public ChessGame(string eventName, string eventSite, string round, string whitePlayer, string blackPlayer, uint whiteElo, uint blackElo, string result, string eventDate, string moves)
        {
            this.eventName = eventName;
            this.eventSite = eventSite;
            this.round = round;
            this.whitePlayer = whitePlayer;
            this.blackPlayer = blackPlayer;
            this.whiteElo = whiteElo;
            this.blackElo = blackElo;
            this.result = result;
            this.eventDate = eventDate;
            this.moves = moves;
        }

        string getMoves()
        {
            return this.moves;
        }

        string getEventDate ()
        {
            return this.eventDate;
        }

        String getResult()
        {
            return this.result;
        }

        uint getBlackElo()
        {
            return this.blackElo;
        }

        uint getWhiteElo()
        {
            return this.whiteElo;
        }

        String getBlackPlayer()
        {
            return this.blackPlayer;
        }

        String getWhitePlayer()
        {
            return this.whitePlayer;
        }

        String getEventName()
        {
            return this.eventName;
        }

        String getEventSite()
        {
            return this.eventSite;
        }

        String getRound()
        {
            return this.round;
        }
    }
}
