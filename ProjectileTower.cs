using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTower : MonoBehaviour
{
    public TowerProperties towerProperties; //used to import stats like damage, firerate, etc.
    public GameObject weapon; //get the object that rotates towards the target if available
    public GameObject projectile; //Projectile the tower shoots
    public Transform projectileOrigin; //Spawnpoint of the projectile
    public Transform projectileList; //List with the shot projectiles to prevent clutter
    public List<GameObject> targets; //List with all available targets

    public float noFireZone; //Zone around the tower it can't target
    public GameObject noFireRange; //Visual object for the noFireRange
    public LayerMask mask; //Mask to set targets and to check if it can be hit, ignores uneccesary gameOjbects

    public AudioClip shoot; //Audio for shooting
    public AudioSource audioSource;

    [HideInInspector] public int damage; //The damage of the projectile
    [HideInInspector] public float slowTime = 0f; //The amount of time it slows down targets hit
    [HideInInspector] public float fireRate; //How fast the tower can shoot

    [HideInInspector] public float timer; //Timer for the fireRate

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

    private void OnTriggerEnter(Collider other) //Adds targets to the target list
    {
        if (other.tag == "Enemy")
        {
            targets.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other) //Removes targets to the target list
    {
        if (other.tag == "Enemy")
        {
            targets.Remove(other.gameObject);
        }
    }

    public virtual void ShootTarget() //Shoots a raycast to the target, if it is not obstructed by terrain it fires a projectile
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

    private void AimAtTarget() //Rotates the weapon to the target if it has one
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

    protected void PlayAudio(AudioClip clip) //Plays the audio
    {
        if (!audioSource.isPlaying)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    private void TimerCountdown() //Countdown timer for fireRate
    {
        if (timer > -100) //Stops counting after 100 ticks to prevent memory overflow
        {
            timer = timer - (Time.deltaTime * 1);
        }
    }

    public Vector3 GetNoFireRange() //Gets the range of the noFireZone for the visual
    {
        return new Vector3(noFireZone * 2, noFireZone * 2, noFireZone * 2);
    }

    public void UpdateStats() //Updates the stats of the tower. For example: on start and after an upgrade
    {
        noFireRange.transform.localScale = new Vector3(noFireZone / 8, noFireZone / 8, noFireZone / 8);

        damage = towerProperties.GetDamage();
        fireRate = towerProperties.GetFireRate();
        slowTime = towerProperties.GetSlowTime();
    }
}
