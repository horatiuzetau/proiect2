using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace proiect2.usefull
{
    public abstract class UsefullMethods
    {
        public static string getOutputTimeBetween(TimeSpan timeSpan)
        {
            if (timeSpan.TotalSeconds == 0)
            {
                return "right now";
            }
            else if (timeSpan.TotalMinutes < 60)
            {
                return Math.Floor(timeSpan.TotalMinutes).ToString() + " minutes ago";
            }
            else if (timeSpan.TotalHours >= 1 && timeSpan.TotalHours < 24)
            {
                return Math.Floor(timeSpan.TotalHours).ToString() + " hours ago";
            }
            else
            {
                return Math.Floor(timeSpan.TotalDays).ToString() + " days ago";
            }
        }
    }
}