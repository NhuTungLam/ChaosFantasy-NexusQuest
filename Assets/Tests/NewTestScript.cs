using NUnit.Framework;
using UnityEngine;

public class EnemyHandlerTests
{
    [Test]
    public void Enemy_Takes_Damage_Correctly()
    {
        var go = new GameObject();
        var handler = go.AddComponent<EnemyHandler>();

        var enemyData = ScriptableObject.CreateInstance<EnemyData>();
        enemyData.MaxHealth = 100;
        enemyData.Damage = 10;
        enemyData.Speed = 5;

        handler.Init(enemyData);

        handler.TakeDamage(30);

        Assert.AreEqual(70, handler.currentHealth);
    }

    [Test]
    public void Enemy_Dies_When_Health_Below_Zero()
    {
        var go = new GameObject();
        var handler = go.AddComponent<EnemyHandler>();

        var enemyData = ScriptableObject.CreateInstance<EnemyData>();
        enemyData.MaxHealth = 50;
        handler.Init(enemyData);

        handler.TakeDamage(100);

        Assert.LessOrEqual(handler.currentHealth, 0);
        
    }
}
