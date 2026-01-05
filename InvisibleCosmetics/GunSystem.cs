using System;
using UnityEngine;
using GorillaTag;
using GorillaLocomotion;

namespace CosmeticsSwitch.GunSystem
{
    public static class SimpleGun
    {
        public static bool isShooting = false;
        public static GameObject GunPointer = null;
        public static LineRenderer GunLine = null;

        public static float rayLength = 50f;
        public static float pointerSize = 0.1f;
        public static float lineWidth = 0.01f;
        public static Color lineColor = Color.red;
        public static Color pointerColor = Color.red;

        public static (RaycastHit hit, GameObject pointer) RenderGun()
        {
            Transform rightHand = GorillaTagger.Instance.rightHandTransform;
            Vector3 startPosition = rightHand.position;
            Vector3 direction = rightHand.forward;
            RaycastHit hit;
            bool hasHit = Physics.Raycast(startPosition, direction, out hit, rayLength, GetLayerMask());
            Vector3 endPosition = hasHit ? hit.point : startPosition + direction * rayLength;

            CreatePointer(endPosition);
            CreateLine(startPosition, endPosition);

            return (hit, GunPointer);
        }

        public static void HideGun()
        {
            if (GunPointer != null) GunPointer.SetActive(false);
            if (GunLine != null) GunLine.gameObject.SetActive(false);
        }

        public static void Cleanup()
        {
            if (GunPointer != null) GameObject.Destroy(GunPointer);
            if (GunLine != null) GameObject.Destroy(GunLine.gameObject);

            GunPointer = null;
            GunLine = null;
        }

        private static void CreatePointer(Vector3 position)
        {
            if (GunPointer == null)
            {
                GunPointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                GameObject.Destroy(GunPointer.GetComponent<Collider>());
                Renderer renderer = GunPointer.GetComponent<Renderer>();
                renderer.material = new Material(Shader.Find("GUI/Text Shader"));
                renderer.material.color = pointerColor;
            }

            GunPointer.SetActive(true);
            GunPointer.transform.position = position;
            GunPointer.transform.localScale = Vector3.one * pointerSize;
        }

        private static void CreateLine(Vector3 start, Vector3 end)
        {
            if (GunLine == null)
            {
                GameObject lineObj = new GameObject("GunLine");
                GunLine = lineObj.AddComponent<LineRenderer>();

                GunLine.material = new Material(Shader.Find("GUI/Text Shader"));
                GunLine.startColor = lineColor;
                GunLine.endColor = lineColor;
                GunLine.startWidth = lineWidth;
                GunLine.endWidth = lineWidth;
                GunLine.useWorldSpace = true;
            }

            GunLine.gameObject.SetActive(true);
            GunLine.positionCount = 2;
            GunLine.SetPosition(0, start);
            GunLine.SetPosition(1, end);
        }

        private static int GetLayerMask()
        {
            int layerMask = 0;

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
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

        public static VRRig GetHitPlayer(RaycastHit hit)
        {
            if (hit.collider != null)
            {
                return hit.collider.GetComponentInParent<VRRig>();
            }
            return null;
        }

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
