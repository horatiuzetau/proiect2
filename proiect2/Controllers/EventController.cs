using Microsoft.AspNet.Identity;
using proiect2.Models;
using proiect2.Models.alex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace proiect2.Controllers
{
    public class EventController : Controller
    {
        private ApplicationDbContext db = ApplicationDbContext.Create();

        public ActionResult All()
        {
            var events = db.Events.OrderByDescending(m => m.Date).ToList();
            ViewBag.Events = events; 

            return View();
        }

        [Authorize(Roles = "User,Host,Administrator")]
        public ActionResult Index()
        {
            var events = db.Events.Include("Category").Include("User");
            ViewBag.Events = events;

            return View();
        }

        [Authorize(Roles = "User,Host,Administrator")]
        public ActionResult Show(int id)
        {
            Event eve = db.Events.Find(id);
            ViewBag.Article = eve;
            ViewBag.Category = eve.Category;
            ViewBag.showButtons = false;
            if (User.IsInRole("Host") || User.IsInRole("Administrator"))
            {
                ViewBag.showButtons = true;
            }
            ViewBag.isAdmin = User.IsInRole("Administrator");
            ViewBag.currentUser = User.Identity.GetUserId();

            return View(eve);
        }

        [Authorize(Roles = "Host,Administrator")]
        public ActionResult New()
        {
            Event eve = new Event();
            eve.Categories = GetAllCategories();
            eve.UserId = User.Identity.GetUserId();
            return View(eve);
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            var selectList = new List<SelectListItem>();
            var categories = from cat in db.Categories
                             select cat;
            foreach (var category in categories)
            {
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.Name.ToString()
                });
            }
            return selectList;
        }

        [HttpPost]
        [Authorize(Roles = "Host,Administrator")]
        public ActionResult New(Event eve)
        {
            var categories = from cat in db.Categories select cat;
            ViewBag.Categories = categories;

            try
            {
                db.Events.Add(eve);
                db.SaveChanges();
                TempData["messageAdd"] = "Evenimentul a fost adaugat!";

                return Redirect("/Event/Show/" + eve.Id);
            }
            catch (Exception e)
            {
                return View();
            }
        }

        [Authorize(Roles = "Host,Administrator")]
        public ActionResult Edit(int id)
        {
            Event eve = db.Events.Find(id);
            ViewBag.Event = eve;
            eve.Categories = GetAllCategories();

            if (eve.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                return View(eve);
            }
            else
            {
                TempData["messageAuth"] = "Nu aveti dreptul sa faceti modificari asupra unui eveniment care nu va apartime!";
                return RedirectToAction("Index");
            }
        }

        [HttpPut]
        [Authorize(Roles = "Host,Administrator")]
        public ActionResult Edit(int id, Event requestEvent)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Event eve = db.Events.Find(id);

                    if (eve.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                    {
                        if (TryUpdateModel(eve))
                        {
                            eve.Title = requestEvent.Title;
                            eve.Content = requestEvent.Content;
                            eve.Date = requestEvent.Date;
                            eve.CategoryId = requestEvent.CategoryId;
                            db.SaveChanges();
                            TempData["messageEdit"] = "Evenimentul a fost modificat.";
                        }
                        return Redirect("/Event/Show/" + id);
                    }
                    else
                    {
                        TempData["messageEdit"] = "Nu aveti dreptul sa faceti modificari asupra unui eveniment care nu va apartime!";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return View(requestEvent);
                }
            }
            catch (Exception e)
            {
                return View(requestEvent);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Host,Administrator")]
        public ActionResult Delete(int id)
        {
            Event eve = db.Events.Find(id);

            if (eve.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                db.Events.Remove(eve);
                db.SaveChanges();
                TempData["messageDelete"] = "Evenimentul a fost sters cu succes.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["messageDelete"] = "Nu aveti dreptul sa stergeti un eveniment care nu va apartine.";
                return RedirectToAction("Index");
            }

        }
    }
}