using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.AspNet.Identity;
using TImerXml.Models;

namespace TImerXml.Controllers
{
    public class HomeController : Controller
    {
        //Index page
        public ActionResult Index()
        {
            return View();
        }

        //Add new user GET
        public ActionResult AddUSer()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddUSer(TimerUser timerUser)
        {
            var destPath = Server.MapPath("~/XmlDataBase/Timerdb.xml");
            if (!System.IO.File.Exists(destPath))
            {
                CreateXmlUser(destPath);
            }
            else
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(destPath);
                var valueResult = CheckUser(xmlDoc, timerUser.TimerUserEmail);
                if (valueResult)
                {
                    //Tempdata
                    TempData["status"] = "error";
                    TempData["msg"] = "The Email already Exist";

                    return View();
                }
                var subRoot = xmlDoc.CreateElement("TimerTask");
                var eEmail = xmlDoc.CreateElement("Email");
                var eName = xmlDoc.CreateElement("Name");

                eEmail.InnerText = timerUser.TimerUserEmail;
                eName.InnerText = timerUser.Name;

                subRoot.AppendChild(eEmail);
                subRoot.AppendChild(eName);
                xmlDoc.DocumentElement?.AppendChild(subRoot);
                xmlDoc.Save(destPath);

                Session["UserClass"] = timerUser;
                TempData["status"] = "success";
                TempData["msg"] = "You are successfully signed in!";
                TempData["myData"] = timerUser;
                return RedirectToAction("AddTask");
            }
            return View();
        }

        //Add new Task
        public ActionResult AddTask()
        {
            if (Session["UserClass"] == null) return RedirectToAction("AddUSer");
            TimerUser user = (TimerUser)Session["UserClass"];
            ViewBag.WelcomeNote = "Welcome " + user.Name + "!";
            ViewBag.Email = user.TimerUserEmail;
            return View();
        }

        [HttpPost]
        public ActionResult AddTask(TimerTask timerTask, string email)
        {
            try
            {
                var destPath = Server.MapPath("~/XmlDataBase/Timerdb.xml");
                XElement root = XElement.Load(destPath);

                IEnumerable<XElement> xElement = root.Elements();
                if (xElement != null)
                {
                    var test = xElement.FirstOrDefault(x =>
                    {
                        var element = x.Element("Email");
                        return element != null && element.Value == email;
                    });
                    test?.Add(new XElement("AddedTask",
                        new XElement("NameofTask", timerTask.NameOfTask),
                        new XElement("Description", timerTask.Description),
                        new XElement("ElaspedTime", timerTask.Timer)
                        ));
                }
                root.Save(destPath);
                return RedirectToAction("CounterView", new { id = timerTask.Timer });
            }
            catch (Exception ex)
            {
                TempData["status"] = "error";
                TempData["msg"] = ex.InnerException;
                return View();
            }
        }


        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(TimerUser timerUser)
        {
            var destPath = Server.MapPath("~/XmlDataBase/Timerdb.xml");
            if (!System.IO.File.Exists(destPath))
            {
                return RedirectToAction("AddUSer");
            }
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(destPath);
            var valueResult = CheckUser(xmlDoc, timerUser.TimerUserEmail);
            if (valueResult)
            {
                var email = (from e in xmlDoc.DocumentElement.ChildNodes.Cast<XmlNode>()
                             where (string)e.SelectSingleNode("Email").InnerText == timerUser.TimerUserEmail
                             select new TimerUser
                             {
                                 TimerUserEmail = e.FirstChild.InnerText,
                                 Name = e.LastChild.InnerText
                             }).FirstOrDefault();
                Session["UserClass"] = email;
                TempData["status"] = "success";
                TempData["msg"] = "Wow! You're successfully logged in!";

                return RedirectToAction("AddTask");
            }

            TempData["status"] = "error";
            TempData["msg"] = "User doesn't exist";
            return RedirectToAction("AddUser");
        }

        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Index");
        }

        public ActionResult ViewAllTasks(string email)
        {
            if (Session["UserClass"] == null)
            {
                return RedirectToAction("AddUser");
            }
            TimerUser user = (TimerUser)Session["UserClass"];
            var destPath = Server.MapPath("~/XmlDataBase/Timerdb.xml");
            XElement root = XElement.Load(destPath);
            var listTasks = from LT in root.Elements("TimerTask")
                            where (string)LT.Element("Email") == user.TimerUserEmail
                            select LT;
            var listTaskENum = (from e in listTasks.Elements()
                                let xElement = e.Element("NameofTask")
                                where xElement != null
                                let element = e.Element("Description")
                                where element != null
                                let xElement1 = e.Element("ElaspedTime")
                                where xElement1 != null
                                select new TimerTask
                                {
                                    NameOfTask = xElement.Value,
                                    Description = element.Value,
                                    Timer = xElement1.Value
                                }).ToList();

            return View(listTaskENum);
        }

        public ActionResult CounterView(string id)
        {
            if (id != null)
            {
                ViewBag.timer = id;
                return View();
            };
            return RedirectToAction("AddTask");
        }

        public bool CheckUser(XmlDocument xmlDoc, string email)
        {

            var allEmails = xmlDoc.GetElementsByTagName("Email");
            if (allEmails.Count > 0)
            {
                for (int i = 0; i < allEmails.Count; i++)
                {
                    if (allEmails[i].InnerXml.ToLower() == email.ToLower())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void CreateXmlUser(string destPath)
        {
            XmlDocument myDocument = new XmlDocument();
            XmlDeclaration declaration = myDocument.CreateXmlDeclaration("1.0", null, null);
            myDocument.AppendChild(declaration);
            XmlElement rooElement = myDocument.CreateElement("Users");
            myDocument.AppendChild(rooElement);
            myDocument.Save(destPath);
        }
    }
}