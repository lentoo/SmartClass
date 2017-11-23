using SmartClass.Models.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartClass.Controllers
{
  [CustomAuthorize]
  public class UsersController : Controller
  {
    public IService.ISys_UserService userService { get; set; }
    public ActionResult SearchUser(string user)
    {
      var users = userService.GetEntity(u => u.F_RealName.Contains(user)).Select(u=>new { u.F_RealName,u.F_Account }).ToArray();
      return Json(users, JsonRequestBehavior.AllowGet);
    }
  }
}