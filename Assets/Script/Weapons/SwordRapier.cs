using System.Collections;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;

public class SwordRapier : WeaponSword
{
    public Transform firePoint;
    private bool isAttacking;
    void Update()
    {
        if (transform.root.TryGetComponent(out PhotonView pv) && !pv.IsMine)
            return;

        if (transform.parent != null && !isAttacking)
            RotateTowardMouse();

        if (interval > 0)
            interval -= Time.deltaTime;
    }

    public override void Attack(CharacterHandler user)
    {
        if (interval > 0) return;
        if (!user.UseMana(manaCost)) return;

        interval = cooldown;
        StartCoroutine(FireRapid(user));
    }

    private IEnumerator FireRapid(CharacterHandler user)
    {
        isAttacking = true;
        Vector3 dir = firePoint.right;
        AnimationAttack(dir);
        int shots = 3;
        while (shots-- > 0)
        {
            user.onAttack?.Invoke();
            user.photonView.RPC("RPC_FireProjectile", RpcTarget.All,
                "slash_rapier",
                firePoint.position + dir * 1f,
                dir.ToVector2(),
                2f,
                0.4f,
                user.currentMight + damage,
                2);

            yield return new WaitForSeconds(0.15f);
        }
        isAttacking = false;
    }
    private void AnimationAttack(Vector2 dir)
    {
        float thrustDistance = 1f;
        float retreatDistance = 0.5f;
        float thrustDuration = 0.1f;
        float retreatDuration = 0.05f;
        float delayBetweenThrusts = 0.05f;

        Vector3 retreatOffset = (Vector3)(-dir.normalized * retreatDistance);
        Vector3 thrustOffset = (Vector3)(dir.normalized * thrustDistance);

        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < 3; i++)
        {
            seq.Append(transform.DOMove(retreatOffset, retreatDuration).SetRelative().SetEase(Ease.InSine));
            seq.Append(transform.DOMove(thrustOffset, thrustDuration).SetRelative().SetEase(Ease.OutSine));
            if (i < 2)
                seq.AppendInterval(delayBetweenThrusts);
        }

        // Return to origin at end
        seq.Append(transform.DOLocalMove(Vector3.zero, 0.1f).SetEase(Ease.OutSine));
    }

    void RotateTowardMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float angle = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        float flip = (angle > -89f && angle < 89f) ? 1f : -1f;
        Vector3 s = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(s.x) * flip, Mathf.Abs(s.y) * flip, s.z);
    }

}
