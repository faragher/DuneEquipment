

using System;
using RimWorld;
using Verse;
using HugsLib;
using HugsLib.Settings;

namespace DuneEquipment
{
    public class DuneEquipment
    {
    }

    public class DuneEquipmentBase : HugsLib.ModBase
    {
        public override string ModIdentifier
        {
            get
            {
                return "DuneEquipment";
            }
        }

        public static SettingHandle<bool> LasgunInteraction;

        public override void DefsLoaded()
        {
           LasgunInteraction = Settings.GetHandle<bool>("isEnabled", "Lasgun Interaction", "When a lasgun interacts with a shield, they interact violently. Set false to disable.", true);
           
        }

    }

    public class CompMovingGlower : CompGlower //A copy of the Glower comp, but with a timed re-update.
    {
        new public CompProperties_MovingGlower Props
        {
            get
            {
                return (CompProperties_MovingGlower)this.props;
            }
        }
        public override void CompTick()//Really the only new thing
        {
            if ((Find.TickManager.TicksGame) % 30 == 24)
            {
                //this.UpdateLit(this.parent.Map);
                //Log.Message("I should have just updated");
                this.parent.Map.glowGrid.RegisterGlower(this);
            }
        }

        // Token: 0x170001E1 RID: 481
        // (get) Token: 0x060009DD RID: 2525 RVA: 0x00035708 File Offset: 0x00033908
        private bool ShouldBeLitNow
        {
            get
            {
                if (!this.parent.Spawned)
                {
                    return false;
                }
                if (!FlickUtility.WantsToBeOn(this.parent))
                {
                    return false;
                }
                CompPowerTrader compPowerTrader = this.parent.TryGetComp<CompPowerTrader>();
                if (compPowerTrader != null && !compPowerTrader.PowerOn)
                {
                    return false;
                }
                CompRefuelable compRefuelable = this.parent.TryGetComp<CompRefuelable>();
                if (compRefuelable != null && !compRefuelable.HasFuel)
                {
                    return false;
                }
                CompSendSignalOnCountdown compSendSignalOnCountdown = this.parent.TryGetComp<CompSendSignalOnCountdown>();
                if (compSendSignalOnCountdown != null && compSendSignalOnCountdown.ticksLeft <= 0)
                {
                    return false;
                }
                CompSendSignalOnPawnProximity compSendSignalOnPawnProximity = this.parent.TryGetComp<CompSendSignalOnPawnProximity>();
                return compSendSignalOnPawnProximity == null || !compSendSignalOnPawnProximity.Sent;
            }
        }

        // Token: 0x060009E0 RID: 2528 RVA: 0x00035818 File Offset: 0x00033A18
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (this.ShouldBeLitNow)
            {
                this.UpdateLit(this.parent.Map);
                this.parent.Map.glowGrid.RegisterGlower(this);
                return;
            }
            this.UpdateLit(this.parent.Map);


        }

        // Token: 0x060009E1 RID: 2529 RVA: 0x00035868 File Offset: 0x00033A68
        public override void ReceiveCompSignal(string signal)
        {
            if (signal == "PowerTurnedOn" || signal == "PowerTurnedOff" || signal == "FlickedOn" || signal == "FlickedOff" || signal == "Refueled" || signal == "RanOutOfFuel" || signal == "ScheduledOn" || signal == "ScheduledOff" || signal == "MechClusterDefeated")
            {
                this.UpdateLit(this.parent.Map);
            }
        }

        // Token: 0x060009E2 RID: 2530 RVA: 0x000358FB File Offset: 0x00033AFB
        public override void PostExposeData()
        {
            Scribe_Values.Look<bool>(ref this.glowOnInt, "glowOn", false, false);
        }

        // Token: 0x060009E3 RID: 2531 RVA: 0x0003590F File Offset: 0x00033B0F
        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            this.UpdateLit(map);
        }

        // Token: 0x04000800 RID: 2048
        private bool glowOnInt;
    }

    public class CompProperties_MovingGlower : CompProperties_Glower
    {
        public CompProperties_MovingGlower()
        {
            this.compClass = typeof(CompMovingGlower);
        }
    }


    //Begin Lasgun
    public class ThingDef_LasgunBeam : ThingDef
    {
        //nop
    }

    public class Projectile_LasgunBeam: Bullet
    {


        public ThingDef_LasgunBeam Def
        {
            get
            {
                return this.def as ThingDef_LasgunBeam;
            }
        }


        protected override void Impact(Thing hitThing)
        {

            if(hitThing != null && (bool)DuneEquipmentBase.LasgunInteraction && hitThing is Pawn hitPawn)
            {
                if (hitPawn.apparel != null)
                {

                    foreach (Apparel apparel in hitPawn.apparel.WornApparel)
                    {
                        var shield = apparel as ShieldBelt;
                        if (shield != null)
                        {

                            if (shield.ShieldState == ShieldState.Active)
                            {
                                int reps = 3;
                                //Log.Message("Lasgun/shield interaction, baby!");
                                IntVec3 position = base.Position;
                                Map map = base.Map;
                                float explosionRadius = 50.0f;
                                DamageDef bomb = DamageDefOf.Bomb;
                                Thing launcher = this.launcher;
                                int damageAmount = 2000;
                                float armorPenetration = 20.0f;
                                SoundDef explosionSound = null;
                                ThingDef equipmentDef = this.equipmentDef;
                                ThingDef def = this.def;
                                ThingDef filth_Fuel = ThingDefOf.Filth_Fuel;
                                for (int i = 0; i < reps; i++) { GenExplosion.DoExplosion(position, map, explosionRadius, bomb, launcher, damageAmount, armorPenetration, explosionSound, equipmentDef, def, this.intendedTarget.Thing, filth_Fuel, 0.2f, 1, false, null, 0f, 1, 0.4f, false, null, null); }
                                position = launcher.Position;
                                for (int i = 0; i < reps; i++) { GenExplosion.DoExplosion(position, map, explosionRadius, bomb, launcher, damageAmount, armorPenetration, explosionSound, equipmentDef, def, this.intendedTarget.Thing, filth_Fuel, 0.2f, 1, false, null, 0f, 1, 0.4f, false, null, null); }
                            }


                        }

                    }
                }
            }

            base.Impact(hitThing);
        }

    }

    


}