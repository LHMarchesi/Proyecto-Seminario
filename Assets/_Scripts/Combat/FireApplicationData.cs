using UnityEngine;

[System.Serializable]
public struct FireApplicationData
{
    [Min(0f)] public float damagePerSecond;
    [Min(0f)] public float duration;
    [Min(0.05f)] public float tickInterval;
    public int stacksToAdd;
    public int maxStacks;
    public GameObject vfxPrefab;
    public Vector3 vfxLocalOffset;

    [Header("Death explosion")]
    public bool explodeOnDeath;
    [Min(0f)] public float explosionRadius;
    [Min(0f)] public float explosionDamagePerStack;
    [Min(1)] public int maxExplosionTargets;
    public LayerMask enemyLayer;
    public GameObject explosionVFXPrefab;
    public Vector3 explosionVFXOffset;
    [Min(0f)] public float explosionVFXLifetime;
}
