using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class HelpPopup : ColorRect
{
	RichTextLabel _text;
	
	public override void _Ready()
	{
		_text = (RichTextLabel)GetNode("Summary");
		
		var lines = new List<string> {
			"\nKeys:",
			"  [arrows] Move",
			"  [.] Wait",
			"  [@] View your character",
			"  [g] Get item on the floor",
			"  [tab] View Deities",
			"  [m] View message log",
			"  [?] View this help",
			"",
			"The gods are watching everyone. And judging. And helping those "
			+ "they favor while hindering those they don't. What items will "
			+ "you use? Who will you fight? Who will you pray to, donate to "
			+ "and sacrafice to?",
			"",
			"In combat, damage is done based on ATK / DEF.",
			"ATK:8 vs DEF:4 = 8/4 = 2 points of damage",
			"ATK:9 vs DEF:4 = 9/4 = 2 and 1/4 left over. So 2 points of damage and a 1/4 chance of an additional point",
			"ATK:2 vs DEF:8 = 2/8 = 0 and 1/4 left over. So 0 points of damage and a 1/4 chance of an additional point",
		};
		
		_text.BbcodeText = string.Join("\n", lines);
	}
	
	public override void _UnhandledInput(InputEvent e)
	{
		if (!Visible)
			return;
		
		if (e is InputEventKey key && key.Pressed)
		{
			if (key.Scancode == (int)KeyList.Escape
				|| (char)key.Unicode == '?')
			{
				GetTree().SetInputAsHandled();
				Hide();
			}
		}
	}
}
