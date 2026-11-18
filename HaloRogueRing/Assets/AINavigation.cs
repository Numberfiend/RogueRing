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
