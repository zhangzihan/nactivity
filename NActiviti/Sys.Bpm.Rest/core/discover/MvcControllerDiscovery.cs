using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Sys.Workflow.Cloud.Services.Core
{
    /// <summary>
    /// mvc controller discovery
    /// </summary>
    internal class MvcControllerDiscovery : IMvcControllerDiscovery
    {
        private List<MvcControllerInfo> _mvcControllers;
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionDescriptorCollectionProvider"></param>
        public MvcControllerDiscovery(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MvcControllerInfo> GetControllers()
        {
            if (_mvcControllers is object)
                return _mvcControllers;

            _mvcControllers = new List<MvcControllerInfo>();
            var items = _actionDescriptorCollectionProvider
                .ActionDescriptors.Items
                .Where(descriptor => descriptor.GetType() == typeof(ControllerActionDescriptor))
                .Select(descriptor => (ControllerActionDescriptor)descriptor)
                .GroupBy(descriptor => descriptor.ControllerTypeInfo.FullName)
                .ToList();

            foreach (var actionDescriptors in items)
            {
                if (!actionDescriptors.Any())
                    continue;

                var actionDescriptor = actionDescriptors.First();
                var controllerTypeInfo = actionDescriptor.ControllerTypeInfo;
                var currentController = new MvcControllerInfo
                {
                    AreaName = controllerTypeInfo.GetCustomAttribute<AreaAttribute>()?.RouteValue,
                    DisplayName = controllerTypeInfo.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName,
                    Name = actionDescriptor.ControllerName,
                };

                var actions = new List<MvcActionInfo>();

                foreach (var descriptor in actionDescriptors.GroupBy(a => a.ActionName).Select(g => g.First()))
                {
                    var methodInfo = descriptor.MethodInfo;
                    //if (IsProtectedAction(controllerTypeInfo, methodInfo))
                    //{
                    actions.Add(new MvcActionInfo
                    {
                        ControllerId = currentController.Id,
                        Name = descriptor.ActionName,
                        DisplayName = methodInfo.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName,
                    });
                    //}
                }

                if (actions.Any())
                {
                    currentController.Actions = actions;
                    _mvcControllers.Add(currentController);
                }
            }

            return _mvcControllers;
        }

        private static bool IsProtectedAction(MemberInfo controllerTypeInfo, MemberInfo actionMethodInfo)
        {
            if (actionMethodInfo.GetCustomAttribute<AllowAnonymousAttribute>(true) is object)
                return false;

            if (controllerTypeInfo.GetCustomAttribute<AuthorizeAttribute>(true) is object)
                return true;

            if (actionMethodInfo.GetCustomAttribute<AuthorizeAttribute>(true) is object)
                return true;

            return false;
        }
    }
}