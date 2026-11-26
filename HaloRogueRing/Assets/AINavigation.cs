using UnityEngine;
using UnityEngine.AI;

public static class AINavigation
{
    private static NavMeshPath scratchPath; 

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        scratchPath = null;
    }

    public static bool HasArrived(NavMeshAgent agent)
    {
        if (agent.pathPending) return false;
        if (agent.pathStatus == NavMeshPathStatus.PathInvalid) return true;
        return agent.remainingDistance <= agent.stoppingDistance + 0.1f;
    }

    public static void FaceTowards(Transform self, Vector3 worldPoint, float degreesPerSecond)
    {
        Vector3 dir = worldPoint - self.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        self.rotation = Quaternion.RotateTowards(
            self.rotation,
            Quaternion.LookRotation(dir),
            degreesPerSecond * Time.deltaTime);
    }

    
    public static bool PathPassesNear(NavMeshPath path, Vector3 point, float minDistance)
    {
        Vector3[] corners = path.corners;
        float minSqr = minDistance * minDistance;

        for (int i = 0; i < corners.Length - 1; i++)
        {
            Vector3 a = corners[i];
            Vector3 ab = corners[i + 1] - a;
            float lengthSqr = ab.sqrMagnitude;
            float t = lengthSqr > 0f ? Mathf.Clamp01(Vector3.Dot(point - a, ab) / lengthSqr) : 0f;
            Vector3 closest = a + ab * t;
            if ((closest - point).sqrMagnitude < minSqr)
                return true;
        }
        return false;
    }

    public static bool IsHiddenFrom(
    Vector3 position, float eyeHeight, Transform threat, float threatHeight,
    LayerMask mask, Transform ignoreRoot)
    {
        Vector3 eye = position + Vector3.up * eyeHeight;
        Vector3 target = threat.position + Vector3.up * threatHeight;

        if (Physics.Linecast(eye, target, out RaycastHit hit, mask, QueryTriggerInteraction.Ignore))
        {
            Transform root = hit.transform.root;
            return root != threat.root && (ignoreRoot == null || root != ignoreRoot.root);
        }
        return false;
    }

    public static bool TryPickCover(
    Vector3 origin, Transform threat, float searchRadius, int candidates,
    float eyeHeight, float threatHeight, LayerMask mask, Transform ignoreRoot,
    float minPassDistance, float minThreatDistance, out Vector3 result)
    {
        result = Vector3.zero;
        if (scratchPath == null) scratchPath = new NavMeshPath();

        bool found = false;
        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < candidates; i++)
        {
            float angle = Random.Range(0f, 360f);
            float radius = Random.Range(3f, searchRadius);
            Vector3 candidate = origin + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                continue;

            if (Vector3.Distance(hit.position, threat.position) < minThreatDistance)
                continue;

            if (!IsHiddenFrom(hit.position, eyeHeight, threat, threatHeight, mask, ignoreRoot))
                continue;

            if (!NavMesh.CalculatePath(origin, hit.position, NavMesh.AllAreas, scratchPath) ||
                scratchPath.status != NavMeshPathStatus.PathComplete)
                continue;

            if (PathPassesNear(scratchPath, threat.position, minPassDistance))
                continue;

            float score = Vector3.Distance(hit.position, threat.position)
                          - Vector3.Distance(origin, hit.position) * 0.5f;
            if (score > bestScore)
            {
                bestScore = score;
                result = hit.position;
                found = true;
            }
        }
        return found;
    }

    public static bool TryPickPointAwayFrom(
        Vector3 origin, Vector3 threat, float distance,
        int candidates, float minPassDistance, out Vector3 result)
    {
        result = Vector3.zero;
        if (scratchPath == null) scratchPath = new NavMeshPath();

        Vector3 away = origin - threat;
        away.y = 0f;
        if (away.sqrMagnitude < 0.0001f)
        {
            Vector2 r = Random.insideUnitCircle.normalized;
            away = new Vector3(r.x, 0f, r.y);
        }
        away.Normalize();

        bool found = false;
        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < candidates; i++)
        {
            Vector3 dir = Quaternion.Euler(0f, Random.Range(-90f, 90f), 0f) * away;
            Vector3 candidate = origin + dir * Random.Range(distance * 0.5f, distance);

            
            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                continue;

            if (!NavMesh.CalculatePath(origin, hit.position, NavMesh.AllAreas, scratchPath) ||
                scratchPath.status != NavMeshPathStatus.PathComplete)
                continue;

            if (PathPassesNear(scratchPath, threat, minPassDistance))
                continue;

            float score = Vector3.Distance(hit.position, threat);
            if (score > bestScore)
            {
                bestScore = score;
                result = hit.position;
                found = true;
            }
        }
        return found;
    }
}
