using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class MessagesPopup : ColorRect
{
	RichTextLabel _text;
	
	public override void _Ready()
	{
		_text = (RichTextLabel)GetNode("Summary");
	}
	
	public override void _UnhandledInput(InputEvent e)
	{
		if (!Visible)
			return;
		
		if (e is InputEventKey key && key.Pressed)
		{
			if (key.Scancode == (int)KeyList.Escape
				|| key.Scancode == (int)KeyList.M)
			{
				GetTree().SetInputAsHandled();
				Hide();
			}
		}
	}
	
	public void Show(Level level, Agent agent)
	{
		_text.BbcodeText = string.Join("\n", agent.Messages);
		Show();
	}
}
