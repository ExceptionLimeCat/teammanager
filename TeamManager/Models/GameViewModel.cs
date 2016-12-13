using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TeamManager.Models
{
    public class GameViewModel
    {
        public Game Game { get; set; }
        public IEnumerable<string> SelectedPlayers { get; set; }
        public IEnumerable<SelectListItem> Players { get; set; }
        public List<ConfirmedPlayer> PlayersIn { get; set; }        
        public List<Player> PlayersUnconfirmed { get; set; }
        public Player_Game Player_Game { get; set; }
        public bool SendEmail { get; set; }   
        public SentMessage LastSentMsg { get; set; }
        public ConfirmedPlayer ConfirmedPlayer { get; set; }    
    }

    public class ConfirmedPlayer : Player
    {
        public int Player_Game_Id { get; set; }        
    }
}