// MbUnit Library 
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
//		QuickGraph Library HomePage: http://www.mbunit.com
//		Author: Jonathan de Halleux

//	Original XmlUnit license
/*
******************************************************************
Copyright (c) 2001, Jeff Martin, Tim Bacon
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above
      copyright notice, this list of conditions and the following
      disclaimer in the documentation and/or other materials provided
      with the distribution.
    * Neither the name of the xmlunit.sourceforge.net nor the names
      of its contributors may be used to endorse or promote products
      derived from this software without specific prior written
      permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
POSSIBILITY OF SUCH DAMAGE.

******************************************************************
*/

#pragma warning disable 1591
#pragma warning disable 3001

namespace MbUnit.Framework.Xml 
{
	using System;
	using System.Xml;    
    
	[Serializable]
	public class Difference 
	{
		private readonly DifferenceType _id;
		private readonly bool _majorDifference;
		private XmlNodeType _controlNodeType;
		private XmlNodeType _testNodeType;
        
		public Difference(DifferenceType id) 
		{
			_id = id;
			_majorDifference = Differences.IsMajorDifference(id);
		}
        
		public Difference(DifferenceType id, XmlNodeType controlNodeType, XmlNodeType testNodeType) 
			: this(id) 
		{
			_controlNodeType = controlNodeType;
			_testNodeType = testNodeType;
		}
        
		public DifferenceType Id 
		{
			get 
			{
				return _id;
			}
		}
        
		public bool MajorDifference 
		{
			get 
			{
				return _majorDifference;
			}
		}
        
		public XmlNodeType ControlNodeType 
		{
			get 
			{
				return _controlNodeType;
			}
		}
        
		public XmlNodeType TestNodeType 
		{
			get 
			{
				return _testNodeType;
			}
		}
        
		public override string ToString() 
		{
			string asString = base.ToString() + " type: " + _id 
				+ ", control Node: " + _controlNodeType.ToString()
				+ ", test Node: " + _testNodeType.ToString();            
			return asString;
		}
	}
}
