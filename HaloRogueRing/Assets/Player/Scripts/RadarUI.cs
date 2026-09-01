using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using NUnit.Framework.Internal.Builders;
public class RadarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MotionTrackerV2 motionTracker;
    [SerializeField] private Transform player;
    [SerializeField] private RectTransform blipContainer;
    [SerializeField] private GameObject blipPrefab;

    [Header("Radar")]
    [SerializeField] private float radarRadius = 90f;

    private Dictionary<Transform, RectTransform> blips = new Dictionary<Transform, RectTransform>();

    private void FixedUpdate()
    {
        UpdateBlips();
    }

    private void UpdateBlips()
    {
        foreach (Transform enemy in motionTracker.trackedEnemies)
        {
            if (!blips.ContainsKey(enemy))
            {
                GameObject newBlip = Instantiate(blipPrefab,blipContainer);
                blips.Add(enemy, newBlip.GetComponent<RectTransform>());

            }
        }

        List<Transform> removeList = new();
        foreach (var pair in blips)
        {
            if (pair.Key == null ||
                !motionTracker.trackedEnemies.Contains(pair.Key))
            {
                Destroy(pair.Value.gameObject);
                removeList.Add(pair.Key);
            }
        }

        foreach (Transform enemy in removeList)
        {
            blips.Remove(enemy);
        }

        //update positions
        foreach (var pair in blips)
        {
            Transform enemy = pair.Key;
            RectTransform blip = pair.Value;

            if (enemy == null)
                continue;

            Vector3 worldOffset = enemy.position - player.position;

            worldOffset.y = 0f;

            Vector3 localOffset =
                Quaternion.Inverse(player.rotation) *
                worldOffset;

            Vector2 radarPos =
                new Vector2(
                    localOffset.x,
                    localOffset.z
                );

            radarPos /= motionTracker.DetectionRadius;
            radarPos *= radarRadius;

            if (radarPos.magnitude > radarRadius)
            {
                radarPos =
                    radarPos.normalized *
                    radarRadius;
            }

            blip.anchoredPosition = radarPos;
        }
    }
}
