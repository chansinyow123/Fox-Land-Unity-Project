using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Plant : MonoBehaviour
{
    [SerializeField] GameObject shootPos = default;
    [SerializeField] GameObject projectile = default;
    [SerializeField] float shootCooldown = 1f;
    Animator animator;
    float shootAnimationTime;
    IEnumerator shootCoroutine;
    GameObject cloneProjectile;

    void Start()
    {
        animator = GetComponent<Animator>();

        SetShootAnimationTime();
    }

    private void SetShootAnimationTime()
    {
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;    //Get Animator controller
        for (int i = 0; i < ac.animationClips.Length; i++)                 //For all animations
        {
            if (ac.animationClips[i].name == "Plant Shoot")        //If it has the same name as your clip
            {
                shootAnimationTime = ac.animationClips[i].length;
            }
        }
    }

    public void Shooting()
    {
        shootCoroutine = ShootAnimation();
        StartCoroutine(shootCoroutine);
    }

    private IEnumerator ShootAnimation()
    {
        while (true)
        {
            animator.SetTrigger("Shoot");
            yield return new WaitForSeconds(shootAnimationTime + shootCooldown);
        }
    }

    //used in animation event
    private void ShootProjectile()
    {
        cloneProjectile = Instantiate(projectile, shootPos.transform.position, Quaternion.identity) as GameObject;
        cloneProjectile.transform.localScale = transform.localScale;
    }

    public void StopShooting()
    {
        StopCoroutine(shootCoroutine);
    }
}
