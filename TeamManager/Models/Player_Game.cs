//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TeamManager.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Player_Game
    {
        public int Player_Game_Id { get; set; }
        public Nullable<int> PlayerId { get; set; }
        public Nullable<int> GameId { get; set; }
        public Nullable<System.DateTime> Updated { get; set; }
        public string UpdatedBy { get; set; }
    
        public virtual Player Player { get; set; }
        public virtual Game Game { get; set; }
    }
}