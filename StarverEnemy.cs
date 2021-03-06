﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace Starvers
{
	public abstract class StarverEnemy
	{
		public int Index 
		{
			get;
			protected set;
		}
		public NPC RealNPC
		{
			get => Main.npc[Index];
		}
		public ref int Life
		{
			get => ref RealNPC.life;
		}
		public ref int LifeMax
		{
			get => ref RealNPC.lifeMax;
		}
		public Vector2 Center
		{
			get => RealNPC.Center;
		}
		public ref Vector2 Velocity
		{
			get => ref RealNPC.velocity;
		}
		public bool Active
		{
			get => RealNPC.active;
			set => RealNPC.active = value;
		}

		protected StarverEnemy(int idx = -1)
		{
			Index = idx;
		}
		#region Projs
		#region Bases
		public int NewProj(Vector2 velocity, int projID, int damage, float knockBack = 1, float ai0 = 0, float ai1 = 0)
		{
			return NewProj(Center, velocity, projID, damage, knockBack, ai0, ai1);
		}
		public int NewProj(Vector2 position, Vector2 velocity, int projID, int damage, float knockBack = 1, float ai0 = 0, float ai1 = 0)
		{
			return Utils.NewProj(position, velocity, projID, damage, knockBack, Main.myPlayer, ai0, ai1);
		}
		public int NewProjNoBC(Vector2 velocity, int projID, int damage, float knockBack = 1, float ai0 = 0, float ai1 = 0)
		{
			return NewProjNoBC(Center, velocity, projID, damage, knockBack, ai0, ai1);
		}
		public int NewProjNoBC(Vector2 position, Vector2 velocity, int projID, int damage, float knockBack = 1, float ai0 = 0, float ai1 = 0)
		{
			return Utils.NewProjNoBC(position, velocity, projID, damage, knockBack, Main.myPlayer, ai0, ai1);
		}
		#endregion
		#region ProjCircle
		/// <summary>
		/// 弹幕圆
		/// </summary>
		/// <param name="Center"></param>
		/// <param name="r"></param>
		/// <param name="speed">向外速率</param>
		/// <param name="Type"></param>
		/// <param name="number">弹幕总数</param>
		public void ProjCircle(Vector2 Center, float r, float speed, int Type, int number, int Damage, float ai0 = 0, float ai1 = 0)
		{
			double averagerad = Math.PI * 2 / number;
			for (int i = 0; i < number; i++)
			{
				NewProj(Center + Vector.FromPolar(averagerad * i, r), Vector.FromPolar(averagerad * i, speed), Type, Damage, 4f, ai0, ai1);
			}
		}
		/// <summary>
		/// 弹幕圆
		/// </summary>
		/// <param name="Center"></param>
		/// <param name="angle">偏转角</param>
		/// <param name="r"></param>
		/// <param name="speed">速率</param>
		/// <param name="Type"></param>
		/// <param name="number">弹幕总数</param>
		public void ProjCircleEx(Vector2 Center, double angle, float r, float speed, int Type, int number, int Damage, float ai0 = 0, float ai1 = 0)
		{
			double averagerad = Math.PI * 2 / number;
			for (int i = 0; i < number; i++)
			{
				NewProj(Center + Vector.FromPolar(angle + averagerad * i, r), Vector.FromPolar(angle + averagerad * i, speed), Type, Damage, 4f, ai0, ai1);
			}
		}

		/// <summary>
		/// 弹幕圆
		/// </summary>
		/// <param name="Center"></param>
		/// <param name="angle">偏转角</param>
		/// <param name="r"></param>
		/// <param name="speed">速率</param>
		/// <param name="Type"></param>
		/// <param name="number">弹幕总数</param>
		public void ProjCircleExNoBC(Vector2 Center, double angle, float r, Action<Projectile> action, int Type, int number, int Damage, float ai0 = 0, float ai1 = 0)
		{
			double averagerad = Math.PI * 2 / number;
			for (int i = 0; i < number; i++)
			{
				var idx = NewProjNoBC(Center + Vector.FromPolar(angle + averagerad * i, r), default, Type, Damage, 4f, ai0, ai1);
				action(Main.projectile[idx]);
			}
		}

		/// <summary>
		/// 弹幕圆
		/// </summary>
		/// <param name="Center"></param>
		/// <param name="angle">偏转角</param>
		/// <param name="r"></param>
		/// <param name="velocity">速度</param>
		/// <param name="Type"></param>
		/// <param name="number">弹幕总数</param>
		public void ProjCircleEx(Vector2 Center, double angle, float r, Vector2 velocity, int Type, int number, int Damage, float ai0 = 0, float ai1 = 0)
		{
			double averagerad = Math.PI * 2 / number;
			for (int i = 0; i < number; i++)
			{
				NewProj(Center + Vector.FromPolar(angle + averagerad * i, r), velocity, Type, Damage, 4f, ai0, ai1);
			}
		}

		/// <summary>
		/// 弹幕圆
		/// </summary>
		/// <param name="Center"></param>
		/// <param name="r"></param>
		/// <param name="speed">向外速率</param>
		/// <param name="Type"></param>
		/// <param name="number">弹幕总数</param>
		public int[] ProjCircleRet(Vector2 Center, float r, float speed, int Type, int number, int Damage, float ai0 = 0, float ai1 = 0)
		{
			double averagerad = Math.PI * 2 / number;
			int[] arr = new int[number];
			for (int i = 0; i < number; i++)
			{
				arr[i] = NewProj(Center + Vector.FromPolar(averagerad * i, r), Vector.FromPolar(averagerad * i, speed), Type, Damage, 4f, ai0, ai1);
			}
			return arr;
		}
		#endregion
		#region ProjSector
		/// <summary>
		/// 扇形弹幕
		/// </summary>
		/// <param name="Center">圆心</param>
		/// <param name="speed">向外速率</param>
		/// <param name="r">半径</param>
		/// <param name="interrad">中心半径的方向</param>
		/// <param name="rad">张角</param>
		/// <param name="Damage">伤害(带加成)</param>
		/// <param name="type"></param>
		/// <param name="num">数量</param>
		/// <param name="ai0"></param>
		/// <param name="ai1"></param>
		public void ProjSector(Vector2 Center, float speed, float r, double interrad, double rad, int Damage, int type, int num, float ai0 = 0, float ai1 = 0)
		{
			double start = interrad - rad / 2;
			double average = rad / num;
			for (int i = 0; i < num; i++)
			{
				NewProj(Center + Vector.FromPolar(start + i * average, r), Vector.FromPolar(start + i * average, speed), type, Damage, 4f, ai0, ai1);
			}
		}
		#endregion
		#region ProjLine
		/// <summary>
		/// 制造速度平行的弹幕直线
		/// </summary>
		/// <param name="Begin">起点</param>
		/// <param name="End">终点</param>
		/// <param name="Vel">速度</param>
		/// <param name="num">数量</param>
		/// <param name="Damage"></param>
		/// <param name="type"></param>
		/// <param name="ai0"></param>
		/// <param name="ai1"></param>
		public void ProjLine(Vector2 Begin, Vector2 End, Vector2 Vel, int num, int Damage, int type, float ai0 = 0, float ai1 = 0)
		{
			Vector2 average = End - Begin;
			average /= num;
			for (int i = 0; i < num; i++)
			{
				NewProj(Begin + average * i, Vel, type, Damage, 3f, ai0, ai1);
			}
		}
		/// <summary>
		/// 制造速度平行的弹幕直线
		/// </summary>
		/// <param name="Begin">起点</param>
		/// <param name="End">终点</param>
		/// <param name="Vel">速度</param>
		/// <param name="num">数量</param>
		/// <param name="Damage"></param>
		/// <param name="type"></param>
		/// <param name="ai0"></param>
		/// <param name="ai1"></param>
		public int[] ProjLineReturns(Vector2 Begin, Vector2 End, Vector2 Vel, int num, int Damage, int type, float ai0 = 0, float ai1 = 0)
		{
			int[] arr = new int[num];
			Vector2 average = End - Begin;
			average /= num;
			for (int i = 0; i < num; i++)
			{
				arr[i] = NewProj(Begin + average * i, Vel, type, Damage, 3f, ai0, ai1);
			}
			return arr;
		}
		#endregion
		#endregion
		#region Methods
		public abstract void Kill();
		public virtual void TurnToAir()
		{
			Active = false;
		}
		#endregion
	}
}
