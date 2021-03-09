using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class PlayScene : Node2D
{
	RichTextLabel _sidebar;
	Control _deityPopup;
	Camera2D _camera;
	
	Level _level;
	Agent _player;
	ICommand _nextPlayerCommand;
	
	public override void _Ready()
	{
		_sidebar = (RichTextLabel)GetNode("CanvasLayer/Sidebar/Text");
		_deityPopup = (Control)GetNode("CanvasLayer/DeityPopup");
		_camera = (Camera2D)GetNode("Camera2D");
		
		_level = (Level)GetNode("Level");
		_level.Setup(32, 32);
		
		var catalog = new Catalog();
		
		for (var i = 0; i < 8; i++)
		{
			var x = Globals.Random.Next(32);
			var y = Globals.Random.Next(32);
			_level.Add(catalog.NewItem(x, y));
		}
		
		_player = catalog.NewPlayer(3, 4);
		_player.Messages.Add("Welcome!");
		_level.Add(_player);
		RemoveChild(_camera);
		_player.AddChild(_camera);
		
		for (var i = 0; i < 32; i++)
		{
			var x = Globals.Random.Next(32);
			var y = Globals.Random.Next(32);
			_level.Add(catalog.NewEnemy(x, y));
		}
	}
	
	public ICommand GetCommandForAi(Agent agent)
	{
		var commands = GetCommandsForAi(agent).ToArray();
		return commands[Globals.Random.Next(commands.Length)];
	}
	
	public IEnumerable<ICommand> GetCommandsForAi(Agent agent)
	{
		if (!agent.Tags.Contains(AgentTag.Stationary))
		{
			if (!_level.GetTile(agent.X - 1, agent.Y).BlocksMovement)
				yield return new MoveBy(-1, 0);
			
			if (!_level.GetTile(agent.X + 1, agent.Y).BlocksMovement)
				yield return new MoveBy(1, 0);
			
			if (!_level.GetTile(agent.X, agent.Y - 1).BlocksMovement)
				yield return new MoveBy(0, -1);
			
			if (!_level.GetTile(agent.X, agent.Y + 1).BlocksMovement)
				yield return new MoveBy(0, 1);
		}
		
		yield return new MoveBy(0, 0);
	}
	
	public override void _UnhandledInput(InputEvent e)
	{
		if (_player == null || _deityPopup.Visible)
			return;
		
		if (e is InputEventKey key && key.Pressed)
		{
			switch (key.Scancode)
			{
				case (int)KeyList.Tab:
					_deityPopup.Show();
					foreach (var a in _level.Agents)
						GD.Print($"{a.DisplayName} {a.HP}/6 {a.AP}+{a.APRegeneration}");
					break;
					
				case (int)KeyList.G:
					_nextPlayerCommand = new PickupItem();
					break;
				case (int)KeyList.Left:
					_nextPlayerCommand = new MoveBy(-1, 0);
					break;
				case (int)KeyList.Right:
					_nextPlayerCommand = new MoveBy( 1, 0);
					break;
				case (int)KeyList.Up:
					_nextPlayerCommand = new MoveBy(0, -1);
					break;
				case (int)KeyList.Down:
					_nextPlayerCommand = new MoveBy(0,  1);
					break;
				case (int)KeyList.Period:
					_nextPlayerCommand = new MoveBy(0,  0);
					break;
			}
		}
		
		var lines = new List<string> {
			$"[@] {_player.DisplayName}",
			$"HP {_player.HP}/6   AP {_player.AP}+{_player.APRegeneration}",
			$"${_player.Money}",
			"Armor: " + _player.Armor?.DisplayName ?? "-none",
			"Weapon: " + _player.Weapon?.DisplayName ?? "-none",
			"",
			"[g] Here:",
			_level.Items.FirstOrDefault(i => i.X == _player.X && i.Y == _player.Y)?.DisplayName ?? "-none-",
			"",
			"[table=2]",
			"[cell][tab] Deity[/cell][cell]Favor[/cell]"
		};
		foreach (var deity in Globals.Deities)
			lines.Add($"[cell]{deity.GetShortTitle()}[/cell][cell]{deity.PlayerFavor}[/cell]]");
		lines.Add("[/table]");
		var skipCount = Math.Max(0, _player.Messages.Count - 10);
		lines.Add("\n[m] Messages:");
		lines.AddRange(_player.Messages.Skip(skipCount));
		
		_sidebar.BbcodeText = string.Join("\n", lines);
	}
	
	public override void _Process(float delta)
	{
		if (_deityPopup.Visible)
			return;
		
		var ticks = 0;
		while (_level.Agents.Any()
			&& !_level.Agents[0].IsBusy
			&& ticks++ < 10)
		{
			if (_level.Agents[0].AP < 1)
			{
				_level.Agents.ForEach(a => a.EndTurn());
				_level.Agents = _level.Agents.OrderByDescending(a => a.AP).ToList();
				Globals.OnEvent(new NextTurn { Player = _player });
			}
			
			var agent = _level.Agents[0];
			if (agent.AP < 1)
				break;
			else if (agent == _player)
			{
				if (_nextPlayerCommand == null)
					break;
					
				_level.Agents.RemoveAt(0);
				_nextPlayerCommand.Do(_level, _player);
				_nextPlayerCommand = null;
				_level.Agents.Add(agent);
				_level.Agents = _level.Agents.OrderByDescending(a => a.AP).ToList();
			}
			else
			{
				_level.Agents.RemoveAt(0);
				GetCommandForAi(agent).Do(_level, agent);
				_level.Agents.Add(agent);
				_level.Agents = _level.Agents.OrderByDescending(a => a.AP).ToList();
			}
		}
	}
}

public class Catalog
{
	PackedScene _agentScene;
	PackedScene _itemScene;
	
	public Catalog()
	{
		_agentScene = (PackedScene)ResourceLoader.Load("res://Agent.tscn");
		_itemScene = (PackedScene)ResourceLoader.Load("res://Item.tscn");
	}
	
	public Agent NewPlayer(int x, int y)
	{
		var agent = ((Agent)_agentScene.Instance()).Setup(x, y, 0, 9);
		agent.DisplayName = "player";
		agent.Team = "player";
		agent.Tags = new List<AgentTag> { AgentTag.Living };
		return agent;
	}
	
	public Agent NewEnemy(int x, int y)
	{
		var constructors = new Func<int,int,Agent>[] {
			NewGobbo, NewSkeleton, NewSpider, NewPig, NewTree
		};
		return constructors[Globals.Random.Next(constructors.Length)](x, y);
	}
	
	public Agent NewGobbo(int x, int y)
	{ 
		var agent = ((Agent)_agentScene.Instance()).Setup(x, y, 31, 5);
		agent.DisplayName = "gobbo";
		agent.Team = "gobbos";
		agent.Tags = new List<AgentTag> { AgentTag.Living };
		return agent;
	}
	
	public Agent NewSkeleton(int x, int y)
	{ 
		var agent = ((Agent)_agentScene.Instance()).Setup(x, y, 37, 5);
		agent.DisplayName = "skeleton";
		agent.Team = "skeleton";
		agent.Tags = new List<AgentTag> { AgentTag.Undead };
		return agent;
	}
	
	public Agent NewPig(int x, int y)
	{ 
		var agent = ((Agent)_agentScene.Instance()).Setup(x, y, 2, 15);
		agent.DisplayName = "pig";
		agent.Team = "beasts";
		agent.Tags = new List<AgentTag> { AgentTag.Living };
		return agent;
	}
	
	public Agent NewSpider(int x, int y)
	{ 
		var agent = ((Agent)_agentScene.Instance()).Setup(x, y, 2, 14);
		agent.DisplayName = "spider";
		agent.Team = "critters";
		agent.Tags = new List<AgentTag> { AgentTag.Living };
		agent.HP = 3;
		agent.APRegeneration = 10;
		return agent;
	}
	
	public Agent NewTree(int x, int y)
	{ 
		var agent = ((Agent)_agentScene.Instance()).Setup(x, y, 0, 26);
		agent.DisplayName = "tree";
		agent.Team = "plants";
		agent.Tags = new List<AgentTag> { AgentTag.Living, AgentTag.Stationary };
		return agent;
	}
	
	public Item NewItem(int x, int y)
	{
		var constructors = new Func<int,int,Item>[] {
			NewSword, NewAxe, NewClub, NewSpear
		};
		return constructors[Globals.Random.Next(constructors.Length)](x, y);
	}
	
	public Item NewSword(int x, int y)
	{
		var item = ((Item)_itemScene.Instance()).Setup(x, y, 26, 21);
		item.DisplayName = "Sword";
		item.Type = ItemType.Weapon;
		item.MadeOf = "metal";
		return item;
	}
	
	public Item NewAxe(int x, int y)
	{
		var item = ((Item)_itemScene.Instance()).Setup(x, y, 41, 21);
		item.DisplayName = "Axe";
		item.Type = ItemType.Weapon;
		item.MadeOf = "metal";
		return item;
	}
	
	public Item NewClub(int x, int y)
	{
		var item = ((Item)_itemScene.Instance()).Setup(x, y, 37, 21);
		item.DisplayName = "Club";
		item.Type = ItemType.Weapon;
		item.MadeOf = "wood";
		return item;
	}
	
	public Item NewSpear(int x, int y)
	{
		var item = ((Item)_itemScene.Instance()).Setup(x, y, 38, 21);
		item.DisplayName = "Spear";
		item.Type = ItemType.Weapon;
		item.MadeOf = "wood";
		return item;
	}
}

public interface ICommand
{
	void Do(Level level, Agent agent);
}

public class MoveBy : ICommand
{
	public int X { get; set; }
	public int Y { get; set; }
	
	public MoveBy(int x, int y)
	{
		X = x;
		Y = y;
	}
	
	public void Do(Level level, Agent agent)
	{
		agent.AP -= 10;
		
		if (level.GetTile(agent.X + X, agent.Y + Y).BlocksMovement)
			return;
		if (X == 0 && Y == 0)
			return;
		
		var nextX = agent.X + X;
		var nextY = agent.Y + Y;
		
		var other = level.Agents.FirstOrDefault(a => a.X == nextX && a.Y == nextY);
		if (other != null)
		{
			other.TakeDamage(1);
			if (other.HP < 1)
			{
				agent.Messages.Add($"You kill the {other.DisplayName}");
				other.Messages.Add("You were killed by a {other.DisplayName}");
				level.Agents.Remove(other);
			}
			else
			{
				agent.Messages.Add($"You deal 1 damage to the {other.DisplayName}");
				other.Messages.Add($"You take 1 damage from the {agent.DisplayName}");
			}
			Globals.OnEvent(new DidAttack(agent, other));
		}
		else
		{
			agent.X = nextX;
			agent.Y = nextY;
			agent.IsBusy = true;
		}
	}
}

public class PickupItem : ICommand
{
	public void Do(Level level, Agent agent)
	{
		agent.AP -= 10;
		var item = level.Items.FirstOrDefault(i => i.X == agent.X && i.Y == agent.Y);
		if (item == null)
			return;
		
		if (item.Type == ItemType.Armor)
		{
			if (agent.Armor != null)
				Drop(level, agent, agent.Armor);
			Pickup(level, agent, item);
			Globals.OnEvent(new UsedItem(agent, agent.Armor));
		}
		
		if (item.Type == ItemType.Weapon)
		{
			if (agent.Weapon != null)
				Drop(level, agent, agent.Weapon);
			Pickup(level, agent, item);
			Globals.OnEvent(new UsedItem(agent, agent.Weapon));
		}
	}
	
	public void Pickup(Level level, Agent agent, Item item)
	{
		switch (item.Type)
		{
			case ItemType.Weapon: agent.Weapon = item; break;
			case ItemType.Armor: agent.Armor = item; break;
		}
		level.Remove(item);
		agent.Messages.Add($"You pick up the {item.DisplayName}");
	}
	
	public void Drop(Level level, Agent agent, Item item)
	{
		item.X = agent.X;
		item.Y = agent.Y;
		level.Add(item);
		item.Position = new Vector2(item.X * 24, item.Y * 24);
		agent.Messages.Add($"You drop your {item.DisplayName}");
	}
}

public static class Globals
{
	public static Random Random { get; } = new Random();
	
	public static List<Deity> Deities { get; set; } = new List<Deity>();
	
	static Globals()
	{
		MakeDeities();
	}
	
	public static void OnEvent(IEvent e)
	{
		foreach (var deity in Deities)
			deity.OnEvent(e);
	}
	
	private static string MakeDeityName()
	{
		var vowels = "aeiou";
		var consonants = "bcdfghjklmnpqrstvwxz";
		if (Random.NextDouble() < 0.66)
			vowels += "y";
		else
			consonants += "y";
		
		for (var i = 0; i < 6; i++)
		{
			var index = Random.Next(consonants.Length);
			consonants = consonants.Substring(0, index) + consonants.Substring(index+1);
		}
		for (var i = 0; i < 6; i++)
		{
			var index = Random.Next(consonants.Length);
			consonants += consonants[index];
		}
		
		var name = "";
		if (Random.NextDouble() < 0.5)
			name += vowels[Random.Next(vowels.Length)];
		while (name.Length < 4)
		{
			name += consonants[Random.Next(consonants.Length)];
			name += vowels[Random.Next(vowels.Length)];
		}
		if (Random.NextDouble() < 0.5)
			name += consonants[Random.Next(consonants.Length)];
		
		if (Random.NextDouble() < 0.25)
		{
			var index = Random.Next(name.Length);
			name = name.Substring(0, index) + name.Substring(index+1);
		}
		
		if (Random.NextDouble() < 0.25)
		{
			var index = Random.Next(name.Length);
			name = name.Substring(0, index) + name.Substring(index, 1) + name.Substring(index);
		}
		
		name = name.Replace("q", "qu");
		name = name.Substring(0, 1).ToUpper() + name.Substring(1);
		return name;
	}
	
	private static void MakeDeities()
	{
		var types = System.Reflection.Assembly
			.GetExecutingAssembly()
			.GetTypes();
		
		var archetypes = types
			.Where(t => typeof(DeityArchetype).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
			.Select(t => (DeityArchetype)Activator.CreateInstance(t))
			.ToList();
			
		var domains = types
			.Where(t => typeof(DeityDomain).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
			.Select(t => (DeityDomain)Activator.CreateInstance(t))
			.ToList();
		
		while (archetypes.Any() 
			&& domains.Any() 
			&& Deities.Count < 6)
		{
			var deity = new Deity {
				Name = MakeDeityName(),
				Pronoun = Globals.Random.NextDouble() < 0.5 ? "he" : "she",
				Archetype = archetypes[Random.Next(archetypes.Count)]
			};
			archetypes.Remove(deity.Archetype);
			
			while (domains.Any() && deity.Domains.Count < deity.Archetype.NumberOfDomains)
			{
				var domain = domains[Random.Next(domains.Count)];
				domains.Remove(domain);
				deity.Domains.Add(domain);
			}
			
			Deities.Add(deity);
		}
		
		foreach (var deity in Deities)
		{
			deity.Finalize(Deities);
			GD.Print(deity.GetFullTitle());
		}
	}
}

public class Deity
{
	public string Name { get; set; }
	public string Pronoun { get; set; }
	public DeityArchetype Archetype { get; set; }
	public List<DeityDomain> Domains { get; set; } = new List<DeityDomain>();
	public List<string> Likes { get; set; } = new List<string>();
	public List<string> Dislikes { get; set; } = new List<string>();
	public int PlayerFavor { get; set; } = 0;
	
	public int StrengthOfLikes { get; set; } = 2;
	public int StrengthOfDislikes { get; set; } = 2;
	public bool AcceptsPrayers { get; set; } = true;
	public bool AcceptsDonations { get; set; } = true;
	public bool AcceptsSacrafices { get; set; } = true;
	
	public string GetFullTitle()
	{
		var title = Pronoun == "he" ? "god" : "godess";
		if (Domains.Count == 0)
			return Name + " the " + Archetype.Name + " " + title;
		else
			return Name + " the " + Archetype.Name + $" {title} of " + Util.AndList(Domains.Select(d => d.Name));
	}
	
	public string GetShortTitle()
	{
		var title = Pronoun == "he" ? "god" : "godess";
		return Name + " the " + Archetype.Name + " " + title;
	}
	
	public void Finalize(IEnumerable<Deity> deities)
	{
		AcceptsPrayers = Globals.Random.NextDouble() < 0.9f;
		AcceptsDonations = Globals.Random.NextDouble() < 0.9f;
		AcceptsSacrafices = Globals.Random.NextDouble() < 0.1f;
		
		Archetype.Finalize(this, deities);
		foreach (var domain in Domains)
			domain.Finalize(this, deities);
		
		Likes = Likes.Distinct().ToList();
		Dislikes = Dislikes.Distinct().ToList();
		
		var common = Likes.Where(Dislikes.Contains).ToArray();
		foreach (var thing in common)
		{
			Likes.Remove(thing);
			Dislikes.Remove(thing);
		}
		
		PlayerFavor += StrengthOfLikes - StrengthOfDislikes;
	}
	
	public void OnEvent(IEvent e)
	{
		Archetype.OnEvent(this, e);
		foreach (var domain in Domains)
			domain.OnEvent(this, e);
	}
	
	public void Like(Agent agent, string what)
	{
		if (agent.Team == "player")
			PlayerFavor += StrengthOfLikes;
		agent.Messages.Add(Name + " likes " + what);
		GD.Print(Name + " likes " + agent.DisplayName);
	}
	
	public void Dislike(Agent agent, string what)
	{
		if (agent.Team == "player")
			PlayerFavor -= StrengthOfDislikes;
		agent.Messages.Add(Name + " dislikes " + what);
		GD.Print(Name + " dislikes " + agent.DisplayName);
	}
}

public abstract class DeityArchetype
{
	public string Name { get; set; }
	public string Description { get; set; }
	public int NumberOfDomains { get; set; } = 3;
	public float ChanceOfLikes { get; set; } = 0.9f;
	public float ChanceOfDislikes { get; set; } = 0.9f;
	public float ChanceOfInteracting { get; set; } = 0.25f;
	
	public DeityArchetype(string name)
	{
		Name = name;
	}
	
	public virtual void Finalize(Deity self, IEnumerable<Deity> deities)
	{
	}
	
	public virtual void OnEvent(Deity self, IEvent e)
	{
	}
}

public abstract class DeityDomain
{
	public string Name { get; set; }
	public string Description { get; set; }
	
	public DeityDomain(string name)
	{
		Name = name;
	}
	
	public virtual void Finalize(Deity self, IEnumerable<Deity> deities)
	{
	}
	
	public virtual void OnEvent(Deity self, IEvent e)
	{
	}
}

public class Tile
{
	public static Tile Floor = new Tile { BlocksMovement = false, Indices = new []{ 0, 1, 2 } };
	public static Tile Wall = new Tile { BlocksMovement = true, Indices = new []{ 3, 4, 5 } };
	
	public int[] Indices { get; set; }
	public bool BlocksMovement { get; set; }
	
	public int RandomIndex()
	{
		return Indices[Globals.Random.Next(Indices.Length)];
	}
}

public enum AgentTag
{
	Living, Undead, Stationary
}

public enum ItemType
{
	Armor, Weapon, Other
}

public interface IEvent
{
}

public class DidAttack : IEvent
{
	public Agent Attacker { get; set; }
	public Agent Attacked { get; set; }
	public DidAttack(Agent attacker, Agent attacked)
	{
		Attacker = attacker;
		Attacked = attacked;
	}
}

public class UsedItem : IEvent
{
	public Agent Agent { get; set; }
	public Item Item { get; set; }
	public UsedItem(Agent agent, Item item)
	{
		Agent = agent;
		Item = item;
	}
}

public class NextTurn : IEvent
{
	public Agent Player { get; set; }
}
