// MbUnit Test Framework
// 
// Copyright (c) 2004 Jonathan de Halleux
//
// This software is provided 'as-is', without any express or implied warranty. 
// 
// In no event will the authors be held liable for any damages arising from 
// the use of this software.
// Permission is granted to anyone to use this software for any purpose, 
// including commercial applications, and to alter it and redistribute it 
// freely, subject to the following restrictions:
//
//		1. The origin of this software must not be misrepresented; 
//		you must not claim that you wrote the original software. 
//		If you use this software in a product, an acknowledgment in the product 
//		documentation would be appreciated but is not required.
//
//		2. Altered source versions must be plainly marked as such, and must 
//		not be misrepresented as being the original software.
//
//		3. This notice may not be removed or altered from any source 
//		distribution.
//		
//		MbUnit HomePage: http://www.mbunit.com
//		Author: Jonathan de Halleux


using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Security.Principal;
using System.Text;

using MbUnit.Core.Exceptions;

namespace MbUnit.Framework
{
    /// <summary>
    /// Class containing generic assert methods for the verification of Roles and Identity information.
	/// </summary>
	public sealed class SecurityAssert
	{
		private SecurityAssert(){}
		
		#region Authentication and Identity related
        /// <summary>
        /// Asserts that <paramref name="identity"/> is authenticated.
        /// </summary>
        /// <param name="identity">The <see cref="IIdentity"/> being tested.</param>
		public static void IsAuthenticated(IIdentity identity)
		{
			Assert.IsNotNull(identity);
			Assert.IsTrue(identity.IsAuthenticated, 
			              "Identity {0} not authentitcated",
			              identity.Name);			
		}

        /// <summary>
        /// Asserts that <paramref name="identity"/> is not authenticated.
        /// </summary>
        /// <param name="identity">The <see cref="IIdentity"/> being tested.</param>
		public static void IsNotAuthenticated(IIdentity identity)
		{
			Assert.IsNotNull(identity);
			Assert.IsFalse(identity.IsAuthenticated, 
			              "Identity {0} authentitcated",
			              identity.Name);						
		}

        /// <summary>
        /// Asserts that the current windows identity is authenticated.
        /// </summary>
		public static void WindowIsAuthenticated()
		{
			IsAuthenticated(WindowsIdentity.GetCurrent());
		}

		/// <summary>
		/// Asserts that the current windows identity is not authenticated.
		/// </summary>				
		public static void WindowIsNotAuthenticated()
		{
			IsNotAuthenticated(WindowsIdentity.GetCurrent());			
		}

        /// <summary>
        /// Asserts that the current windows identity is in <param name="role"/>.
        /// </summary>
        /// <param name="role">The <see cref="WindowsBuiltInRole">role</see> to which the current windows identity should belong.</param>
		public static void WindowsIsInRole(WindowsBuiltInRole role)
		{
			WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
			Assert.IsTrue(
				principal.IsInRole(WindowsBuiltInRole.Administrator),
				"User {0} is not in role {0}",
				principal.Identity.Name,
				role
				);
		}

		/// <summary>
		/// Asserts that the current windows identity is in the <see cref="WindowsBuiltInRole.Administrator"/> role.
		/// </summary>						
		public static void WindowsIsInAdministrator()
		{
			WindowsIsInRole(WindowsBuiltInRole.Administrator);
		}
		
		/// <summary>
		/// Asserts that the current windows identity is in the <see cref="WindowsBuiltInRole.Guest"/> role.
		/// </summary>								
		public static void WindowsIsInGuest()
		{
			WindowsIsInRole(WindowsBuiltInRole.Guest);
		}
		
		/// <summary>
		/// Asserts that the current windows identity is in the <see cref="WindowsBuiltInRole.PowerUser"/> role.
		/// </summary>								
		public static void WindowsIsInPowerUser()
		{
			WindowsIsInRole(WindowsBuiltInRole.PowerUser);
		}		
		
		/// <summary>
		/// Asserts that the current windows identity is in the <see cref="WindowsBuiltInRole.User"/> role.
		/// </summary>								
		public static void WindowsIsInUser()
		{
			WindowsIsInRole(WindowsBuiltInRole.User);
		}
		#endregion 
		
		#region SQL Injection
		// http://sqljunkies.com/WebLog/rhurlbut/archive/2003/09/28/243.aspx
		
		#endregion
		
		#region Buffer overrun
		
		#endregion
	}
}
