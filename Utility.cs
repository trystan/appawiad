using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public static class Util
{
	public static string AndList(IEnumerable<string> terms)
	{
		var array = terms.ToArray();
		switch (array.Length)
		{
			case 0:
				return "";
			case 1:
				return array[0];
			case 2:
				return $"{array[0]} and {array[1]}";
			default:
				var last = array.Last();
				var rest = array.Take(array.Length - 1).ToArray();
				return string.Join(", ", rest) + ", and " + last;
		}
	}
	
	public static string OrList(IEnumerable<string> terms)
	{
		var array = terms.ToArray();
		switch (array.Length)
		{
			case 0:
				return "";
			case 1:
				return array[0];
			case 2:
				return $"{array[0]} or {array[1]}";
			default:
				var last = array.Last();
				var rest = array.Take(array.Length - 1).ToArray();
				return string.Join(", ", rest) + ", or " + last;
		}
	}
}
