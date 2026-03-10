using UnityEngine;
using UnityEngine.AI;

public class EnemyAIBase : MonoBehaviour
{
    #region General Variables
    [Header("AI Configuration")]
    [SerializeField] NavMeshAgent agent; //Ref al cerebro NavMesh del objeto
    [SerializeField] Transform target; //Ref a la posición del player a perseguir
    [SerializeField] LayerMask targetLayer; // Define la capa de la target (Detección)
    [SerializeField] LayerMask groundLayer; // Define la capa del suelo (Definir puntos navegables)

    [Header("Patroling Stats")]
    [SerializeField] float walkPointRange = 8f; //Radio Máximo de margen espacial para buscar punto a perseguir
    Vector3 walkPoint; // Posición del punto a perseguir
    bool walkPointSet; // Si es falso, busca punto. Si es verdadero, no puede buscar punto

    [Header("Attacking Stats")]
    [SerializeField] float timeBetweenAttacks = 1f; //Tiempo de espera entre ataques
    [SerializeField] GameObject projectile; //Ref al prefab del proyectil a disparar
    [SerializeField] Transform shootPoint; //Punto inicial del disparo
    [SerializeField] float shootSpeedY; //Potencia del disparo Vertical (Solo Catapulta)
    [SerializeField] float shootSpeedZ = 10f; //Potencia del disparo hacia delante (Siempre está)
    bool alreadyAttacked; //Se pregunta si estamos atacando para no stakear ataques

    [Header("States & Detection Areas")]
    [SerializeField] float sightRange = 8f; //Radio de detección del player para perseguirlo
    [SerializeField] float attackRange = 2f; //Radio de detección del player para perseguirlo
    [SerializeField] bool targetInSightRange; //Determina si entra en el estado PERSEGUIR
    [SerializeField] bool targetInAttackRange; //Determina si entra en el estado ATACAR

    [Header("Stuck Dection")]
    [SerializeField] float stuckCheckTime = 2f; //Tiempo que tarda el agente en detectar que está stuck
    [SerializeField] float stuckThreshold = 0.1f; //Margen de detección de stuck
    [SerializeField] float maxStuckDuration = 3f; //Tiempo maximo de estar stuck

    float stuckTimer; // Reloj que cuenta el tiempo de estar stuck
    float lastCheckTime; //Tiempo de checkeo previo a estar stuck
    Vector3 lastPosition; //Posición del ultimo walkpoint perseguido

    #endregion


    private void Awake()
    {
        target = GameObject.Find("Player").transform; 
        agent = GetComponent<NavMeshAgent>();
        lastPosition = transform.position;
        lastCheckTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        EnemyStateUpdater();
        CheckIfStuck();
    }

    void EnemyStateUpdater()
    {
        // Acción que se encarga de los estados de la IA
        //Esfera de detección fisica
        Collider[] hits = Physics.OverlapSphere(transform.position, sightRange, targetLayer);
        targetInSightRange = hits.Length > 0;
        //Si está persiguiendo, calcula la distancia para saber si puede atacar o no
        if (targetInSightRange)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            targetInAttackRange = distance <= attackRange;

        }

        //Logica de los cambios de estado
        if (!targetInSightRange && !targetInAttackRange) Patroling(); 
        else if (targetInSightRange && !targetInAttackRange) ChaseTarget();
        else if (targetInSightRange && targetInAttackRange) AttackTarget();

    }

    void Patroling()
    { 
        //Define que el objeto patrulle y genere objetos de patrulla random
        //1 - Revisa si hay puntos a patrullar
        if (!walkPointSet)
        {
            //Si no hay Walkpoint, busca uno nuevo
            SearchWalkPoint();
        }
        else agent.SetDestination(walkPoint); //Si hay walkpoint, se dirige a él

        //2 - Una vez ha llegado al punto hay que decirle al sistema que genere uno nuevo
        if ((transform.position - walkPoint).sqrMagnitude < 1f)
        {
            walkPointSet = false;
        }

    }

    void SearchWalkPoint()
    {
        //Acción que busca un punto de patrulla si no lo hay
        int attempts = 0; //Número de intentos para encontrar un punto nuevo
        const int maxAttempts = 5;

        while (!walkPointSet && attempts < maxAttempts)
        {
            attempts++;
            Vector3 randomPoint = transform.position + new Vector3(Random.Range(-walkPointRange, walkPointRange), 0, Random.Range(-walkPointRange, walkPointRange));

            //Chequear si el punto esta en un lugar en el que haya NuvfMesh Surface
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                walkPoint = hit.position; //Detecta el Vector3 Random a perseguir
                if (Physics.Raycast(walkPoint, -transform.up, 2f, groundLayer))
                {
                    walkPointSet = true; //Tenemos punto y el agente va hacia él
                }
                
            }

        }
    }

    void ChaseTarget()
    {
        // Le dice al agente que persiga al target
        agent.SetDestination(target.position);

    }

    void AttackTarget()
    {
        //Acción que determina el ataque al objetivo

        //1 - Detener el movimiento
        agent.SetDestination(transform.position);

        //2 - Rotación suavizada para mirar al target
        Vector3 direction = (target.position - transform.position).normalized; 
        //Condiciónal que revisa si agente y target NO se están mirando
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction); 
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, agent.angularSpeed * Time.deltaTime); 
        }

        //3 - Definir el ataque en si
        //Solo atacará si no está atacando
        if (!alreadyAttacked)
        {
            Rigidbody rb = Instantiate(projectile, shootPoint.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * shootSpeedZ, ForceMode.Impulse);
            alreadyAttacked = true;
            Invoke (nameof(ResetAttack), timeBetweenAttacks); 
        }
    }

    void ResetAttack()
    {
        //Acción que reinicia el ataque para poder volver a atacar
        alreadyAttacked = false;
    }

    void CheckIfStuck()
    {
        //Acción que revisa si el agente está atrapado
        if (Time.time - lastCheckTime > stuckCheckTime)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            if (distanceMoved < stuckThreshold && agent.hasPath)
            {
                stuckTimer += stuckCheckTime;
            }
            else 
            {
                stuckTimer = 0f;
            }
            if (stuckTimer > maxStuckDuration)
            {
                walkPointSet = false; 
                agent.ResetPath();
                stuckTimer = 0f; 
            }

            lastPosition = transform.position;
            lastCheckTime = Time.time;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying) return; //Solo se ejecutan los gizmos en editor de Unity

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }

}
