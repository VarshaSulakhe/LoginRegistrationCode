using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using InstructionalResource2.Models;
using System.Net.Mail;
using System.Net;
using System.Web.Security;

namespace InstructionalResource2.Controllers
{
    public class UserController : Controller
    {
        //Registration Action
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        //registration POST action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailAuthenticated,ActivationCode")]User user){

            bool Status = false;
            string Message = "";
            //model validation

            if (ModelState.IsValid)
            {
                #region //email already exists

                var isExists = IsEmailExists(user.EmailId);
                if(isExists)
                {
                    ModelState.AddModelError("EmailExists", "Email already exists");
                    return View(user);
                }

                #endregion

                #region   generate activation code
                user.ActivationCode = Guid.NewGuid();
                #endregion

                #region PasswordHashing
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword);
                #endregion
                user.IsEmailAuthenticated = false;

                #region Save to database
                using (InstructionalResourcesDBEntities dc= new InstructionalResourcesDBEntities()){
                    dc.Users.Add(user);
                    dc.SaveChanges();

                     //send email to user
                    SendVerificationLinkEmail(user.EmailId,user.ActivationCode.ToString());
                    Message="Registration successfully done. Account activation activation link has been sent to your email ID :"+user.EmailId;
                    Status=true;
                }
                #endregion

            }
            else
            {
                Message = "Invalid Request";
            }

            ViewBag.message=Message;
            ViewBag.status=Status;
            return View(user);
        }
      
        
        //Verify account
        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            using (InstructionalResourcesDBEntities dc = new InstructionalResourcesDBEntities())
            {
                dc.Configuration.ValidateOnSaveEnabled = false;     //if confirm password doesnt match 
                var v = dc.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.IsEmailAuthenticated = true;
                    dc.SaveChanges();
                    Status = true;
                }
                else
                {
                    ViewBag.Message = "Invalid Request";
                }
            }
            ViewBag.Status = Status;
            return View();
        }

        //login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        //login POST
        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult Login(UserLogin login,string ReturnUrl)
        {
            string message = "";
            using (InstructionalResourcesDBEntities dc = new InstructionalResourcesDBEntities())
            {
                
                var v = dc.Users.Where(a => a.EmailId == login.EmailId).FirstOrDefault();
                if (v != null && v.IsEmailAuthenticated==true)
                {
                    if (string.Compare(Crypto.Hash(login.Password),v.Password)==0)
                    {
                        int timeout = login.RememberMe ? 525600 : 1;  //525600 is min 1 year
                        var ticket = new FormsAuthenticationTicket(login.EmailId,login.RememberMe,timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);

                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }

                    }
                    else
                    {
                        message = "Invalid credential provided.Please verify your account before logging in.";
                    }
                }
                else
                {
                    message = "Invalid credentials provided";
                }
            }

                ViewBag.Message=message;
            return View();
        }

        //logout
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }

        [NonAction]
        public bool IsEmailExists(string EmailId)
        {
            using(InstructionalResourcesDBEntities dc = new InstructionalResourcesDBEntities())
            {
                var v = dc.Users.Where(a => a.EmailId == EmailId).FirstOrDefault();
                return v != null;
            }
        }

        [NonAction]
        public void SendVerificationLinkEmail(string emailId, string ActivationCode)
        {
            var verifyUrl = "/User/VerifyAccount/" + ActivationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("yashaswinimr@gmail.com", "Yashaswini");
            var toEmail = new MailAddress(emailId);
            var fromEmailPwd = "Ekm23E*Dwtym";
            string subject ="Your account is successfully created!!";
            string body="<br/><br/> We are excited to tell you that your account is created successfully.Please click on the below link to verify your account"+"<a href='"+link+"'>"+link+"</a>";

            var smtp=new SmtpClient{
                Host = "smtp.gmail.com",
                Port=587,
                EnableSsl=true,
                DeliveryMethod=SmtpDeliveryMethod.Network,
                UseDefaultCredentials=false,
                Credentials=new NetworkCredential(fromEmail.Address,fromEmailPwd)};

            using (var message=new MailMessage(fromEmail,toEmail)
            {
                Subject = subject,
                Body=body,
                IsBodyHtml=true
            })
                try
                {
                    smtp.Send(message);
                }
                catch (SmtpException e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                } 
            

        }

    }
}