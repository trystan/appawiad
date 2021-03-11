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
			"  [c] View your character",
			"  [g] Get item on the floor",
			"  [tab] View Deities",
			"  [m] View message log",
			"  [h] View this help",
			"",
			"The gods are watching everyone. And judging. And helping those "
			+ "they favor while hindering those they don't. What items will "
			+ "you use? Who will you fight? Who will you pray to, donate to "
			+ "and sacrafice to?",
			"",
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
				|| key.Scancode == (int)KeyList.H)
			{
				GetTree().SetInputAsHandled();
				Hide();
			}
		}
	}
}
