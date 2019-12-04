﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;

namespace Starvers.AuraSystem.Realms.Generics
{
	using Vector = TOFOUT.Terraria.Server.Vector2;
	public class ReflectingRealm<ReflectorType> : StarverRealm
		where ReflectorType : IRealmReflector, new()
	{
		private int Owner;
		private ReflectorType Reflector;
		private StarverPlayer OwnerPlayer => Starver.Players[Owner];
		public override Vector2 Center
		{
			get => base.Center;
			set => Reflector.Center = base.Center = value;
		}
		public ReflectingRealm() : base(true)
		{
			Owner = Main.myPlayer;
			DefaultTimeLeft = 60 * 30;
			Reflector = new ReflectorType();
		}

		public ReflectingRealm(int owner) : this()
		{
			if (0 <= owner && owner <= 255)
			{
				Owner = owner;
			}
			else
			{
				Owner = -1;
			}
		}

		public override void Start()
		{
			base.Start();
			Reflector.Start();
		}

		public override void Kill()
		{
			base.Kill();
			Reflector.Kill();
		}

		public override bool InRange(Entity entity)
		{
			return Reflector.InRange(entity);
		}

		public override bool IsCross(Entity entity)
		{
			return Reflector.IsCross(entity);
		}

		protected override void SetDefault()
		{

		}

		protected override void InternalUpdate()
		{
			Reflector.Center = Center;
			Reflector.Update(TimeLeft);
			if (Owner == Main.myPlayer)
			{
				foreach (var proj in Main.projectile)
				{
					if (proj.active && proj.hostile == false && Reflector.AtBorder(proj) && proj.aiStyle >= 0)
						Reflect(proj);
				}
				foreach (var player in Starver.Players)
				{
					if (player != null && player.Active && Reflector.AtBorder(player))
						Reflect(player);
				}
			}
			else if (Owner == -1)
			{
				foreach (var proj in Main.projectile)
				{
					if (proj.active && Reflector.AtBorder(proj) && proj.aiStyle >= 0)
						Reflect(proj);
				}
				foreach (var npc in Main.npc)
				{
					if (npc.active && Reflector.AtBorder(npc))
						Reflect(npc);
				}
				foreach (var player in Starver.Players)
				{
					if (player != null && player.Active && Reflector.AtBorder(player))
						Reflect(player);
				}
			}
			else if (OwnerPlayer == null)
			{
				Kill();
			}
			else
			{
				foreach (var proj in Main.projectile)
				{
					if (proj.active && proj.hostile && Reflector.AtBorder(proj) && CanHitOwner(proj) && proj.aiStyle >= 0)
						Reflect(proj);
				}
				foreach (var npc in Main.npc)
				{
					if (npc.active && !npc.friendly && Reflector.AtBorder(npc))
						Reflect(npc);
				}
				foreach (var player in Starver.Players)
				{
					if (player != null && player.Active && CanHitOwner(player) && Reflector.AtBorder(player))
						Reflect(player);
				}
			}
		}

		protected void Reflect(Projectile proj)
		{
			Reflector.Reflect(proj);
			proj.SendData();
		}
		protected void Reflect(NPC npc)
		{
			Reflector.Reflect(npc);
			npc.SendData();
		}
		protected void Reflect(Player player)
		{
			Reflector.Reflect(player);
			player.SendData();
		}

		protected bool CanHitOwner(Player player)
		{
			return player.hostile &&
				(player.team != OwnerPlayer.Team || player.team == 0);
		}
		protected bool CanHitOwner(Projectile proj)
		{
			if (proj.owner == Main.myPlayer)
				return proj.hostile;
			Player player = Main.player[proj.owner];
			if (player.hostile && (player.team != OwnerPlayer.Team || player.team == 0))
				return true;
			return false;
		}
	}
}