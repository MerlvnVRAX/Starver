﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using System.Reflection;
using MySql;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using System.Threading;

namespace Starvers
{
	using Vector = TOFOUT.Terraria.Server.Vector2;
	public static class Utils
	{
		#region Properties
		public static Random Rand { get { return Starver.Rand; } }
		public static DirectoryInfo MainFolder { get { return Starver.MainFolder; } }
		public static DirectoryInfo BossFolder { get { return Starver.BossFolder; } }
		public static DirectoryInfo PlayerFolder { get { return Starver.PlayerFolder; } }
		public static MySqlConnection DB { get { return StarverPlayer.DB; } }
		public static StarverPlayer[] Players { get { return Starver.Players; } }
		public static StarverConfig Config { get { return StarverConfig.Config; } }
		#endregion
		#region SaveAll
		public static void SaveAll()
		{
			for (int i = 0; i < 40; i++)
			{
				if (Players[i] == null || Players[i].UserID < 0)
				{
					continue;
				}
				Players[i].Save();
			}
		}
		#endregion
		#region UpGradeAll
		public static void UpGradeAll(int lvlup)
		{
			if (Config.SaveMode == SaveModes.MySQL)
			{
				SaveAll();
				using (MySqlConnection connection = DB.Clone() as MySqlConnection)
				{
					connection.Open();
					using (MySqlCommand cmd = new MySqlCommand("Select UserID,Level from Starver", connection))
					using (MySqlDataReader Reader = cmd.ExecuteReader(CommandBehavior.Default))
					{
						do
						{
							try
							{
								if (Reader.Read())
								{
									int UserID = Reader.Get<int>("UserID");
									int Level = Reader.Get<int>("Level");
									if (Level > 120)
									{
										Level += lvlup;
									}
									DB.Query("update Starver set Level=@0 WHERE UserID=@1", Level, UserID);
								}
							}
							catch (Exception e)
							{
								TSPlayer.Server.SendInfoMessage(e.ToString());
							}
						}
						while (Reader.NextResult());
					}
				}
			}
			else
			{
				FileInfo[] files = PlayerFolder.GetFiles("*.json");
				foreach (var ply in Starver.Players)
				{
					if (ply is null)
					{
						continue;
					}
					ply.Save();
				}
				foreach (var file in files)
				{
					StarverPlayer player = StarverPlayer.Read(file.Name, true);
					if (player.Level > 120)
					{
						player.Level += lvlup;
					}
					player.Save();
				}
			}
			foreach (var ply in Starver.Players)
			{
				if (ply is null)
				{
					continue;
				}
				ply.Reload();
			}
		}
		#endregion
		#region AverageLevel
		public static int AverageLevel
		{
			get
			{
				int num = 0;
				double level = 0;
				if (Config.SaveMode == SaveModes.MySQL)
				{
					SaveAll();
					MySqlCommand cmd = new MySqlCommand("Select UserID,Level from Starver", DB);
					MySqlDataReader Reader = cmd.ExecuteReader();
					do
					{
						try
						{
							int UserID = Reader.Get<int>("UserID");
							int Level = Reader.Get<int>("Level");
							if (Level < Config.LevelNeed)
							{
								continue;
							}
							level += Level;
							num++;
						}
						catch (Exception e)
						{
							TSPlayer.Server.SendInfoMessage(e.ToString());
						}
					}
					while (Reader.NextResult());
				}
				else
				{
					FileInfo[] files = PlayerFolder.GetFiles("*.json");
					foreach (var file in files)
					{
						StarverPlayer player = StarverPlayer.Read(file.Name);
						level += player.Level;
						num++;
					}
				}
				int result = (int)Math.Min(level / num, int.MaxValue);
				return result;
			}
		}
		#endregion
		#region CalculateLife
		public static int CalculateLife(int Level)
		{
			int Result = Math.Min(30000, Level / 3);
			return Result;
		}
		#endregion
		#region StrikeMe
		public static void StrikeMe(this NPC RealNPC, int Damage, float knockback, StarverPlayer player)
		{
			RealNPC.playerInteraction[player.Index] = true;
			int realdamage = (int)(Damage * (Rand.NextDouble() - 0.5) / 4 - RealNPC.defense * 0.5);
			RealNPC.life = Math.Max(RealNPC.life - realdamage, 0);
			RealNPC.SendCombatMsg(realdamage.ToString(), Color.Yellow);
			if (RealNPC.life < 1)
			{
				RealNPC.checkDead();
			}
			else
			{
				RealNPC.velocity.LengthAdd(knockback * (1f - RealNPC.knockBackResist));
				TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", RealNPC.whoAmI);
			}
		}
		#endregion
		#region cmd
		public static StarverPlayer SPlayer(this CommandArgs args)
		{
			if (args.Player.Name != "Server")
			{
				return Starver.Players[args.Player.Index];
			}
			else
			{
				return StarverPlayer.Server;
			}
		}
		#endregion
		#region SetLife
		public static void SetLife(this TSPlayer player, int Life)
		{
			player.TPlayer.SetLife(Life);
		}
		public static void SetLife(this Player player, int Life)
		{
			Life = Math.Min(Life, 30000);
			player.statLifeMax = Life;
			NetMessage.SendData((int)PacketTypes.PlayerHp, -1, -1, null, player.whoAmI);
		}
		#endregion
		#region SendCombatText
		public static void SendCombatMsg(this Entity entity, string msg, Color color)
		{
			NetMessage.SendData((int)PacketTypes.CreateCombatTextExtended, -1, -1, NetworkText.FromLiteral(msg), (int)color.PackedValue, entity.position.X + Rand.Next(entity.width), entity.position.Y + Rand.Next(entity.height), 0.0f, 0, 0, 0);
		}
		#endregion
		#region Vector2
		public static Vector2 FromPolar(double rad, float length)
		{
			return new Vector2((float)(Math.Cos(rad) * length), (float)(Math.Sin(rad) * length));
		}
		public static double Angle(this Vector2 vector)
		{
			return Math.Atan2(vector.Y, vector.X);
		}
		public static double Angle(ref this Vector2 vector, double rad)
		{
			vector = FromPolar(rad, vector.Length());
			return rad;
		}
		public static double AngleAdd(ref this Vector2 vector, double rad)
		{
			rad += Math.Atan2(vector.Y, vector.X);
			vector = FromPolar(rad, vector.Length());
			return rad;
		}
		public static Vector2 Deflect(this Vector2 vector2, double rad)
		{
			Vector2 vector = vector2;
			vector2.AngleAdd(rad);
			return vector;
		}
		public static void Length(ref this Vector2 vector, float length)
		{
			vector = FromPolar(vector.Angle(), length);
		}
		public static void LengthAdd(ref this Vector2 vector, float length)
		{
			vector = FromPolar(vector.Angle(), length + vector.Length());
		}
		public static Vector2 ToLenOf(this Vector2 vector, float length)
		{
			vector.Normalize();
			vector *= length;
			return vector;
		}
		public static Vector2 Symmetry(this Vector2 vector, Vector2 Center)
		{
			return Center * 2f - vector;
		}
		public static Vector2 Vertical(this Vector2 vector)
		{
			return new Vector2(-vector.Y, vector.X);
		}
		public static Vector ToVector(this Vector2 value)
		{
			return new Vector(value.X, value.Y);
		}
		public static Vector2 ToVector2(this Vector value)
		{
			return new Vector2(value.X, value.Y);
		}
		#endregion
		#region rands
		public static double NextAngle(this Random rand)
		{
			return rand.NextDouble() * Math.PI * 2;
		}
		/// <summary>
		/// 使用样例:
		/// <para>Range = PI / 12</para>
		/// 返回为 [-PI, PI) / (PI / (PI / 12)) = [-PI / 12, PI / 12)
		/// </summary>
		/// <param name="rand"></param>
		/// <param name="Range"></param>
		/// <returns></returns>
		public static double NextAngle(this Random rand, double Range)
		{
			return (rand.NextAngle() - Math.PI) / (Math.PI / Range);
		}
		public static double NextDouble(this Random rand, double min, double max)
		{
			if (max < min)
			{
				throw new ArgumentException("最大值必须大等于最小值");
			}
			return (max - min) * rand.NextDouble() + min;
		}
		public static Vector2 NextVector2(this Random rand, float Length)
		{
			return FromPolar(rand.NextAngle(), Length);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="rand"></param>
		/// <param name="X">实际变为-X / 2 ~ X / 2</param>
		/// <param name="Y">实际变为-Y / 2 ~ Y / 2</param>
		/// <returns></returns>
		public static Vector2 NextVector2(this Random rand, float X, float Y)
		{
			return new Vector2((float)((rand.NextDouble() - 0.5) * X), (float)((rand.NextDouble() - 0.5) * Y));
		}
		/// <summary>
		/// 随机获取一个元素
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <returns></returns>
		public static T Next<T>(this T[] array)
		{
			return array[Rand.Next(array.Length)];
		}
		#endregion
		#region SendData
		public static void SendData(this NPC npc)
		{
			NetMessage.SendData((int)PacketTypes.NpcUpdate, -1, -1, null, npc.whoAmI);
		}
		#endregion
		#region SendData
		public static void SendData(this Projectile npc)
		{
			NetMessage.SendData((int)PacketTypes.ProjectileNew, -1, -1, null, npc.whoAmI);
		}
		#endregion
		#region TheWorldSkill
		#region ReadNPC
		public unsafe static void ReadNPCState(Vector2* NPCVelocity, int* NPCAI)
		{
			int t = -1;
			foreach (var npc in Terraria.Main.npc)
			{
				t++;
				if (!npc.active)
				{
					continue;
				}
				NPCVelocity[t] = npc.velocity;
				NPCAI[t] = npc.aiStyle;
				npc.aiStyle = -1;
				npc.velocity = Vector2.Zero;
				npc.SendData();
			}
		}
		#endregion
		#region ReadProj
		public unsafe static void ReadProjState(Vector2* ProjVelocity, int* ProjAI)
		{
			int t = -1;
			foreach (var proj in Terraria.Main.projectile)
			{
				t++;
				if (!proj.active)
				{
					continue;
				}
				ProjVelocity[t] = proj.velocity;
				ProjAI[t] = proj.aiStyle;
				proj.aiStyle = -1;
				proj.velocity = Vector2.Zero;
				proj.SendData();
			}
		}
		#endregion
		#region UpdateNPC
		public static void UpdateNPCState()
		{
			foreach (var npc in Terraria.Main.npc)
			{
				if (!npc.active)
				{
					continue;
				}
				npc.SendData();
			}
		}
		#endregion
		#region UpdateProj
		public static void UpdateProjState()
		{
			foreach (var proj in Terraria.Main.projectile)
			{
				if (!proj.active)
				{
					continue;
				}
				proj.SendData();
			}
		}
		#endregion
		#region RestoreNPC
		public unsafe static void RestoreNPCState(Vector2* NPCVelocity, int* NPCAI)
		{
			int t = -1;
			foreach (var npc in Terraria.Main.npc)
			{
				t++;
				if (!npc.active)
				{
					continue;
				}
				npc.velocity = NPCVelocity[t];
				npc.aiStyle = NPCAI[t];
				npc.SendData();
			}
		}
		#endregion
		#region RestoreProj
		public unsafe static void RestoreProjState(Vector2* ProjVelocity, int* ProjAI)
		{
			int t = -1;
			foreach (var proj in Terraria.Main.projectile)
			{
				t++;
				if (!proj.active)
				{
					continue;
				}
				proj.velocity = ProjVelocity[t];
				proj.aiStyle = ProjAI[t];
				proj.SendData();
			}
		}
		#endregion
		#endregion
		#region else
		public static void Exception(string message)
		{
			new Thread(() =>
			{
				throw new Exception(message);
			}).Start();
		}
		public static void Exception(Exception e)
		{
			new Thread(() =>
			{
				throw e;
			}).Start();
		}
		#endregion
	}
}
