using BepInEx;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RealisticRecoil
{
    [BepInPlugin("com.servph.realisticrecoil", "Realistic Recoil", "1.0")]
    public class RealisticRecoilEntry : BaseUnityPlugin
    {
        public Player LocalPlayer { get; private set; }
        public static RealisticRecoilEntry Instance;
        private SkillManager.GClass1630 _buffs = new SkillManager.GClass1630();
        private BackendConfigSettingsClass.AimingConfiguration _aimingConfig;
        private static bool _isShooting;
        public Boolean HRHasRan { get; private set; }
        public Boolean LRHasRan { get; private set; }
        public Boolean PFLHasRan { get; private set; }
        public Boolean PFRHasRan { get; private set; }
        public Boolean VignetteTest { get; private set; }
        public static Vector2 DefaultBackwardsValues = new Vector2(0.65f, 1.05f);
        public static Vector2 DefaultXYValues = new Vector2(0.9f, 1.15f);

        public Vector3 HighReadyCamera = new Vector3(0, 0, 0);
        public Vector3 HighReadyOffset = new Vector3(0, -0.06f, -0.18f);
        public Vector3 HighReadyRotation = new Vector3(-55, 0, 0);

        public Vector3 LowReadyCamera = new Vector3(0, 0, 0);
        public Vector3 LowReadyOffset = new Vector3(0, -0.03f, 0.035f);
        public Vector3 LowReadyRotation = new Vector3(20, -5, 0);

        public Vector3 PointFireCamera = new Vector3(0, 0, 0);
        public Vector3 PointFireLeftOffset = new Vector3(-0.04f, -0.03f, 0f);
        public Vector3 PointFireLeftRotation = new Vector3(0, -45, 0);

        public Vector3 PointFireRightOffset = new Vector3(-0.01f, -0.03f, 0f);
        public Vector3 PointFireRightRotation = new Vector3(0, 45, 0);
        public ConfigEntry<BepInEx.Configuration.KeyboardShortcut> HighReadyBind { get; private set; }
        public ConfigEntry<BepInEx.Configuration.KeyboardShortcut> LowReadyBind { get; private set; }
        public ConfigEntry<BepInEx.Configuration.KeyboardShortcut> PointFireLeftBind { get; private set; }
        public ConfigEntry<BepInEx.Configuration.KeyboardShortcut> PointFireRightBind { get; private set; }
        public ConfigEntry<Boolean> HighReady { get; private set; }
        public ConfigEntry<Boolean> LowReady { get; private set; }
        public ConfigEntry<Boolean> PointFireRight { get; private set; }
        public ConfigEntry<Boolean> PointFireLeft { get; private set; }
        public ConfigEntry<Boolean> StressVignette { get; private set; }
        public ConfigEntry<Single> CameraRecoilAmount { get; private set; }

        // RecoilXY, RecoilZ, Convergence Multiplier, Dispersion Multiplier
        public Dictionary<string, ValueTuple<float, float, float, float>> SpecificGunModifiers = new Dictionary<string, ValueTuple<float, float, float, float>>
        {
            {"5fc3f2d5900b1d5091531e57",(3.2f, 0.1f, 12f, 8f) }, // VECTOR 9mm
            {"5fb64bc92b1b027b1f50bcf2",(3.2f, 0.1f, 12f, 8f) }, // Vector 45ACP
            {"6259b864ebedf17603599e88",(0.85f, 1.3f, 6f, 0f) }, // Benelli
            {"576165642459773c7a400233",(0.7f, 0.7f, 5f, 0f) }, // Saiga 12g
            {"627e14b21713922ded6f2c15",(0.5f, 1.7f, 6f, 0f) }, // AXMC
            {"5ae08f0a5acfc408fb1398a1",(0.7f, 1.3f, 3f, 0f) }, // Mosin Sniper
            {"5cadc190ae921500103bb3b6",(1.8f, 0.5f, 4f, 0f) }, // M9A3 Beretta
            {"5b1fa9b25acfc40018633c01",(2.3f, 1.1f, 8f, 0f) }, // Glock 18C
            {"6275303a9f372d6ea97f9ec7",(0.4f, 2f, 3f, 0f) }, // MSGL
            {"5e81ebcd8e146c7080625e15",(0.7f, 3f, 3f, 0f) }, // FN40
            {"5c501a4d2e221602b412b540",(0.7f, 3f, 3f, 0f) }, // VEPR 101 762x51
            {"CETME_AMELI_WEAP",(1.4f, 3f, 14f, 0f) }, // Katto Ameli LMG
            {"AK5C_WEAP",(3f, 0.4f, 6f, 0f) }, // Katto AK5C
            {"DEAGLE_WEAP",(1f, 0.4f, 3.4f, 0f) }, // Katto DEAGLE
            {"KAR98K_WEAP",(1f, 1.8f, 6f, 0f) }, // Katto KAR98
            {"CROM_CR7_WEAP_FDE",(0.8f, 0.4f, 6f, 0f) }, // Katto CR7 FDE
            {"CROM_CR7_WEAP_BLK",(0.8f, 0.4f, 6f, 0f) }, // Katto CR7 BLK
            {"weapon_stiletto",(0.777f, 1.4f, 2f, 0f) }, // Tron's Stiletto
        };
        public Dictionary<string, Dictionary<string, ValueTuple<float, float, float, float>>> CaliberModifiers = new Dictionary<string, Dictionary<string, ValueTuple<float, float, float, float>>>
        {
            // RecoilXY, RecoilZ, Convergence Multiplier, Dispersion Multiplier

            {
                "AssaultRifle", new Dictionary<string, ValueTuple<float, float, float, float>>
                {
                    {"556x45NATO", (0.6f, 0.65f, 8f, 0f) },
                    {"762x51", (1.3f, 0.6f, 5.7f, 0f) },
                    {"762x39", (0.7f, 0.8f, 9f, 0f) },
                    {"762x35", (0.85f, 0.95f, 5f, 0f) },
                    {"545x39", (0.65f, 0.55f, 4.5f, 0f) },
                    {"127x55", (1.3f, 0.9f, 3f, 0f) },
                    {"12.7x99", (0.4f, .6f, 12f, 0f) }, //katto 50 BMG
                }
            },
            {
                "assaultRifle", new Dictionary<string, ValueTuple<float, float, float, float>>
                {
                    {"556x45NATO", (0.6f, 0.65f, 8f, 0f) },
                    {"762x51", (1f, 0.6f, 5.7f, 0f) },
                    {"762x39", (0.7f, 0.8f, 9f, 0f) },
                    {"762x35", (0.85f, 0.95f, 5f, 0f) },
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
                    {"366TKM", (0.75f, 0.85f, 3f, 0f) },
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
                    {"6.8 TVCM", (1f, 8f, 12f, 0f) },
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

            this.HighReadyBind = this.Config.Bind("Stance Control", "High Ready Bind", new BepInEx.Configuration.KeyboardShortcut());
            this.LowReadyBind = this.Config.Bind("Stance Control", "Low Ready Bind", new BepInEx.Configuration.KeyboardShortcut());
            this.PointFireLeftBind = this.Config.Bind("Stance Control", "Left Point Fire Bind", new BepInEx.Configuration.KeyboardShortcut());
            this.PointFireRightBind = this.Config.Bind("Stance Control", "Right Point Fire Bind", new BepInEx.Configuration.KeyboardShortcut());
            this.HighReady = this.Config.Bind("Stance Control", "High Ready Enabled", false, "The bind controls this");
            this.LowReady = this.Config.Bind("Stance Control", "Low Ready Enabled", false, "The bind controls this");
            this.PointFireLeft = this.Config.Bind("Stance Control", "Left Point Fire Enabled", false, "The bind controls this");
            this.PointFireRight = this.Config.Bind("Stance Control", "Right Point Fire Enabled", false, "The bind controls this");
            this.StressVignette = this.Config.Bind("Weapons Control", "Stress Tunnel Vision", false, "Turns on / off Stress Vignette");
            this.CameraRecoilAmount = this.Config.Bind("Weapons Control", "Camera Recoil", 1f);
        }

        private BackendConfigSettingsClass.AimingConfiguration GClass1315_0
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
                VignetteTest = false;
                return;
            }

            GameWorld gameWorld = Singleton<GameWorld>.Instance;


            if (this.LocalPlayer == null && gameWorld.AllAlivePlayersList.Count > 0)
            {
                this.LocalPlayer = (Player)gameWorld.AllAlivePlayersList[0];
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
                var _cameraRecoilAmount = Instance.CameraRecoilAmount.Value;



                if (!Instance.LocalPlayer.HandsController.IsAiming)
                {
                    Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.RecoilStrengthXy = DefaultXYValues * Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.ConvertFromTaxanomy(controller.Item.Template.RecoilForceUp * num + this.GClass1315_0.RecoilVertBonus) * (modifiers.Item1 * 2) * 1.3f;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.RecoilStrengthZ = DefaultBackwardsValues * Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.ConvertFromTaxanomy(controller.Item.Template.RecoilForceBack * num + this.GClass1315_0.RecoilBackBonus) * (modifiers.Item2 * 0.6f * 1.3f);
                }
                else
                {
                    Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.RecoilStrengthXy = DefaultXYValues * Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.ConvertFromTaxanomy(controller.Item.Template.RecoilForceUp * num + this.GClass1315_0.RecoilVertBonus) * modifiers.Item1 * 1.3f;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.RecoilStrengthZ = DefaultBackwardsValues * Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.ConvertFromTaxanomy(controller.Item.Template.RecoilForceBack * num + this.GClass1315_0.RecoilBackBonus) * modifiers.Item2 * 1.3f;
                }
                Instance.LocalPlayer.ProceduralWeaponAnimation.HandsContainer.Recoil.ReturnSpeed = BetterConvergenceCalculator;
                Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.RecoilRadian = RecoilRadian;
                Instance.LocalPlayer.ProceduralWeaponAnimation._shouldMoveWeaponCloser = false;
                Instance.LocalPlayer.ProceduralWeaponAnimation.CameraSmoothOut = 12;
                Instance.LocalPlayer.ProceduralWeaponAnimation.CameraSmoothSteady = 14;
                Instance.LocalPlayer.ProceduralWeaponAnimation.CameraSmoothTime = 14;
                Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.ShotVals[3].Intensity = cameraRecoil * _cameraRecoilAmount;
                Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.ShotVals[4].Intensity = -cameraRecoil * _cameraRecoilAmount;

                if (Instance.LocalPlayer.HandsController.IsAiming == true)
                {
                    HighReady.Value = false;
                    LowReady.Value = false;
                    PointFireLeft.Value = false;
                    PointFireRight.Value = false;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.ZeroAdjustments();
                    Instance.LocalPlayer.ProceduralWeaponAnimation.ZeroAdjustments();
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindfireBlender.Speed = 4f;
                }


                if (Input.GetKeyDown(HighReadyBind.Value.MainKey))
                {
                    HighReady.Value = !HighReady.Value;
                }
                if (HighReady.Value)
                {
                    HRHasRan = false;
                    LowReady.Value = false;
                    PointFireLeft.Value = false;
                    PointFireRight.Value = false;
                    Instance.LocalPlayer.MovementContext.BlindFire = 0;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindfireBlender.Speed = 3.5f;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireCamera = Vector3.Lerp(Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireCamera, HighReadyCamera, Time.deltaTime * 12);
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireOffset = Vector3.Lerp(Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireOffset, HighReadyOffset, Time.deltaTime * 12);
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireRotation = Vector3.Lerp(Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireRotation, HighReadyRotation, Time.deltaTime * 12);
                    Instance.LocalPlayer.ProceduralWeaponAnimation.StartBlindFire(1);
                }
                if (HighReady.Value == false && HRHasRan == false)
                {
                    Instance.LocalPlayer.ProceduralWeaponAnimation.StartBlindFire(0);
                    Instance.LocalPlayer.ProceduralWeaponAnimation.ZeroAdjustments();
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
                    PointFireLeft.Value = false;
                    PointFireRight.Value = false;
                    Instance.LocalPlayer.MovementContext.BlindFire = 0;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindfireBlender.Speed = 3.5f;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireCamera = Vector3.Lerp(Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireCamera, LowReadyCamera, Time.deltaTime * 12);
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireOffset = Vector3.Lerp(Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireOffset, LowReadyOffset, Time.deltaTime * 12);
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireRotation = Vector3.Lerp(Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireRotation, LowReadyRotation, Time.deltaTime * 12);
                    Instance.LocalPlayer.ProceduralWeaponAnimation.StartBlindFire(1);
                }
                if (LowReady.Value == false && LRHasRan == false)
                {
                    Instance.LocalPlayer.ProceduralWeaponAnimation.StartBlindFire(0);
                    Instance.LocalPlayer.ProceduralWeaponAnimation.ZeroAdjustments();
                    LRHasRan = true;
                }


                if (Input.GetKeyDown(PointFireLeftBind.Value.MainKey))
                {
                    PointFireLeft.Value = !PointFireLeft.Value;
                }
                if (PointFireLeft.Value)
                {
                    PFLHasRan = false;
                    HighReady.Value = false;
                    LowReady.Value = false;
                    PointFireRight.Value = false;
                    Instance.LocalPlayer.MovementContext.BlindFire = 0;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindfireBlender.Speed = 5f;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireCamera = Vector3.Lerp(Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireCamera, PointFireCamera, Time.deltaTime * 12);
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireOffset = Vector3.Lerp(Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireOffset, PointFireLeftOffset, Time.deltaTime * 12);
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireRotation = Vector3.Lerp(Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireRotation, PointFireLeftRotation, Time.deltaTime * 12);
                    Instance.LocalPlayer.ProceduralWeaponAnimation.StartBlindFire(1);

                    Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.RecoilStrengthXy = DefaultXYValues * Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.ConvertFromTaxanomy(controller.Item.Template.RecoilForceUp * num + this.GClass1315_0.RecoilVertBonus) * (modifiers.Item1 * 0.6f);
                    Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.RecoilStrengthZ = DefaultBackwardsValues * Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.ConvertFromTaxanomy(controller.Item.Template.RecoilForceBack * num + this.GClass1315_0.RecoilBackBonus) * (modifiers.Item2 * 2f);
                }
                if (PointFireLeft.Value == false && PFLHasRan == false)
                {
                    Instance.LocalPlayer.ProceduralWeaponAnimation.StartBlindFire(0);
                    Instance.LocalPlayer.ProceduralWeaponAnimation.ZeroAdjustments();
                    PFLHasRan = true;
                }



                if (Input.GetKeyDown(PointFireRightBind.Value.MainKey))
                {
                    PointFireRight.Value = !PointFireRight.Value;
                }
                if (PointFireRight.Value)
                {
                    PFRHasRan = false;
                    HighReady.Value = false;
                    LowReady.Value = false;
                    PointFireLeft.Value = false;
                    Instance.LocalPlayer.MovementContext.BlindFire = 0;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindfireBlender.Speed = 5f;
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireCamera = Vector3.Lerp(Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireCamera, PointFireCamera, Time.deltaTime * 12);
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireOffset = Vector3.Lerp(Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireOffset, PointFireRightOffset, Time.deltaTime * 12);
                    Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireRotation = Vector3.Lerp(Instance.LocalPlayer.ProceduralWeaponAnimation.BlindFireRotation, PointFireRightRotation, Time.deltaTime * 12);
                    Instance.LocalPlayer.ProceduralWeaponAnimation.StartBlindFire(1);

                    Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.RecoilStrengthXy = DefaultXYValues * Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.ConvertFromTaxanomy(controller.Item.Template.RecoilForceUp * num + this.GClass1315_0.RecoilVertBonus) * (modifiers.Item1 * 0.6f);
                    Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.RecoilStrengthZ = DefaultBackwardsValues * Instance.LocalPlayer.ProceduralWeaponAnimation.Shootingg.ConvertFromTaxanomy(controller.Item.Template.RecoilForceBack * num + this.GClass1315_0.RecoilBackBonus) * (modifiers.Item2 * 2f);
                }
                if (PointFireRight.Value == false && PFRHasRan == false)
                {
                    Instance.LocalPlayer.ProceduralWeaponAnimation.StartBlindFire(0);
                    Instance.LocalPlayer.ProceduralWeaponAnimation.ZeroAdjustments();
                    PFRHasRan = true;
                }



                if (StressVignette.Value)
                {
                    _isShooting = (bool)(typeof(Player.FirearmController).GetField("bool_3", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(controller) as bool?);
                    if (_isShooting == true)
                    {
                        CameraClass.Instance.Camera.GetComponent<CC_FastVignette>().enabled = true;
                        if (VignetteTest == false)
                        {
                            CameraClass.Instance.Camera.GetComponent<CC_FastVignette>().darkness = 0f;
                            VignetteTest = true;
                        }

                        if (CameraClass.Instance.Camera.GetComponent<CC_FastVignette>().darkness < 130f)
                        {
                            CameraClass.Instance.Camera.GetComponent<CC_FastVignette>().darkness = CameraClass.Instance.Camera.GetComponent<CC_FastVignette>().darkness + .3f * (controller.Item.Template.bFirerate * (0.003f / 2));
                        }
                    }
                    else
                    {
                        if (CameraClass.Instance.Camera.GetComponent<CC_FastVignette>().darkness > 0)
                        {
                            CameraClass.Instance.Camera.GetComponent<CC_FastVignette>().darkness = CameraClass.Instance.Camera.GetComponent<CC_FastVignette>().darkness - .24f;
                        }
                    }
                }



            }
        }
    }
}
