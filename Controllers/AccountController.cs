using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Data;
using System.Security.Claims;
using FYP_Project.Models;
using FYP_Project.Controllers;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.SqlServer.Server;
using Microsoft.AspNetCore.Http;

namespace FYP_Project.Controllers
{
    public class AccountController : Controller
    {

        private const string LOGIN_SQL =
         @"SELECT * FROM Player 
            WHERE UserName = '{0}' 
              AND PlayerPw = HASHBYTES('SHA1', '{1}')";

        private const string LASTLOGIN_SQL =
           @"UPDATE Player SET LastLogin=GETDATE() WHERE UserName='{0}'";

        private const string ROLE_COL = "Role";
        private const string NAME_COL = "Name";

        private const string REDIRECT_CNTR = "Account";
        private const string REDIRECT_ACTN = "PlayerDashboard";

        private const string LOGIN_VIEW = "LogIn";


        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            TempData["ReturnUrl"] = returnUrl;
            return View(LOGIN_VIEW);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(PlayerLogin user)
        {
            if (!AuthenticateUser(user.UserName, user.Password, out ClaimsPrincipal principal))
            {
                ViewData["Message"] = "Incorrect User ID or Password";
                ViewData["MsgType"] = "danger";
                return View(LOGIN_VIEW);
            }
            else
            {
                HttpContext.SignInAsync(
                   CookieAuthenticationDefaults.AuthenticationScheme,
                   principal,
               new AuthenticationProperties
               {
                   IsPersistent = user.RememberMe
               });

                // Update the Last Login Timestamp of the User
                DBUtl.ExecSQL(LASTLOGIN_SQL, user.UserName);

                if (TempData["returnUrl"] != null)
                {
                    string returnUrl = TempData["returnUrl"].ToString();
                    if (Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                }

                ViewData["name"] = user.UserName;


                return RedirectToAction(REDIRECT_ACTN, REDIRECT_CNTR);
            }
        }

        [Authorize]
        public IActionResult Logoff(string returnUrl = null)
        {

            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");

        }

        [AllowAnonymous]
        public IActionResult Forbidden()
        {
            return View();
        }


        public IActionResult ManageAccount()
        {
            return View("ManageAccount");
        }
        public IActionResult ListPlayers()
        {
            List<Player> list = DBUtl.GetList<Player>("SELECT * FROM Player;");
            return View(list);
        }


        public IActionResult ManageAccount2(string plyr)
        {
          //var p = HttpContext.Session.GetString("UserName");

           //string Psql = "SELECT Name, UserName, Email FROM Player WHERE Role ='player' and UserName='{0}';";
           string Psql = "SELECT Name, UserName, Email FROM Player WHERE Role ='player' and UserName='gavinBelson';";

           // DataTable dt = DBUtl.GetTable(Psql, p, plyr);
            DataTable dt = DBUtl.GetTable(Psql, plyr);
            if (dt.Rows.Count == 1)
            {
                string name = dt.Rows[0]["Name"].ToString();
                string email = dt.Rows[0]["Email"].ToString();
                string username = dt.Rows[0]["UserName"].ToString();


                TempData["Name"] = name;
                ViewData["Email"] = email;
                ViewData["UserName"] = username;

           
            }

            return View("ManageAccount");
        }






        [AllowAnonymous]
        public IActionResult Register()
        {
            return View("SignUp");
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Register(Player usr)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Invalid Input";
                ViewData["MsgType"] = "warning";
                return View("SignUp");
            }
            else
            {
                string insert =
                   @"INSERT INTO Player(UserName, PlayerPw, Name, Email, Role) VALUES
                 ('{0}',HASHBYTES('SHA1', '{1}'), '{2}', '{3}', 'player')";
                if (DBUtl.ExecSQL(insert, usr.UserName, usr.PlayerPw, usr.Name, usr.Email, usr.UserRole) == 1)
                {
                    /*string template = @"Hi {0},<br/><br/>
                               Welcome to THE FACE GAME!
                               Your username is <b>{1}</b> and password is <b>{2}</b>.";
                    string title = "Registration Successul - Welcome";
                    string message = String.Format(template, usr.Name, usr.Name, usr.PlayerPw);
                    string result;
                    if (EmailUtl.SendEmail(usr.Email, title, message, out result))
                    {
                        ViewData["Message"] = "Player Successfully Registered";
                        ViewData["MsgType"] = "success";
                    }
                    else
                    {
                        ViewData["Message"] = result;
                        ViewData["MsgType"] = "warning";
                    }*/


                }
                else
                {
                    ViewData["Message"] = DBUtl.DB_Message;
                    ViewData["MsgType"] = "danger";
                }

                return View("PlayerDashboard");
            }
        }

        [AllowAnonymous]
        public IActionResult VerifyUserID(string userName)
        {
            string select = $"SELECT * FROM Player WHERE UserName='{userName}'";
            if (DBUtl.GetTable(select).Rows.Count > 0)
            {
                return Json($"[{userName}] already in use");
            }
            return Json(true);
        }

        private bool AuthenticateUser(string uid, string pw, out ClaimsPrincipal principal)
        {
            principal = null;

            DataTable ds = DBUtl.GetTable(LOGIN_SQL, uid, pw);
            if (ds.Rows.Count == 1)
            {

                string name = ds.Rows[0][NAME_COL].ToString();
                string role = ds.Rows[0][ROLE_COL].ToString();

                HttpContext.Session.SetString("Name", name);

                HttpContext.Session.SetString("Role", role);

                /*
                 principal =
                    new ClaimsPrincipal(
                       new ClaimsIdentity(
                          new Claim[] {
                         new Claim(ClaimTypes.NameIdentifier, uid),
                         new Claim(ClaimTypes.Name, ds.Rows[0][NAME_COL].ToString()),
                         new Claim(ClaimTypes.Role, ds.Rows[0][ROLE_COL].ToString())
                          }, "Basic"
                       )
                    );
                    **/


                return true;
            }
            return false;
        }



        //[Authorize(Roles = "admin")]
        public IActionResult Users()
        {
            List<Player> list = DBUtl.GetList<Player>("SELECT PlayerID, UserName, Name, Email, Rank, LastLogin FROM player WHERE Role='player';");


            int j = list.Count();
            ViewData["Total"] = j;
            TempData["Total"] = j;


            ViewData["List"] = list;
            return View(list);
        }

     

        public IActionResult PlayerDashboard(PlayerLogin user)
        {

            var role = HttpContext.Session.GetString("Role");

            if (role == "admin")
            {
                Users();
                return View("AdminDashboard");
            }

            else
            {
                
                return View("PlayerDashboard");
            }


        }



        public IActionResult Delete(string id)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Invalid!";
                ViewData["MsgType"] = "warning";
                return View(REDIRECT_ACTN);
            }

            else
            {
                string delete = @"DELETE FROM Player WHERE UserName='{0}'";

                int res = DBUtl.ExecSQL(delete, id);

                if (res == 1)
                {
                    TempData["Message"] = "Player Deleted!";
                    TempData["MsgType"] = "danger";

                }

                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "warning";

                }
                return RedirectToAction(REDIRECT_ACTN, REDIRECT_CNTR);
            }

        }


        //Views

        public IActionResult Rank()
        {
            Users();
            return View("Rank");
        }







    }
}







