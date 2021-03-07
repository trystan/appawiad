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
		playerView.Agent = _player = new Agent(3, 4, 0, 9);
		_level.Agents.Add(_player);
		AddChild(playerView);
		
		for (var i = 0; i < 10; i++)
		{
			var view = (AgentView)_agentView.Instance();
			var x = Globals.Random.Next(32);
			var y = Globals.Random.Next(32);
			view.Agent = new Agent(x, y, 26, 5);
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
		if (!_level.GetTile(agent.X - 1, agent.Y).BlocksMovement)
			yield return new MoveBy(-1, 0);
		
		if (!_level.GetTile(agent.X + 1, agent.Y).BlocksMovement)
			yield return new MoveBy(1, 0);
		
		if (!_level.GetTile(agent.X, agent.Y - 1).BlocksMovement)
			yield return new MoveBy(0, -1);
		
		if (!_level.GetTile(agent.X, agent.Y + 1).BlocksMovement)
			yield return new MoveBy(0, 1);
		
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
	
	private static void MakeDeities()
	{
		var types = System.Reflection.Assembly
			.GetExecutingAssembly()
			.GetTypes();
		
		var names = "Thor Zeus Zom Posiden Loki Baal Asmodeus".Split(' ').ToList();
		
		var archetypes = types
			.Where(t => typeof(DeityArchetype).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
			.Select(t => (DeityArchetype)Activator.CreateInstance(t))
			.ToList();
			
		var domains = types
			.Where(t => typeof(DeityDomain).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
			.Select(t => (DeityDomain)Activator.CreateInstance(t))
			.ToList();
		
		while (names.Any() 
			&& archetypes.Any() 
			&& domains.Any() 
			&& Deities.Count < 6)
		{
			var deity = new Deity {
				Name = names[Random.Next(names.Count)],
				Archetype = archetypes[Random.Next(archetypes.Count)]
			};
			names.Remove(deity.Name);
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
	public DeityArchetype Archetype { get; set; }
	public List<DeityDomain> Domains { get; set; } = new List<DeityDomain>();
	public List<string> Likes { get; set; } = new List<string>();
	public List<string> Dislikes { get; set; } = new List<string>();
	
	public string GetFullTitle()
	{
		switch (Domains.Count)
		{
			case 0:
				return Name + " the " + Archetype.Name + " god";
			case 1:
				return Name + " the " + Archetype.Name + " god of " + Domains[0].Name;
			case 2:
				return Name + " the " + Archetype.Name + " god of " + Domains[0].Name + " and " + Domains[1].Name;
			default:
				var last = Domains[Domains.Count - 1];
				var rest = Domains.Take(Domains.Count - 1).ToList();
				return Name + " the " + Archetype.Name + " god of " + string.Join(", ", rest.Select(d => d.Name)) + ", and " + last.Name;
		}
	}
	
	public void Finalize(IEnumerable<Deity> deities)
	{
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

public class Agent
{
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
