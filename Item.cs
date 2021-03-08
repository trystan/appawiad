using Godot;
using System;

public class Item : Sprite
{
	public string DisplayName { get; set; }
	
	public int X { get; set; }
	public int Y { get; set; }
	
	public int SpriteX { get; set; }
	public int SpriteY { get; set; }
	
	public ItemType Type { get; set; }
	public string MadeOf { get; set; }
	
	public Item Setup(int x, int y, int spriteX, int spriteY)
	{
		X = x;
		Y = y;
		SpriteX = spriteX;
		SpriteY = spriteY;
		return this;
	}
	
	public override void _Ready()
	{
		RegionRect = new Rect2(SpriteX * 26 + 2, SpriteY * 26 + 2, 24, 24);
		Position = new Vector2(X * 24, Y * 24);
	}
}
