using NUnit.Framework;
using UnityEngine;

public class CharacterHandlerTests
{
    [Test]
    public void TakeDamage_Should_ReduceHealth()
    {
        var go = new GameObject();
        var handler = go.AddComponent<CharacterHandler>();

        var data = ScriptableObject.CreateInstance<CharacterData>();
        data.MaxHealth = 100;
        data.MoveSpeed = 3f;
        data.ProjectileSpeed = 1f;
        data.Magnet = 2f;

        handler.characterData = data;

        handler.Awake(); // g?i sau khi characterData ?ã ???c set

        handler.TakeDamage(30);

        Assert.AreEqual(70, handler.currentHealth);
    }

}
