using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;

public class WeaponSwordBlue : WeaponSword
{
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
                AnimationSwingFromRight();
                currentSlashIndex = 1;
                user.photonView.RPC("RPC_FireProjectile", RpcTarget.All,
                "slash_blue",
                transform.position,
                Vector2.zero,
                0f,
                1f,
                user.currentMight + damage,
                isLeft? 1 : 0);
                break;
            case 1:
                AnimationSwingFromLeft();
                currentSlashIndex = 0;
                user.photonView.RPC("RPC_FireProjectile", RpcTarget.All,
                "slash_blue",
                transform.position,
                Vector2.zero,
                0f,
                1f,
                user.currentMight + damage,
                isLeft ? 0 : 1);
                break;
        }
    }
    private void AnimationSwingFromRight()
    {
        KillCurrentAnimation();
        transform.localEulerAngles = new Vector3(0, 0, -85f);
        currentAnimSequence = DOTween.Sequence();
        currentAnimSequence.Append(transform.DOLocalRotate(new Vector3(0, 0, -120f), 0.2f))
            .Append(transform.DOLocalRotate(new Vector3(0, 0, 235f), 0.3f, RotateMode.LocalAxisAdd).SetEase(Ease.InExpo));
    }
    private void AnimationSwingFromLeft()
    {
        KillCurrentAnimation();
        transform.localEulerAngles = new Vector3(0, 0, 115f);
        currentAnimSequence = DOTween.Sequence();
        currentAnimSequence.Append(transform.DOLocalRotate(new Vector3(0, 0, 150f), 0.2f))
            .Append(transform.DOLocalRotate(new Vector3(0, 0, -235f), 0.3f, RotateMode.LocalAxisAdd).SetEase(Ease.InExpo));
    }
    protected override void AnimationReturnIdle(float speed = 1)
    {
        currentAnimSequence = DOTween.Sequence();
        currentAnimSequence.Append(transform.DOLocalMove(Vector3.zero, 0.5f * speed))
            .Join(transform.DOLocalRotate(new Vector3(0, 0, -85), 0.5f * speed, RotateMode.FastBeyond360));
    }
}
