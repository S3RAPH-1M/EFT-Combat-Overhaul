using BepInEx;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using UnityEngine;

namespace RealisticRecoil
{
    [BepInPlugin("com.servph.realisticrecoil", "Realistic Recoil", "1.0")]
    public class RealisticRecoilEntry : BaseUnityPlugin
    {
        public Player LocalPlayer { get; private set; }
        public static RealisticRecoilEntry Instance;
        private SkillsClass.GClass1673 _buffs = new SkillsClass.GClass1673();
        private BackendConfigSettingsClass.GClass1310 _aimingConfig;
        public Boolean HRHasRan { get; private set; }
        public Boolean LRHasRan { get; private set; }
        public static Vector2 DefaultBackwardsValues = new Vector2(0.65f, 1.05f);
        public static Vector2 DefaultXYValues = new Vector2(0.9f, 1.15f);

        public Vector3 HighReadyCamera = new Vector3(0, 0, 0);
        public Vector3 HighReadyOffset = new Vector3(0, -0.06f, -0.18f);
        public Vector3 HighReadyRotation = new Vector3(-55, 0, 0);

        public Vector3  LowReadyCamera = new Vector3(0, 0, 0);
        public Vector3 LowReadyOffset = new Vector3(0, -0.03f, 0.035f);
        public Vector3 LowReadyRotation = new Vector3(20, -5, 0);

        public ConfigEntry<BepInEx.Configuration.KeyboardShortcut> HighReadyBind { get; private set; }
        public ConfigEntry<BepInEx.Configuration.KeyboardShortcut> LowReadyBind { get; private set; }
        public ConfigEntry<Boolean> HighReady { get; private set; }
        public ConfigEntry<Boolean> LowReady { get; private set; }

        public Dictionary<string, ValueTuple<float, float, float, float>> SpecificGunModifiers = new Dictionary<string, ValueTuple<float, float, float, float>>
        {
            {"5fc3f2d5900b1d5091531e57",(3.2f, 0.1f, 12f, 8f) }, // VECTOR 9mm
            {"5fb64bc92b1b027b1f50bcf2",(3.2f, 0.1f, 12f, 8f) }, // Vector 45ACP
            {"6259b864ebedf17603599e88",(0.85f, 1.3f, 6f, 0f) }, // Benelli
            {"576165642459773c7a400233",(0.7f, 0.7f, 5f, 0f) }, // Saiga 12g
            {"627e14b21713922ded6f2c15",(0.5f, 1.7f, 6f, 0f) }, // AXMC
            {"5cadc190ae921500103bb3b6",(1.8f, 0.5f, 4f, 0f) }, // M9A3 Beretta
            {"6275303a9f372d6ea97f9ec7",(0.4f, 2f, 3f, 0f) }, // MSGL
            {"5e81ebcd8e146c7080625e15",(0.7f, 3f, 3f, 0f) }, // FN40
            {"CETME_AMELI_WEAP",(1.4f, 3f, 14f, 0f) }, // Katto Ameli LMG
            {"AK5C_WEAP",(3f, 0.4f, 12f, 0f) }, // Katto AK5C
            {"DEAGLE_WEAP",(2f, 0.4f, 6f, 0f) }, // Katto DEAGLE
            {"KAR98K_WEAP",(1f, 1.8f, 6f, 0f) }, // Katto KAR98
            {"CROM_CR7_WEAP_FDE",(0.8f, 0.4f, 6f, 0f) }, // Katto CR7 FDE
            {"CROM_CR7_WEAP_BLK",(0.8f, 0.4f, 6f, 0f) }, // Katto CR7 BLK
        };
        public Dictionary<string, Dictionary<string, ValueTuple<float, float, float, float>>> CaliberModifiers = new Dictionary<string, Dictionary<string, ValueTuple<float, float, float, float>>>
        {
            // RecoilXY, RecoilZ, Convergence Multiplier, Dispersion Multiplier

            {
                "AssaultRifle", new Dictionary<string, ValueTuple<float, float, float, float>>
                {
                    {"556x45NATO", (0.9f, 0.45f, 8f, 0f) },
                    {"762x51", (1f, 0.6f, 5.7f, 0f) },
                    {"762x39", (1.333f, 0.55f, 12f, 0f) },
                    {"762x35", (0.65f, 0.55f, 5f, 0f) },
                    {"545x39", (0.65f, 0.55f, 4.5f, 0f) },
                    {"127x55", (1.3f, 0.9f, 3f, 0f) },
                    {"12.7x99", (0.4f, .6f, 12f, 0f) }, //katto 50 BMG
                }
            },
            {
                "AssaultCarbine", new Dictionary<string, ValueTuple<float, float, float, float>>
                {
                    {"762x51", (0.7f, 0.6f, 0f, 0f) },
                    {"556x45NATO", (0.6f, 0.35f, 10f, 0f) },
                    {"762x39", (0.6f, 0.55f, 1.6f, 0f) },
                    {"366TKM", (0.75f, 0.85f, 0f, 0f) },
                    {"9x39", (0.65f, 0.55f, 7f, 0f) },
                    {"127x55", (1.3f, 0.9f, 0f, 0f) }
                }
            },
            {
                "MachineGun", new Dictionary<string, ValueTuple<float, float, float, float>>
                {
                    {"545x39", (0.60f, 1.1f, 4f, 0f) },
                }
            },
            {
                "MarksmanRifle", new Dictionary<string, ValueTuple<float, float, float, float>>
                {
                    {"86x70", (0.8f, 1.7f, 6f, 0f) },
                    {"762x54R", (0.6f, 1.3f, 6f, 0f) },
                    {"762x51", (1.18f, 1.7f, 12f, 0f) },
                    {"9x39", (0.65f, 0.55f, 12f, 0f) },
                }
            },
            {
                "Smg", new Dictionary<string, ValueTuple<float, float, float, float>>
                {
                    {"9x21", (0.9f, 0.3f, 1.7f, 1f) },
                    {"9x19PARA", (0.5f, 0.9f, 4f, 3f) },
                    {"9x18PM", (0.65f, 0.6f, 4f, 1f) },
                    {"762x25TT", (1f, 0.3f, 4f, 1f) },
                    {"46x30", (0.75f, 0.7f, 3.2f, 1f) },
                    {"1143x23ACP", (0.85f, 0.7f, 4f, 1f) },
                    {"57x28", (0.8f, 0.5f, 3f, 1f) },
                    {"762x54R", (0.6f, .8f, 12f, 0f) }, // katto DP-27 
                    {"357SIG", (0.6f, 0.7f, 6f, 0f) }, // katto Scorpion EVO
                }
            },
            {
                "Shotgun", new Dictionary<string, ValueTuple<float, float, float, float>>
                {
                    {"12g", (0.8f, 0.6f, 7f, 0f) },
                    {"20g", (1.2f, 0.8f, 4f, 0f) },
                    {"23x75", (2f, 2f, 2f, 0f) },
                }
            },
            {
                "Revolver", new Dictionary<string, ValueTuple<float, float, float, float>>
                {
                    {"9x33R", (1.9f, 0.55f, 1.7f, 0f) },
                    {"9x19PARA", (1.1f, 0.55f, 3f, 0f) },
                    {"127x55", (1.3f, 0.9f, 0.4f, 0f) }
                }
            },
            {
                "Pistol", new Dictionary<string, ValueTuple<float, float, float, float>>
                {
                    {"9x33R", (0.65f, 0.55f, 6f, 0f) },
                    {"9x21", (0.578f, 0.3f, 6f, 0f) },
                    {"9x19PARA", (1.3f, 1.1f, 4f, 0f) },
                    {"9x18PM", (0.75f, 0.4f, 5f, 0f) },
                    {"57x28", (0.18f, 0.3f, 0.8f, 0f) },
                    {"1143x23ACP", (1.6f, 0.3f, 3.8f, 0f) },
                    {"762x25TT", (0.85f, 0.7f, 6f, 0f) },
                    {"127x55", (1.3f, 0.9f, 6f, 0f) }
                }
            },
        };

        public void Awake()
        {
            Instance = this;

            this.HighReadyBind = this.Config.Bind("Weapon Control", "High Ready Bind", new BepInEx.Configuration.KeyboardShortcut());
            this.LowReadyBind = this.Config.Bind("Weapon Control", "Low Ready Bind", new BepInEx.Configuration.KeyboardShortcut());
            this.HighReady = this.Config.Bind("Weapons Control", "High Ready Enabled", false, "The bind controls this");
            this.LowReady = this.Config.Bind("Weapons Control", "Low Ready Enabled", false, "The bind controls this");
        }

        private BackendConfigSettingsClass.GClass1310 GClass1310_0
        {
            get
            {
                if (this._aimingConfig == null)
                {
                    this._aimingConfig = Singleton<BackendConfigSettingsClass>.Instance.Aiming;
                }
                return this._aimingConfig;
            }
        }

        public void Update()
        {
            if (!Singleton<GameWorld>.Instantiated)
            {
                this.LocalPlayer = null;
                return;
            }

            GameWorld gameWorld = Singleton<GameWorld>.Instance;


            if (this.LocalPlayer == null && gameWorld.RegisteredPlayers.Count > 0)
            {
                this.LocalPlayer = gameWorld.RegisteredPlayers[0];
                return;
            }

            if (Instance.LocalPlayer != null && Instance.LocalPlayer.HandsController != null && Instance.LocalPlayer.HandsAnimator != null && Instance.LocalPlayer.ProceduralWeaponAnimation != null && Instance.LocalPlayer.HandsController is Player.FirearmController controller)
            {
                Instance.LocalPlayer.ProceduralWeaponAnimation.CrankRecoil = true;
                if (CameraClass.Instance == null)
                {
                    return;
                }
                if (Instance.LocalPlayer.HandsController == null)
                {
                    return;
                }
                if (Instance.LocalPlayer.HandsController.Item == null)
                {
                    return;
                }
                if (Instance.LocalPlayer.HandsController.Item.Template == null)
                {
                    return;
                }
                if (Instance.LocalPlayer.HandsController.Item.Template.Parent == null)
                {
                    return;
                }

                ValueTuple<float, float, float, float> modifiers = (1f, 1f, 0f, 0f);
                if (!SpecificGunModifiers.TryGetValue(controller.Item.TemplateId, out modifiers) && (!CaliberModifiers.TryGetValue(Instance.LocalPlayer.HandsController.Item.Template.Parent._name, out var currentmodifiers) || !currentmodifiers.TryGetValue(controller.Item.AmmoCaliber, out modifiers)))
                {
                    modifiers = (1f, 1f, 1f, 1f);
                }
                //Math related to Recoil and General IK
                float num = Mathf.Max(0f, 1f + controller.Item.RecoilDelta) * (1f - this._buffs.RecoilSupression.x - this._buffs.RecoilSupression.y * 0.1f);
                var BetterConvergenceCalculator = controller.Item.Template.Convergence * modifiers.Item3;
                var RecoilRadian = Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.RecoilDegree * 0.017453292f * modifiers.Item4;
                var RecoilDampening = .45f * (modifiers.Item1 / 10) * (modifiers.Item2);
                var cameraRecoil = controller.Item.Template.CameraRecoil;



                if (!Instance.LocalPlayer.HandsController.IsAiming)
                {
                    Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.RecoilStrengthXy = DefaultXYValues * Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.ConvertFromTaxanomy(controller.Item.Template.RecoilForceUp * num + this.GClass1310_0.RecoilVertBonus) * (modifiers.Item1 * 2);
                    Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.RecoilStrengthZ = DefaultBackwardsValues * Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.ConvertFromTaxanomy(controller.Item.Template.RecoilForceBack * num + this.GClass1310_0.RecoilBackBonus) * (modifiers.Item2 * 0.6f);
                }
                else
                {
                    Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.RecoilStrengthXy = DefaultXYValues * Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.ConvertFromTaxanomy(controller.Item.Template.RecoilForceUp * num + this.GClass1310_0.RecoilVertBonus) * modifiers.Item1;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.RecoilStrengthZ = DefaultBackwardsValues * Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.ConvertFromTaxanomy(controller.Item.Template.RecoilForceBack * num + this.GClass1310_0.RecoilBackBonus) * modifiers.Item2;
                }
                Instance.LocalPlayer.ProceduralWeaponAnimation.HandsContainer.Recoil.ReturnSpeed = BetterConvergenceCalculator;
                Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.RecoilRadian = RecoilRadian;
                Instance.LocalPlayer.ProceduralWeaponAnimation._shouldMoveWeaponCloser = false;
                Instance.LocalPlayer.ProceduralWeaponAnimation.CameraSmoothOut = 14;
                Instance.LocalPlayer.ProceduralWeaponAnimation.CameraSmoothSteady = 16;
                Instance.LocalPlayer.ProceduralWeaponAnimation.CameraSmoothTime = 16;
                Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.ShotVals[3].Intensity = cameraRecoil * 0.65f;
                Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.ShotVals[4].Intensity = -cameraRecoil * 0.65f;

                if (Instance.LocalPlayer.HandsController.IsAiming == true)
                {
                    HighReady.Value = false;
                    LowReady.Value = false;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindfireBlender.Speed = 10f;
                }

                if (Input.GetKeyDown(HighReadyBind.Value.MainKey))
                {
                    HighReady.Value = !HighReady.Value;
                }
                if (HighReady.Value)
                {
                    HRHasRan = false;
                    LowReady.Value = false;
                    Instance.LocalPlayer.MovementContext.BlindFire = 0;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindfireBlender.Speed = 5f;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireCamera = HighReadyCamera;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireOffset = HighReadyOffset;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireRotation = HighReadyRotation;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.StartBlindFire(1);
                }
                if (HighReady.Value == false && HRHasRan == false)
                {
                    Instance.LocalPlayer.ProceduralWeaponAnimation.StartBlindFire(0);
                    HRHasRan = true;
                }


                if (Input.GetKeyDown(LowReadyBind.Value.MainKey))
                {
                    LowReady.Value = !LowReady.Value;
                }
                if (LowReady.Value)
                {
                    LRHasRan = false;
                    HighReady.Value = false;
                    Instance.LocalPlayer.MovementContext.BlindFire = 0;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindfireBlender.Speed = 5f;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireCamera = LowReadyCamera;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireOffset = LowReadyOffset;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireRotation = LowReadyRotation;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.StartBlindFire(1);
                }
                if (LowReady.Value == false && LRHasRan == false)
                {
                    Instance.LocalPlayer.ProceduralWeaponAnimation.StartBlindFire(0);
                    LRHasRan = true;
                }



            }
        }
    }


}