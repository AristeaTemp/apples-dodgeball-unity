using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Setup")]
    // Το ballPrefab αφαιρέθηκε από εδώ. Ο GameManager είναι υπεύθυνος.
    public Transform handGripPoint;
    public Transform throwPoint;
    public GameObject catchZone;
    public LineRenderer laserLine;

    [Header("Gameplay Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 500f;
    public float throwForce = 20f;
    public float catchDuration = 0.5f;
    public float laserRange = 50f;

    [Header("Aim Assist Settings")]
    public float aimAssistRange = 20f;
    public float aimAssistAngle = 30f;

    // Private variables
    private Animator animator;
    private bool hasBall = false;
    private GameObject heldBall;
    private int moveSpeedHash;
    private int isThrowingHash;

    void Start()
    {
        animator = GetComponent<Animator>();
        moveSpeedHash = Animator.StringToHash("moveSpeed");
        isThrowingHash = Animator.StringToHash("isThrowing");

        catchZone.SetActive(false);
        if (laserLine != null) laserLine.enabled = false;

        // Η λογική "startWithBall" αφαιρέθηκε. Ο παίκτης ξεκινά πάντα χωρίς μπάλα.
    }

    void Update()
    {
        HandleMovement();
        HandleActions();
        HandleAiming();
    }

    // --- ΝΕΑ ΣΥΝΑΡΤΗΣΗ ΓΙΑ ΟΤΑΝ ΧΤΥΠΙΕΤΑΙ Ο ΠΑΙΚΤΗΣ ---
    void OnCollisionEnter(Collision collision)
    {
        // Ελέγχουμε αν μας χτύπησε μια μπάλα που έχει πετάξει ζόμπι
        if (collision.gameObject.CompareTag("ZombieBall"))
        {
            Debug.Log("Player was hit by a Zombie!");

            // Ενημερώνουμε τον GameManager (π.χ., για να χάσουμε ζωή/μπάλα)
            if (GameManager.instance != null)
            {
                // GameManager.instance.PlayerWasHit(); // Θα το ενεργοποιήσουμε όταν φτιάξουμε τη λογική στο GameManager
            }

            // Καταστρέφουμε την μπάλα του ζόμπι
            Destroy(collision.gameObject);
        }
    }

    // --- ΟΙ ΥΠΟΛΟΙΠΕΣ ΣΥΝΑΡΤΗΣΕΙΣ ΠΑΡΑΜΕΝΟΥΝ ΙΔΙΕΣ ---

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 moveDirection = new Vector3(h, 0, v).normalized;

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
        animator.SetFloat(moveSpeedHash, moveDirection.magnitude);

        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(moveDirection, Vector3.up), rotationSpeed * Time.deltaTime);
        }
    }

    void HandleActions()
    {
        if (Input.GetMouseButtonDown(0) && hasBall)
        {
            animator.SetTrigger(isThrowingHash);
        }
        if (Input.GetMouseButtonDown(1))
        {
            StartCoroutine(ActivateCatchZone());
        }
    }

    void HandleAiming()
    {
        if (laserLine == null || throwPoint == null) return;

        if (hasBall)
        {
            laserLine.enabled = true;
            laserLine.SetPosition(0, throwPoint.position);
            Transform target = FindTargetZombie();

            if (target != null)
            {
                laserLine.SetPosition(1, target.position);
                laserLine.startColor = Color.red;
                laserLine.endColor = Color.red;
            }
            else
            {
                laserLine.SetPosition(1, throwPoint.position + throwPoint.forward * laserRange);
                laserLine.startColor = Color.green;
                laserLine.endColor = Color.green;
            }
        }
        else
        {
            laserLine.enabled = false;
        }
    }

    public void ThrowBall()
    {
        if (heldBall == null) return;

        Debug.Log("PLAYER: Threw the ball!");
        heldBall.transform.SetParent(null);
        Rigidbody rb = heldBall.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        heldBall.GetComponent<Collider>().enabled = true;

        Transform target = FindTargetZombie();
        Vector3 throwDirection = (target != null) ? (target.position - throwPoint.position).normalized : throwPoint.forward;

        rb.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);
        Destroy(heldBall, 5f); // Κατέστρεψε την μπάλα μετά από 5 δευτερόλεπτα
        heldBall = null;
        hasBall = false;
    }

    Transform FindTargetZombie()
    {
        Transform bestTarget = null;
        float bestAngle = aimAssistAngle;
        Collider[] colliders = Physics.OverlapSphere(transform.position, aimAssistRange);

        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Zombie"))
            {
                Vector3 dirToTarget = (col.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, dirToTarget);
                if (angle < bestAngle)
                {
                    bestAngle = angle;
                    bestTarget = col.transform;
                }
            }
        }
        return bestTarget;
    }

    public void CatchBall(GameObject ballToCatch)
    {
        if (hasBall) return;
        if (GameManager.instance != null) GameManager.instance.PlayerTookBall();
        hasBall = true;
        heldBall = ballToCatch;
        Rigidbody rb = heldBall.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        Collider col = heldBall.GetComponent<Collider>();
        if (col != null) col.enabled = false;
        heldBall.transform.SetParent(handGripPoint);
        heldBall.transform.localPosition = Vector3.zero;
    }

    IEnumerator ActivateCatchZone()
    {
        catchZone.SetActive(true);
        yield return new WaitForSeconds(catchDuration);
        catchZone.SetActive(false);
    }


    public bool HasBall()
    {
        return hasBall;
    }
}