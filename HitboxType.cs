using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Ariadne
{
    public enum HitboxType
    {
		[HBDescriptor(name: "None", color: ColorType.Transparent)]
		 None,
		[HBDescriptor(name: "Knight", color: ColorType.Yellow)]
		 Knight,
		[HBDescriptor(name: "Enemy", color: ColorType.Red)]
		 Enemy,
		[HBDescriptor(name: "Attack", color: ColorType.Cyan)]
		 Attack,
		[HBDescriptor(name: "Terrain", color: ColorType.White, isStatic: true, isUnion: true)]
		 Terrain,
		[HBDescriptor(name: "Trigger", color: ColorType.Blue)]
		 Trigger,
		[HBDescriptor(name: "Breakable", color: ColorType.Pink)]
		 Breakable,
		[HBDescriptor(name: "Transition", color: ColorType.DarkBlue)]
		 Transition,
		[HBDescriptor(name: "Switch", color: ColorType.LightGray)]
		 Switch,
		[HBDescriptor(name: "Gate", color: ColorType.Gray)]
		 Gate,
		[HBDescriptor(name: "GrubBottle", color: ColorType.Greenish)]
		 Bottle,
		[HBDescriptor(name: "Bench", color: ColorType.DarkGray)]
		 Bench,
		[HBDescriptor(name: "HazardRespawn", color: ColorType.Purple)]
		 HazardRespawn,
		[HBDescriptor(name: "Other", color: ColorType.Orange, minShowLevel: ShowHitbox.Verbose)]
		 Other,
		[HBDescriptor(name: "GeoStore", color: ColorType.Yellowish)]
		 GeoStore,
		[HBDescriptor(name: "GeoToken", color: ColorType.Yellowish)]
		 GeoToken,
		[HBDescriptor(name: "SoulStore", color: ColorType.Whitish)]
		 SoulStore,
		[HBDescriptor(name: "Grass", color: ColorType.Greenish)]
		 Grass,
		[HBDescriptor(name: "Elevator", color: ColorType.BlueGray)]
		 Elevator,
		[HBDescriptor(name: "MaskShard", color: ColorType.Pinkish)]
		 MaskShard,
		[HBDescriptor(name: "SoulShard", color: ColorType.Blueish)]
		 SoulShard,
		[HBDescriptor(name: "SecretArea", color: ColorType.DarkYellow)]
		 SecretArea,
		[HBDescriptor(name: "Lifeblood", color: ColorType.LightBlue)]
		 Lifeblood,
		[HBDescriptor(name: "HotSpring", color: ColorType.Blueish)]
		 HotSpring,
		[HBDescriptor(name: "Upgrade", color: ColorType.Pinkish)]
		 Upgrade,
		[HBDescriptor(name: "StaticHazard", color: ColorType.Orangish, 
			isStatic: true, isUnion: true, isIntersection: true)]
		 StaticHazard,
		[HBDescriptor(name: "NPC", color: ColorType.Purplish)]
		 NPC,
		[HBDescriptor(name: "DreamNail", color: ColorType.LightGray)]
		 DreamNail,
		[HBDescriptor(name: "Shop", color: ColorType.Blueish)]
		 Shop,
		[HBDescriptor(name: "Item", color: ColorType.Whitish)]
		 Item,
		[HBDescriptor(name: "Boss", color: ColorType.Green)]
		 Boss,
}

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class HBDescriptorAttribute : Attribute
    {
        public string Name { get; }

        public Color Color { get; }
        public bool IsStatic { get; }
        public bool IsUnion { get; }
        public bool IsIntersection { get; }
        public ShowHitbox MinShowLevel { get; }

        public HBDescriptorAttribute(string name, ColorType color, 
			bool isStatic = false,
			bool isUnion = false,
			bool isIntersection = false,
			ShowHitbox minShowLevel = ShowHitbox.Show)
        {
            Name = name;
            Color = color.GetColor();
			IsStatic = isStatic;
			IsUnion = isUnion;
			IsIntersection = isIntersection;
			MinShowLevel = minShowLevel;
        }
    }

	public static class HitboxTypeExtensions
	{
		private static HBDescriptorAttribute[] prefetchedDescriptors = PrefetchDescriptors();

		/// <summary>
		/// Prefetch the HBDescriptorAttributes so that reflection is only done once.
		/// </summary>
		private static HBDescriptorAttribute[] PrefetchDescriptors()
		{
			var hbValues = (HitboxType[])Enum.GetValues(typeof(HitboxType));
			var descriptors = new HBDescriptorAttribute[hbValues.Length];

            foreach (var hbtype in hbValues)
            {
                descriptors[(int)hbtype] = typeof(HitboxType)
					.GetField(Enum.GetName(typeof(HitboxType), hbtype))
					.GetCustomAttribute<HBDescriptorAttribute>(false);
            }
			return descriptors;
        }
        public static Color GetColor(this HitboxType hitboxType)
        {
            return prefetchedDescriptors[(int)hitboxType].Color;
        }

        public static string GetName(this HitboxType hitboxType)
        {
            return prefetchedDescriptors[(int)hitboxType].Name;
        }

        public static int GetDepth(this HitboxType hitboxType)
        {
            return 3;
        }

        public static bool GetIsStatic(this HitboxType hitboxType)
        {
            return prefetchedDescriptors[(int)hitboxType].IsStatic;
        }

        public static bool GetIsUnion(this HitboxType hitboxType)
        {
            return prefetchedDescriptors[(int)hitboxType].IsUnion;
        }

        public static bool GetIsIntersection(this HitboxType hitboxType)
        {
            return prefetchedDescriptors[(int)hitboxType].IsIntersection;
        }

        public static ShowHitbox GetMinShowLevel(this HitboxType hitboxType)
        {
            return prefetchedDescriptors[(int)hitboxType].MinShowLevel;
        }
    }
}
