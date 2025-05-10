using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitBox : MonoBehaviour
{
    public Action HitEffect, UpdateFunc, OnDestroy;
    public float lifespan;

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
            HitEffect?.Invoke();
        }
    }
}
