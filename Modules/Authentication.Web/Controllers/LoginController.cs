﻿using System;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Controllers;

namespace GenHTTP.Modules.Authentication.Web.Controllers
{

    public class LoginController : BaseController
    {

        private Func<IRequest, string, string, ValueTask<IUser?>> PerformLogin { get; }

        public LoginController(Func<IRequest, string, string, ValueTask<IUser?>> performLogin)
        {
            PerformLogin = performLogin;
        }

        public IHandlerBuilder Index()
        {
            // ToDo: already logged in
            return RenderLogin();
        }

        [ControllerAction(RequestMethod.POST)]
        public async Task<IHandlerBuilder> Index(string user, string password, IRequest request)
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
            {
                return RenderLogin(user, "Please enter username and password.");
            }

            var authenticatedUser = await PerformLogin(request, user, password);

            if (authenticatedUser != null)
            {
                request.SetUser(authenticatedUser);

                return Redirect.To("{web-auth}/", true);
            }
            else
            {
                return RenderLogin(user, "Invalid username or password.");
            }
        }

        private IHandlerBuilder RenderLogin(string? username = null, string? errorMessage = null) => RenderAccountEntry("Login", "Login", username, errorMessage);

    }

}
