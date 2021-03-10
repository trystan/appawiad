using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class AlterPopup : ColorRect
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
			if (key.Scancode == (int)KeyList.Escape)
			{
				Hide();
			}
		}
	}
	
	public void Show(Level level, Agent agent)
	{
		var lines = new List<string> { "== Alter ==" };
		var key = 'a';
		var addNewLine = false;
		foreach (var d in Globals.Deities.Where(d => d.AcceptsPrayers))
		{
			var favor = d.FavorPerTeam[agent.Team];
			var nextFavor = favor + 25;
			lines.Add($"[{key++}] Pray to {d.GetFullTitle()}\n\t(-50 AP, {favor} -> {nextFavor} favor)");
			addNewLine = true;
		}
		
		if (addNewLine) lines.Add("");
		addNewLine = false;
		foreach (var d in Globals.Deities.Where(d => d.AcceptsDonations))
		{
			var favor = d.FavorPerTeam[agent.Team];
			var nextFavor = favor + agent.Money * 2;
			lines.Add($"[{key++}] Donate to {d.GetFullTitle()}\n\t(give all money, {favor} -> {nextFavor} favor)");
			addNewLine = true;
		}
		
		if (addNewLine) lines.Add("");
		addNewLine = false;
		foreach (var d in Globals.Deities.Where(d => d.AcceptsSacrifices))
		{
			var favor = d.FavorPerTeam[agent.Team];
			var nextFavor = favor + 50;
			lines.Add($"[{key++}] Sacrifice to {d.GetFullTitle()}\n\t(-5 HP, {favor} -> {nextFavor} favor)");
			addNewLine = true;
		}
		
		_text.Text = string.Join("\n", lines);
		Show();
	}
}
