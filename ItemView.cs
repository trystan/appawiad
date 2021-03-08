using Godot;
using System;

public class ItemView : Sprite
{
	public Item Item { get; set; }
	
	public override void _Ready()
	{
		RegionRect = new Rect2(Item.SpriteX * 26 + 2, Item.SpriteY * 26 + 2, 24, 24);
		Position = new Vector2(Item.X * 24, Item.Y * 24);
	}
	
	public override void _Process(float delta)
	{
		if (Item.IsPickedUp)
			QueueFree();
	}
}
