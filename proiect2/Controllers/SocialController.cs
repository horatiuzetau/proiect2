using Microsoft.AspNet.Identity;
using proiect2.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using proiect2.usefull;
using proiect2.Models.usefull;
using System.Data.Entity.Infrastructure;
using proiect2.Models.hory;

namespace proiect2.Controllers
{


    public class SocialController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        //Vezi lista de prieteni - Se mai lucreaza
        [HttpGet]
        public ActionResult SeeFriends()
        {

            //Pot lua prietenii fie asa, fie sa iau userul respectiv si sa includ prietenii. Mai vedem
            string currentId = User.Identity.GetUserId();
            var friends = db.FriendShips.Include(m => m.Friend)
                .Where(m => m.UserId == currentId)
                .Select(m => m.Friend);

            ViewBag.Friends = friends;
            return View();
        }

        //Vezi friend requesturile, asta e nefolosita intrucat se vad si in index
        [HttpGet]
        public ActionResult SeeFriendRequests()
        {
            string id = User.Identity.GetUserId();

            //Luam friend requesturile din baza de date; trebuie sa indeplineasca regulile:
            //-----sa aibe ToUserId == ID userului logat
            //-----sa aibe FromUserId != ID userlui logat
            //-----sa aiba statusul PENDING

            var friendRequests = db.FriendRequests
                .Where(m => m.ToUserId == id && m.FromUserId != id && m.Status == RequestStatus.PENDING)
                .Select(m => new { Name = m.FromUser.UserName, Id = m.FromUserId, m.Date })
                .ToList();

            //Pentru a transmite informatiain view, cream o lista custom 
            List<FriendRequestListItem> friendRequestsList = new List<FriendRequestListItem>();

            foreach (var fr in friendRequests)
            {
                friendRequestsList.Add(new FriendRequestListItem(fr.Name, fr.Id, fr.Date));
            }

            ViewBag.FriendRequests = friendRequestsList;
            return View();
        }

        //Toti userii inafara de cel curent. Arata situatia in raport cu fiecare. (prieteni, invitatie trimisa, invitatie primita)
        [HttpGet]
        public ActionResult Index()
        {
            //Id-ul userului logat
            var currentId = User.Identity.GetUserId();
            //Query spre baza de date
            //Asa includ si prietenii userului scos din baza de date
            var users = db.Users
                //.Include(m => m.Friended.Select(y => y.Friend))
                .Include(m => m.FriendRequestsSent).Include(m => m.FriendRequestsReceived)
                .Where(m => m.Id != currentId);

            //Cream lista pentru output
            List<UserListItem> userList = new List<UserListItem>();
            foreach(var item in users)
            {
                userList.Add(new UserListItem(item, currentId));
                /*
                 * Afisam relatia de prietenie, adica prietenii fiecarui user prin lista de prieteni, nu prin query la prietenii
                 * 
                foreach(var jo in item.Friended)
                {
                    Debug.WriteLine("Friend of  " + item.UserName + " - " + jo.Friend.UserName);
                }
                Debug.WriteLine("");
                */
            }
            ViewBag.Users = userList;


            //Adaugam mesajul daca exista
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Mesaj = TempData["message"];
            }
            return View();
        }
        
        //Accept friend request
        [NonAction]
        public void AddToFriends(string joe, string moe)
        {

            //Relatie de prietenie din ambele parti
            FriendShip relOne = new FriendShip();
            relOne.UserId = joe;
            relOne.FriendId = moe;

            FriendShip relTwo = new FriendShip();
            relTwo.UserId = moe;
            relTwo.FriendId = joe;

            try
            {
                db.FriendShips.Add(relOne);
                db.FriendShips.Add(relTwo);
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        //Delete from friend list
        [HttpDelete]
        public ActionResult RemoveFromFriends(string friendId)
        {
            string currentId = User.Identity.GetUserId();

            try
            {
                var friendShipsBetween = db.FriendShips
                    .Where(m => (m.UserId == currentId && m.FriendId == friendId) || (m.UserId == friendId && m.FriendId == m.UserId));

                var friendRequestsBetween = db.FriendRequests
                    .Where(m => (m.ToUserId == currentId && m.FromUserId == friendId) || (m.ToUserId == friendId && m.FromUserId == currentId));

                db.FriendShips.RemoveRange(friendShipsBetween);
                db.FriendRequests.RemoveRange(friendRequestsBetween);
                db.SaveChanges();

                TempData["message"] = "Ai scapat de acest prieten!";
                return RedirectToAction("Index");
            }catch(Exception e)
            {
                TempData["message"] = e.Message;
                return RedirectToAction("Index");
            }

        }
        
        [HttpPut]
        public ActionResult AcceptFriendRequest(string fromId)
        {
            //Id curent al userului logat
            string currentId = User.Identity.GetUserId();

            try
            {
                //Updatam statusul friend requestului in ACCEPTED
                FriendRequest fr = db.FriendRequests
                    .Where(m => m.FromUserId == fromId && m.ToUserId == currentId)
                    .FirstOrDefault();

                if (TryUpdateModel(fr))
                {
                    fr.Status = RequestStatus.ACCEPTED;
                }
                else
                {
                    throw new Exception("Nu se poate updata statusul cererii");
                }

                //Adaugam relatia de prietenie in baza de date
                AddToFriends(currentId, fromId);
                db.SaveChanges();

                TempData["message"] = "Suntei prieteni acum! Bravo!";
                return RedirectToAction("Index");
            }
            catch(DbUpdateException e)
            {
                TempData["message"] = e.InnerException;
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                TempData["message"] = e.Message;
                return RedirectToAction("Index");
            }

        }

        //Decline friend request
        [HttpDelete]
        public ActionResult DeclineFriendRequest(string fromId)
        {
            //Id to, cel logat
            string currentId = User.Identity.GetUserId();

            try
            {
                FriendRequest fr = db.FriendRequests
                   .Where(m => m.FromUserId == fromId && m.ToUserId == currentId)
                   .FirstOrDefault();

                db.FriendRequests.Remove(fr);
                db.SaveChanges();
                TempData["message"] = "Ai resins cererea de prietenie. Asta e! Nu are de ce sa se supere.";
                return RedirectToAction("Index");
            }
            catch(Exception e)
            {
                TempData["message"] = e.Message;
                return RedirectToAction("Index");
            }


        }
        
        //Send friend request
        [HttpPost]
        public ActionResult SendFriendRequest(string toId)
        {
            //From
            string fromId = User.Identity.GetUserId();

            //Verify if the friend request already exists
            var existsAlready = db.FriendRequests
                .Where(m => (m.FromUserId == fromId && m.ToUserId == toId) || (m.FromUserId == toId && m.ToUserId == fromId))
                .FirstOrDefault();

            //The friend request
            FriendRequest friendRequest = new FriendRequest();
            try
            {
                if (existsAlready != null)
                    throw new Exception("O cerere de prietenie intre voi exista deja!");

                if (toId == fromId)
                {
                    throw new Exception("Deja esti prietenul tau! Lupta se desfasoara in interior, nu pe platforma noastra.");
                }

                //The user to whom we send the request
                ApplicationUser ToUser = db.Users.Find(toId);
                //-------from
                ApplicationUser FromUser = db.Users.Find(fromId);

                friendRequest.ToUserId = toId;
                friendRequest.FromUserId = fromId;

                friendRequest.Date = DateTime.Now;
                friendRequest.Status = RequestStatus.PENDING;
                //friendRequest.Status = RequestStatus.PENDING;
                db.FriendRequests.Add(friendRequest);
                db.SaveChanges();

                TempData["message"] = "Cerere trimisa cu succes! Sa speram ca o sa accepte!";
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                TempData["message"] = e.Message;
                return RedirectToAction("Index");
            }
        }

        //Cancel friend request
        [HttpDelete]
        public ActionResult CancelFriendRequest(string toId)
        {
            //Id from
            string currentId = User.Identity.GetUserId();

            //Cautam in FriendRequests unul cu from = currentId si to = toId
            try
            {
                FriendRequest fr = db.FriendRequests
                    .Where(m => m.FromUserId == currentId && m.ToUserId == toId)
                    .FirstOrDefault();

                db.FriendRequests.Remove(fr);
                db.SaveChanges();

                TempData["message"] = "Ai anulat cererea cu succes! Poate te-ai razgandit, e ghinionul lui.";
                return RedirectToAction("Index");
            }
            catch (Exception e) {
                TempData["message"] = e.Message;
                return RedirectToAction("Index");
            }


        }


    }


}