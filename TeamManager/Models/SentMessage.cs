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
    
    public partial class SentMessage
    {
        public int SentMessageId { get; set; }
        public int PlayerId { get; set; }
        public int GameId { get; set; }
        public string MessageBody { get; set; }
        public string Sender { get; set; }
        public Nullable<System.DateTime> SentTimeStamp { get; set; }
        public string Response { get; set; }
        public Nullable<System.DateTime> Updated { get; set; }
    
        public virtual Player Player { get; set; }
        public virtual Game Game { get; set; }
    }
}