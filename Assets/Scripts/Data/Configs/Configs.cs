﻿/*
 * Config
 * 20150929 11:32:10
 * Loccy
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemConfig : IConfig 
{
	public uint id;
	public string name;
	public uint sellPrice;
	public Dictionary<string, string> buff;
	public string describe;

	public void onReadConfig()
	{

	}
}

public class ShopGoodsConfig : IConfig
{
	public uint id;
	public uint goldPrice;
	public uint crystal;

	public void onReadConfig()
	{
		
	}
}

public class MapConfig : IConfig
{
	public uint id;
	public string name;
	public string describe;

	public void onReadConfig()
	{
		
	}
}

public class NPCConfig : IConfig
{
	//BaseProp
	public uint id;
	public string name;
	public string model;
	public string icon;
	public EntityType type;
	public int startLevel;
	public int level;
	public NPCAIType idleAi;
	public NPCAIType ai;
	public Dictionary<string,double> property;
	public double moveSpeed;
	public double attackSpeed;

	public int hp;
	public double physical_a;//物攻
	public double physical_p;//物穿
	public double physical_d;//物防
	public double magical_a;//法强
	public double magical_p;//法穿
	public double magical_d;//魔抗

	//============Skill===========
	public uint baseSkill;//
	public List<uint> skillList;//
	public List<uint> addBuffList;//

	public bool touchHert;
	public bool isShow;//

	public int score;

	public void onReadConfig()
	{

	}
}