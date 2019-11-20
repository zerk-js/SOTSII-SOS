// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.Dice
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;

namespace Kerberos.Sots.Framework
{
	public class Dice
	{
		private static readonly Random Random = new Random();
		private List<Dice.Die> _dice;
		private int _constant;

		public Dice()
		{
		}

		public Dice(string value)
		  : this()
		{
			Dice.Parse(this, value);
		}

		[XmlAttribute("Value")]
		public string Value
		{
			get
			{
				return this.ToString();
			}
			set
			{
				Dice.Parse(this, value);
			}
		}

		[XmlIgnore]
		public bool IsZero
		{
			get
			{
				return this._constant == 0 && (this._dice == null || this._dice.Count <= 0);
			}
		}

		private void Clear()
		{
			if (this._dice != null)
				this._dice.Clear();
			this._constant = 0;
		}

		public int AverageRoll
		{
			get
			{
				int num = 0;
				for (int index = 0; index < this._dice.Count; ++index)
					num += this._dice[index].Count * (this._dice[index].Sides + 1) / 2;
				return num + this._constant;
			}
		}

		public int Roll()
		{
			return this.Roll(Dice.Random);
		}

		public int Roll(Random random)
		{
			return this.Roll(random, null);
		}

		public int Roll(Random random, string results)
		{
			if (results != null)
				results = string.Empty;
			int num1 = 0;
			if (this._dice != null)
			{
				for (int index1 = 0; index1 < this._dice.Count; ++index1)
				{
					for (int index2 = 0; index2 < this._dice[index1].Count; ++index2)
					{
						int num2 = random.NextInclusive(1, this._dice[index1].Sides);
						if (results != null)
						{
							if (!string.IsNullOrEmpty(results))
								results += " + ";
							results += string.Format("{0}(D{1}", (object)num2, (object)this._dice[index1].Sides);
						}
						num1 += num2;
					}
				}
			}
			if (this._constant != 0)
			{
				num1 += this._constant;
				if (results != null)
				{
					if (!string.IsNullOrEmpty(results))
						results += " + ";
					results += this._constant.ToString();
				}
			}
			if (results != null && num1 != this._constant)
				results.Insert(0, string.Format("{0}=", (object)num1));
			return num1;
		}

		public override string ToString()
		{
			string str = string.Empty;
			if (this._dice != null)
			{
				for (int index = 0; index < this._dice.Count; ++index)
				{
					if (!string.IsNullOrEmpty(str))
						str += " + ";
					str += this._dice[index].ToString();
				}
			}
			if (this._constant != 0)
			{
				if (!string.IsNullOrEmpty(str))
					str += " + ";
				str += this._constant.ToString();
			}
			if (string.IsNullOrEmpty(str))
				str = "0";
			return str;
		}

		private static void AddDice(List<Dice.Die> dice, int count, int sides)
		{
			for (int index = 0; index < dice.Count; ++index)
			{
				if (dice[index].Sides == sides)
				{
					dice[index] = new Dice.Die()
					{
						Count = dice[index].Count + sides,
						Sides = dice[index].Sides
					};
					return;
				}
			}
			dice.Add(new Dice.Die()
			{
				Count = count,
				Sides = sides
			});
		}

		private static void Parse(Dice output, string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				output.Clear();
			}
			else
			{
				try
				{
					foreach (char c in value)
					{
						if (!char.IsDigit(c) && !char.IsWhiteSpace(c) && (c != 'D' && c != 'd') && (c != '+' && c != '-'))
							throw new ArgumentException(string.Format("Invalid character {0}", (object)c));
					}
					List<Dice.Die> dice = new List<Dice.Die>();
					int num = 0;
					string str1 = value;
					char[] chArray1 = new char[1] { '+' };
					foreach (string str2 in str1.Split(chArray1))
					{
						char[] chArray2 = new char[2] { 'D', 'd' };
						string[] strArray = str2.Split(chArray2);
						if (strArray.Length != 1 || strArray.Length != 2)
						{
							if (strArray.Length == 1)
							{
								num += int.Parse(strArray[0], (IFormatProvider)NumberFormatInfo.InvariantInfo);
							}
							else
							{
								if (strArray.Length != 2)
									throw new ArgumentException("Invalid format.  Should be '3D6' or '17'");
								int count = int.Parse(strArray[0], (IFormatProvider)NumberFormatInfo.InvariantInfo);
								int sides = int.Parse(strArray[1], (IFormatProvider)NumberFormatInfo.InvariantInfo);
								if (sides < 2)
									throw new ArgumentException("Dice need at last 2 sides.");
								Dice.AddDice(dice, count, sides);
							}
						}
					}
					output._dice = dice;
					output._constant = num;
				}
				catch (Exception ex)
				{
					throw new ArgumentException(string.Format("'{0}' is not a valid string for dice.", (object)value), ex);
				}
			}
		}

		private struct Die
		{
			public int Count;
			public int Sides;

			public override string ToString()
			{
				return string.Format("{0}D{1}", (object)this.Count, (object)this.Sides);
			}
		}
	}
}
