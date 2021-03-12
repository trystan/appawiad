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
				var timedEffects = next.Player.StatusEffects.Where(ef => ef.TurnsRemaining != null).ToArray();
				if (timedEffects.Length > 0
					&& Globals.Random.NextDouble() < 0.01)
				{
					var effect = timedEffects[Globals.Random.Next(timedEffects.Length)];
					next.Player.Messages.Add($"{self.Name} is bored of {effect.Name}");
					effect.End(null, next.Player);
				}
				
				switch (Globals.Random.Next(100))
				{
					case 0:
						self.Like(e.Level, next.Player, "you just because");
						break;
					case 1:
						self.Dislike(e.Level, next.Player, "you just because");
						break;
				}
				
				foreach (var key in self.FavorPerTeam.Keys.ToArray())
				{
					if (Globals.Random.NextDouble() < 0.9)
						continue;
						
					if (self.FavorPerTeam[key] > 0)
						self.FavorPerTeam[key] += Globals.Random.Next(3) - Globals.Random.Next(4);
					else
						self.FavorPerTeam[key] += Globals.Random.Next(4) - Globals.Random.Next(3);
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
		self.DonationMultiplier = -1;
		self.SacrificeCost = -100;
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
		self.DonationMultiplier = 1;
		self.SacrificeCost = 10;
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
		self.SacrificeCost -= 2;
		self.Likes.Add("bones");
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
			case UsedItem used:
				var wasGood = used.PreviousItem?.MadeOf == "bone";
				var good = used.Item?.MadeOf == "bone";
				if (good && !wasGood)
					self.Like(e.Level, used.Agent, "your " + used.Item.DisplayName);
				else if (!good && wasGood)
					self.Dislike(e.Level, used.Agent, "your " + used.Item.DisplayName);
				break;
				
			case DidAttack attack when attack.Attacked.HP < 1:
				if (likesKillingLiving && attack.Attacked.Tags.Contains(AgentTag.Living))
					self.Like(e.Level, attack.Attacker, "killing the living");
				if (dislikesKillingUndead && attack.Attacked.Tags.Contains(AgentTag.Undead))
					self.Dislike(e.Level, attack.Attacker, "killing the undead");
				if (Globals.Random.NextDouble() < 0.25)
					self.Like(e.Level, attack.Attacked, "how you died");
				break;
		}
	}
	
	public override void AddToBlessing(Deity self, Level level, Agent agent, StatusEffect blessing)
	{
	}
	
	public override void AddToCurse(Deity self, Level level, Agent agent, StatusEffect curse)
	{
		curse.AddDefendEffect("-DEF vs undead", a => {
			if (a.Attacker.Team == "undead")
				a.DefendBonus--;
		});
	}
	
	public override IEnumerable<string> GetPreferredMaterials()
	{
		yield return "bone";
	}
}

public class OfHealth : DeityDomain
{
	public OfHealth() : base("health")
	{
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		self.SacrificeCost = -100;
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
		blessing.AddAttackEffect("+ATK vs undead", a => {
			if (a.Defender.Team == "undead")
				a.AttackBonus++;
		});
		
		blessing.AddEffect("+HP max",
			() => { agent.HPMax++; agent.HP++; },
			() => { agent.HPMax--; agent.HP = Math.Min(agent.HP, agent.HPMax); });
	}
	
	public override void AddToCurse(Deity self, Level level, Agent agent, StatusEffect curse)
	{
		curse.AddDefendEffect("-DEF vs undead", a => {
			if (a.Attacker.Team == "undead")
				a.DefendBonus--;
		});
	}
	
	int _totalHealed;
	public override IEnumerable<StatusEffect> GetGoodInterventions(Deity self, Level level, Agent agent)
	{
		if (_totalHealed < 10)
		{
			var healing = new StatusEffect {
				Name = $"the healing presence of [color={Globals.TextColorGood}]{self.Name}[/color]",
				TurnsRemaining = 5
			};
			healing.AddTurnEffect(null, () => {
				if (agent.HP < agent.HPMax)
					_totalHealed++;
				agent.TakeDamage(-1);
			});
			
			yield return healing;
		}
	}
}

public class OfLove : DeityDomain
{
	Deity _lover;
	
	public OfLove() : base("love")
	{
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		self.ChanceOfBlessing += 0.25f;
		self.ChanceOfCurse -= 0.25f;
		self.StrengthOfLikes *= 2;
		self.StrengthOfDislikes /= 2;
		self.SacrificeCost += 5;
		
		_lover = self;
		while (_lover == self)
			_lover = deities.ToArray()[Globals.Random.Next(deities.Count())];
		
		self.Likes.Add(_lover.GetShortTitle());
		_lover.Likes.Add(self.GetShortTitle());
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
		self.DonationMultiplier = 2;
		
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
	
	int totalGiven;
	public override void AddToBlessing(Deity self, Level level, Agent agent, StatusEffect blessing)
	{
		if (totalGiven < 25)
		{
			blessing.AddEffect("instant +money",
				() => { totalGiven++; agent.Money++; },
				() => { });
		}
	}
	
	public override void AddToCurse(Deity self, Level level, Agent agent, StatusEffect curse)
	{
		if (agent.Money > 0)
		{
			curse.AddEffect("instant -money",
				() => { totalGiven--; agent.Money--; },
				() => { });
		}
	}
	
	public override IEnumerable<string> GetPreferredMaterials()
	{
		yield return "gold";
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
					self.Like(e.Level, attack.Attacker, "killing enemies");
				if (dislikesKillingAllies && attack.Attacked.Team == attack.Attacker.Team)
					self.Dislike(e.Level, attack.Attacker, "killing allies");
				break;
		}
	}
	
	public override void AddToBlessing(Deity self, Level level, Agent agent, StatusEffect blessing)
	{
		blessing.AddAttackEffect("+ATK vs all", a => a.AttackBonus++);
		blessing.AddDefendEffect("+DEF vs all", a => a.DefendBonus++);
	}
	
	public override void AddToCurse(Deity self, Level level, Agent agent, StatusEffect curse)
	{
		curse.AddAttackEffect("-ATK vs all", a => a.AttackBonus--);
		curse.AddDefendEffect("-DEF vs all", a => a.DefendBonus--);
	}
	
	int _chanceOfWeapon = 16;
	int _chanceOfArmor = 16;
	public override IEnumerable<StatusEffect> GetGoodInterventions(Deity self, Level level, Agent agent)
	{
		var materials = self.GetPreferredMaterials().ToArray();
		if (Globals.Random.Next(100) < _chanceOfWeapon)
		{
			var weapon = new StatusEffect {
				Name = null,
				TurnsRemaining = 1
			};
			weapon.AddEffect(null, 
				() => {
					_chanceOfWeapon /= 2;
					var item = level.Catalog.NewWeapon(0,0);
					if (materials.Any())
					{
						var material = materials[Globals.Random.Next(materials.Length)];
						item.MadeOf = material;
						item.DisplayName = $"{material} {item.DisplayName} of {self.Name}";
					}
					else
						item.DisplayName = $"{item.DisplayName} of {self.Name}";
					agent.Messages.Add($"[color={Globals.TextColorGood}]{self.Name}[/color] gives you a gift");
					new PickupItem().Do(level, agent, item);
				},
				() => {});
			yield return weapon;
		}
		
		if (Globals.Random.Next(100) < _chanceOfArmor)
		{
			var armor = new StatusEffect {
				Name = null,
				TurnsRemaining = 1
			};
			armor.AddEffect(null, 
				() => {
					_chanceOfArmor /= 2;
					var item = level.Catalog.NewArmor(0,0);
					if (materials.Any())
					{
						var material = materials[Globals.Random.Next(materials.Length)];
						item.MadeOf = material;
						item.DisplayName = $"{material} armor of {self.Name}";
					}
					else
						item.DisplayName = $"{item.DisplayName} of {self.Name}";
					agent.Messages.Add($"[color={Globals.TextColorGood}]{self.Name}[/color] gives you a gift");
					new PickupItem().Do(level, agent, item);
				},
				() => {});
			yield return armor;
		}
	}
}

public class OfAttack : DeityDomain
{
	bool likesAttacking;
	bool dislikesDefending;
	public OfAttack() : base("attacking")
	{
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		likesAttacking = Globals.Random.NextDouble() < self.Archetype.ChanceOfLikes;
		if (likesAttacking)
			self.Likes.Add("attacking");
		dislikesDefending = Globals.Random.NextDouble() < self.Archetype.ChanceOfDislikes;
		if (dislikesDefending)
			self.Dislikes.Add("defending");
	}
	
	public override void OnEvent(Deity self, IEvent e)
	{
		switch (e)
		{
			case DidAttack attack:
				if (likesAttacking && Globals.Random.NextDouble() < 0.2)
					self.Like(e.Level, attack.Attacker, "attacking");
				if (dislikesDefending && Globals.Random.NextDouble() < 0.2)
					self.Dislike(e.Level, attack.Attacker, "defending");
				break;
		}
	}
	
	public override void AddToBlessing(Deity self, Level level, Agent agent, StatusEffect blessing)
	{
		blessing.AddAttackEffect("+ATK vs all", a => a.AttackBonus += 2);
	}
	
	public override void AddToCurse(Deity self, Level level, Agent agent, StatusEffect curse)
	{
		curse.AddAttackEffect("-ATK vs all", a => a.AttackBonus -= 2);
	}
	
	int _chanceOfItem = 16;
	public override IEnumerable<StatusEffect> GetGoodInterventions(Deity self, Level level, Agent agent)
	{
		if (Globals.Random.Next(100) < _chanceOfItem)
		{
			var armor = new StatusEffect {
				Name = null,
				TurnsRemaining = 1
			};
			armor.AddEffect(null, 
				() => {
					_chanceOfItem /= 2;
					var item = level.Catalog.NewWeapon(0,0);
					item.ATK = 3;
					var materials = self.GetPreferredMaterials().ToArray();
					if (materials.Any())
					{
						var material = materials[Globals.Random.Next(materials.Length)];
						item.MadeOf = material;
						item.DisplayName = $"{material} {item.DisplayName} of {self.Name}";
					}
					else
						item.DisplayName = $"{item.DisplayName} of {self.Name}";
					agent.Messages.Add($"[color={Globals.TextColorGood}]{self.Name}[/color] gives you a gift");
					new PickupItem().Do(level, agent, item);
				},
				() => {});
			yield return armor;
		}
	}
}

public class OfProtection : DeityDomain
{
	bool likesDefending;
	bool dislikesAttacking;
	
	public OfProtection() : base("protection")
	{
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		likesDefending = Globals.Random.NextDouble() < self.Archetype.ChanceOfLikes;
		if (likesDefending)
			self.Likes.Add("defending");
		dislikesAttacking = Globals.Random.NextDouble() < self.Archetype.ChanceOfDislikes;
		if (dislikesAttacking)
			self.Dislikes.Add("attacking");
	}
	
	public override void OnEvent(Deity self, IEvent e)
	{
		switch (e)
		{
			case DidAttack attack:
				if (likesDefending && Globals.Random.NextDouble() < 0.2)
					self.Like(e.Level, attack.Attacked, "defending");
				if (dislikesAttacking && Globals.Random.NextDouble() < 0.2)
					self.Dislike(e.Level, attack.Attacked, "attacking");
				break;
		}
	}
	
	public override void AddToBlessing(Deity self, Level level, Agent agent, StatusEffect blessing)
	{
		blessing.AddAttackEffect("+DEF vs all", a => a.DefendBonus += 2);
	}
	
	public override void AddToCurse(Deity self, Level level, Agent agent, StatusEffect curse)
	{
		curse.AddAttackEffect("-DEF vs all", a => a.DefendBonus -= 2);
	}
	
	int _chanceOfItem = 16;
	public override IEnumerable<StatusEffect> GetGoodInterventions(Deity self, Level level, Agent agent)
	{
		if (Globals.Random.Next(100) < _chanceOfItem)
		{
			var armor = new StatusEffect {
				Name = null,
				TurnsRemaining = 1
			};
			armor.AddEffect(null, 
				() => {
					_chanceOfItem /= 2;
					var item = level.Catalog.NewArmor(0,0);
					item.DEF = 3;
					var materials = self.GetPreferredMaterials().ToArray();
					if (materials.Any())
					{
						var material = materials[Globals.Random.Next(materials.Length)];
						item.MadeOf = material;
						item.DisplayName = $"{material} armor of {self.Name}";
					}
					else
						item.DisplayName = $"{item.DisplayName} of {self.Name}";
					agent.Messages.Add($"[color={Globals.TextColorGood}]{self.Name}[/color] gives you a gift");
					new PickupItem().Do(level, agent, item);
				},
				() => {});
			yield return armor;
		}
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
	
	public override IEnumerable<StatusEffect> GetGoodInterventions(Deity self, Level level, Agent agent)
	{
		var power = new StatusEffect {
			Name = $"the subtle power of [color={Globals.TextColorGood}]{self.Name}[/color]",
			TurnsRemaining = 10
		};
		power.AddTurnEffect("+AP", () => agent.AP += 4);
		
		yield return power;
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
		blessing.AddAttackEffect("+ATK vs plants", a => {
			if (a.Defender.Team == "plants")
				a.AttackBonus++;
		});
	}
	
	private StatusEffect GetFire(Agent target)
	{
		var onFire = new StatusEffect {
			Name = "[color=#ff0000]On fire![/color]",
			TurnsRemaining = 5
		};
		onFire.AddEffect("-HP",
			() => target.BeginFire(),
			() => target.EndFire());
		onFire.AddTurnEffect(null, () => target.TakeDamage(1));
		return onFire;
	}
	
	public override IEnumerable<StatusEffect> GetBadInterventions(Deity self, Level level, Agent agent)
	{
		var disliked = new StatusEffect {
			Name = $"disliked by [color={Globals.TextColorBad}]{self.Name}[/color]",
			TurnsRemaining = 10
		};
		disliked.AddEffect(null,
			() => GetFire(agent).Begin(level, agent),
			() => { });
		
		yield return disliked;
	}
	
	int _chanceOfFire = 16;
	public override IEnumerable<StatusEffect> GetGoodInterventions(Deity self, Level level, Agent agent)
	{
		if (Globals.Random.Next(100) < _chanceOfFire)
		{
			var fire = new StatusEffect {
				Name = $"{self.Name}'s fiery protection",
				TurnsRemaining = 10
			};
			fire.AddEffect(null, () => { _chanceOfFire /= 2; }, () => {});
			fire.AddTurnEffect(null, 
				() => {
					for (var ox = -3; ox < 3; ox++)
					{
						for (var oy = -3; oy < 3; oy++)
						{
							var other = level.GetAgent(agent.X + ox, agent.Y + oy);
							if (other != null && other != agent && Globals.Random.NextDouble() < 0.5)
							{
								GetFire(other).Begin(level, other);
								agent.Messages.Add($"[color={Globals.TextColorGood}]{self.Name}[/color] sets {other.DisplayName} on fire");
							}
						}
					}
					
				});
			yield return fire;
		}
	}
	
	public override IEnumerable<string> GetPreferredMaterials()
	{
		yield return "fire";
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
	
	int _chanceOfStun = 16;
	public override IEnumerable<StatusEffect> GetGoodInterventions(Deity self, Level level, Agent agent)
	{
		if (Globals.Random.Next(100) < _chanceOfStun)
		{
			var protection = new StatusEffect {
				Name = $"{self.Name}'s sunning protection",
				TurnsRemaining = 10
			};
			protection.AddEffect(null, () => { _chanceOfStun /= 2; }, () => {});
			protection.AddTurnEffect(null, 
				() => {
					for (var ox = -3; ox < 3; ox++)
					{
						for (var oy = -3; oy < 3; oy++)
						{
							var other = level.GetAgent(agent.X + ox, agent.Y + oy);
							if (other != null && other != agent && Globals.Random.NextDouble() < 0.5)
							{
								var stun  = new StatusEffect {
									Name = "Stunned",
									TurnsRemaining = 10,
								};
								stun.AddTurnEffect("-AP", () => { other.AP -= 1; });
								stun.Begin(level, other);
								agent.Messages.Add($"[color={Globals.TextColorGood}]{self.Name}[/color] stuns {other.DisplayName}");
							}
						}
					}
				});
			yield return protection;
		}
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
				if (likesUsingWood)
				{
					var wasGood = used.PreviousItem?.MadeOf == "wood";
					var good = used.Item?.MadeOf == "wood";
					if (good && !wasGood)
						self.Like(e.Level, used.Agent, "your " + used.Item.DisplayName);
					else if (!good && wasGood)
						self.Dislike(e.Level, used.Agent, "your " + used.Item.DisplayName);
				}
				break;
				
			case DidAttack attack when attack.Attacked.HP < 1:
				if (dislikesKillingTrees && attack.Attacked.Team == "plants")
					self.Dislike(e.Level, attack.Attacker, "killing plants");
				break;
		}
	}
	
	public override void AddToBlessing(Deity self, Level level, Agent agent, StatusEffect blessing)
	{
		if (likesUsingWood) {
			blessing.AddAttackEffect("+wood weapon ATK", a => {
				if (a.Attacker.Weapon?.MadeOf == "wood")
					a.AttackBonus++;
			});
		}
	}
	
	public override void AddToCurse(Deity self, Level level, Agent agent, StatusEffect curse)
	{
		curse.AddAttackEffect("-stone weapon ATK", a => {
			if (a.Attacker.Weapon?.MadeOf == "stone")
				a.AttackBonus--;
		});
		curse.AddAttackEffect("-metal weapon ATK", a => {
			if (a.Attacker.Weapon?.MadeOf == "metal")
				a.AttackBonus--;
		});
	}
	
	public override IEnumerable<string> GetPreferredMaterials()
	{
		yield return "wood";
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
				if (likesUsingStone)
				{
					var wasGood = used.PreviousItem?.MadeOf == "stone";
					var good = used.Item?.MadeOf == "stone";
					if (good && !wasGood)
						self.Like(e.Level, used.Agent, "your " + used.Item.DisplayName);
					else if (!good && wasGood)
						self.Dislike(e.Level, used.Agent, "your " + used.Item.DisplayName);
				}
				if (likesUsingMetal)
				{
					var wasGood = used.PreviousItem?.MadeOf == "metal";
					var good = used.Item?.MadeOf == "metal";
					if (good && !wasGood)
						self.Like(e.Level, used.Agent, "your " + used.Item.DisplayName);
					else if (!good && wasGood)
						self.Dislike(e.Level, used.Agent, "your " + used.Item.DisplayName);
				}
				break;
		}
	}
	
	public override void AddToBlessing(Deity self, Level level, Agent agent, StatusEffect blessing)
	{
		if (likesUsingStone) {
			blessing.AddAttackEffect("+stone weapon ATK", a => {
				if (a.Attacker.Weapon?.MadeOf == "stone")
					a.AttackBonus++;
			});
		}
		if (likesUsingMetal) {
			blessing.AddAttackEffect("+metal weapon ATK", a => {
				if (a.Attacker.Weapon?.MadeOf == "metal")
					a.AttackBonus++;
			});
		}
	}
	
	public override void AddToCurse(Deity self, Level level, Agent agent, StatusEffect curse)
	{
		curse.AddAttackEffect("-wood weapon ATK", a => {
			if (a.Attacker.Weapon?.MadeOf == "wood")
				a.AttackBonus--;
		});
	}
	
	public override IEnumerable<string> GetPreferredMaterials()
	{
		yield return "stone";
		yield return "metal";
	}
}
