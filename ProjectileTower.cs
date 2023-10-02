using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTower : MonoBehaviour
{
    public TowerProperties towerProperties;
    public GameObject weapon;
    public GameObject projectile;
    public Transform projectileOrigin;
    public Transform projectileList;
    public List<GameObject> targets;

    public float noFireZone;
    public GameObject noFireRange;
    public LayerMask mask;

    public AudioClip shoot;
    public AudioSource audioSource;

    [HideInInspector] public int damage;
    [HideInInspector] public float slowTime = 0f;
    [HideInInspector] public float fireRate;

    [HideInInspector] public float timer;

    public virtual void Start()
    {
        UpdateStats();
    }

    void Update()
    {
        TimerCountdown();
        AimAtTarget();
        ShootTarget();        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            targets.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Enemy")
        {
            targets.Remove(other.gameObject);
        }
    }

    public virtual void ShootTarget()
    {
        if (targets.Count > 0 && targets[0] == null)
        {
            targets.RemoveAt(0);
        }
        if (targets.Count > 0 && timer <= 0)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] != null)
                {
                    RaycastHit hitInfo;
                    if (Physics.Raycast(transform.position, targets[i].transform.position - transform.position, out hitInfo, mask))
                    {
                        if (hitInfo.transform.gameObject.tag == targets[i].tag)
                        {
                            float offset = Vector3.Distance(transform.position, targets[i].transform.position);

                            if (offset > noFireZone)
                            {
                                GameObject newProjectile = Instantiate(projectile, projectileOrigin.position, Quaternion.identity, projectileList);
                                newProjectile.GetComponent<Projectile>().SetTarget(targets[i].transform);
                                newProjectile.GetComponent<Projectile>().SetDamage(damage);
                                newProjectile.GetComponent<Projectile>().SetSlowTime(slowTime);
                                PlayAudio(shoot);
                                timer = fireRate;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    targets.RemoveAt(i);
                }
            }
        }
    }

    private void AimAtTarget()
    {
        if (weapon != null)
        {
            if (targets.Count > 0 && targets[0] != null)
            {
                Vector3 lookPos = targets[0].transform.position - transform.position;
                lookPos.y = 0;
                Quaternion rotation = Quaternion.LookRotation(lookPos);
                weapon.transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 1000);
            }
        }
        
    }

    protected void PlayAudio(AudioClip clip)
    {
        if (!audioSource.isPlaying)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    private void TimerCountdown()
    {
        if (timer > -100)
        {
            timer = timer - (Time.deltaTime * 1);
        }
    }

    public Vector3 GetNoFireRange()
    {
        return new Vector3(noFireZone * 2, noFireZone * 2, noFireZone * 2);
    }

    public void UpdateStats()
    {
        noFireRange.transform.localScale = new Vector3(noFireZone / 8, noFireZone / 8, noFireZone / 8);

        damage = towerProperties.GetDamage();
        fireRate = towerProperties.GetFireRate();
        slowTime = towerProperties.GetSlowTime();
    }
}
