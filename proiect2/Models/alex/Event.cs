using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace proiect2.Models.alex
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        //Titlul
        [Required]
        [DataType(DataType.Text)]
        public string Title { get; set; }

        //Continutul
        //[Required]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }

        //Data
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }



        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }
        
        //Categorii
        //Aici ar putea fi implementa un sistem de categorii - subcategorii
        //iar userul va alege din toate subcategoriile, asadar va apartine unei categorii oricum
        //e mai mult aici, intrucat va fi un many to many
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
        
        //Pentru view
        public IEnumerable<SelectListItem> Categories { get; set; }
    }
}