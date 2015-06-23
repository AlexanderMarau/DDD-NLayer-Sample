﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLayer.Application.Modules;
using NLayer.Application.UserSystemModule.DTOs;
using NLayer.Application.UserSystemModule.Services;
using NLayer.Infrastructure.Authorize;
using NLayer.Infrastructure.Authorize.AuthObject;
using NLayer.Presentation.WebHost.Helper;
using NLayer.Presentation.WebHost.Models;
using NLayer.Presentation.WebHost.Resources;
using PagedList;

namespace NLayer.Presentation.WebHost.Areas.UserSystem.Controllers
{
    public class UserController : UserSystemBaseController
    {
        IUserService _userService;

        IMenuService _menuService; 
        
        private NLayerServiceResolver _serviceResolver;

        public NLayerServiceResolver ServiceResolver
        {
            get { return _serviceResolver ?? (_serviceResolver = new NLayerServiceResolver()); }
        }

        //public IServiceResolver ServiceResolver { get; set; }

        private IAuthorizeManager AuthorizeManager
        {
            get
            {
                return ServiceResolver.Resolve<IAuthorizeManager>();
            }
        }

        #region Constructor

        public UserController(IUserService userService, IMenuService menuService)
        {
            _userService = userService;
            _menuService = menuService;
        }

        #endregion

        public ActionResult Index(string userName, int? page)
        {
            var list = _userService.FindBy(userName, page.HasValue ? page.Value : 1, DisplayExtensions.DefaultPageSize);

            ViewBag.UserName = userName;

            return View(list);
        }

        public ActionResult EditUser(Guid? id)
        {
            var user = id.HasValue ? _userService.FindBy(id.Value) : new UserDTO();
            return View(user);
        }

        [HttpPost]
        public ActionResult SearchUser(string userName)
        {
            return Json(new AjaxResponse
            {
                Succeeded = true,
                ShowMessage = false,
                RedirectUrl = Url.Action("Index", new
                {
                    userName,
                })
            });
        }

        public ActionResult EditUserPermission(Guid userId)
        {
            var menus = new List<MenuDTO>();

            var modules = NLayerModulesManager.Instance.ListAll();
            foreach (var module in modules)
            {
                menus.AddRange(_menuService.FindByModule(module.Type.ToString()));
            }

            var user = _userService.FindBy(userId);

            var permissions = _userService.GetUserPermission(userId);

            ViewBag.Modules = modules;
            ViewBag.Menus = menus;
            ViewBag.User = user;
            ViewBag.Permissions = permissions;

            return View();
        }

        [HttpPost]
        public ActionResult EditUserPermission(Guid userId, List<string> permissions)
        {
            var pList = new List<Guid>();

            foreach (var s in permissions)
            {
                Guid id;
                if (Guid.TryParse(s, out id))
                {
                    pList.Add(id);
                }
            }

            if (pList.Count > 0)
            {
                _userService.UpdateUserPermission(userId, pList);
            }

            return Json(new AjaxResponse
            {
                Succeeded = true,
                ShowMessage = true,
                Message = CommonResource.Msg_Operate_Ok,
                RedirectUrl = string.Empty
            });
        }

        [HttpPost]
        public ActionResult EditUser(UserDTO user)
        {
            if (user.LastLogin == DateTime.MinValue)    //最后登录时间字段为空时，数据为datetime默认的{0001/1/1 0:00:00}，新增或修改用户时报错
                user.LastLogin = Convert.ToDateTime("1900-01-01T00:00:00.000");

            if (user.Id == Guid.Empty)
            {
                _userService.Add(user);
            }
            else
            {
                _userService.Update(user);
            }

            return Json(new AjaxResponse
            {
                Succeeded = true,
                ShowMessage = false,
                RedirectUrl = Url.Action("Index")
            });
        }

        public ActionResult RemoveUser(Guid id)
        {
            _userService.Remove(id);

            return Json(new AjaxResponse
            {
                Succeeded = true,
                ShowMessage = false,
                RedirectUrl = Url.Action("Index")
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EffectiveUserPermission(Guid userId)
        {
            var user = _userService.FindBy(userId);

            ViewBag.AuthorizeUser = AuthorizeManager.GetAuthorizeUserInfo(
                new UserToken() { UserId = user.Id, LastLoginToken = user.LastLoginToken });

            return View();
        }

        public ActionResult EffectPermission()
        {
            AuthorizeManager.ClearCache();
            return Json(new AjaxResponse
            {
                Succeeded = true,
                ShowMessage = true,
                Message = "权限缓存清除成功",
                RedirectUrl = Request.UrlReferrer == null ? Url.Action("Index") : Request.UrlReferrer.ToString()
            }, JsonRequestBehavior.AllowGet);
        }
    }
}