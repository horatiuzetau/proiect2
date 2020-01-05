using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using proiect2.Models.hory;

namespace proiect2.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    { 
        public virtual ICollection<FriendRequest> FriendRequestsSent { get; set; }
        public virtual ICollection<FriendRequest> FriendRequestsReceived { get; set; }
        public virtual ICollection<FriendShip> Friended { get; set; }
        public virtual ICollection<FriendShip> BeFriended { get; set; }

        //Metodele astea 3 ajuta la identificarea tipului de cerere dintre Userul current logat, cel folosit ca parametru
        //si userul pentru care se face verificarea : cel preluat dintr-o lista, preluata si ea din bazad de date.
        public RequestStatus getFRType(string id)
        {
            var a = doesFRReceivedExists(id);
            var b = doesFRSentExists(id);

            if (a == ExistStatus.ACCEPTED || b == ExistStatus.ACCEPTED)
                return RequestStatus.ACCEPTED;
            else if (a == ExistStatus.EXIST)
                return RequestStatus.SENT;
            else if (b == ExistStatus.EXIST)
                return RequestStatus.PENDING;
            else
                return RequestStatus.NULL;
        }
        private ExistStatus doesFRSentExists(string id)
        {
            //Trecem prin toate requesturile trimise
            foreach (var fr in FriendRequestsSent)
                if (fr.ToUserId == id)
                    if (fr.Status == RequestStatus.ACCEPTED)
                        return ExistStatus.ACCEPTED;
                    else
                        return ExistStatus.EXIST;
            //Daca nu exista in requesturile primite niciun request de la userul din parametru,  
            return ExistStatus.DONT_EXIST;
        }
        public ExistStatus doesFRReceivedExists(string id)
        {
            //Trecem prin toate requesturile primite
            foreach(var fr in FriendRequestsReceived)
                if(fr.FromUserId == id)
                    if (fr.Status == RequestStatus.ACCEPTED)
                        return ExistStatus.ACCEPTED;
                    else
                        return ExistStatus.EXIST;

            //Daca nu exista in requesturile trimsie niciun request de la userul din parametru,  
            return ExistStatus.DONT_EXIST;
        }

        public enum ExistStatus { 
            EXIST, DONT_EXIST, ACCEPTED
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FriendShip>().HasKey(m => new { m.UserId, m.FriendId });
            modelBuilder.Entity<FriendRequest>().HasKey(m => new { m.FromUserId, m.ToUserId });

            //Incerc sa explic cu cuvintele mele ce se intampla mai jos
            //Tehnica este luata de pe internet, insa zic eu inteleasa
            //Aici zic: ApplicationUser are cereri de prietenie trimise daca el este FromUser
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(m => m.FriendRequestsSent)
                .WithRequired(s => s.FromUser)
                .WillCascadeOnDelete(false);

            //Aici zic: ApplicationUser primeste cerere de prietenie daca el este ToUser
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(m => m.FriendRequestsReceived)
                .WithRequired(s => s.ToUser)
                .WillCascadeOnDelete(false);

            //Zic aici asa: ApplicationUser are mai multe relatii de prietenie, in care el este userul
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(m => m.Friended)
                .WithRequired(s => s.User)
                .WillCascadeOnDelete(false);

            //Iar aici: ApplicationUser are mai multe relatii de prietenie, in care el este prietenul
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(m => m.BeFriended)
                .WithRequired(s => s.Friend)
                .WillCascadeOnDelete(false);
            
            

            base.OnModelCreating(modelBuilder);

        }

        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<FriendShip> FriendShips { get; set; }
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}