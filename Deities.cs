using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class Chaotic : DeityArchetype
{
	public Chaotic() : base("chaotic")
	{
		ChanceOfLikes = 0.5f;
		ChanceOfDislikes = 0.5f;
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		self.ChanceOfBlessing = 0.5f;
		self.ChanceOfCurse = 0.5f;
		if (Globals.Random.NextDouble() < self.Archetype.ChanceOfLikes)
			self.Likes.Add("chaos");
		if (Globals.Random.NextDouble() < self.Archetype.ChanceOfDislikes)
			self.Dislikes.Add("order");
		Description = $"{self.Name} is chaotic and unpredictable. Favor can be gained or lost without warning.";
	}
	
	public override void OnEvent(Deity self, IEvent e)
	{
		switch (e)
		{
			case NextTurn next:
				if (next.Player.StatusEffects.Count > 0
					&& Globals.Random.NextDouble() < 0.01)
				{
					var effect = next.Player.StatusEffects[Globals.Random.Next(next.Player.StatusEffects.Count)];
					next.Player.Messages.Add($"{self.Name} tires of {effect.Name}");
					effect.End(null, next.Player);
				}
				
				foreach (var key in self.FavorPerTeam.Keys.ToArray())
				{
					if (self.FavorPerTeam[key] > 0)
						self.FavorPerTeam[key] += Globals.Random.Next(3) - Globals.Random.Next(4);
					else
						self.FavorPerTeam[key] += Globals.Random.Next(4) - Globals.Random.Next(3);
				}
				break;
				
			case DidAttack attack when Globals.Random.NextDouble() < 0.05f:
				switch (Globals.Random.Next(4))
				{
					case 0:
						self.Like(attack.Attacker, "your style");
						break;
					case 1:
						self.Like(attack.Attacked, "your style");
						break;
					case 2:
						self.Dislike(attack.Attacker, "your style");
						break;
					case 3:
						self.Dislike(attack.Attacked, "your style");
						break;
				}
				break;
		}
	}
	
	public override void AddToBlessing(Deity self, Level level, Agent agent, StatusEffect blessing)
	{
		blessing.TurnsRemaining += Globals.Random.Next(10);
		blessing.TurnsRemaining -= Globals.Random.Next(10);
	}
	
	public override void AddToCurse(Deity self, Level level, Agent agent, StatusEffect curse)
	{
		curse.TurnsRemaining += Globals.Random.Next(10);
		curse.TurnsRemaining -= Globals.Random.Next(10);
	}
}

public class Obsessed : DeityArchetype
{
	public Obsessed() : base("obsessed")
	{
		NumberOfDomains = 1;
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		self.ChanceOfBlessing += 0.25f;
		self.ChanceOfCurse += 0.25f;
		self.StrengthOfLikes *= 2;
		self.StrengthOfDislikes *= 2;
		
		Description = $"{self.Name} only cares about {self.Domains[0].Name}.";
		if (Globals.Random.NextDouble() < self.Archetype.ChanceOfLikes)
			self.Likes.Add(self.Domains[0].Name);
	}
	
	public override void AddToBlessing(Deity self, Level level, Agent agent, StatusEffect blessing)
	{
		blessing.TurnsRemaining += 50;
	}
	
	public override void AddToCurse(Deity self, Level level, Agent agent, StatusEffect curse)
	{
		curse.TurnsRemaining += 50;
	}
}

public class Trickster : DeityArchetype
{
	public Deity Rival { get; set; }
	
	public Trickster() : base("trickster")
	{
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		Description = $"Like all tricksters, {self.Name}'s fondness of tricks and pranks and often gets others into trouble.";
		
		if (Globals.Random.NextDouble() < self.Archetype.ChanceOfLikes)
			self.Likes.Add("illusions");
		
		var rival = deities.ToArray()[Globals.Random.Next(deities.Count())];
		if (rival != self)
		{
			Rival = rival;
			self.Dislikes.Add(rival.GetShortTitle());
		}
	}
}

public class Sleeping : DeityArchetype
{
	public Sleeping() : base("sleeping")
	{
		NumberOfDomains++;
		ChanceOfInteracting = 0;
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		self.ChanceOfBlessing = -10;
		self.ChanceOfCurse = -10;
		self.AcceptsPrayers = false;
		self.AcceptsDonations = false;
		self.AcceptsSacrafices = false;
		Description = $"{self.Name} slumbers forever and does not intervine in the mundane world.";
	}
}

public class Vengeful : DeityArchetype
{
	public Vengeful() : base("vengeful")
	{
		ChanceOfLikes = 0.1f;
		ChanceOfDislikes = 10.0f;
		ChanceOfInteracting = 0.5f;
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		self.ChanceOfBlessing -= 0.25f;
		self.ChanceOfCurse += 0.25f;
		self.StrengthOfLikes /= 2;
		self.StrengthOfDislikes *= 3;
		self.AcceptsPrayers = true;
		self.AcceptsDonations = true;
		self.AcceptsSacrafices = true;
		Description = "A vengeful deity is easy to displease but often intervenes to help those who worship them.";
	}
	
	public override void AddToBlessing(Deity self, Level level, Agent agent, StatusEffect blessing)
	{
		blessing.TurnsRemaining -= 10;
		
		if (agent.HP < 6)
		{
			blessing.AddEffect("instant +HP",
				() => { agent.HP++; },
				() => { });
		}
	}
	
	public override void AddToCurse(Deity self, Level level, Agent agent, StatusEffect curse)
	{
		curse.TurnsRemaining += 15;
		
		if (agent.HP > 3)
		{
			curse.AddEffect("instant -HP",
				() => { agent.HP--; },
				() => { });
		}
	}
}





public class OfDeath : DeityDomain
{
	bool likesKillingLiving;
	bool dislikesKillingUndead;
	
	public OfDeath() : base("death")
	{
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		self.FavorPerTeam["undead"] += 50;
		self.AcceptsSacrafices = true;
		likesKillingLiving = Globals.Random.NextDouble() < self.Archetype.ChanceOfLikes;
		dislikesKillingUndead = Globals.Random.NextDouble() < self.Archetype.ChanceOfDislikes;
		if (likesKillingLiving)
			self.Likes.Add("killing living beings");
		if (dislikesKillingUndead)
			self.Dislikes.Add("killing undead beings");
	}
	
	public override void OnEvent(Deity self, IEvent e)
	{
		switch (e)
		{
			case DidAttack attack when attack.Attacked.HP < 1:
				if (likesKillingLiving && attack.Attacked.Tags.Contains(AgentTag.Living))
					self.Like(attack.Attacker, "killing the living");
				if (dislikesKillingUndead && attack.Attacked.Tags.Contains(AgentTag.Undead))
					self.Dislike(attack.Attacker, "killing the undead");
				self.Like(attack.Attacked, "how you died");
				break;
		}
	}
	
	public override void AddToBlessing(Deity self, Level level, Agent agent, StatusEffect blessing)
	{
	}
	
	public override void AddToCurse(Deity self, Level level, Agent agent, StatusEffect curse)
	{
		curse.AddDefendEffect("+damage from undead", a => {
			if (a.Attacker.Team == "undead")
				a.TotalDamage++;
		});
	}
}

public class OfHealth : DeityDomain
{
	public OfHealth() : base("health")
	{
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		self.AcceptsSacrafices = false;
		if (Globals.Random.NextDouble() < self.Archetype.ChanceOfLikes)
			self.Likes.Add("healing living beings");
		if (Globals.Random.NextDouble() < self.Archetype.ChanceOfDislikes)
			self.Dislikes.Add("healing undead beings");
	}
	
	public override void OnEvent(Deity self, IEvent e)
	{
		if (Globals.Random.NextDouble() > self.Archetype.ChanceOfInteracting)
			return;
		
		switch (e)
		{
			case NextTurn turn:
				if (Globals.Random.NextDouble() < self.Archetype.ChanceOfInteracting)
				{
					if (self.PlayerFavor > turn.Player.HP * 5
						&& Globals.Random.Next(10) < turn.Player.HP)
					{
//						TODO
//						self.PlayerFavor--;
//						turn.Player.HP++;
//						turn.Player.Messages.Add($"{self.Name} healed you");
					}
					if (self.PlayerFavor > turn.Player.HP * 5
						&& Globals.Random.Next(20) < turn.Player.AP)
					{
//						TODO
//						self.PlayerFavor--;
//						turn.Player.AP += 4;
//						turn.Player.Messages.Add($"{self.Name} has given you a boost of speed");
					}
				}
				break;
		}
	}
	
	public override void AddToBlessing(Deity self, Level level, Agent agent, StatusEffect blessing)
	{
		blessing.AddAttackEffect("+damage to undead", a => {
			if (a.Defender.Team == "undead")
				a.TotalDamage++;
		});
		
		blessing.AddEffect("+HP max",
			() => { agent.HPMax++; agent.HP++; },
			() => { agent.HPMax--; agent.HP = Math.Min(agent.HP, agent.HPMax); });
	}
	
	public override void AddToCurse(Deity self, Level level, Agent agent, StatusEffect curse)
	{
		curse.AddDefendEffect("+damage from undead", a => {
			if (a.Attacker.Team == "undead")
				a.TotalDamage++;
		});
	}
}

public class OfLove : DeityDomain
{
	public OfLove() : base("love")
	{
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		self.ChanceOfBlessing += 0.25f;
		self.ChanceOfCurse -= 0.25f;
		self.StrengthOfLikes *= 2;
		self.StrengthOfDislikes /= 2;
		
		var other = self;
		while (other == self || other.Archetype is Trickster t && t.Rival == self)
			other = deities.ToArray()[Globals.Random.Next(deities.Count())];
		
		self.Likes.Add(other.GetShortTitle());
		other.Likes.Add(self.GetShortTitle());
	}
	
	public override void AddToBlessing(Deity self, Level level, Agent agent, StatusEffect blessing)
	{
		if (agent.HP < agent.HPMax)
		{
			blessing.AddEffect("instant +HP",
				() => { agent.HP++; },
				() => { });
		}
	}
	
	public override void AddToCurse(Deity self, Level level, Agent agent, StatusEffect curse)
	{
	}
}

public class OfWriting : DeityDomain
{
	public OfWriting() : base("writing")
	{
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		if (Globals.Random.NextDouble() < self.Archetype.ChanceOfLikes)
			self.Likes.Add("reading scrolls");
		if (Globals.Random.NextDouble() < self.Archetype.ChanceOfDislikes)
			self.Dislikes.Add("destroying scrolls");
	}
}

public class OfCommerce : DeityDomain
{
	bool likesMoney;
	
	public OfCommerce() : base("commerce")
	{
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		self.AcceptsDonations = true;
		
		likesMoney = Globals.Random.NextDouble() < self.Archetype.ChanceOfLikes;
		if (likesMoney)
			self.Likes.Add("money");
	}
	
	public override void OnEvent(Deity self, IEvent e)
	{
		switch (e)
		{
			case NextTurn turn:
				if (Globals.Random.NextDouble() > self.Archetype.ChanceOfInteracting
					&& self.PlayerFavor > turn.Player.Money
					&& Globals.Random.Next(50) < turn.Player.Money)
				{
//					TODO
//					self.PlayerFavor--;
//					turn.Player.Money++;
//					turn.Player.Messages.Add($"{self.Name} gave you money");
				}
				break;
		}
	}
	
	public override void AddToBlessing(Deity self, Level level, Agent agent, StatusEffect blessing)
	{
		blessing.AddEffect("instant +money",
			() => { agent.Money++; },
			() => { });
	}
	
	public override void AddToCurse(Deity self, Level level, Agent agent, StatusEffect curse)
	{
		if (agent.Money > 0)
		{
			curse.AddEffect("instant -money",
				() => { agent.Money--; },
				() => { });
		}
	}
}

public class OfWar : DeityDomain
{
	bool likesKillingEnemies;
	bool dislikesKillingAllies;
	
	public OfWar() : base("war")
	{
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		likesKillingEnemies = Globals.Random.NextDouble() < self.Archetype.ChanceOfLikes;
		if (likesKillingEnemies)
			self.Likes.Add("killing enemies");
		dislikesKillingAllies = Globals.Random.NextDouble() < self.Archetype.ChanceOfDislikes;
		if (dislikesKillingAllies)
			self.Dislikes.Add("killing allies");
	}
	
	public override void OnEvent(Deity self, IEvent e)
	{
		switch (e)
		{
			case NextTurn turn:
				if (Globals.Random.NextDouble() < self.Archetype.ChanceOfInteracting
					&& Globals.Random.Next(100) < self.PlayerFavor
					&& self.PlayerFavor > 10
					&& turn.Player.APRegeneration < 12)
				{
//					TODO
//					turn.Player.APRegeneration++;
//					self.PlayerFavor -= 10;
//					turn.Player.Messages.Add($"You feel faster.");
//					turn.Player.Messages.Add($"{self.Name} has improved your AP regeneration.");
				}
				break;
				
			case DidAttack attack when attack.Attacked.HP < 1:
				if (likesKillingEnemies && attack.Attacked.Team != attack.Attacker.Team)
					self.Like(attack.Attacker, "killing enemies");
				if (dislikesKillingAllies && attack.Attacked.Team == attack.Attacker.Team)
					self.Dislike(attack.Attacker, "killing allies");
				break;
		}
	}
	
	public override void AddToBlessing(Deity self, Level level, Agent agent, StatusEffect blessing)
	{
		blessing.AddAttackEffect("+damage to all", a => a.TotalDamage++);
		blessing.AddDefendEffect("-damage from all", a => a.TotalDamage--);
	}
	
	public override void AddToCurse(Deity self, Level level, Agent agent, StatusEffect curse)
	{
		curse.AddAttackEffect("-damage to all", a => a.TotalDamage--);
		curse.AddDefendEffect("+damage from all", a => a.TotalDamage++);
	}
}

public class OfStorms : DeityDomain
{
	public OfStorms() : base("storms")
	{
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		if (Globals.Random.NextDouble() < self.Archetype.ChanceOfLikes)
			self.Likes.Add("winter");
		if (Globals.Random.NextDouble() < self.Archetype.ChanceOfLikes)
			self.Likes.Add("rain");
		if (Globals.Random.NextDouble() < self.Archetype.ChanceOfDislikes)
			self.Dislikes.Add("fire");
	}
	
	public override void AddToBlessing(Deity self, Level level, Agent agent, StatusEffect blessing)
	{
		blessing.AddEffect("instant +AP",
			() => { agent.AP += 50; },
			() => { });
	}
	
	public override void AddToCurse(Deity self, Level level, Agent agent, StatusEffect curse)
	{
		curse.AddEffect("instant -AP",
			() => { agent.AP -= 20; },
			() => { });
	}
}

public class OfAgriculture : DeityDomain
{
	public OfAgriculture() : base("agriculture")
	{
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		self.FavorPerTeam["plants"] += 20;
	}
	
	public override void AddToBlessing(Deity self, Level level, Agent agent, StatusEffect blessing)
	{
		blessing.AddEffect("+AP regen",
			() => { agent.APRegeneration += 3; },
			() => { agent.APRegeneration -= 3; });
	}
	
	public override void AddToCurse(Deity self, Level level, Agent agent, StatusEffect curse)
	{
		curse.AddEffect("-AP regen",
			() => { agent.APRegeneration -= 2; },
			() => { agent.APRegeneration += 2; });
	}
}

public class OfFire : DeityDomain
{
	public OfFire() : base("fire")
	{
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		if (Globals.Random.NextDouble() < self.Archetype.ChanceOfLikes)
			self.Likes.Add("fire");
		if (Globals.Random.NextDouble() < self.Archetype.ChanceOfDislikes)
			self.Dislikes.Add("rain");
	}
	
	public override void AddToBlessing(Deity self, Level level, Agent agent, StatusEffect blessing)
	{
		blessing.AddAttackEffect("+vs plants", a => {
			if (a.Defender.Team == "plants")
				a.TotalDamage++;
		});
	}
	
	public override void AddToCurse(Deity self, Level level, Agent agent, StatusEffect curse)
	{
	}
}

public class OfStars : DeityDomain
{
	public OfStars() : base("stars")
	{
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		if (Globals.Random.NextDouble() < self.Archetype.ChanceOfLikes)
			self.Likes.Add("being outdoors");
		if (Globals.Random.NextDouble() < self.Archetype.ChanceOfDislikes)
			self.Dislikes.Add("being indoors");
	}
}

public class OfSun : DeityDomain
{
	public OfSun() : base("the sun")
	{
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		if (Globals.Random.NextDouble() < self.Archetype.ChanceOfLikes)
			self.Likes.Add("fire");
	}
}

public class OfMoon : DeityDomain
{
	public OfMoon() : base("the moon")
	{
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		if (Globals.Random.NextDouble() < self.Archetype.ChanceOfLikes)
			self.Likes.Add("being outdoors");
		if (Globals.Random.NextDouble() < self.Archetype.ChanceOfDislikes)
			self.Dislikes.Add("being indoors");
	}
}

public class OfForests : DeityDomain
{
	bool likesUsingWood;
	bool dislikesKillingTrees;
	
	public OfForests() : base("forests")
	{
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		self.FavorPerTeam["plants"] += 40;
		likesUsingWood = Globals.Random.NextDouble() < self.Archetype.ChanceOfLikes;
		if (likesUsingWood)
			self.Likes.Add("using wooden items");
		dislikesKillingTrees = Globals.Random.NextDouble() < self.Archetype.ChanceOfDislikes;
		if (dislikesKillingTrees)
			self.Dislikes.Add("killing plants");
	}
	
	public override void OnEvent(Deity self, IEvent e)
	{
		switch (e)
		{
			case UsedItem used:
				if (likesUsingWood && used.Item.MadeOf == "wood")
					self.Like(used.Agent, "your " + used.Item.DisplayName);
				break;
				
			case DidAttack attack when attack.Attacked.HP < 1:
				if (dislikesKillingTrees && attack.Attacked.Team == "plants")
					self.Dislike(attack.Attacker, "killing plants");
				break;
		}
	}
	
	public override void AddToBlessing(Deity self, Level level, Agent agent, StatusEffect blessing)
	{
		if (likesUsingWood) {
			blessing.AddAttackEffect("+wood weapon damage", a => {
				if (a.Attacker.Weapon.MadeOf == "wood")
					a.TotalDamage++;
			});
		}
	}
	
	public override void AddToCurse(Deity self, Level level, Agent agent, StatusEffect curse)
	{
		curse.AddAttackEffect("-stone weapon damage", a => {
			if (a.Attacker.Weapon.MadeOf == "stone")
				a.TotalDamage--;
		});
		curse.AddAttackEffect("-metal weapon damage", a => {
			if (a.Attacker.Weapon.MadeOf == "metal")
				a.TotalDamage--;
		});
	}
}

public class OfMountains : DeityDomain
{
	bool likesUsingStone;
	bool likesUsingMetal;
	
	public OfMountains() : base("mountains")
	{
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		likesUsingStone = Globals.Random.NextDouble() < self.Archetype.ChanceOfLikes;
		if (likesUsingStone)
			self.Likes.Add("using stone items");
		likesUsingMetal = Globals.Random.NextDouble() < self.Archetype.ChanceOfLikes;
		if (likesUsingMetal)
			self.Likes.Add("using metal items");
	}
	
	public override void OnEvent(Deity self, IEvent e)
	{
		switch (e)
		{
			case UsedItem used:
				if (likesUsingStone && used.Item.MadeOf == "stone")
					self.Like(used.Agent, "your " + used.Item.DisplayName);
				if (likesUsingMetal && used.Item.MadeOf == "metal")
					self.Like(used.Agent, "your " + used.Item.DisplayName);
				break;
		}
	}
	
	public override void AddToBlessing(Deity self, Level level, Agent agent, StatusEffect blessing)
	{
		if (likesUsingStone) {
			blessing.AddAttackEffect("+stone weapon damage", a => {
				if (a.Attacker.Weapon.MadeOf == "stone")
					a.TotalDamage++;
			});
		}
		if (likesUsingMetal) {
			blessing.AddAttackEffect("+metal weapon damage", a => {
				if (a.Attacker.Weapon.MadeOf == "metal")
					a.TotalDamage++;
			});
		}
	}
	
	public override void AddToCurse(Deity self, Level level, Agent agent, StatusEffect curse)
	{
		curse.AddAttackEffect("-wood weapon damage", a => {
			if (a.Attacker.Weapon.MadeOf == "wood")
				a.TotalDamage--;
		});
	}
}
