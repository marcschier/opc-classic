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

using System.Drawing;

namespace scpl 
{
	public class Vector2D
	{
		public Vector2D()
		{
			X = 0.0;
			Y = 0.0;
		}

		public Vector2D( Point p )
		{
			X = p.X;
			Y = p.Y;
		}

		public Vector2D( double x, double y )
		{
			X = x;
			Y = y;
		}

		public double norm()
		{
			return System.Math.Sqrt( X*X + Y*Y );
		}

		public double X;
		public double Y;

		public static Vector2D operator-( Vector2D a, Vector2D b )
		{
			return new Vector2D( a.X - b.X, a.Y - b.Y );
		}
		
		public static Vector2D operator+( Vector2D a, Vector2D b )
		{
			return new Vector2D( a.X + b.X, a.Y + b.Y );
		}

		public static Vector2D operator*( double a, Vector2D b )
		{
			return new Vector2D( a * b.X, a * b.Y );
		}
	}
}