using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health System Management")]
    [SerializeField] int maxHealth = 100; //Cantidad máxima de vida del enemigo
    [SerializeField] int health; //Vida actual del enemigo

    [Header("Feedback Configuration")]
    [SerializeField] Material damagedMat; //Material feedback de daño
    [SerializeField] GameObject deathVfx; //Efecto de partículas de muerte
    [SerializeField] MeshRenderer enemyRend; //Almacén del material base del enemigo
    Material baseMat; //Almacen del material base del enemigo

    private void Awake()
    {
        health = maxHealth; //La vida se pone en el máximo
        baseMat = enemyRend.material; //Se referencia el material base
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            health = 0; //La vida no puede ser menor a 0    
            deathVfx.SetActive(true); 
            deathVfx.transform.position = transform.position; 
            gameObject.SetActive(false); //Si la vida llega a 0, el enemigo se apaga (muere)
        }
        
    }
    public void TakeDamage(int damage)
    {
        health -= damage; //Quitarle una cantidad determinada de vida al enemigo
        enemyRend.material = damagedMat; //Se cambia al material de feedback de daño
        Invoke(nameof(ResetEnemyMaterial), 0.1f); //Espera de tiempo que permite ver el parpadeo
    }

    void ResetEnemyMaterial()
    {
        //Devuelve el material del enemigo a su materialoriginal
        enemyRend.material = baseMat;
    }
}
