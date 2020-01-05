using proiect2.Models.hory;
using proiect2.usefull;
using System;

namespace proiect2.Models.usefull
{
    public class UserListItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public RequestStatus Status { get; set; }

        public UserListItem() { }
        public UserListItem(ApplicationUser user, string currentId)
        {
            Id = user.Id;
            Name = user.UserName;
            //Vedem daca e pending sau sent
            Status = user.getFRType(currentId);
        }

    }



    public class FriendRequestListItem
    {
        public string Name { get; set; }
        public string Id { get; set; }

        //Aici putem pune timpul de cand a fost trimisa cererea
        public TimeSpan TimeBetween { get; set; }
        public virtual string OutputTimeBetween
        {
            get
            {
                return UsefullMethods.getOutputTimeBetween(TimeBetween);
            }
        }

        public FriendRequestListItem(string name, string id, DateTime date)
        {
            Id = id;
            Name = name;
            TimeBetween = DateTime.Now - date;
        }

    }
}