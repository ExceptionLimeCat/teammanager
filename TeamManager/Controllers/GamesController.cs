using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using TeamManager.Models;

namespace TeamManager.Controllers
{
    public class GamesController : Controller
    {
        private SoccerEntities db = new SoccerEntities();
        //private string confirmUrl = "http://alexpitts.net/TeamManager/Games/ConfirmAttendance/";
        private string confirmUrl = System.Configuration.ConfigurationManager.AppSettings["ConfirmUrl"];
        
        #region Public Methods

        // GET: Games
        public ActionResult Index()
        {
            return View(db.Games.ToList());
        }

        // GET: Games/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            GameViewModel gmv = new GameViewModel();
            Game game = db.Games.Find(id);
            if (game == null)
            {
                return HttpNotFound();
            }
            else
            {
                gmv.Game = game;                
                
                gmv.PlayersIn = GetConfirmedPlayers(gmv.Game.GameId);                

                gmv.LastSentMsg =
                    (from msg in db.SentMessages
                     where msg.GameId == gmv.Game.GameId
                     orderby msg.SentTimeStamp descending
                     select msg).FirstOrDefault();                

            }

            return View(gmv);
        }

        // GET: Games/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Games/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "GameId,GameDateTime,Location,Updated,UpdatedBy,SendEmail")] Game game)
        {
            if (ModelState.IsValid)
            {
                //Game game = gmv.Game;
                game.Updated = DateTime.Now;
                //TODO: Get current UserName here
                game.UpdatedBy = "alex";
                db.Games.Add(game);
                db.SaveChanges();

                if(game.SendEmail)
                {
                    List<Player> recipients = db.Players.ToList();
                    SendEmail(game, recipients);
                }

                return RedirectToAction("Index");
            }

            return View(game);
        }

        // GET: Games/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            GameViewModel gmv = new GameViewModel();
            Game game = db.Games.Find(id);
            if (game == null)
            {
                return HttpNotFound();
            }
            else
            {
                gmv.Game = game;
                gmv.Player_Game = new Player_Game() { GameId = game.GameId };

                gmv.PlayersIn = GetConfirmedPlayers(gmv.Game.GameId);

                gmv.PlayersUnconfirmed = GetUnconfirmedPlayers(gmv.Game.GameId);
            }
            return View(gmv);
        }

        // POST: Games/Edit/5        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "GameId,GameDateTime,Location,Updated,UpdatedBy")] Game game)
        {
            if (ModelState.IsValid)
            {
                db.Entry(game).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(game);
        }

        // GET: Games/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Game game = db.Games.Find(id);
            if (game == null)
            {
                return HttpNotFound();
            }
            return View(game);
        }

        // POST: Games/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Game game = db.Games.Find(id);

            foreach (SentMessage sm in game.SentMessages.ToList())
                db.Entry(sm).State = EntityState.Deleted;

            db.Entry(game).State = EntityState.Deleted;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult ConfirmAttendance(int id)
        {
            int searchId = id < 0 ? id * -1 : id;
            SentMessage sm = db.SentMessages.Find(searchId);

            if (id > 0)
            {
                Player_Game pg = ConfirmPlayer(sm.PlayerId, sm.GameId);
                sm.Updated = DateTime.Now;
                sm.Response = "1";
                db.Entry(sm).State = EntityState.Modified;                
            }
            else
            {
                Player p = db.Players.Find(sm.PlayerId);

                //Remove confirmations if previously responded Yes
                List<Player_Game> pgsToDelete =
                        (from pg in p.Player_Game
                         where pg.GameId == sm.GameId
                         select pg).ToList();

                foreach(Player_Game pg1 in pgsToDelete)
                {
                    db.Entry(pg1).State = EntityState.Deleted;
                }

                sm.Updated = DateTime.Now;
                sm.Response = "-1";
                db.Entry(sm).State = EntityState.Modified;                
            }

            db.SaveChanges();

            return RedirectToAction("Details", new { id = sm.GameId });
        }

        // POST: Games/ConfirmPlayer        
        [HttpPost]        
        public ActionResult ConfirmPlayer([Bind(Include = "GameId,PlayerId")] Player_Game player_game)//(int? gameId, int? playerId)
        {
            if (ModelState.IsValid)
            {
                //Player_Game player_game = new Player_Game();                
                player_game.Updated = DateTime.Now;
                player_game.UpdatedBy = "alex";
                db.Player_Game.Add(player_game);
                db.SaveChanges();

                Player p = db.Players.Find(player_game.PlayerId);

                StringBuilder html = new StringBuilder();
                html.Append("<li onclick=\"selectConfirmedElm(this)\" class=\"list-group-item\" data-player-id=\"");
                html.Append(player_game.PlayerId + "\">");
                html.Append(p.FullName);
                html.Append("</li>");

                return Content(html.ToString());
                //return Json(player_game);
                //return RedirectToAction("Edit", new { id = player_game.GameId });
            }

            return RedirectToAction("Index");
        }

        // POST: Games/UnconfirmPlayer        
        [HttpPost]
        public ActionResult UnconfirmPlayer([Bind(Include = "Player_Game_Id,GameId,PlayerId")] Player_Game player_game)
        {
            if (ModelState.IsValid)
            {
                Player_Game pg = db.Player_Game.Find(player_game.Player_Game_Id);
                db.Player_Game.Remove(pg);                
                db.SaveChanges();

                Player p = db.Players.Find(player_game.PlayerId);

                StringBuilder html = new StringBuilder();
                html.Append("<li onclick=\"selectAvailElm(this)\" class=\"list-group-item\" data-player-id=\"");
                html.Append(player_game.PlayerId + "\">");
                html.Append(p.FullName);
                html.Append("</li>");

                return Content(html.ToString());
            }

            return RedirectToAction("Index");
        }

        public ActionResult EmailUnconfirmed(int id)
        {                        
            Game game = db.Games.Find(id);
            List<Player> recipients = GetUnconfirmedPlayers(id);
            SendEmail(game, recipients);

            return RedirectToAction("Details", new { id = id });
        }

        #endregion

        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods
        //This probably shoud just return List<Player_Game>
        private List<ConfirmedPlayer> GetConfirmedPlayers(int gameId)
        {
            List<ConfirmedPlayer> confirmedPlayers =
                (from pg in db.Player_Game
                 join player in db.Players on pg.PlayerId equals player.PlayerId
                 where pg.GameId == gameId
                 select new ConfirmedPlayer()
                 {
                     Player_Game_Id = pg.Player_Game_Id,
                     PlayerId = player.PlayerId,
                     FirstName = player.FirstName,
                     LastName = player.LastName,
                     Phone = player.Phone,
                     Email = player.Email,
                     Paid = player.Paid,
                     ShirtSize = player.ShirtSize,
                     ShortsSize = player.ShortsSize,
                     Updated = player.Updated,
                     UpdatedBy = player.UpdatedBy
                 }).ToList();

            return confirmedPlayers;
        }

        private List<Player> GetConfirmedOutPlayers(int gameId)
        {
            try
            {
                List<Player> PlayersOut =
                                    (from player in db.Players
                                     from sm in db.SentMessages
                                     where sm.GameId == gameId
                                     && player.PlayerId == sm.PlayerId
                                     && sm.Response == "-1"
                                     select player).ToList();

                return PlayersOut;
            }
            catch(Exception ex)
            {
                throw new Exception("GetConfirmedOutPlayers", ex);
            }
        }

        private List<Player> GetUnconfirmedPlayers(int gameId)
        {
            try
            {
                List<ConfirmedPlayer> PlayersIn = GetConfirmedPlayers(gameId);

                List<Player> PlayersOut = GetConfirmedOutPlayers(gameId);

                List<Player> PlayersUnconfirmed =
                    (from player in db.Players
                     where player.IsActive == true
                     select player).ToList();

                var excludedIDs = new HashSet<int>(PlayersIn.Select(p => p.PlayerId));
                PlayersUnconfirmed = PlayersUnconfirmed.Where(p => !excludedIDs.Contains(p.PlayerId)).ToList();

                excludedIDs = new HashSet<int>(PlayersOut.Select(p => p.PlayerId));
                PlayersUnconfirmed = PlayersUnconfirmed.Where(p => !excludedIDs.Contains(p.PlayerId)).ToList();

                return PlayersUnconfirmed;
            }
            catch (Exception ex)
            {
                throw new Exception("GetUnconfirmedPlayers", ex);
            }
       }

        private void SendEmail(Game game, List<Player> recipients)
        {
            try
            {
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.Credentials = new System.Net.NetworkCredential("soccertm04@gmail.com", "futbolito");
                client.EnableSsl = true;
                MailAddress from = new MailAddress("TeamManager@alexpitts.net",
                    "Team Manager",
                        System.Text.Encoding.UTF8);

                foreach (Player r in recipients)
                {
                    MailAddress to = new MailAddress(r.Email);

                    SentMessage msg = new SentMessage();
                    msg.GameId = game.GameId;
                    msg.PlayerId = r.PlayerId;
                    msg.MessageBody = string.Empty;
                    msg.Response = "0";
                    //TODO: Get current user here
                    msg.Sender = "alex";
                    msg.SentTimeStamp = DateTime.Now;
                    msg.Updated = DateTime.Now;
                    db.SentMessages.Add(msg);
                    db.SaveChanges();

                    MailMessage message = new MailMessage(from, to);
                    message.IsBodyHtml = true;
                    message.Subject = r.FullName + " Please Confirm Attendence";
                    message.Body = CreateEmailMsg(game, r, msg.SentMessageId);
                    // Use the application or machine configuration to get the 
                    // host, port, and credentials.                
                    client.Send(message);

                    
                }
            }
            catch(Exception ex)
            {
                throw new Exception("SendEmail", ex);
            }            
        }

        private string CreateEmailMsg(Game game, Player recipient, int sentMessageId)
        {
            StringBuilder sb = new StringBuilder();

            string emailStyle = "<STYLE>BODY{font-family: Segoe UI,Frutiger,Frutiger Linotype,Dejavu Sans,Helvetica Neue,Arial,sans-serif;}TABLE{border: solid 1px #b9b9b9;border-collapse:collapse;}TD{border: solid 1px #b9b9b9;padding:5px;}TD.LBL{background:#f2f2f2;font-weight:bold;}</STYLE>";

            sb.Append("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 3.2//EN\"><HTML><HEAD><META NAME=\"Generator\" CONTENT=\"MS Exchange Server version 08.01.0240.003\"><TITLE></TITLE>");
            sb.Append(emailStyle);
            sb.Append("</HEAD><BODY><P>Hello ");
            sb.Append(recipient.FullName);
            sb.Append(",</P>");
            sb.Append("<P>Please confirm your attendance at the following game:</P>");
            sb.Append("<TABLE><TR>");
            sb.Append("<TD class=\"LBL\">Location</TD>");
            sb.Append("<TD class=\"LBL\">Time</TD>");            
            sb.Append("</TR>");
            sb.Append("<TR><TD>");
            sb.Append(game.Location);
            sb.Append("</TD><TD>");
            sb.Append(game.GameDateTime); 
            sb.Append("</TD></TR>");
            sb.Append("</TABLE>");
            sb.Append("<BR>");
            sb.Append("<A href=\"" + confirmUrl);
            sb.Append(sentMessageId);
            sb.Append("\">Yes</A> | ");
            sb.Append("<A href=\"" + confirmUrl + "-");
            sb.Append(sentMessageId);
            sb.Append("\">No</A>");
            sb.Append("</BODY></HTML>");

            return sb.ToString();
        }

        private Player_Game ConfirmPlayer(int playerid, int gameid)
        {
            Player_Game pg = 
                (from p_g in db.Player_Game
                 where p_g.PlayerId == playerid && p_g.GameId == gameid
                 select p_g).FirstOrDefault();

            if (pg == null)
            {

                Player p = db.Players.Find(playerid);

                //If responsed No to previous email and now has had a change 
                //of heart.
                List<SentMessage> msgs =
                        (from sm in p.SentMessages
                         where sm.GameId == gameid
                         && sm.Response == "-1"
                         select sm).ToList();

                foreach(SentMessage sm1 in msgs)
                {
                    sm1.Response = "0";
                    sm1.Updated = DateTime.Now;
                    db.Entry(sm1).State = EntityState.Modified;
                }

                //Confirm Player
                Player_Game player_game = new Player_Game();
                player_game.PlayerId = playerid;
                player_game.GameId = gameid;
                player_game.Updated = DateTime.Now;                

                player_game.UpdatedBy = p.FullName;


                db.Player_Game.Add(player_game);
                db.SaveChanges();

                player_game = (from pg1 in db.Player_Game
                               where pg1.GameId == gameid
                                  && pg1.PlayerId == playerid
                               select pg1).FirstOrDefault();

                return player_game;
            }
            else
                return (Player_Game)null;
        }

        #endregion
    }
}
