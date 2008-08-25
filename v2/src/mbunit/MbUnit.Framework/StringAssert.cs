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
using System.Text;
using System.Text.RegularExpressions;

using MbUnit.Core.Exceptions;

namespace MbUnit.Framework
{
	/// <summary>
    /// Class containing generic assert methods for <see cref="String"/>s
	/// </summary>
	public sealed class StringAssert
	{
		private StringAssert(){}

        /// <summary>
        /// Asserts that two strings <paramref name="s1"/> and <paramref name="s2"/> are equal, ignoring the case
        /// </summary>
        /// <param name="s1">Expected string</param>
        /// <param name="s2">Actual string</param>
		public static void AreEqualIgnoreCase(string s1, string s2)
		{
			if (s1==null || s2==null)
				Assert.AreEqual(s1,s2);
			else
				Assert.AreEqual(s1.ToLower(), s2.ToLower());
		}

        /// <summary>
        /// Asserts that the string <paramref name="s"/> is non null and empty
        /// </summary>
        /// <param name="s">String to test.</param>
		public static void IsEmpty(String s)
		{
			Assert.IsNotNull(s,"String is null");
			Assert.AreEqual(0,s.Length,
			                "String count is not 0");
		}

        /// <summary>
        /// Asserts that the string <paramref name="s"/> is non null and non empty
        /// </summary>
        /// <param name="s">String to test.</param>
		public static void IsNonEmpty(String s)
		{
			Assert.IsNotNull(s,"String is null");
			Assert.IsTrue(s.Length!=0,
			                "String count is 0");
		}

        /// <summary>
        /// Asserts the regular expression <paramref name="reg"/> makes a full match on <paramref name="s"/>
        /// </summary>
        /// <param name="s">String to test.</param>
        /// <param name="reg">Regular expression as a string</param>
		public static void FullMatch(String s, string reg)
		{
			Regex regex = new Regex(reg);
			FullMatch(s,regex);
		}

        /// <summary>
        /// Asserts the regular expression <paramref name="regex"/> makes a full match on <paramref name="s"/>
        /// </summary>
        /// <param name="s">String to test.</param>
        /// <param name="regex">Regular expression as a <see cref="Regex"/> object</param>
		public static void FullMatch(String s, Regex regex)
		{
			Assert.IsNotNull(regex);
			
			Match m = regex.Match(s);
			Assert.IsTrue(m.Success, "Match is not successful");
			Assert.AreEqual(s.Length, m.Length, "Not a full match");
		}

        /// <summary>
        /// Asserts the regular expression <paramref name="reg"/> makes a match on <paramref name="s"/>
        /// </summary>
        /// <param name="s">String to test.</param>
        /// <param name="reg">Regular expression as a string</param>
		public static void Like(String s, string reg)
		{
			Regex regex = new Regex(reg);
			Like(s,regex);
		}

        /// <summary>
        /// Asserts the regular expression <paramref name="regex"/> makes a match on <paramref name="s"/>
        /// </summary>
        /// <param name="s">String to test.</param>
        /// <param name="regex">Regular expression as a <see cref="Regex"/> object</param>
		public static void Like(String s, Regex regex)
		{
			Assert.IsNotNull(regex);
			
			Match m = regex.Match(s);
			Assert.IsTrue(m.Success, "Match is not successful");
		}

        /// <summary>
        /// Asserts the regular expression <paramref name="reg"/> makes no match on <paramref name="s"/>
        /// </summary>
        /// <param name="s">String to test.</param>
        /// <param name="reg">Regular expression as a string</param>
		public static void NotLike(String s, string reg)
		{
			Regex regex = new Regex(reg);
			NotLike(s,regex);
		}

        /// <summary>
        /// Asserts the regular expression <paramref name="regex"/> makes no match on <paramref name="s"/>
        /// </summary>
        /// <param name="s">String to test.</param>
        /// <param name="regex">Regular expression as a <see cref="Regex"/> object</param>
		public static void NotLike(String s, Regex regex)
		{
			Assert.IsNotNull(regex);
			
			Match m = regex.Match(s);
			Assert.IsFalse(m.Success, "Match was found successful");
		}

        /// <summary>
        /// Asserts the string <paramref name="s"/> contains none of the <see cref="char"/>s in <paramref name="anyOf"/>
        /// </summary>
        /// <param name="s">String to test.</param>
        /// <param name="anyOf">Array of characters to search for.</param>
		public static void DoesNotContain(String s, params char[] anyOf)
		{
			if (s==null)
				return;
			
			Assert.AreEqual(-1, s.IndexOfAny(anyOf),
			                "{0} contains at {1}",
			                s,s.IndexOfAny(anyOf));
		}

        /// <summary>
        /// Asserts the string <paramref name="s"/> starts with the string <paramref name="pattern"/>
        /// </summary>
        /// <param name="s">The string to check</param>
        /// <param name="pattern">The string that might be found at the start of <paramref name="s"/></param>
        public static void StartsWith(String s, string pattern)
        {
            Assert.IsTrue(s.StartsWith(pattern), "String [[{0}]] does not start with [[{1}]]",
                s, pattern);
        }

        /// <summary>
        /// Asserts the string <paramref name="s"/> ends with the string <paramref name="pattern"/>
        /// </summary>
        /// <param name="s">The string to check</param>
        /// <param name="pattern">The string that might be found at the end of <paramref name="s"/></param>
        public static void EndsWith(String s, string pattern)
        {
            Assert.IsTrue(s.EndsWith(pattern), "String [[{0}]] does not end with [[{1}]]",
                s, pattern);
        }

        /// <summary>
        /// Asserts the string <paramref name="s"/> contains the string <paramref name="contain"/>
        /// </summary>
        /// <param name="s">The string to check</param>
        /// <param name="contain">The string that might be found within <paramref name="s"/></param>
        public static void Contains(String s, string contain)
        {
            Assert.IsTrue(s.IndexOf(contain) >= 0, "String [[{0}]] does not contain [[{1}]]",
                s, contain);
        }
    }
}
