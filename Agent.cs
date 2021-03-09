using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Agent : Sprite
{
	public Vector2 PreviousPosition { get; set; }
	public bool IsMoving { get; set; }
	
	public override void _Ready()
	{
		RegionRect = new Rect2(SpriteX * 26 + 2, SpriteY * 26 + 2, 24, 24);
		Position = new Vector2(X * 24, Y * 24);
		PreviousPosition = new Vector2(X, Y);
	}
	
	public override void _Process(float delta)
	{
		if (HP < 1) {
			QueueFree();
		} else if (IsMoving)
		{
			var speed = 24 * 8 * delta;
			var target = new Vector2(X * 24, Y * 24);
			var diff = target - Position;
			if (diff.Length() < speed)
			{
				Position = target;
				IsMoving = false;
				IsBusy = false;
			}
			else
			{
				Position += diff.Normalized() * speed;
			}
		}
		else if (X != PreviousPosition.x || Y != PreviousPosition.y)
		{
			IsMoving = true;
			PreviousPosition = new Vector2(X, Y);
		}
	}
	
	public string DisplayName { get; set; }
	
	public List<StatusEffect> StatusEffects { get; set; } = new List<StatusEffect>();
	public List<AgentTag> Tags { get; set; } = new List<AgentTag>();
	public string Team { get; set; }
	public Item Armor { get; set; }
	public Item Weapon { get; set; }
	public bool IsBusy { get; set; }
	
	public int X { get; set; }
	public int Y { get; set; }
	
	public int SpriteX { get; set; }
	public int SpriteY { get; set; }
	
	public int HP { get; set; } = 6;
	public int HPMax { get; set; } = 6;
	
	public int ATK { get; set; } = 3;
	public int DEF { get; set; } = 3;
	
	public int AP { get; set; } = 10;
	public int APRegeneration { get; set; } = 10;
	public int Money { get; set; }
	
	public List<string> Messages { get; set; } = new List<string>();
	
	public Agent Setup(int x, int y, int spriteX, int spriteY)
	{
		X = x;
		Y = y;
		SpriteX = spriteX;
		SpriteY = spriteY;
		return this;
	}
	
	public void TakeDamage(Attack attack)
	{
		HP -= Math.Max(0, attack.TotalDamage);
	}
	
	public void EndTurn()
	{
		AP += Math.Max(1, APRegeneration);
		foreach (var effect in StatusEffects.ToArray())
			effect.OnTurn(null, this);
	}
}
