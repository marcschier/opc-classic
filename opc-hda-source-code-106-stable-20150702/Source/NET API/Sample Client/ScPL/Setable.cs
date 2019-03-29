/*
ScPl - A plotting library for .NET
Copyright (C) 2003 Matt Howlett

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
*/

using System;

namespace scpl
{
	public class Setable
	{
		public Setable()
		{
			s = false;
		}

		public Setable( object o )
		{
			d = o;
			s = true;
		}

		public void UnSet()
		{
			s = false;
		}

		public bool isSet 
		{
			get
			{
				return s;
			}
		}

		public object Data 
		{
			get 
			{
				if (s) 
				{
					return d;
				} 
				else 
				{
					throw new System.Exception("Data Not Set");
				}
			}
			set 
			{
				d = value;
				s = true;
			}
		}

		bool s;
		object d;
	}
}
