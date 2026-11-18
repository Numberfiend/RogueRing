using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public static class FiringPositions
{
    public struct Query
    {
        public Vector3 Requester;        
        public Transform Target;         
        public float TargetHeight;       
        public float EyeHeight;          
        public float MinRange;
        public float MaxRange;
        public int Candidates;
        public float Separation;         
        public float MinPassDistance;    
        public LayerMask LineOfSightMask;
    }

    private static readonly Dictionary<Component, Vector3> claims = new Dictionary<Component, Vector3>();
    private static NavMeshPath path; 
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        claims.Clear();
        path = null;
    }

    public static void Release(Component owner)
    {
        claims.Remove(owner);
    }

    public static bool TryClaim(Component owner, in Query q, out Vector3 result)
    {
        result = Vector3.zero;
        if (q.Target == null) return false;
        if (path == null) path = new NavMeshPath();

        Vector3 targetPoint = q.Target.position + Vector3.up * q.TargetHeight;

        bool found = false;
        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < q.Candidates; i++)
        {
            
            float angle = Random.Range(0f, 360f);
            float radius = Random.Range(q.MinRange, q.MaxRange);
            Vector3 raw = q.Target.position + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius;

            if (!NavMesh.SamplePosition(raw, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                continue;
            Vector3 point = hit.position;

            
            Vector3 flat = point - q.Target.position;
            flat.y = 0f;
            float distance = flat.magnitude;
            if (distance < q.MinRange * 0.9f || distance > q.MaxRange * 1.1f)
                continue;

            
            float nearestClaim = float.MaxValue;
            bool tooClose = false;
            foreach (KeyValuePair<Component, Vector3> claim in claims)
            {
                if (claim.Key == owner) continue;

                Vector3 delta = claim.Value - point;
                delta.y = 0f;
                float d = delta.magnitude;
                if (d < q.Separation) { tooClose = true; break; }
                if (d < nearestClaim) nearestClaim = d;
            }
            if (tooClose) continue;

            
            Vector3 eye = point + Vector3.up * q.EyeHeight;
            if (Physics.Linecast(eye, targetPoint, out RaycastHit los, q.LineOfSightMask,
                    QueryTriggerInteraction.Ignore) &&
                los.transform.root != q.Target.root)
                continue;

            
            if (!NavMesh.CalculatePath(q.Requester, point, NavMesh.AllAreas, path) ||
                path.status != NavMeshPathStatus.PathComplete)
                continue;
            if (AINavigation.PathPassesNear(path, q.Target.position, q.MinPassDistance))
                continue;

            
            float travel = Vector3.Distance(q.Requester, point);
            float score = -travel
                          + Mathf.Min(nearestClaim, 10f) * 0.5f
                          + Random.Range(0f, 2f);

            if (score > bestScore)
            {
                bestScore = score;
                result = point;
                found = true;
            }
        }

        if (found)
            claims[owner] = result;

        return found;
    }
}
