using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Movement Stats")]
    public float moveSpeed = 2f;
    public float chaseSpeed = 3.5f;
    public float chargeSpeed = 6f;

    [Header("Combat Stats")]
    public int maxHealth = 3;
    public int contactDamage = 1;
    public float visionRange = 6f;
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    public float windUpTime = 0.4f;
    public float losePlayerDuration = 1.5f;
}