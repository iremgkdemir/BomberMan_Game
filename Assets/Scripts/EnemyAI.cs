using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : NetworkBehaviour
{
    public playerMovement parent;

    NavMeshAgent agent;

    Vector3 OyuncuPosizyonu;
    Vector3 GezintiPosizyonu;

    public LayerMask Zemin, Oyuncumu;

    public float health=20;

    public Vector3 yuruyusNoktasi;
    bool yuruyusNoktasiBelirlendi;
    public float yuruyusNoktasiMesafesi;

    public float saldirilarArasiSure;
    bool saldirildi;
    public GameObject projectile;

    public float gorusMesafesi, atakMesafesi;
    public bool OyuncuGorusMesafesinde, OyuncuSaldiriMesafesinde;

    private Animator animator;

    void Start()
    {
        if (global.MultiPlayermi > 1) if (!IsOwner) { return; }
        
        agent = GetComponent<NavMeshAgent>();
        OyuncuPosizyonu = transform.position;
        GezintiPosizyonu = GezintiPozisyonuBelirle();
        animator = GetComponent<Animator>();

        agent.speed = 2.0f;
        agent.acceleration = 4.0f;
        agent.angularSpeed = 120.0f;
    }

    void Update()
    {
        if (global.MultiPlayermi > 1)
            if (!IsOwner) { return; }

        if (global.MultiPlayermi > 1) if (!IsServer) { return; }
        
        if (global.PlayMod.Value == global.Mods.Waiting ||
            global.PlayMod.Value == global.Mods.TimeOver ||
            global.PlayMod.Value == global.Mods.GameOver ||
            global.PlayMod.Value == global.Mods.Ending ||
            global.PlayMod.Value == global.Mods.StartNewGame ||
            global.PlayMod.Value == global.Mods.WaitingForPlayer) return;

        OyuncuGorusMesafesinde = Physics.CheckSphere(transform.position, gorusMesafesi, Oyuncumu);
        OyuncuSaldiriMesafesinde = Physics.CheckSphere(transform.position, atakMesafesi, Oyuncumu);

        if (!OyuncuGorusMesafesinde && !OyuncuSaldiriMesafesinde) Geziyor();
        if (OyuncuGorusMesafesinde && !OyuncuSaldiriMesafesinde) OyuncuyaGit();
        if (OyuncuSaldiriMesafesinde && OyuncuGorusMesafesinde) OyuncuyaSaldir();

        //animator.SetBool("idle", OyuncuGorusMesafesinde && OyuncuSaldiriMesafesinde);
        animator.SetBool("isWalking", !OyuncuSaldiriMesafesinde && !OyuncuGorusMesafesinde);
        animator.SetBool("isAttacking", OyuncuSaldiriMesafesinde && OyuncuGorusMesafesinde);
    }

    private Vector3 RastgeleYonBelirle()
    {
        return new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(1f, 1f)).normalized;
    }

    private Vector3 GezintiPozisyonuBelirle()
    {
        return OyuncuPosizyonu + RastgeleYonBelirle() * Random.Range(10f, 10f);
    }


    public void Geziyor()
    {
        if (!yuruyusNoktasiBelirlendi) GezintiPozisyonuBul();

        if (yuruyusNoktasiBelirlendi)
            agent.SetDestination(yuruyusNoktasi);

        Vector3 distanceToWalkPoint = transform.position - yuruyusNoktasi;

        if (distanceToWalkPoint.magnitude < 1f)
            yuruyusNoktasiBelirlendi = false;

        animator.SetBool("isWalking", true);
    }

    private void GezintiPozisyonuBul()
    {
        float randomZ = Random.Range(-yuruyusNoktasiMesafesi, yuruyusNoktasiMesafesi);
        float randomX = Random.Range(-yuruyusNoktasiMesafesi, yuruyusNoktasiMesafesi);

        yuruyusNoktasi = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(yuruyusNoktasi, -transform.up, 2f, Zemin))
            yuruyusNoktasiBelirlendi = true;
    }

    private void OyuncuyaGit()
    {
        agent.SetDestination(global.Player1.transform.position);
        animator.SetBool("isWalking", true);
    }

    private void OyuncuyaSaldir()
    {
        agent.SetDestination(global.Player1.transform.position);
        transform.LookAt(global.Player1.transform);

        if (!saldirildi)
        {
            Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
            rb.AddForce(transform.up * 8f, ForceMode.Impulse);

            saldirildi = true;
            Invoke(nameof(ResetAttack), saldirilarArasiSure);
        }

        animator.SetBool("isAttacking", true);
    }

    private void ResetAttack()
    {
        saldirildi = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        animator.SetTrigger("takeDamage");

        if (health <= 0)
        {
            animator.SetTrigger("die");
            Invoke(nameof(DestroyEnemy), 0.5f);
        }
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }
        private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            global.audioManager.PlaySFX(global.audioManager.auPlayerHi);
            collision.gameObject.GetComponent<playerMovement>().die();
            animator.SetTrigger("die");
            Invoke(nameof(DestroyEnemy), 0.5f);
        }
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Rock"))
        {
            GezintiPosizyonu = GezintiPozisyonuBelirle();
        }
        if (collision.gameObject.CompareTag("Bomb"))
        {
            TakeDamage(10);  // Or whatever logic you want for bomb damage
        }
    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, atakMesafesi);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, gorusMesafesi);
    }
}