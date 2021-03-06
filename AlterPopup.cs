using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class AlterPopup : ColorRect
{
	RichTextLabel _text;
	Dictionary<char,Action> _actions = new Dictionary<char,Action>();
	
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
			var character = (char)key.Unicode;
			
			if (key.Scancode == (int)KeyList.Escape)
			{
				GetTree().SetInputAsHandled();
				Hide();
			}
			else if (_actions.ContainsKey(character))
			{
				_actions[character]();
				GetTree().SetInputAsHandled();
				Hide();
			}
		}
	}
	
	public void Show(Level level, Agent agent)
	{
		_actions.Clear();
		if (agent.StatusEffects.Any(e => e.Name == "Athiest"))
			ShowForAthiest(level, agent);
		else
			ShowForBeliever(level, agent);
	}
	
	public void ShowForAthiest(Level level, Agent agent)
	{
		var alterX = -1;
		var alterY = -1;
		for (var ox = -1; ox < 2; ox++)
		{
			for (var oy = -1; oy < 2; oy++)
			{
				if (level.GetTile(agent.X + ox, agent.Y + oy) == Tile.Alter)
				{
					alterX = agent.X + ox;
					alterY = agent.Y + oy;
				}
			}
		}
		var lines = new List<string> { "== Alter ==" };
		lines.Add("[b] Become a believer.\n\t(-25 AP, +10 favor with each deity)");
		_actions['b'] = () => {
			var effect = agent.StatusEffects.Single(e => e.Name == "Athiest");
			effect.End(level, agent);
			agent.AP -= 25;
			foreach (var d in Globals.Deities)
				d.FavorPerTeam[agent.Team] += 10;
			level.SetTile(alterX, alterY, Tile.ExhaustedAlter);
			agent.Messages.Add($"[color=#ffff99]You are no longer an athiest[/color]");
		};
		_text.Text = string.Join("\n", lines);
		Show();
	}
	
	public void ShowForBeliever(Level level, Agent agent)
	{
		var alterX = -1;
		var alterY = -1;
		for (var ox = -1; ox < 2; ox++)
		{
			for (var oy = -1; oy < 2; oy++)
			{
				if (level.GetTile(agent.X + ox, agent.Y + oy) == Tile.Alter)
				{
					alterX = agent.X + ox;
					alterY = agent.Y + oy;
				}
			}
		}
		
		var lines = new List<string> { "== Alter ==" };
		var key = 'a';
		var addNewLine = false;
		foreach (var d in Globals.Deities.Where(d => d.AcceptsPrayers))
		{
			var favor = d.FavorPerTeam[agent.Team];
			var nextFavor = favor + 25;
			_actions[key] = () => {
				agent.AP -= 50;
				agent.Messages.Add($"You pray to [color={Globals.TextColorGood}]{d.Name}[/color]");
				d.FavorPerTeam[agent.Team] = nextFavor;
				level.SetTile(alterX, alterY, Tile.ExhaustedAlter);
				if (nextFavor > 0)
					agent.Messages.Add($"[color={Globals.TextColorGood}]{d.Name}[/color] is pleased");
				else
					agent.Messages.Add($"[color={Globals.TextColorGood}]{d.Name}[/color] is placated");
			};
			lines.Add($"[{key++}] Pray to {d.GetFullTitle()}\n\t(-50 AP, {favor} -> {nextFavor} favor)");
			addNewLine = true;
		}
		
		if (addNewLine) lines.Add("");
		addNewLine = false;
		foreach (var d in Globals.Deities.Where(d => d.DonationMultiplier > 0))
		{
			var favor = d.FavorPerTeam[agent.Team];
			var nextFavor = (int)(favor + agent.Money * d.DonationMultiplier);
			_actions[key] = () => {
				agent.Money = 0;
				agent.Messages.Add($"You donate {agent.Money} HP to [color={Globals.TextColorGood}]{d.Name}[/color]");
				d.FavorPerTeam[agent.Team] = nextFavor;
				level.SetTile(alterX, alterY, Tile.ExhaustedAlter);
				if (nextFavor > 0)
					agent.Messages.Add($"[color={Globals.TextColorGood}]{d.Name}[/color] is pleased");
				else
					agent.Messages.Add($"[color={Globals.TextColorGood}]{d.Name}[/color] is placated");
			};
			lines.Add($"[{key++}] Donate to {d.GetFullTitle()}\n\t(give all money, {favor} -> {nextFavor} favor)");
			addNewLine = true;
		}
		
		if (addNewLine) lines.Add("");
		addNewLine = false;
		foreach (var d in Globals.Deities.Where(d => d.SacrificeCost > 0))
		{
			var favor = d.FavorPerTeam[agent.Team];
			var nextFavor = favor + 10;
			_actions[key] = () => {
				agent.Messages.Add($"You sacrifice {d.SacrificeCost} HP to [color={Globals.TextColorGood}]{d.Name}[/color]");
				agent.TakeDamage(d.SacrificeCost);
				d.FavorPerTeam[agent.Team] = nextFavor;
				level.SetTile(alterX, alterY, Tile.ExhaustedAlter);
				if (nextFavor > 0)
					agent.Messages.Add($"[color={Globals.TextColorGood}]{d.Name}[/color] is pleased");
				else
					agent.Messages.Add($"[color={Globals.TextColorGood}]{d.Name}[/color] is placated");
			};
			lines.Add($"[{key++}] Sacrifice to {d.GetFullTitle()}\n\t(-{d.SacrificeCost} HP, {favor} -> {nextFavor} favor)");
			addNewLine = true;
		}
		
		_text.Text = string.Join("\n", lines);
		Show();
	}
}
