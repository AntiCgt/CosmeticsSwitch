using System;
using UnityEngine;
using GorillaTag;
using GorillaLocomotion;

namespace CosmeticsSwitch.GunSystem
{
    public static class SimpleGun
    {
        // ========== ОСНОВНЫЕ ПЕРЕМЕННЫЕ ==========
        public static bool isShooting = false;
        public static GameObject GunPointer = null;
        public static LineRenderer GunLine = null;

        // Настройки (можно менять если нужно)
        public static float rayLength = 50f;
        public static float pointerSize = 0.1f;
        public static float lineWidth = 0.01f;
        public static Color lineColor = Color.red;
        public static Color pointerColor = Color.red;

        // ========== ОСНОВНОЙ МЕТОД ==========

        /// <summary>
        /// Рендерит луч из правой руки и возвращает информацию о попадании
        /// </summary>
        public static (RaycastHit hit, GameObject pointer) RenderGun()
        {
            // 1. Получаем позицию правой руки
            Transform rightHand = GorillaTagger.Instance.rightHandTransform;
            Vector3 startPosition = rightHand.position;
            Vector3 direction = rightHand.forward;

            // 2. Делаем рейкаст
            RaycastHit hit;
            bool hasHit = Physics.Raycast(startPosition, direction, out hit, rayLength, GetLayerMask());

            // 3. Определяем конечную точку
            Vector3 endPosition = hasHit ? hit.point : startPosition + direction * rayLength;

            // 4. Создаем/обновляем визуальные элементы
            CreatePointer(endPosition);
            CreateLine(startPosition, endPosition);

            // 5. Возвращаем результат
            return (hit, GunPointer);
        }

        /// <summary>
        /// Скрывает луч и указатель
        /// </summary>
        public static void HideGun()
        {
            if (GunPointer != null) GunPointer.SetActive(false);
            if (GunLine != null) GunLine.gameObject.SetActive(false);
        }

        /// <summary>
        /// Полностью очищает объекты оружия
        /// </summary>
        public static void Cleanup()
        {
            if (GunPointer != null) GameObject.Destroy(GunPointer);
            if (GunLine != null) GameObject.Destroy(GunLine.gameObject);

            GunPointer = null;
            GunLine = null;
        }

        // ========== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ==========

        private static void CreatePointer(Vector3 position)
        {
            // Создаем указатель если его нет
            if (GunPointer == null)
            {
                GunPointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                GameObject.Destroy(GunPointer.GetComponent<Collider>()); // Убираем коллайдер

                // Настраиваем материал
                Renderer renderer = GunPointer.GetComponent<Renderer>();
                renderer.material = new Material(Shader.Find("GUI/Text Shader"));
                renderer.material.color = pointerColor;
            }

            // Обновляем позицию и размер
            GunPointer.SetActive(true);
            GunPointer.transform.position = position;
            GunPointer.transform.localScale = Vector3.one * pointerSize;
        }

        private static void CreateLine(Vector3 start, Vector3 end)
        {
            // Создаем линию если ее нет
            if (GunLine == null)
            {
                GameObject lineObj = new GameObject("GunLine");
                GunLine = lineObj.AddComponent<LineRenderer>();

                // Настраиваем линию
                GunLine.material = new Material(Shader.Find("GUI/Text Shader"));
                GunLine.startColor = lineColor;
                GunLine.endColor = lineColor;
                GunLine.startWidth = lineWidth;
                GunLine.endWidth = lineWidth;
                GunLine.useWorldSpace = true;
            }

            // Обновляем позиции
            GunLine.gameObject.SetActive(true);
            GunLine.positionCount = 2;
            GunLine.SetPosition(0, start);
            GunLine.SetPosition(1, end);
        }

        private static int GetLayerMask()
        {
            // Создаем маску слоев (исключаем невидимые слои)
            int layerMask = 0;

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    // Исключаем слои которые не должны быть видны
                    if (!layerName.Contains("Ignore") &&
                        !layerName.Contains("Transparent") &&
                        !layerName.Contains("UI") &&
                        !layerName.Contains("Water"))
                    {
                        layerMask |= 1 << i;
                    }
                }
            }

            return layerMask;
        }

        /// <summary>
        /// Проверяет, попал ли луч в игрока
        /// </summary>
        public static VRRig GetHitPlayer(RaycastHit hit)
        {
            if (hit.collider != null)
            {
                return hit.collider.GetComponentInParent<VRRig>();
            }
            return null;
        }

        /// <summary>
        /// Проверяет, попал ли луч в локального игрока (себя)
        /// </summary>
        public static bool IsHitLocalPlayer(RaycastHit hit)
        {
            VRRig rig = GetHitPlayer(hit);
            if (rig != null)
            {
                return rig == GorillaTagger.Instance.offlineVRRig;
            }
            return false;
        }
    }
}