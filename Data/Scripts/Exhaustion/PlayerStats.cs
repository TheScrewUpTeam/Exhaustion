using Sandbox.Common.ObjectBuilders;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace TSUT.Exhaustion
{
    /// <summary>
    /// Stats of a player character.
    /// </summary>
    public class PlayerStats
    {
        public PlayerStats(IMyPlayer player, MyEntityStat staminaStat)
        {            
            this.player = player;
            stamina = staminaStat;
        }

        public IMyPlayer player;
        public MyEntityStat stamina;

        private static bool warningSoundPlayed = false;
        private static MyStringHash fatigueDamage = MyStringHash.GetOrCompute("Fatigue");
        private static readonly float gravityConstant = 9.81f * MyPerGameSettings.CharacterGravityMultiplier;
        Dictionary<MyStringHash, HashSet<int>> knownEffects = new Dictionary<MyStringHash, HashSet<int>>();

        private static Config config;

        public static void UpdateConfig(Config config)
        {
            PlayerStats.config = config;
        }

        public void Recalculate()
        {
            if (stamina == null) {
                return;
            }

            // If character died, then reset stats and skip further calculations, so
            // they're not potentially negative on re-spawn.
            if (player.Character.IsDead)
            {
                stamina.Value = PlayerStats.config.StaminaAfterDeath;
                MovementCosts.Died();
                return;
            }

            float staminaDelta = MovementCosts.GetStaminaCost(player.Character);

            // DEBUG
            //var msg = $"{currMovementState} {prevMovementState} {player.Character.Integrity}";
            //MyLog.Default.WriteLineAndConsole(msg);
            //MyAPIGateway.Utilities.ShowNotification(msg, 1000);

            float gravityInfluence;
            if (staminaDelta < 0.0f)
            {
                gravityInfluence = Math.Max(PlayerStats.config.GravityInfluence, player.Character.Physics.Gravity.Length() / gravityConstant);
            }
            else
            {
                // MAGICNUM 1.0f: for simplicity, stamina recovery doesn't get affected by gravity.
                gravityInfluence = 1.0f;
            }


            float workCost = 0f;            
            IMyHandheldGunObject<MyDeviceBase> tool = player.Character.EquippedTool as IMyHandheldGunObject<MyDeviceBase>;

            if (tool != null && tool.IsShooting) {
                workCost = WorkCosts.GetStaminaCost(tool);
            }

            if (workCost > 0f && staminaDelta > 0f) {
                staminaDelta = 0f;
            }

            // DEBUG
            //MyAPIGateway.Utilities.ShowNotification($"Delta: {staminaDelta}, Work: {workCost}, Value: {stamina.Value}", 1000);

            float result = staminaDelta * gravityInfluence + workCost;

            // Apply negative stamina as damage, with some scaling.
            if (stamina.Value + result < 0.0f)
            {
                player.Character.DoDamage(-result * PlayerStats.config.FatigueDamage, fatigueDamage, true);
            }

            if (result > 0) {
                stamina.Increase(result, null);
            } else {
                stamina.Decrease(-result, null);
            }

            // Play warning sound
            if (stamina.Value < 0.25f && !warningSoundPlayed)
            {
                string entityName = (player.Character as VRage.Game.ModAPI.Interfaces.IMyControllableEntity).Entity.Name;
                MyVisualScriptLogicProvider.PlaySingleSoundAtEntity("MyStaminaWarningSound", entityName);
                warningSoundPlayed = true;
            }
            if (stamina.Value > 0.25f) {
                warningSoundPlayed = false;
            }

            // Clamp stamina between -100% (unattainable enough) and current health.
            stamina.Value = Math.Max(-1.0f, Math.Min(stamina.Value, player.Character.Integrity / 100.0f));
        }

        internal void ProcessStat(MyEntityStat meelStat)
        {
            if (meelStat == null) {
                return;
            }
            HashSet<int> cachedEffects;
            if (!knownEffects.TryGetValue(meelStat.StatId, out cachedEffects)) {
                cachedEffects = new HashSet<int>();
                knownEffects.Add(meelStat.StatId, cachedEffects);
            }
            if (meelStat.HasAnyEffect()) {
                var effects = meelStat.GetEffects();
                foreach(var effectKVP in effects) {
                    if (cachedEffects.Contains(effectKVP.Key))
                        continue;
                    cachedEffects.Add(effectKVP.Key);
                    MyEntityStatRegenEffect effect = effectKVP.Value;
                    stamina.AddEffect(effect.Amount, effect.Interval, effect.Duration);
                }
            } else if (cachedEffects.Count > 0) {
                    cachedEffects.Clear();
            }
        }
    }

    public static class WorkCosts
    {
        private static bool WorkCostEnabled;
        private static float costLow;
        private static float costMed;
        private static float costHigh;
        private static float costNone;

        internal static void SetFromConfig(Config config)
        {
            costLow  = config.CostLow;
            costMed  = config.CostMedium;
            costHigh = config.CostHigh;
            costNone = config.CostNone;
            
            WorkCostEnabled = config.UseStaminaWork;
        }

        internal static float GetStaminaCost(IMyHandheldGunObject<MyDeviceBase> tool) 
        {
            if (!WorkCostEnabled) {
                return costNone;
            }
            var defId = tool.DefinitionId;

            if (defId.TypeId == typeof(MyObjectBuilder_Welder))
            {
                return costLow;
            }
            if (defId.TypeId == typeof(MyObjectBuilder_AngleGrinder))
            {
                return costMed;
            }
            if (defId.TypeId == typeof(MyObjectBuilder_HandDrill))
            {
                return costHigh;
            }
            return 0f;
        }
    }

    /// <summary>
    /// Helper class to map a movement state to its cost.
    /// </summary>
    public static class MovementCosts
    {
        private static bool MovementCostEnabled;
        private static bool DrivingCostEnabled;
        private static MyCharacterMovementEnum prevMovementState = MyCharacterMovementEnum.Standing;

        private static double lastControlTime = 0f;
        
        private static float gainHigh;
        private static float gainMed;
        private static float gainLow;
        private static float costNone;
        private static float costLow;
        private static float costMed;
        private static float costHigh;

        // helpers
        private static float walk;
        private static float crouchWalk;
        private static float run;

        internal static Dictionary<MyCharacterMovementEnum, float> Map;

        internal static void SetFromConfig(Config config)
        {
            MovementCostEnabled = config.UseStaminaMovement;
            DrivingCostEnabled = config.UseStaminaDriving;

            gainHigh = config.GainHigh;
            gainMed  = config.GainMedium;
            gainLow  = config.GainLow;
            costNone = config.CostNone;
            costLow  = config.CostLow;
            costMed  = config.CostMedium;
            costHigh = config.CostHigh;

            walk       = gainLow;
            crouchWalk = costLow;
            run        = costLow;

            Map = new Dictionary<MyCharacterMovementEnum, float>
            {
                // Enum values generated by MSVS2019 drom:
                // Assembly VRage.Game, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
                // C:\Program Files (x86)\Steam\steamapps\common\SpaceEngineers\Bin64\VRage.Game.dll
                { MyCharacterMovementEnum.Standing,                costNone   },
                { MyCharacterMovementEnum.Sitting,                 gainLow   },
                { MyCharacterMovementEnum.Crouching,               gainLow   },
                { MyCharacterMovementEnum.Flying,                  costLow    },
                { MyCharacterMovementEnum.Falling,                 gainLow    },
                { MyCharacterMovementEnum.Jump,                    costHigh   },
                { MyCharacterMovementEnum.Died,                    costNone   },
                { MyCharacterMovementEnum.Ladder,                  gainLow    },
                { MyCharacterMovementEnum.Walking,                 walk       },
                { MyCharacterMovementEnum.CrouchWalking,           crouchWalk },
                { MyCharacterMovementEnum.BackWalking,             walk       },
                { MyCharacterMovementEnum.CrouchBackWalking,       crouchWalk },
                { MyCharacterMovementEnum.WalkStrafingLeft,        walk       },
                { MyCharacterMovementEnum.CrouchStrafingLeft,      crouchWalk },
                { MyCharacterMovementEnum.WalkingLeftFront,        walk       },
                { MyCharacterMovementEnum.CrouchWalkingLeftFront,  crouchWalk },
                { MyCharacterMovementEnum.WalkingLeftBack,         walk       },
                { MyCharacterMovementEnum.CrouchWalkingLeftBack,   crouchWalk },
                { MyCharacterMovementEnum.WalkStrafingRight,       walk       },
                { MyCharacterMovementEnum.CrouchStrafingRight,     crouchWalk },
                { MyCharacterMovementEnum.WalkingRightFront,       walk       },
                { MyCharacterMovementEnum.CrouchWalkingRightFront, crouchWalk },
                { MyCharacterMovementEnum.WalkingRightBack,        walk       },
                { MyCharacterMovementEnum.CrouchWalkingRightBack,  crouchWalk },
                { MyCharacterMovementEnum.LadderUp,                costLow    },
                { MyCharacterMovementEnum.LadderDown,              costLow    },
                { MyCharacterMovementEnum.Running,                 run        },
                { MyCharacterMovementEnum.Backrunning,             run        },
                { MyCharacterMovementEnum.RunStrafingLeft,         run        },
                { MyCharacterMovementEnum.RunningLeftFront,        run        },
                { MyCharacterMovementEnum.RunningLeftBack,         run        },
                { MyCharacterMovementEnum.RunStrafingRight,        run        },
                { MyCharacterMovementEnum.RunningRightFront,       run        },
                { MyCharacterMovementEnum.RunningRightBack,        run        },
                { MyCharacterMovementEnum.Sprinting,               costMed    },
                { MyCharacterMovementEnum.RotatingLeft,            costNone   },
                { MyCharacterMovementEnum.CrouchRotatingLeft,      costLow    },
                { MyCharacterMovementEnum.RotatingRight,           costNone   },
                { MyCharacterMovementEnum.CrouchRotatingRight,     costLow    },
                { MyCharacterMovementEnum.LadderOut,               costNone   },
            };
        }

        internal static void Died() {
            prevMovementState = MyCharacterMovementEnum.Died;
        }

        internal static float CockpitStaminaUsage(IMyCharacter character) 
        {
            if (!DrivingCostEnabled) {
                return gainMed;
            }
            var controller = character.Parent as IMyCockpit;
            if (controller != null)
            {
                bool isDriving = controller.MoveIndicator != Vector3.Zero || controller.RotationIndicator != Vector2.Zero || controller.RollIndicator != 0f;
                bool isUsingTools = CheckIfGridToolsAreActive(controller);

                if (isDriving || isUsingTools) {
                    lastControlTime = MyAPIGateway.Session.ElapsedPlayTime.TotalSeconds;
                    return costLow;
                } else if (MyAPIGateway.Session.ElapsedPlayTime.TotalSeconds - lastControlTime > 3f)
                    return gainMed;
            }

            return 0f;
        }

        private static bool CheckIfGridToolsAreActive(IMyCockpit controller)
        {
            var isFiringPrimary = controller.IsShooting;

            return isFiringPrimary;
        }

        internal static float GetStaminaCost(IMyCharacter character)
        {
            float delta;

            // Logic for cryo/beds/seats/cockpits
            var parentBlock = character?.Parent as IMyCubeBlock;
            if (parentBlock != null)
            {
                string subtype = parentBlock.BlockDefinition.SubtypeName.ToLower();

                if (subtype.Contains("cryo"))
                {
                    return gainHigh;
                }
                if (subtype.Contains("bed") || subtype.Contains("medical"))
                {
                    return gainHigh;
                }
                if (subtype.Contains("seat") || subtype.Contains("chair"))
                {
                    return gainMed;
                }
                if (subtype.Contains("cockpit"))
                {
                    return CockpitStaminaUsage(character);
                }
            }

            if (!MovementCostEnabled) {
                return costNone;
            }

            MyCharacterMovementEnum currMovementState = character.CurrentMovementState;

            if (prevMovementState != MyCharacterMovementEnum.Jump)
            {
                delta = MovementCosts.Map[currMovementState];
            }
            else
            {
                // Character falls soon after jumping; dupe cost on first recalc after that
                // so the stamina change doesn't look too inconsistent from jump to jump.
                delta = MovementCosts.Map[MyCharacterMovementEnum.Jump];
            }

            // Update for next time.
            prevMovementState = character.CurrentMovementState;

            return delta;
        }
    }
}
