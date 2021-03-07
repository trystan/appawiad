using Godot;
using System;

public class TitleScene : Control
{
	Label _title;
	Label _footer;
	
	public override void _Ready()
	{
		_title = (Label)GetNode("Title");
		_title.RectPosition = new Vector2(OS.WindowSize.x / 2 - _title.RectSize.x / 2, 20);
		
		_footer = (Label)GetNode("Footer");
		_footer.RectPosition = new Vector2(OS.WindowSize.x / 2 - _title.RectSize.x / 2, OS.WindowSize.y - 50);
	}
	
	public override void _UnhandledInput(InputEvent e)
	{
		if (e is InputEventKey key && key.Pressed)
		{
			switch (key.Scancode)
			{
				case (int)KeyList.A:
					GD.Print("priest");
					GetTree().ChangeScene("res://PlayScene.tscn");
					break;
					
				case (int)KeyList.B:
					GD.Print("paladin");
					GetTree().ChangeScene("res://PaladinSetupScene.tscn");
					break;
					
				case (int)KeyList.C:
					GD.Print("athiest");
					GetTree().ChangeScene("res://PlayScene.tscn");
					break;
			}
		}
	}
}
