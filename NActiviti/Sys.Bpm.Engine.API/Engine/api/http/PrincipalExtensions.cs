// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Sys.IdentityModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;

namespace Sys.Extentions
{
    /// <summary>
    /// Extension methods for <see cref="System.Security.Principal.IPrincipal"/> and <see cref="System.Security.Principal.IIdentity"/> .
    /// </summary>
    public static class PrincipalExtensions
    {
        /// <summary>
        /// Gets the subject identifier.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetSubjectId(this IPrincipal principal)
        {
            return principal.Identity.GetSubjectId();
        }

        /// <summary>
        /// Gets the subject identifier.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">sub claim is missing</exception>
        [DebuggerStepThrough]
        public static string GetSubjectId(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = id.FindFirst(JwtClaimTypes.Subject);

            if (claim is null) throw new InvalidOperationException("sub claim is missing");
            return claim.Value;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetDisplayName(this ClaimsPrincipal principal)
        {
            var name = principal.Identity.Name;
            if (!string.IsNullOrWhiteSpace(name)) return name;

            var sub = principal.FindFirst(JwtClaimTypes.Subject);
            if (sub is object) return sub.Value;

            return string.Empty;
        }

        /// <summary>
        /// Gets the email.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetEmail(this ClaimsPrincipal principal)
        {
            var email = principal.FindFirst(JwtClaimTypes.Email);
            if (email is object) return email.Value;

            return string.Empty;
        }

        /// <summary>
        /// Gets the phone.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetPhone(this ClaimsPrincipal principal)
        {
            var phone = principal.FindFirst(JwtClaimTypes.PhoneNumber);
            if (phone is object) return phone.Value;

            return string.Empty;
        }

        /// <summary>
        /// Gets the phone.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetTenantId(this ClaimsPrincipal principal)
        {
            var tenantId = principal.FindFirst(JwtClaimTypes.TenantId);
            if (tenantId is object) return tenantId.Value;

            return string.Empty;
        }


        /// <summary>
        /// Gets the authentication method.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetAuthenticationMethod(this IPrincipal principal)
        {
            return principal.Identity.GetAuthenticationMethod();
        }

        /// <summary>
        /// Gets the authentication method claims.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static IEnumerable<Claim> GetAuthenticationMethods(this IPrincipal principal)
        {
            return principal.Identity.GetAuthenticationMethods();
        }

        /// <summary>
        /// Gets the authentication method.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">amr claim is missing</exception>
        [DebuggerStepThrough]
        public static string GetAuthenticationMethod(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = id.FindFirst("amr");

            if (claim is null) throw new InvalidOperationException("amr claim is missing");
            return claim.Value;
        }

        /// <summary>
        /// Gets the authentication method claims.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static IEnumerable<Claim> GetAuthenticationMethods(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            return id.FindAll("amr");
        }

        /// <summary>
        /// Gets the identity provider.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetIdentityProvider(this IPrincipal principal)
        {
            return principal.Identity.GetIdentityProvider();
        }

        /// <summary>
        /// Gets the identity provider.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">idp claim is missing</exception>
        [DebuggerStepThrough]
        public static string GetIdentityProvider(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = id.FindFirst("idp");

            if (claim is null) throw new InvalidOperationException("idp claim is missing");
            return claim.Value;
        }

        /// <summary>
        /// Determines whether this instance is authenticated.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns>
        ///   <c>true</c> if the specified principal is authenticated; otherwise, <c>false</c>.
        /// </returns>
        [DebuggerStepThrough]
        public static bool IsAuthenticated(this IPrincipal principal)
        {
            return principal is object && principal.Identity is object && principal.Identity.IsAuthenticated;
        }
    }
}