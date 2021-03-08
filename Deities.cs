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
		if (Globals.Random.NextDouble() < self.Archetype.ChanceOfLikes)
			self.Likes.Add("chaos");
		if (Globals.Random.NextDouble() < self.Archetype.ChanceOfDislikes)
			self.Dislikes.Add("order");
		Description = $"{self.Name} is chaotic and unpredictable. Favor can be gained or lost without warning.";
	}
	
	public override void OnEvent(Deity self, IEvent e)
	{
		if (Globals.Random.NextDouble() < 0.99)
			return;
		
		switch (e)
		{
			case NextTurn next:
				self.PlayerFavor += Globals.Random.Next(5) - Globals.Random.Next(5);
				break;
				
			case DidAttack attack:
				switch (Globals.Random.Next(4))
				{
					case 0:
						self.Like(attack.Attacker);
						break;
					case 1:
						self.Like(attack.Attacked);
						break;
					case 2:
						self.Dislike(attack.Attacker);
						break;
					case 3:
						self.Dislike(attack.Attacked);
						break;
				}
				break;
		}
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
		Description = $"{self.Name} only cares about {self.Domains[0].Name}.";
		if (Globals.Random.NextDouble() < self.Archetype.ChanceOfLikes)
			self.Likes.Add(self.Domains[0].Name);
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
		self.AcceptsPrayers = true;
		self.AcceptsDonations = true;
		self.AcceptsSacrafices = true;
		Description = "A vengeful deity is easy to displease but often intervenes to help those who worship them.";
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
					self.Like(attack.Attacker);
				if (dislikesKillingUndead && attack.Attacked.Tags.Contains(AgentTag.Undead))
					self.Dislike(attack.Attacker);
				self.Like(attack.Attacked);
				break;
		}
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
				if (self.PlayerFavor > 0 && turn.Player.HP < 6)
				{
					self.PlayerFavor--;
					turn.Player.HP++;
					turn.Player.Messages.Add($"{self.Name} healed you");
				}
				break;
		}
	}
}

public class OfLove : DeityDomain
{
	public OfLove() : base("love")
	{
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
		var other = self;
		while (other == self || other.Archetype is Trickster t && t.Rival == self)
			other = deities.ToArray()[Globals.Random.Next(deities.Count())];
		
		self.Likes.Add(other.GetShortTitle());
		other.Likes.Add(self.GetShortTitle());
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
		if (Globals.Random.NextDouble() > self.Archetype.ChanceOfInteracting)
			return;
		
		switch (e)
		{
			case NextTurn turn:
				if (self.PlayerFavor > 0 && turn.Player.Money < 99)
				{
					self.PlayerFavor--;
					turn.Player.Money++;
					turn.Player.Messages.Add($"{self.Name} gave you money");
				}
				break;
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
			case DidAttack attack when attack.Attacked.HP < 1:
				if (likesKillingEnemies && attack.Attacked.Team != attack.Attacker.Team)
					self.Like(attack.Attacker);
				if (dislikesKillingAllies && attack.Attacked.Team == attack.Attacker.Team)
					self.Dislike(attack.Attacker);
				break;
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
}

public class OfAgriculture : DeityDomain
{
	public OfAgriculture() : base("agriculture")
	{
	}
	
	public override void Finalize(Deity self, IEnumerable<Deity> deities)
	{
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
		if (Globals.Random.NextDouble() < self.Archetype.ChanceOfDislikes)
			self.Dislikes.Add("winter");
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
				if (likesUsingWood && used.Item.Material == "wood")
					self.Like(used.Agent);
				break;
				
			case DidAttack attack when attack.Attacked.HP < 1:
				if (dislikesKillingTrees && attack.Attacked.Team == "plants")
					self.Dislike(attack.Attacker);
				break;
		}
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
				if (likesUsingStone && used.Item.Material == "stone")
					self.Like(used.Agent);
				if (likesUsingMetal && used.Item.Material == "metal")
					self.Like(used.Agent);
				break;
		}
	}
}
