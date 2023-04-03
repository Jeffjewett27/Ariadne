using GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ariadne
{
    public interface IColliderClassifier
    {
        HitboxType ClassifyCollider(Collider2D collider2D);
    }

    public class DetailedClassifier : IColliderClassifier
    {
        public static readonly Dictionary<string, HitboxType> FsmMappings = new()
        {
            { "npc_control", HitboxType.NPC },
            { "npc_dream_dialogue", HitboxType.DreamNail },
            { "ghost_npc_dreamnail", HitboxType.DreamNail },
            { "Bench Control", HitboxType.Bench },
            { "Stag Bell", HitboxType.Trigger },
            { "Tram Door", HitboxType.Trigger },
            { "Switch Control", HitboxType.Switch },
            { "Call Lever", HitboxType.Switch },
            { "Shop Region", HitboxType.Shop },
            { "Chest Control", HitboxType.GeoStore },
            { "Shiny Control", HitboxType.Item },
            { "Inspection", HitboxType.Trigger },
            { "Great Door", HitboxType.Gate },
            { "Bone Gate", HitboxType.Gate },
            { "Pillar Control", HitboxType.Gate },
            { "shockwave", HitboxType.Enemy },
        };

        public HitboxType ClassifyCollider(Collider2D collider2D)
        {
            if (collider2D == null)
            {
                return HitboxType.None;
            }

            HitboxType hbType = HitboxType.None;

            if (collider2D is not (BoxCollider2D or PolygonCollider2D or EdgeCollider2D or CircleCollider2D))
                return HitboxType.None;

            GameObject go = collider2D.gameObject;
            if (collider2D.GetComponent<DamageHero>()
                || collider2D.gameObject.LocateMyFSM("damages_hero"))
            {
                if (go.name.Contains("Spike") && go.GetComponent<TinkEffect>() != null
                    || go.name.Contains("Acid"))
                    hbType = HitboxType.StaticHazard;
                else
                    hbType = HitboxType.Enemy;
            }
            else if (go.GetComponent<HealthManager>()
                || go.LocateMyFSM("health_manager_enemy")
                || go.LocateMyFSM("health_manager"))
            {
                hbType = HitboxType.Boss;
            }
            else if (go.layer == (int)PhysLayers.TERRAIN)
            {
                if (go.GetComponent<GeoControl>() != null)
                    hbType = HitboxType.GeoStore;
                else if (go.GetComponent<HealthCocoon>() != null)
                {
                    hbType = HitboxType.Lifeblood;
                }
                else if (go.name.StartsWith("Spa Floor"))
                {
                    hbType = HitboxType.HotSpring;
                }
                else if (go.name.Contains("Break")
                    || go.name.Contains("Collapse")
                    || go.name.StartsWith("Loose Floor")
                    || go.name.StartsWith("Quake Floor")
                    || go.GetComponent<Breakable>() != null)
                    hbType = HitboxType.Breakable;
                else if (
                    collider2D.name.Contains("Gate")
                    || collider2D.name.StartsWith("sliding_wall")
                    || collider2D.name.StartsWith("Bottom Block")
                    || collider2D.name.Contains("Solid")
                    || collider2D.name.StartsWith("full_wall")
                    || collider2D.name.StartsWith("Inactive Block")
                    || collider2D.name.StartsWith("Bottle Physical")
                    || collider2D.name.StartsWith("Rising Pillar")
                    || collider2D.name.StartsWith("Terrain Block")
                    || go.GetComponent<SlopePlat>() != null
                    || go.name.Contains("Acid Blocker")
                    || go.GetComponent<PlayMakerFSM>() != null
                    //TODO find common denominator
                    )
                {
                    if (collider2D.isTrigger)
                        hbType = HitboxType.Switch;
                    else
                        hbType = HitboxType.Gate;
                }
                else if (go.tag == "Platform" || collider2D.name.StartsWith("elev_main"))
                {
                    hbType = HitboxType.Elevator;
                }
                else if (collider2D.isTrigger)
                    hbType = HitboxType.Other;
                else
                    hbType = HitboxType.Terrain;
            }
            else if (go == HeroController.instance?.gameObject && !collider2D.isTrigger)
            {
                hbType = HitboxType.Knight;
            }
            else if (go.GetComponent<DamageEnemies>() || go.LocateMyFSM("damages_enemy") || go.name == "Damager" && go.LocateMyFSM("Damage"))
            {
                hbType = HitboxType.Attack;
            }
            else if (collider2D.isTrigger && collider2D.GetComponent<HazardRespawnTrigger>())
            {
                hbType = HitboxType.HazardRespawn;
            }
            else if (collider2D.isTrigger && collider2D.GetComponent<TransitionPoint>())
            {
                hbType = HitboxType.Transition;
            }
            else if (collider2D.GetComponent<Breakable>() != null)
            {
                NonBouncer bounce = collider2D.GetComponent<NonBouncer>();
                if (bounce == null || !bounce.active)
                {
                    hbType = HitboxType.Trigger;
                }
            }
            //else if (collider2D.GetComponent<RestBench>() != null)
            //{
            //    hbType = HitboxType.Bench;
            //}
            else if (collider2D.GetComponent<ScuttlerControl>() != null)
            {
                hbType = HitboxType.Lifeblood;
            }
            else if (collider2D.name.StartsWith("Grub Bottle"))
            {
                hbType = HitboxType.Bottle;
            }
            else if (collider2D.name.Contains("Geo"))
            {
                if (go.GetComponent<GeoControl>() != null)
                    hbType = HitboxType.GeoStore;
                else
                    hbType = HitboxType.GeoToken;
            }
            else if (collider2D.name.Contains("Soul"))
            {
                //var components = go.GetComponents(typeof(Component));
                //Ariadne.MLog(components.ToString());
                if (go.GetComponent<PersistentIntItem>() != null)
                    hbType = HitboxType.SoulStore;
                else
                    hbType = HitboxType.Other;
            }
            else if (collider2D.name.ToLower().Contains("grass"))
            {
                hbType = HitboxType.Grass;
            }
            else if (go.GetComponent<PersistentBoolItem>() != null)
            {
                if (collider2D.name.StartsWith("Heart Piece"))
                {
                    hbType = HitboxType.MaskShard;
                }
                else if (collider2D.name.StartsWith("Vessel Fragment"))
                {
                    hbType = HitboxType.SoulShard;
                }
                else if (collider2D.name.Contains("Mask"))
                {
                    hbType = HitboxType.SecretArea;
                }
            }
            else if (go.GetComponent<SpriteRenderer>()?.sprite?.texture?.name.StartsWith("fireball_collect") ?? false)
            {
                hbType = HitboxType.Upgrade;
            }

            foreach (var fsm in go.GetComponents<PlayMakerFSM>())
            {
                HitboxType fsmType;
                if (FsmMappings.TryGetValue(fsm.FsmName, out fsmType))
                    hbType = fsmType;
            }

            if (hbType.Equals(HitboxType.None) && Ariadne.settings.ShowHitBoxes >= ShowHitbox.VerboseLogs)
            {
                hbType = HitboxType.Other;
            }

            return hbType;
        }
    }
}
