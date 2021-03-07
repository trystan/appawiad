using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class PaladinSetupScene : Control
{
	Dictionary<uint, Deity> _choices = new Dictionary<uint, Deity>();
	
	public override void _Ready()
	{
		var title = (Label)GetNode("Title");
		title.RectPosition = new Vector2(OS.WindowSize.x / 2 - title.RectSize.x / 2, 20);
		
		var text = (RichTextLabel)GetNode("List");
		text.RectPosition = new Vector2(10, 50);
		text.RectSize = new Vector2(OS.WindowSize.x - 20, OS.WindowSize.y - 100);
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
				+ (deity.AcceptsDonations ? " does " : " does not ") + "accept donations, and"
				+ (deity.AcceptsSacrafices ? " does " : " does not ") + "accept sacrafices.";
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
				var diety = _choices[key.Unicode];
				
				GetTree().ChangeScene("res://PlayScene.tscn");
			}
		}
	}
}
