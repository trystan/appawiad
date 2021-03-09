using Godot;
using System;

public class TitleScene : CanvasLayer
{
	public override void _UnhandledInput(InputEvent e)
	{
		if (e is InputEventKey key && key.Pressed)
		{
			switch (key.Scancode)
			{
				case (int)KeyList.A:
					foreach (var deity in Globals.Deities)
						deity.FavorPerTeam["player"] += 20;
					GD.Print("priest");
					GetTree().ChangeScene("res://PlayScene.tscn");
					break;
					
				case (int)KeyList.B:
					foreach (var deity in Globals.Deities)
						deity.FavorPerTeam["player"] += 2;
					GD.Print("paladin");
					GetTree().ChangeScene("res://PaladinSetupScene.tscn");
					break;
					
				case (int)KeyList.C:
					foreach (var deity in Globals.Deities)
						deity.FavorPerTeam["player"] = Globals.Random.Next(-5,6);
					GD.Print("athiest");
					GetTree().ChangeScene("res://PlayScene.tscn");
					break;
			}
		}
	}
}
