using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    
    public Transform player;

    // Η απόσταση της κάμερας από τον παίκτη.
    public Vector3 offset = new Vector3(-1, 5, -7);

    // Το LateUpdate() είναι ο καλύτερος τρόπος για κάμερες που ακολουθούν παίκτες.
    void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        // Η νέα θέση της κάμερας είναι η θέση του παίκτη + η απόσταση (offset).
       transform.position = player.position + offset;
    }
}