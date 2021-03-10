using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class CharacterPopup : ColorRect
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
				|| key.Scancode == (int)KeyList.C)
			{
				Hide();
			}
		}
	}
	
	public void Show(Agent player)
	{
		var lines = new List<string> {
			$"{player.DisplayName}",
			$"HP {player.HP}/{player.HPMax}   AP {player.AP} (+{player.APRegeneration})",
			$"${player.Money}",
			"\nArmor: " + (player.Armor?.DisplayName ?? "-none-")
		};
		if (player.Armor != null)
		{
			lines.Add($"   Material: {player.Armor.MadeOf}");
			lines.Add($"   ATK: {player.Armor.ATK}");
			lines.Add($"   DEF: {player.Armor.DEF}");
		}
		lines.Add("\nWeapon: " + (player.Weapon?.DisplayName ?? "-none-"));
		if (player.Weapon != null)
		{
			lines.Add($"   Material: {player.Weapon.MadeOf}");
			lines.Add($"   ATK: {player.Weapon.ATK}");
			lines.Add($"   DEF: {player.Weapon.DEF}");
		}
		lines.AddRange(new [] {
			$"\nTotal ATK: {player.ATK}",
			$"Total DEF: {player.DEF}",
			"\nEffects:"
		});
		
		if (player.StatusEffects.Any())
		{
			foreach (var effect in player.StatusEffects)
			{
				lines.Add(effect.Name);
				foreach (var description in effect.Descriptions)
					lines.Add(" " + description);
			}
		}
		else
		{
			lines.Add(" -none-");
		}
		
		lines.AddRange(new [] {
			"",
			"[table=2]",
			"[cell]Deity[/cell][cell]Favor[/cell]"
		});
		foreach (var deity in Globals.Deities)
			lines.Add($"[cell]{deity.GetFullTitle()}[/cell][cell]{deity.PlayerFavor}[/cell]]");
		lines.Add("[/table]");
		var skipCount = Math.Max(0, player.Messages.Count - 10);
		lines.Add("\nMessages:");
		lines.AddRange(player.Messages.Skip(skipCount));
		
		_text.BbcodeText = string.Join("\n", lines);
		
		Show();
	}
}
