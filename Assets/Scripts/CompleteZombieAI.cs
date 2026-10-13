using UnityEngine;
using UnityEngine.AI;
using System.Collections;

// Απαιτεί τα components NavMeshAgent και Animator για να λειτουργήσει το script.
[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class CompleteZombieAI : MonoBehaviour
{
    // --- Private Μεταβλητές ---
    private Animator animator;              // Αναφορά στον Animator του Zombie.
    private NavMeshAgent agent;             // Αναφορά στον NavMeshAgent για την κίνηση.
    private Transform playerTarget;         // Αναφορά στη θέση του παίκτη για να ξέρει πού να στοχεύσει.
    private bool hasReactedToBall = false;  // "Μνήμη" για να μην αντιδρά συνεχώς στην ίδια μπάλα.

    // --- "Διακόπτες" που ελέγχει ο GameManager ---
    [HideInInspector] public bool canDodge = false;         // Επιτρέπεται το dodge σε αυτό το level;
    [HideInInspector] public bool canCatchAndThrow = false; // Επιτρέπεται το catch & throw σε αυτό το level;

    // --- Ρυθμίσεις για τη Ρίψη (εμφανίζονται στον Inspector) ---
    [Header("Throwing Logic")]
    public Transform handGripPoint;     // Το σημείο μέσα στην παλάμη όπου "κολλάει" η μπάλα.
    public Transform throwPoint;        // Το σημείο από όπου εκτοξεύεται η μπάλα.
    public float throwForce = 15f;      // Η δύναμη της ρίψης.
    public float delayBeforeThrow = 2f; // Πόσα δευτερόλεπτα περιμένει αφού πιάσει την μπάλα.

    // --- Μεταβλητές Κατάστασης ---
    private bool hasBall = false;       // "Θυμάται" αν κρατάει μπάλα.
    private GameObject heldBall;        // Αναφορά στο αντικείμενο της μπάλας που κρατάει.

    [Header("Probabilities")]
    [Range(0, 1)] public float dodgeChance = 0.3f;
    [Range(0, 1)] public float catchChance = 0.3f;

    [Header("Audio")]
    public AudioClip deathSound;

    // Το Awake() εκτελείται μία φορά στην αρχή, πριν το Start().
    void Awake()
    {
        // Παίρνουμε αυτόματα τα components που είναι συνδεδεμένα στο ίδιο GameObject.
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        // Βρίσκουμε τον παίκτη στη σκηνή μέσω του Tag "Player".
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            // Αποθηκεύουμε τη θέση του για να ξέρουμε πού να στοχεύσουμε.
            playerTarget = playerObject.transform;
        }
    }

    // Αυτή η συνάρτηση καλείται από τον GameManager για να πει στο ζόμπι τι επιτρέπεται να κάνει.
    public void ConfigureAI(bool allowsDodge, bool allowsCatch)
    {
        canDodge = allowsDodge;
        canCatchAndThrow = allowsCatch;
    }

    // Καλείται όταν ένα άλλο αντικείμενο μπει στον Trigger Collider του ζόμπι.
    void OnTriggerEnter(Collider other)
    {
        // Αν επιτρέπεται το dodge, αν το αντικείμενο είναι μπάλα και αν δεν έχουμε ήδη αντιδράσει.
        if (canDodge && other.CompareTag("Ball") && !hasReactedToBall)
        {
            hasReactedToBall = true;        // Σημειώνουμε ότι αντιδράσαμε.
            animator.SetTrigger("doDodge"); // Δίνουμε σήμα στον Animator να παίξει το dodge.
            Dodge(other.transform);         // Εκτελούμε την κίνηση της αποφυγής.
            Invoke("ResetReaction", 2f);    // Μετά από 2 δευτ., του επιτρέπουμε να αντιδράσει ξανά.
        }
    }

    // Καλείται όταν η μπάλα κάνει φυσική σύγκρουση με το ζόμπι.
    void OnCollisionEnter(Collision collision)
    {
        // Αν το αντικείμενο είναι μπάλα και δεν κρατάμε ήδη άλλη.
        if (collision.gameObject.CompareTag("Ball") && !hasBall)
        {
            bool wasCaught = false; // Βοηθητική μεταβλητή για να δούμε αν την έπιασε.

            // Αν επιτρέπεται το πιάσιμο σε αυτό το level.
            if (canCatchAndThrow && Random.value < catchChance)
            {
                CatchBall(collision.gameObject); // Εκτελούμε τη λογική του πιασίματος.
                wasCaught = true;
            }

            // Αν δεν την έπιασε (είτε γιατί δεν επιτρεπόταν, είτε γιατί κάτι πήγε λάθος).
            if (!wasCaught)
            {
                if (deathSound != null)
                {
                    AudioSource.PlayClipAtPoint(deathSound, transform.position);
                }
                // CONSOLE LOG
                Debug.Log(gameObject.name + " ---> Was HIT and DESTROYED!");

                if (GameManager.instance != null) GameManager.instance.ZombieWasDestroyed(); // Ενημερώνουμε τον GameManager.
                Destroy(collision.gameObject); // Καταστρέφουμε την μπάλα.
                Destroy(gameObject);           // Καταστρέφουμε το ζόμπι.
            }
        }
    }

    // Η λογική για το πιάσιμο της μπάλας.
    void CatchBall(GameObject ballToCatch)
    {
        if (hasBall) return; // Αν κρατάει ήδη μπάλα, σταματάει.

        // CONSOLE LOG
        Debug.Log(gameObject.name + " ---> CAUGHT the ball!");

        agent.isStopped = true; // "Παγώνει" την κίνησή του.
        hasBall = true;
        heldBall = ballToCatch;
        animator.SetTrigger("doCatch"); // Παίζει το animation του πιασίματος.

        // Απενεργοποιούμε τη φυσική της μπάλας και την "κολλάμε" στο χέρι.
        Rigidbody rb = heldBall.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        heldBall.GetComponent<Collider>().enabled = false;
        heldBall.transform.SetParent(handGripPoint);
        heldBall.transform.localPosition = Vector3.zero;

        // Ξεκινάμε την αντίστροφη μέτρηση για να την πετάξει πίσω.
        StartCoroutine(ThrowBallAfterDelay());
    }

    // Coroutine για τη ρίψη με καθυστέρηση.
    IEnumerator ThrowBallAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeThrow); // Περιμένουμε.
        if (heldBall == null || playerTarget == null) yield break;

        animator.SetTrigger("doThrow"); // Παίζουμε το animation της ρίψης.
        yield return new WaitForSeconds(0.5f); // Μικρή παύση για να συγχρονιστεί με το animation.

        if (heldBall == null) yield break;

        // CONSOLE LOG
        Debug.Log(gameObject.name + " ---> THROWING the ball back!");

        // "Απελευθερώνουμε" την μπάλα και της δίνουμε δύναμη προς τον παίκτη.
        heldBall.transform.SetParent(null);
        Rigidbody rb = heldBall.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        heldBall.GetComponent<Collider>().enabled = true;

        // Δίνουμε στην μπάλα που πετάει το ζόμπι ένα ειδικό tag για να την αναγνωρίζει ο παίκτης.
        heldBall.tag = "ZombieBall";

        Vector3 directionToPlayer = (playerTarget.position - throwPoint.position).normalized;
        rb.AddForce(directionToPlayer * throwForce, ForceMode.VelocityChange);

        // "Ξεχνάμε" την μπάλα και επιτρέπουμε στο ζόμπι να κινηθεί ξανά.
        heldBall = null;
        hasBall = false;
        agent.isStopped = false;
    }

    // Η λογική για την κίνηση της αποφυγής.
    void Dodge(Transform ballTransform)
    {
        // CONSOLE LOG
        Debug.Log(gameObject.name + " ---> DODGED!");

        // Υπολογίζει μια κατεύθυνση κάθετη στην πορεία της μπάλας και πηγαίνει εκεί.
        Vector3 dirFromBall = (transform.position - ballTransform.position).normalized;
        Vector3 dodgeDirection = Vector3.Cross(dirFromBall, Vector3.up);
        agent.SetDestination(transform.position + dodgeDirection * 5f);
    }

    // Μηδενίζει τη "μνήμη" του ζόμπι ώστε να μπορεί να αντιδράσει ξανά.
    void ResetReaction()
    {
        hasReactedToBall = false;
    }
}