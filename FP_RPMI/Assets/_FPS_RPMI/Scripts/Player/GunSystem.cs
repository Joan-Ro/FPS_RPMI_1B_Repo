using UnityEngine;
using UnityEngine.InputSystem;

public class GunSistem : MonoBehaviour
{
    #region General Variables
    [Header("General References")]
    [SerializeField] Camera fpsCam; //Ref si disparamos desde el centro de la cam
    [SerializeField] Transform shootPoint; //Ref si disparamos desde la punta del cañón
    [SerializeField] LayerMask impactLayer; //Layer con la que interactua el raycast
    RaycastHit hit; //Variable para almacenar la información de los objetos con los que puede chocar el raycast

    [Header("Weapon Parameters")]
    [SerializeField] int damage = 10; //Daño que inflige el arma por bala
    [SerializeField] float range = 100f; //Alcance del arma, distancia máxima a la que puede llegar el raycast
    [SerializeField] float spread = 0; //Radio de dispersión del arma al disparar
    [SerializeField] float shootingCooldown = 0.2f; //Tiempo de espera entre disparos
    [SerializeField] float reloadTime = 1.5f; //Tiempo que tarda en recargar el arma
    [SerializeField] bool allowButtonHold = false; //Permite disparar manteniendo el botón presionado

    [Header("Bullet Managment")]
    [SerializeField] int ammoSize = 30; //Cantidad máxima de balas en el cargador
    [SerializeField] int bulletsPerTap = 1; //Cantidad de balas que se disparan por cada vez que se aprieta el botón de disparo
    int bulletsLeft; //Cantidad de balas restantes en el cargador

    [Header("Feedback References")]
    [SerializeField] GameObject impactEffect; //Ref al VFX de impacto de Bala

    [Header("Dev - Gun State Bools")]
    [SerializeField] bool shooting; //Bool para controlar si el arma está disparando o no
    [SerializeField] bool canShoot; //Bool para controlar si el arma puede disparar o no (controla el cooldown entre disparos)
    [SerializeField] bool reloading; //Bool para controlar si el arma está recargando o no

    #endregion

    private void Awake()
    {
        bulletsLeft = ammoSize; //Al iniciar el juego, el cargador del arma está lleno
        canShoot = true; //Al iniciar el juego, el arma puede disparar
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Shoot()
    {
        //  ESTE ES EL  METODO MAS IMNPORTANTE
        // SE DEFINE DISPARO POR RYCAST -> UTILIZABLE POR CUALQUIER MECÁNICA

        //Almacenar la dirección de disparo y modificarla en el caso de haber dispersión 
        Vector3 direction = fpsCam.transform.forward; //Dirección de disparo inicial, desde el centro de la cámara hacia adelante

        //Añadir dispersión aleatoria según el valor de spread
        direction.x += Random.Range(-spread, spread); 
        direction.y += Random.Range(-spread, spread);

        //DECLARACIÓN DEL RAYCAST
        //Physics.Raycast(Origen del rayo, dirección, almacen de la info del impacto, longitud del rayo, layer con la que impacta el rayo)
        if (Physics.Raycast(fpsCam.transform.position, direction, out hit, range, impactLayer))
        {
            //PODEMOS CODEAR TODOS LOS EFECTOD QUE QUIERO PARA LA INTRACCIÓN
            Debug.Log(hit.collider.name);
        }
    }

    #region

    public void OnShot(InputAction.CallbackContext context)
    {
        Shoot();
    }

    public void OnReload(InputAction.CallbackContext context)
    {

    }

    #endregion







}
