using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FYP_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using FYP_Project.Controllers;
using Microsoft.SqlServer.Server;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.ImgHash;
using Emgu.Util;
using Emgu.CV.CvEnum;


namespace FYP_Project.Controllers
{
    public class GameController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LetsPlay()
        {
            return View("LetsPlay");
        }



        public IActionResult PublicSession()
        {
            return View("PublicSession");
        }

        public IActionResult Display2()
        {
            return View("Display2");
        }



        public string Title { get; set; }
        public void OnGet(string title)
        {
            Title = title;
        }

        //Random Alphanumeric 
        //For Generating 6 character aplhanumeric Game Session Key
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        public IActionResult NewSession()
        {
            return View("NewSession");
        }



        [HttpPost]
        public IActionResult NewSession(NewSession ns)
        {

            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Invalid Input";
                ViewData["MsgType"] = "warning";
                return RedirectToAction("NewSession");
            }

            else
            {


                ns.sessionKey = RandomString(6);
                ViewData["SessionID"] = ns.sessionKey;

                string insert = @"INSERT INTO session(sessionName, sessionKey, gameTime)
                              VALUES( '{0}', '{1}', '{2}')";

                if (DBUtl.ExecSQL(insert, ns.SessionName, ns.sessionKey, ns.gameTime) == 1)
                {

                }

                else
                {
                    ViewData["Message"] = DBUtl.DB_Message;
                    ViewData["MsgType"] = "danger";
                }

            }

            return View("DisplayID");

        }

        [Authorize(Roles = "admin")]
        public IActionResult Sessions()
        {
            List<NewSession> list = DBUtl.GetList<NewSession>("SELECT sessionID, SessionName, sessionKey, gameTime FROM session;");

            int x = list.Count();
            ViewData["All"] = x;
            TempData["All"] = x;


            ViewData["List"] = list;


            return View(list);
        }

        public IActionResult ManageSession()
        {
            Sessions();
            return View("ManageSession");
        }

        public IActionResult Delete(string id)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Invalid!";
                ViewData["MsgType"] = "warning";
                return View();
            }

            else
            {
                string delete = @"DELETE FROM session WHERE SessionKey='{0}'";

                int res = DBUtl.ExecSQL(delete, id);

                if (res == 1)
                {
                    TempData["Message"] = "Session Deleted!";
                    TempData["MsgType"] = "danger";

                }

                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "warning";

                }
                return RedirectToAction("ManageSession");
            }

        }



        public IActionResult JoinSession()
        {
            return View("JoinSession");
        }

        public IActionResult GamePlay()
        {
            return View("PlayerUpload");
        }



        [HttpPost]
        public IActionResult JinSession(NewSession ns)
        {
            if (!AuthenticateUser(ns.sessionKey, out ClaimsPrincipal principal))
            {
                ViewData["Message"] = "Invalid Session ID!";
                ViewData["MsgType"] = "danger";
                return View("JoinSession");
            }
            else
            {

                if (TempData["returnUrl"] != null)
                {
                    string returnUrl = TempData["returnUrl"].ToString();
                    if (Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                }

                return RedirectToAction();
            }
        }

        public IActionResult StartGame()
        {
            return View("Timer");
        }









        private const string KEY_SQL =
         @"SELECT * FROM session 
            WHERE SessionKey = '{0}'";

        private const string KEY_COL = "SessionKey";

        public IActionResult VerifySessionID(string id)
        {
            string select = $"SELECT * FROM session WHERE SessionKey='{id}'";
            if (DBUtl.GetTable(select).Rows.Count > 0)
            {
                return Json($"[{id}] is invalid!");
            }
            return Json(true);
        }

        private bool AuthenticateUser(string sid, out ClaimsPrincipal principal)
        {
            principal = null;

            DataTable ds = DBUtl.GetTable(KEY_SQL, sid);
            if (ds.Rows.Count == 1)
            {



                string key = ds.Rows[0][KEY_COL].ToString();

                HttpContext.Session.SetString("SessionKey", key);


                /*
                 principal =
                    new ClaimsPrincipal(
                       new ClaimsIdentity(
                          new Claim[] {
                         new Claim(ClaimTypes.NameIdentifier, sid),
                         new Claim(ClaimTypes.Name, ds.Rows[0][KEY_COL].ToString()),
                          }, "Basic"
                       )
                    );
                  **/


                return true;
            }
            return false;
        }





        //STARTING THE GAME IF THERE ARE 5 OR MORE PLAYERS.
        public IActionResult SavePlayer1(string sKey)
        {

            string key = @"SELECT SessionKey='{0}' FROM session";
            int countKey = DBUtl.ExecSQL(key, sKey);

            if (countKey > 5)
            {

                return RedirectToAction("PlayerUpload");
            }

            else
            {
                return View("JoinSession");
            }

        }
        /*
        public IActionResult SavePlayer2(Player player) 
        {
            string startFlag = canPlayStart(string sessionKey);

            // save the player
            // based on the start flag i.e. true/ false show the views

        }
        **/
        public IActionResult canPlayStart(string sessionKey)
        {
            //bool startFlag = false;

            string SQL = @"SELECT COUNT(*) from session where SessionKey = '{0}'";

            int res = DBUtl.ExecSQL(SQL, sessionKey);
            if (res == 4)
            {
                TempData["Message"] = "Ready to Play ";
                TempData["MsgType"] = "info";

                return View(StartGame());


            }

            else
            {
                TempData["Message"] = DBUtl.DB_Message;
                TempData["MsgType"] = "warning";

            }


            return RedirectToAction();
        }

        /*


                controllerMethodSave(Player player)
                {


                    startFlag = canPlayStart(String sessionKey);

                    // save the player
                    // based on the start flag i.e. true/ false show the views
                }



                function boolean canPlayStart(String sessionKey)
                {

                    boolean startFlag = false;
                    String SQL = "SELECT COUNT(*) from <<table_name>> where <<SessionKey>> = sessionKey";

                    int rowCount = result from DB;

                    if (rowcount == 4)
                    {
                        startFlag = true;
                        "


        return startFlag;


                    }

            **/









        //
        //      FACE DECTECTION 
        //


        public partial class Form1 : Form
        {
            public Form1()
            {
            }
        }

        //FOR UPLOADING IMAGES TO PUBLIC SESSIONS
        public IActionResult PublicUpload()
        {
            return View("UploadPublic");
        }
        public IActionResult PublicUpload2()
        {
            return View("UploadPublic2");
        }
        public IActionResult PublicUpload3()
        {
            return View("UploadPublic3");
        }

        public IActionResult PhotoPublicUpload(IFormFile picture)
        {
            if (picture == null)
            {
                ViewData["Message"] = "Please Select Photo to Upload!";
                ViewData["MsgType"] = "danger";
                return View("UploadPublic");
            }

            string fname = ImageUpload(picture);
            ViewData["Picture"] = fname;
            ViewData["Message"] = "Photo to Uploaded Successfully!";
            ViewData["MsgType"] = "success";

            return View("UploadPublic");
        }



        private IWebHostEnvironment _env;
        public GameController(IWebHostEnvironment environment)
        {
            _env = environment;
        }


        private string ImageUpload(IFormFile photo)
        {
            string fext = Path.GetExtension(photo.FileName);
            string uname = Guid.NewGuid().ToString();
            string fname = uname + fext;
            string fullpath = Path.Combine(_env.WebRootPath, "photos/" + fname);
            FileStream fs = new FileStream(fullpath, FileMode.Create);
            photo.CopyTo(fs);

            // put in the codes for emgu here
            //1. open the image using fullpath
            Image<Bgr, Byte> img = new Image<Bgr, Byte>(fullpath);


            //2. process the image (convert to grayscale)
            //Most important part, the part you need to adjust (Doing face detection first, try one, two or more)
            Image<Gray, Byte> greyed = img.Convert<Gray, Byte>();
            CascadeClassifier _cascadeClassifier = new CascadeClassifier(@"C:\\Emgu\\emgucv-windesktop 4.2.0.3662\\etc\\haarcascades\\" + "haarcascade_frontalface_alt2.xml");
            var faces = _cascadeClassifier.DetectMultiScale(greyed, 1.1, 10, Size.Empty);
            foreach (var face in faces)
            {
                Rectangle rect = new Rectangle(face.X, face.Y, face.Width, face.Height);
                img.Draw(rect, new Bgr(255, 0, 0), 3);
            }



            //3. save the image to a new filename
            //If it doesnt work, run it, page source (ctrl u), scroll down to find what the path is
            string converting = Path.Combine(_env.WebRootPath, "photos/conversion/" + fname);
            img.Save(converting);
            string converted = "conversion/" + fname;


            //4. return this new filename
            return converted;

            //return fname;
        }
















    }
}