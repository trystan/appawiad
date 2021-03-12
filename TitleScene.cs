using Godot;
using System;

public class TitleScene : CanvasLayer
{
	Catalog _catalog = new Catalog();
	
	public override void _Ready()
	{
		Globals.Player = _catalog.NewPlayer(3, 4);
	}
	
	public override void _UnhandledInput(InputEvent e)
	{
		if (e is InputEventKey key && key.Pressed)
		{
			switch (key.Scancode)
			{
				case (int)KeyList.A:
					foreach (var deity in Globals.Deities)
						deity.FavorPerTeam["player"] += 20;
					GetTree().ChangeScene("res://PlayScene.tscn");
					break;
					
				case (int)KeyList.B:
					foreach (var deity in Globals.Deities)
						deity.FavorPerTeam["player"] += 2;
					Globals.Player.Weapon = _catalog.NewSword(0,0);
					Globals.Player.Armor = _catalog.NewHeavyArmor(0,0);
					Globals.Player.ATK += Globals.Player.Weapon.ATK + Globals.Player.Armor.ATK;
					Globals.Player.DEF += Globals.Player.Weapon.DEF + Globals.Player.Armor.DEF;
					
					GetTree().ChangeScene("res://PaladinSetupScene.tscn");
					break;
					
				case (int)KeyList.C:
					foreach (var deity in Globals.Deities)
						deity.FavorPerTeam["player"] = Globals.Random.Next(-5,6);
					Globals.Player.Weapon = _catalog.NewWeapon(0,0);
					Globals.Player.Armor = _catalog.NewArmor(0,0);
					Globals.Player.ATK += Globals.Player.Weapon.ATK + Globals.Player.Armor.ATK;
					Globals.Player.DEF += Globals.Player.Weapon.DEF + Globals.Player.Armor.DEF;
					GetTree().ChangeScene("res://PlayScene.tscn");
					break;
			}
		}
	}
}
