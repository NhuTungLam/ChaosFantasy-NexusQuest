using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitBox : MonoBehaviour
{
    public Action UpdateFunc, OnDestroy;
    public float lifespan;
    public Action<CharacterHandler> HitEffect;
    public bool canPierce = false;
    private void Update()
    {
        UpdateFunc?.Invoke();
        lifespan -= Time.deltaTime;
        if (lifespan < 0)
        {
            OnDestroy?.Invoke();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            if (collision.gameObject.TryGetComponent<CharacterHandler>(out CharacterHandler handler))
            {
                HitEffect?.Invoke(handler);
            }
            if (canPierce == false)
            {
                OnDestroy?.Invoke();
            }
        }
    }
}
