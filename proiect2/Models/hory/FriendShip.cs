using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace proiect2.Models.hory
{
    public class FriendShip
    {
        /*
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        */
        //Nu stiam cum sa ii numesc: friend, user / user1, user2? Am ales sa ii numesc Joe si Moe, tovarasii.
        public string UserId { get; set; }
        public virtual ApplicationUser User{ get; set; }

        public string FriendId{ get; set; }
        public virtual ApplicationUser Friend{ get; set; }



        [DataType(DataType.Date)]
        public DateTime Since { get; set; }

        public FriendShip() { Since = DateTime.Now; }
    }
}