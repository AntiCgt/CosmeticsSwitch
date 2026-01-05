using BepInEx;
using CosmeticsSwitch.GunSystem;
using GorillaNetworking;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using static GorillaNetworking.CosmeticsController;
using static PerfTestGorillaSlot;

namespace CosmeticsSwitch
{
    [BepInPlugin("AC.CosmeticsSwitch", "Cosmetics Switch", "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        private Dictionary<VRRig, bool> playerCosmeticsState = new Dictionary<VRRig, bool>();
        private bool myCosmeticsEnabled = true;
        void Awake()
        {
            Harmony harmony = new Harmony("AC.CosmeticsSwitch");
            harmony.PatchAll();
        }
        void Start()
        {
        }

        void Update()
        {

            if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f)
            {
                var (hit, pointer) = SimpleGun.RenderGun();

                if (hit.collider != null)
                {
                    var hitRig = SimpleGun.GetHitPlayer(hit);
                    if (hitRig != null && !hitRig.isLocal)
                    {
                        if (playerCosmeticsState.TryGetValue(hitRig, out bool currentState))
                        {
                            if (currentState)
                            {
                                foreach (var item in hitRig.cosmeticSet.items)
                                {
                                    hitRig.cosmeticsObjectRegistry?.Cosmetic(item.displayName)?.DisableItem((CosmeticSlots)item.itemCategory);
                                }
                                playerCosmeticsState[hitRig] = false;
                            }
                            else
                            {
                                foreach (var item in hitRig.cosmeticSet.items)
                                {
                                    hitRig.cosmeticsObjectRegistry?.Cosmetic(item.displayName)?.EnableItem(GetSlotByItemCategory(item), hitRig);
                                }
                                playerCosmeticsState[hitRig] = true;
                            }
                        }
                        else
                        {
                            foreach (var item in hitRig.cosmeticSet.items)
                            {
                                hitRig.cosmeticsObjectRegistry?.Cosmetic(item.displayName)?.DisableItem((CosmeticSlots)item.itemCategory);
                            }
                            playerCosmeticsState[hitRig] = false;
                        }
                    }
                }
            }
            else
            {
                SimpleGun.HideGun();
            }


            if (UnityInput.Current.GetKeyDown(KeyCode.U))
            {
                foreach (CosmeticsController.CosmeticItem item in VRRig.LocalRig.cosmeticSet.items)
                {
                    var registry = GorillaTagger.Instance.offlineVRRig.cosmeticsObjectRegistry;
                    var cosmeticInstance = registry.Cosmetic(item.displayName);

                    cosmeticInstance.EnableItem(GetSlotByItemCategory(item), VRRig.LocalRig);
                }
            }
            ;

            if (UnityInput.Current.GetKeyDown(KeyCode.I))
            {
                foreach (CosmeticsController.CosmeticItem item in VRRig.LocalRig.cosmeticSet.items)
                {
                    var registry = GorillaTagger.Instance.offlineVRRig.cosmeticsObjectRegistry;
                    var cosmeticInstance = registry.Cosmetic(item.displayName);

                    DisableCosmeticsForPlayer(VRRig.LocalRig);
                }
            }
            ;

            if (UnityInput.Current.GetKeyDown(KeyCode.O))
            {
                myCosmeticsEnabled = !myCosmeticsEnabled;

                if (myCosmeticsEnabled)
                {
                    foreach (CosmeticsController.CosmeticItem item in VRRig.LocalRig.cosmeticSet.items)
                    {
                        var registry = GorillaTagger.Instance.offlineVRRig.cosmeticsObjectRegistry;
                        var cosmeticInstance = registry.Cosmetic(item.displayName);

                        cosmeticInstance.EnableItem(GetSlotByItemCategory(item), VRRig.LocalRig);
                    }
                }
                else
                {
                    foreach (CosmeticsController.CosmeticItem item in VRRig.LocalRig.cosmeticSet.items)
                    {
                        var registry = GorillaTagger.Instance.offlineVRRig.cosmeticsObjectRegistry;
                        var cosmeticInstance = registry.Cosmetic(item.displayName);

                        DisableCosmeticsForPlayer(VRRig.LocalRig);
                    }
                }
            }
        }

        void OnDestroy()
        {

        }

        CosmeticSlots GetSlotByItemCategory(CosmeticItem item)
        {
            switch (item.itemCategory)
            {
                case CosmeticCategory.Hat:
                    return CosmeticSlots.Hat;
                case CosmeticCategory.Face:
                    return CosmeticSlots.Face;
                case CosmeticCategory.Badge:
                    return CosmeticSlots.Badge;
                case CosmeticCategory.Arms:
                    return CosmeticSlots.Arms;
                case CosmeticCategory.Chest:
                    return CosmeticSlots.Chest;
                case CosmeticCategory.Fur:
                    return CosmeticSlots.Fur;
                case CosmeticCategory.TagEffect:
                    return CosmeticSlots.TagEffect;
                case CosmeticCategory.Shirt:
                    return CosmeticSlots.Shirt;
                case CosmeticCategory.Pants:
                    return CosmeticSlots.Pants;
                case CosmeticCategory.Back:
                    return CosmeticSlots.Back;
                default:
                    return CosmeticSlots.Hat;
            }
        }
        void DisableCosmeticsForPlayer(VRRig playerRig)
        {

            foreach (var item in playerRig.cosmeticSet.items)
            {
                var cosmetic = playerRig.cosmeticsObjectRegistry?.Cosmetic(item.displayName);
                cosmetic?.DisableItem((CosmeticSlots)item.itemCategory);
            }
        }
    }
}
