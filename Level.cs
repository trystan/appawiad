using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Level : Node2D
{
	TileMap _tileMap;
	
	public Tile[,] Tiles { get; private set; }
	public List<Agent> Agents { get; set; } = new List<Agent>();
	public List<Item> Items { get; set; } = new List<Item>();
	
	public override void _Ready()
	{
		_tileMap = (TileMap)GetNode("TileMap");
	}
	
	public override void _Draw()
	{
		var color = new Color(1,1,1,0.1f);
		
		for (var x = 0; x < 32; x++)
			DrawLine(new Vector2(x*24, 0), new Vector2(x*24, 32*24), color);
		
		for (var y = 0; y < 32; y++)
			DrawLine(new Vector2(0, y*24), new Vector2(32*24, y*24), color);
	}
	
	public override void _Process(float delta)
	{
		Agents.RemoveAll(a => a.HP < 1);
	}
	
	public void Setup(int w, int h)
	{
		Tiles = new Tile[w,h];
		
		for (var x = 0; x < Tiles.GetLength(0); x++)
		{
			for (var y = 0; y < Tiles.GetLength(1); y++)
			{
				Tiles[x,y] = Globals.Random.NextDouble() < 0.1 ? Tile.Wall : Tile.Floor;
			}
		}
		
		for (var x = 0; x < Tiles.GetLength(0); x++)
		{
			for (var y = 0; y < Tiles.GetLength(1); y++)
			{
				var tile = Tiles[x,y];
				_tileMap.SetCell(x, y, tile.RandomIndex());
			}
		}
	}
	
	public Tile GetTile(int x, int y)
	{
		if (x < 0 || x >= Tiles.GetLength(0) || y < 0 || y >= Tiles.GetLength(1))
			return Tile.Wall;
		return Tiles[x,y];
	}
	
	public void Add(Item item)
	{
		Items.Add(item);
		AddChild(item);
	}
	
	public void Remove(Item item)
	{
		Items.Remove(item);
		RemoveChild(item);
	}
	
	public void Add(Agent agent)
	{
		Agents.Add(agent);
		AddChild(agent);
	}
	
	public void Remove(Agent agent)
	{
		Agents.Remove(agent);
		RemoveChild(agent);
	}
}
