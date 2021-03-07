using Godot;
using System;
using System.Linq;

public class PaladinSetupScene : Control
{
	public override void _Ready()
	{
		var title = (Label)GetNode("Title");
		title.RectPosition = new Vector2(OS.WindowSize.x / 2 - title.RectSize.x / 2, 50);
		
		var list = (VBoxContainer)GetNode("VBoxContainer");
		var chars = "abcdefghijklmnopqrstuvwxyz";
		
		var i = 0;
		foreach (var deity in Globals.Deities)
		{
			var text = new RichTextLabel {
				BbcodeEnabled = false,
				RectMinSize = new Vector2(OS.WindowSize.x - 100, 80)
			};
			
			text.BbcodeText = "== " + deity.GetFullTitle() + " ==";
			text.BbcodeText += "\n" + deity.Archetype.Description;
			if (deity.Likes.Any() || deity.Dislikes.Any())
			{
				text.BbcodeText += "\n";
				if (deity.Likes.Any())
					text.BbcodeText += "Likes " + string.Join(", ", deity.Likes) + ". ";
				if (deity.Dislikes.Any())
					text.BbcodeText += "Dislikes " + string.Join(", ", deity.Dislikes) + ". ";
			}
			text.BbcodeText += $"\n-- press [{chars[i]}] to devote yourself to {deity.Name} --";
			
			list.AddChild(text);
			i++;
		}
	}
}
