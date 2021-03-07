using Godot;
using System;

public class AgentView : Sprite
{
	public Agent Agent { get; set; }
	public Vector2 PreviousPosition { get; set; }
	public bool IsMoving { get; set; }
	
	public override void _Ready()
	{
		RegionRect = new Rect2(Agent.SpriteX * 26 + 2, Agent.SpriteY * 26 + 2, 24, 24);
		Position = new Vector2(Agent.X * 24, Agent.Y * 24);
		PreviousPosition = new Vector2(Agent.X, Agent.Y);
	}
	
	public override void _Process(float delta)
	{
		if (IsMoving)
		{
			var speed = 24 * 8 * delta;
			var target = new Vector2(Agent.X * 24, Agent.Y * 24);
			var diff = target - Position;
			if (diff.Length() < speed)
			{
				Position = target;
				IsMoving = false;
			}
			else
			{
				Position += diff.Normalized() * speed;
			}
		}
		else if (Agent.X != PreviousPosition.x || Agent.Y != PreviousPosition.y)
		{
			IsMoving = true;
			PreviousPosition = new Vector2(Agent.X, Agent.Y);
		}
	}
}
