using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class PaladinSetupScene : CanvasLayer
{
	Dictionary<uint, Deity> _choices = new Dictionary<uint, Deity>();
	
	public override void _Ready()
	{
		var text = (RichTextLabel)GetNode("List");
		var chars = "abcdefghijklmnopqrstuvwxyz";
		
		var i = 0;
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
			_choices[(uint)chars[i]] = deity;
			
			text.BbcodeText += $"\n-- press [{chars[i]}] to devote yourself to {deity.Name} --";
			text.BbcodeText += "\n\n";
			i++;
		}
	}
	
	public override void _UnhandledInput(InputEvent e)
	{
		if (e is InputEventKey key && key.Pressed)
		{
			if (_choices.ContainsKey(key.Unicode))
			{
				foreach (var other in Globals.Deities)
					other.FavorPerTeam["player"] -= 5;
				
				var deity = _choices[key.Unicode];
				deity.FavorPerTeam["player"] += 85;
				var title = deity.GetShortTitle();
				
				foreach (var other in Globals.Deities)
				{
					if (other.Likes.Contains(title))
						deity.FavorPerTeam["player"] += 10;
					if (other.Dislikes.Contains(title))
						deity.FavorPerTeam["player"] -= 10;
				}
				
				GetTree().ChangeScene("res://PlayScene.tscn");
			}
		}
	}
}
