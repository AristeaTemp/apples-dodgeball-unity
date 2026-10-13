using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// --- RandomZombieRunner ---
// Αυτό το script κάνει ένα GameObject (π.χ. ένα ζόμπι) να περιπλανιέται τυχαία σε μια περιοχή.
// Δεν χρησιμοποιεί NavMesh, αλλά κινείται με απλές εντολές κώδικα (transform.Translate).
public class RandomZombieRunner : MonoBehaviour
{
    // --- Ρυθμίσεις Κίνησης (εμφανίζονται στον Inspector) ---
    [Header("Movement Settings")]
    public float moveSpeed = 3f;      // Η ταχύτητα με την οποία κινείται.
    public float wanderRadius = 6f;   // (Δεν χρησιμοποιείται σε αυτή την έκδοση, η SetNewWanderPoint χρησιμοποιεί το sceneBounds).
    public float wanderTimer = 3f;    // Κάθε πόσα δευτερόλεπτα διαλέγει νέο προορισμό.

    // --- Ρυθμίσεις Animation ---
    [Header("Animation Settings")]
    public float animationSpeed = 1f; // Πολλαπλασιαστής ταχύτητας για το animation.

    // --- Όρια Σκηνής ---
    [Header("Scene Bounds")]
    public float sceneBounds = 8f;    // Τα όρια της αρένας στον άξονα Χ και Ζ (από -8 έως +8).

    // --- Private Μεταβλητές (εσωτερικές) ---
    private Animator animator;        // Αναφορά στον Animator.
    private Vector3 wanderPoint;      // Ο τρέχων τυχαίος προορισμός.
    private float wanderTime;         // Χρονόμετρο για την αλλαγή προορισμού.

    // Το Start() καλείται μία φορά στην αρχή του παιχνιδιού.
    void Start()
    {
        // Παίρνουμε αυτόματα το component Animator.
        animator = GetComponent<Animator>();

        // --- Αρχικοποίηση Φυσικής (Rigidbody) ---
        // Ελέγχουμε αν υπάρχει Rigidbody. Αν όχι, προσθέτουμε ένα.
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        // "Κλειδώνουμε" την κίνηση στον άξονα Υ και τις περιστροφές για να μην πέφτει ή τουμπάρει.
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false; // Δεν θέλουμε να το επηρεάζει η βαρύτητα, το ελέγχουμε χειροκίνητα.

        // --- Αρχικοποίηση του Collider ---
        // Ελέγχουμε αν υπάρχει Collider. Αν όχι, προσθέτουμε έναν Capsule Collider.
        if (GetComponent<Collider>() == null)
        {
            CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = 0.5f;
            collider.center = new Vector3(0, 1, 0);
        }

        // --- Αρχικοποίηση του Animator ---
        if (animator != null)
        {
            // Απενεργοποιούμε το Root Motion γιατί η κίνηση ελέγχεται πλήρως από αυτό το script.
            animator.applyRootMotion = false;
            animator.speed = animationSpeed;
        }

        // Ορίζουμε τον πρώτο τυχαίο προορισμό.
        SetNewWanderPoint();
    }

    // Το Update() καλείται σε κάθε καρέ.
    void Update()
    {
        // Εξασφαλίζουμε ότι το ζόμπι παραμένει πάντα στο ύψος του εδάφους (y = 0).
        Vector3 pos = transform.position;
        pos.y = 0;
        transform.position = pos;

        // Ελέγχουμε αν πρέπει να διαλέξουμε νέο προορισμό.
        // Αυτό συμβαίνει είτε αν φτάσαμε κοντά στον τρέχοντα προορισμό, είτε αν πέρασε ο χρόνος (wanderTimer).
        if (Vector3.Distance(transform.position, wanderPoint) < 1f || Time.time - wanderTime > wanderTimer)
        {
            SetNewWanderPoint();
        }

        // Υπολογίζουμε την κατεύθυνση προς τον προορισμό.
        Vector3 moveDirection = (wanderPoint - transform.position).normalized;
        moveDirection.y = 0; // Αγνοούμε την κίνηση πάνω/κάτω.

        // Κινούμε το ζόμπι.
        if (moveDirection != Vector3.zero)
        {
            // Εφαρμόζουμε την κίνηση στο transform.
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

            // Περιστρέφουμε το ζόμπι για να "κοιτάει" εκεί που πηγαίνει.
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }

        // Αναγκάζουμε το ζόμπι να παίζει συνεχώς το animation "Running".
        if (animator != null)
        {
            // Σημείωση: Αυτό μπορεί να αντικατασταθεί με animator.SetFloat("speed", 1); για καλύτερο έλεγχο.
            animator.Play("Running");
        }

        // Κρατάμε το ζόμπι μέσα στα όρια της σκηνής για να μην φύγει εκτός.
        pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -sceneBounds, sceneBounds);
        pos.z = Mathf.Clamp(pos.z, -sceneBounds, sceneBounds);
        transform.position = pos;
    }

    // Συνάρτηση που βρίσκει έναν νέο τυχαίο προορισμό.
    void SetNewWanderPoint()
    {
        // Δημιουργεί τυχαίες συντεταγμένες x και z μέσα στα όρια της σκηνής.
        float x = Random.Range(-sceneBounds, sceneBounds);
        float z = Random.Range(-sceneBounds, sceneBounds);

        wanderPoint = new Vector3(x, 0, z);
        wanderTime = Time.time; // Μηδενίζουμε το χρονόμετρο.
    }

    // Σχεδιάζει γραμμές-βοηθήματα μέσα στον Editor για να βλέπουμε τον προορισμό του ζόμπι.
    void OnDrawGizmosSelected()
    {
        // Σχεδιάζει μια πράσινη σφαίρα στον προορισμό και μια γραμμή από το ζόμπι προς αυτόν.
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(wanderPoint, 0.5f);
        Gizmos.DrawLine(transform.position, wanderPoint);
    }
}