using BepInEx;
using CosmeticsSwitch.GunSystem;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using static GorillaNetworking.CosmeticsController;

namespace CosmeticsSwitch
{
    [BepInPlugin("AC.CosmeticsSwitch", "Cosmetics Switch", "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        private Dictionary<VRRig, bool> playerCosmeticsState = new Dictionary<VRRig, bool>();
        private bool myCosmeticsEnabled = true;
        private float lastTriggerTime = 0f;
        private const float COOLDOWN_TIME = 0.5f;
        private VRRig lastHitRig = null;

        void Awake()
        {
            Harmony harmony = new Harmony("AC.CosmeticsSwitch");
            harmony.PatchAll();
        }

        void Update()
        {
            float currentTime = Time.time;

            if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f)
            {
                var (hit, pointer) = SimpleGun.RenderGun();

                if (hit.collider != null)
                {
                    var hitRig = SimpleGun.GetHitPlayer(hit);

                    // Проверяем кулдаун и изменение цели
                    if (hitRig != null && !hitRig.isLocal &&
                        (currentTime - lastTriggerTime >= COOLDOWN_TIME || hitRig != lastHitRig))
                    {
                        // ПРОВЕРЯЕМ В СЛОВАРЕ
                        if (playerCosmeticsState.TryGetValue(hitRig, out bool currentState))
                        {
                            // Есть в словаре - переключаем
                            if (currentState)
                            {
                                // Было включено - выключаем
                                DisableCosmeticsForPlayer(hitRig);
                                playerCosmeticsState[hitRig] = false;
                            }
                            else
                            {
                                // Было выключено - включаем
                                EnableCosmeticsForPlayer(hitRig);
                                playerCosmeticsState[hitRig] = true;
                            }
                        }
                        else
                        {
                            // Нет в словаре - первый раз, выключаем
                            DisableCosmeticsForPlayer(hitRig);
                            playerCosmeticsState[hitRig] = false;
                        }

                        // Обновляем время последнего срабатывания и цель
                        lastTriggerTime = currentTime;
                        lastHitRig = hitRig;
                    }
                }
            }
            else
            {
                SimpleGun.HideGun();
                // Сбрасываем последнюю цель при отпускании триггера
                lastHitRig = null;
            }

            // Остальной код с клавишами U, I, O остается без изменений
            if (UnityInput.Current.GetKeyDown(KeyCode.U))
            {
                EnableCosmeticsForPlayer(VRRig.LocalRig);
            }

            if (UnityInput.Current.GetKeyDown(KeyCode.I))
            {
                DisableCosmeticsForPlayer(VRRig.LocalRig);
            }

            if (UnityInput.Current.GetKeyDown(KeyCode.O))
            {
                myCosmeticsEnabled = !myCosmeticsEnabled;

                if (myCosmeticsEnabled)
                {
                    EnableCosmeticsForPlayer(VRRig.LocalRig);
                }
                else
                {
                    DisableCosmeticsForPlayer(VRRig.LocalRig);
                }
            }
        }

        CosmeticSlots[] GetSlotsByItemCategory(CosmeticItem item)
        {
            switch (item.itemCategory)
            {
                case CosmeticCategory.Hat:
                    return new CosmeticSlots[] { CosmeticSlots.Hat };
                case CosmeticCategory.Face:
                    return new CosmeticSlots[] { CosmeticSlots.Face };
                case CosmeticCategory.Badge:
                    return new CosmeticSlots[] { CosmeticSlots.Badge };
                case CosmeticCategory.Arms:
                    // Возвращаем оба слота для рук
                    return new CosmeticSlots[] { CosmeticSlots.ArmLeft, CosmeticSlots.ArmRight, CosmeticSlots.HandLeft, CosmeticSlots.HandRight };
                case CosmeticCategory.Chest:
                    return new CosmeticSlots[] { CosmeticSlots.Chest };
                case CosmeticCategory.Fur:
                    return new CosmeticSlots[] { CosmeticSlots.Fur };
                case CosmeticCategory.TagEffect:
                    return new CosmeticSlots[] { CosmeticSlots.TagEffect };
                case CosmeticCategory.Shirt:
                    return new CosmeticSlots[] { CosmeticSlots.Shirt };
                case CosmeticCategory.Pants:
                    return new CosmeticSlots[] { CosmeticSlots.Pants };
                case CosmeticCategory.Back:
                    return new CosmeticSlots[] {CosmeticSlots.BackLeft, CosmeticSlots.BackRight };

                default:
                    return new CosmeticSlots[] { CosmeticSlots.Hat }; // Значение по умолчанию
            }
        }

        void DisableCosmeticsForPlayer(VRRig playerRig)
        {
            foreach (var item in playerRig.cosmeticSet.items)
            {
                var slots = GetSlotsByItemCategory(item);
                var cosmetic = playerRig.cosmeticsObjectRegistry?.Cosmetic(item.displayName);

                if (cosmetic != null)
                {
                    foreach (var slot in slots)
                    {
                        cosmetic.DisableItem(slot);
                    }
                }
            }
        }

        void EnableCosmeticsForPlayer(VRRig playerRig)
        {
            foreach (var item in playerRig.cosmeticSet.items)
            {
                var slots = GetSlotsByItemCategory(item);
                var cosmetic = playerRig.cosmeticsObjectRegistry?.Cosmetic(item.displayName);

                if (cosmetic != null)
                {
                    foreach (var slot in slots)
                    {
                        cosmetic.EnableItem(slot, playerRig);
                    }
                }
            }
        }
    }
}
