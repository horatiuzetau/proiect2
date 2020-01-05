using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace proiect2.Models.hory
{
    public class FriendRequest
    {

        //To
        public string ToUserId { get; set; }
        public virtual ApplicationUser ToUser { get; set; }

        //From
        public string FromUserId { get; set; }
        
        public virtual ApplicationUser FromUser { get; set; }

        //Date
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public RequestStatus Status { get; set; }
    }

    public enum RequestStatus
    {
        PENDING, ACCEPTED, SENT, NULL
    }
}