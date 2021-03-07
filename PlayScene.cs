using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class PlayScene : Node2D
{
	TileMap _tileMap;
	PackedScene _agentView;
	Level _level;
	Agent _player;
	ICommand _nextPlayerCommand;
	
	public override void _Ready()
	{
		_agentView = (PackedScene)ResourceLoader.Load("res://AgentView.tscn");
		_tileMap = (TileMap)GetNode("TileMap");
		
		var catalog = new Catalog();
		
		_level = new Level(20, 20);
		
		for (var x = 0; x < _level.Tiles.GetLength(0); x++)
		{
			for (var y = 0; y < _level.Tiles.GetLength(1); y++)
			{
				var tile = _level.Tiles[x,y];
				_tileMap.SetCell(x, y, tile.RandomIndex());
			}
		}
		
		var playerView = (AgentView)_agentView.Instance();
		playerView.Agent = _player = new Agent(3, 4, 0, 9) {
			Name = "player",
			Team = "player",
			Tags = new List<AgentTag> { AgentTag.Living }
		};
		_level.Agents.Add(_player);
		AddChild(playerView);
		
		for (var i = 0; i < 32; i++)
		{
			var view = (AgentView)_agentView.Instance();
			var x = Globals.Random.Next(32);
			var y = Globals.Random.Next(32);
			
			view.Agent = catalog.NewEnemy(x, y);
			_level.Agents.Add(view.Agent);
			AddChild(view);
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
		if (_player == null)
			return;
		
		if (e is InputEventKey key && key.Pressed)
		{
			switch (key.Scancode)
			{
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
	}
	
	public override void _Process(float delta)
	{
		if (_level.Agents.Any() && !_level.Agents[0].IsBusy)
		{
			var agent = _level.Agents[0];
			
			if (agent == _player)
			{
				if (_nextPlayerCommand != null)
				{
					_level.Agents.RemoveAt(0);
					_nextPlayerCommand.Do(_level, _player);
					_nextPlayerCommand = null;
					_level.Agents.Add(agent);
				}
			}
			else
			{
				_level.Agents.RemoveAt(0);
				GetCommandForAi(agent).Do(_level, agent);
				_level.Agents.Add(agent);
			}
		}
	}
	
	public override void _Draw()
	{
		var color = new Color(1,1,1,0.1f);
		
		for (var x = 0; x < 32; x++)
		{
			DrawLine(new Vector2(x*24, 0), new Vector2(x*24, 32*24), color);
		}
		
		for (var y = 0; y < 32; y++)
		{
			DrawLine(new Vector2(0, y*24), new Vector2(32*24, y*24), color);
		}
	}
}

public class Catalog
{
	public Agent NewEnemy(int x, int y)
	{
		switch (Globals.Random.Next(5))
		{
			case 0: return NewGobbo(x, y);
			case 1: return NewSkeleton(x, y);
			case 2: return NewSpider(x, y);
			case 3: return NewPig(x, y);
			case 4: return NewTree(x, y);
		}
		return null;
	}
	
	public Agent NewGobbo(int x, int y) => new Agent(x, y, 31, 5) {
		Name = "gobbo",
		Team = "gobbos",
		Tags = new List<AgentTag> { AgentTag.Living }
	};
	
	public Agent NewSkeleton(int x, int y) => new Agent(x, y, 37, 5) {
		Name = "skeleton",
		Team = "skeleton",
		Tags = new List<AgentTag> { AgentTag.Undead }
	};
	
	public Agent NewPig(int x, int y) => new Agent(x, y, 2, 15) {
		Name = "pig",
		Team = "beasts",
		Tags = new List<AgentTag> { AgentTag.Living }
	};
	
	public Agent NewSpider(int x, int y) => new Agent(x, y, 2, 14) {
		Name = "spider",
		Team = "critters",
		Tags = new List<AgentTag> { AgentTag.Living }
	};
	
	public Agent NewTree(int x, int y) => new Agent(x, y, 0, 26) {
		Name = "tree",
		Team = "plant",
		Tags = new List<AgentTag> { AgentTag.Living, AgentTag.Stationary }
	};
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
		if (level.GetTile(agent.X + X, agent.Y + Y).BlocksMovement)
			return;
		if (X == 0 && Y == 0)
			return;
		
		var nextX = agent.X + X;
		var nextY = agent.Y + Y;
		
		var other = level.Agents.FirstOrDefault(a => a.X == nextX && a.Y == nextY);
		if (other != null)
		{
			other.Die();
			level.Agents.Remove(other);
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
	}
	
	public void OnEvent(IEvent e)
	{
		Archetype.OnEvent(this, e);
		foreach (var domain in Domains)
			domain.OnEvent(this, e);
	}
	
	public void Like(Agent agent)
	{
		GD.Print(Name + " likes " + agent.Name);
	}
	
	public void Dislike(Agent agent)
	{
		GD.Print(Name + " dislikes " + agent.Name);
	}
}

public abstract class DeityArchetype
{
	public string Name { get; set; }
	public string Description { get; set; }
	public int NumberOfDomains { get; set; } = 3;
	public float ChanceOfLikes { get; set; } = 0.9f;
	public float ChanceOfDislikes { get; set; } = 0.9f;
	
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

public class Level
{
	public Tile[,] Tiles { get; private set; }
	public List<Agent> Agents { get; private set; } = new List<Agent>();
	
	public Level(int w, int h)
	{
		Tiles = new Tile[32,32];
		
		for (var x = 0; x < Tiles.GetLength(0); x++)
		{
			for (var y = 0; y < Tiles.GetLength(1); y++)
			{
				Tiles[x,y] = Globals.Random.NextDouble() < 0.1 ? Tile.Wall : Tile.Floor;
			}
		}
	}
	
	public Tile GetTile(int x, int y)
	{
		if (x < 0 || x >= Tiles.GetLength(0) || y < 0 || y >= Tiles.GetLength(1))
			return Tile.Wall;
		return Tiles[x,y];
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

public class Agent
{
	public string Name { get; set; }
	
	public List<AgentTag> Tags { get; set; } = new List<AgentTag>();
	public string Team { get; set; }
	public bool IsBusy { get; set; }
	
	public int X { get; set; }
	public int Y { get; set; }
	
	public int SpriteX { get; set; }
	public int SpriteY { get; set; }
	
	public int HP { get; set; } = 1;
	
	public Agent(int x, int y, int spriteX, int spriteY)
	{
		X = x;
		Y = y;
		SpriteX = spriteX;
		SpriteY = spriteY;
	}
	
	public void Die()
	{
		HP = -1;
	}
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
