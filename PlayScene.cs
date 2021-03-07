using Godot;
using System;

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
		
		var view = (AgentView)_agentView.Instance();
		view.Agent = _player = new Agent(3, 4, 0, 9);
		AddChild(view);
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
		if (_player != null && _nextPlayerCommand != null)
		{
			_nextPlayerCommand.Do(_level, _player);
			_nextPlayerCommand = null;
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
		agent.X += X;
		agent.Y += Y;
	}
}

public static class Globals
{
	public static Random Random { get; } = new Random();
}

public class Level
{
	public Tile[,] Tiles { get; private set; }
	
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
}

public class Tile
{
	public static Tile Floor = new Tile { Indices = new []{ 0, 1, 2 } };
	public static Tile Wall = new Tile { Indices = new []{ 3, 4, 5 } };
	
	public int[] Indices { get; set; }
	
	public int RandomIndex()
	{
		return Indices[Globals.Random.Next(Indices.Length)];
	}
}

public class Agent
{
	public int X { get; set; }
	public int Y { get; set; }
	
	public int SpriteX { get; set; }
	public int SpriteY { get; set; }
	
	public Agent(int x, int y, int spriteX, int spriteY)
	{
		X = x;
		Y = y;
		SpriteX = spriteX;
		SpriteY = spriteY;
	}
}
