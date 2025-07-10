using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;
using System;

public class WeaponSword : WeaponBase
{
    public float returnToIdleInterval;
    public Action onAttack;

    protected float returnToIdleTimer;
    protected int currentSlashIndex = 0;
    protected Sequence currentAnimSequence;

    void Update()
    {
        if (transform.root.TryGetComponent(out PhotonView pv) && !pv.IsMine)
            return;

        if (interval > 0)
            interval -= Time.deltaTime;

        if (returnToIdleTimer < returnToIdleInterval)
            returnToIdleTimer += Time.deltaTime;
        else if (currentSlashIndex != 0)
        {
            AnimationReturnIdle();
            currentSlashIndex = 0;
        }

    }
    public override void Interact(CharacterHandler user = null)
    {
        base.Interact(user);
        AnimationReturnIdle();
    }
    public override void Attack(CharacterHandler user)
    {
        if (interval > 0) return;
        interval = cooldown;
        returnToIdleTimer = 0f;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Vector2 direction = (mouseWorld - user.transform.position).normalized;
        bool isLeft = mouseWorld.x < user.transform.position.x;

        switch (currentSlashIndex)
        {
            case 0:
                AnimationSlashDownward();
                currentSlashIndex = 1;
                user.photonView.RPC("RPC_FireProjectile", RpcTarget.All,
                "slash_white_down",
                transform.position + (isLeft ? -1 : 1) * new Vector3(1.7f, 0),
                Vector2.zero,
                0f,
                0.4f,
                user.currentMight + damage,
                isLeft? 1 : 0);
                break;
            case 1:
                AnimationSlashUpward();
                currentSlashIndex = 0;
                user.photonView.RPC("RPC_FireProjectile", RpcTarget.All,
                "slash_white_up",
                transform.position + (isLeft ? -1 : 1) * new Vector3(0.8f, 0),
                Vector2.zero,
                0f,
                0.4f,
                user.currentMight + damage,
                isLeft ? 1 : 0);
                break;
        }
    }

    protected void AnimationSlashDownward(float speed = 1f)
    {
        KillCurrentAnimation();
        transform.localEulerAngles = Vector3.zero;
        currentAnimSequence = DOTween.Sequence();
        currentAnimSequence.Append(transform.DOLocalRotate(new Vector3(0, 0, 50f), 0.08f * speed))
            .Append(transform.DOLocalRotate(new Vector3(0, 0, -120f), 0.12f * speed).SetEase(Ease.InExpo));
    }
    protected void AnimationSlashUpward(float speed = 1f) 
    {
        KillCurrentAnimation();
        transform.localEulerAngles = new Vector3(0, 0, -120f);
        currentAnimSequence = DOTween.Sequence();
        currentAnimSequence.Append(transform.DOLocalRotate(new Vector3(0, 0, -170f), 0.08f * speed))
            .Append(transform.DOLocalRotate(new Vector3(0, 0, 0f), 0.12f * speed).SetEase(Ease.InExpo));
    }
    protected virtual void AnimationReturnIdle(float speed = 1f)
    {
        currentAnimSequence = DOTween.Sequence();
        currentAnimSequence.Append(transform.DOLocalMove(Vector3.zero, 0.2f * speed))
            .Join(transform.DOLocalRotate(Vector3.zero, 0.2f * speed));
    }
    protected void KillCurrentAnimation()
    {
        if (currentAnimSequence != null && currentAnimSequence.IsActive())
            currentAnimSequence.Kill(); 
    }



}
