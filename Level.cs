using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Level : Node2D
{
	TileMap _tileMap;
	
	public Catalog Catalog { get; set; }
	public Tile[,] Tiles { get; private set; }
	public List<Agent> Agents { get; set; } = new List<Agent>();
	public List<Item> Items { get; set; } = new List<Item>();
	public int Depth { get; set; }
	
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
	
	public void Setup(Catalog catalog, int w, int h)
	{
		Depth = 0;
		Catalog = catalog;
		Setup(w, h);
	}
	
	public void Setup(int w, int h)
	{
		foreach (var item in Items)
			item.QueueFree();
		foreach (var item in Items.ToArray())
			Remove(item);
		foreach (var agent in Agents.ToArray())
			Remove(agent);
		Depth++;
		
		Tiles = new Tile[w,h];
		
		var solidPercent = 0.25 - Depth * 0.01;
		for (var x = 0; x < Tiles.GetLength(0); x++)
		{
			for (var y = 0; y < Tiles.GetLength(1); y++)
			{
				Tiles[x,y] = Globals.Random.NextDouble() < solidPercent ? Tile.Wall : Tile.Floor;
			}
		}
		
		var alterX = -1;
		var alterY = -1;
		while (GetTile(alterX, alterY).BumpEffect != TileBumpEffect.None)
		{
			alterX = Globals.Random.Next(w);
			alterY = Globals.Random.Next(h);
		}
		Tiles[alterX, alterY] = Tile.Alter;
		
		var downX = -1;
		var downY = -1;
		while (GetTile(downX, downY).BumpEffect != TileBumpEffect.None)
		{
			downX = Globals.Random.Next(w);
			downY = Globals.Random.Next(h);
		}
		Tiles[downX, downY] = Tile.DownStairs;
		
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
	
	public void SetTile(int x, int y, Tile tile)
	{
		Tiles[x,y] = tile;
		_tileMap.SetCell(x, y, tile.RandomIndex());
	}
	
	public Agent GetAgent(int x, int y)
	{
		return Agents.FirstOrDefault(a => a.X == x && a.Y == y);
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
	
	public void Remove(Agent agent, bool queueFree = true)
	{
		Agents.Remove(agent);
		
		try
		{
			RemoveChild(agent);
		}
		catch (System.ObjectDisposedException)
		{
		}
		
		if (queueFree)
			agent.QueueFree();
	}
}
