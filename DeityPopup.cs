using Godot;
using System;
using System.Linq;

public class DeityPopup : ColorRect
{
	public override void _Ready()
	{
		var text = (RichTextLabel)GetNode("Summary");
		
		foreach (var deity in Globals.Deities)
		{
			text.BbcodeText += "== " + deity.GetFullTitle() + " ==";
			text.BbcodeText += "\n" + deity.Archetype.Description;
			
			if (deity.Likes.Any())
			{
				text.BbcodeText += " " + deity.Pronoun.Substring(0,1).ToUpper() + deity.Pronoun.Substring(1) + " ";
				text.BbcodeText += "likes " + Util.AndList(deity.Likes) + ".";
			}
			
			if (deity.Dislikes.Any())
			{
				text.BbcodeText += " " + deity.Pronoun.Substring(0,1).ToUpper() + deity.Pronoun.Substring(1) + " ";
				text.BbcodeText += "dislikes " + Util.AndList(deity.Dislikes) + ".";
			}
			
			text.BbcodeText += "\n" + deity.Name 
				+ (deity.AcceptsPrayers ? " does " : " does not ") + "accept prayers,"
				+ ((deity.DonationMultiplier > 0) ? " does " : " does not ") + "accept donations, and"
				+ ((deity.SacrificeCost > 0) ? " does " : " does not ") + "accept sacrifices.";
			
			text.BbcodeText += "\n\n";
		}
	}
	
	public override void _UnhandledInput(InputEvent e)
	{
		if (!Visible)
			return;
		
		if (e is InputEventKey key && key.Pressed)
		{
			if (key.Scancode == (int)KeyList.Escape
				|| key.Scancode == (int)KeyList.Tab)
			{
				GetTree().SetInputAsHandled();
				Hide();
			}
		}
	}
}
