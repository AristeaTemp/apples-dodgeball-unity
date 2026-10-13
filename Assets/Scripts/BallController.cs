using UnityEngine;

// Αυτό το script ελέγχει τη συμπεριφορά της μπάλας.
// Πρέπει να είναι συνδεδεμένο στο Prefab της μπάλας.
public class BallController : MonoBehaviour
{
    // --- OnDestroy ---
    // Αυτή η συνάρτηση καλείται αυτόματα από τη Unity μία στιγμή ΠΡΙΝ το αντικείμενο καταστραφεί.
    // Την χρησιμοποιούμε για να είμαστε σίγουροι ότι ο GameManager θα ενημερωθεί, ανεξάρτητα από τον λόγο καταστροφής.
    private void OnDestroy()
    {
        // Ελέγχουμε αν ο GameManager υπάρχει ακόμα στη σκηνή (για ασφάλεια).
        if (GameManager.instance != null)
        {
            // Ειδοποιούμε τον GameManager ότι μια μπάλα καταστράφηκε, ώστε να ξεκινήσει τη λογική της ήττας ή του respawn.
            GameManager.instance.BallWasDestroyed();
        }
    }

    // --- OnTriggerEnter ---
    // Αυτή η συνάρτηση καλείται όταν ο Collider της μπάλας μπει μέσα σε έναν άλλο Collider που είναι ρυθμισμένος ως "Trigger".
    // Τη χρησιμοποιούμε για να ανιχνεύσουμε το "πιάσιμο" από τον παίκτη.
    private void OnTriggerEnter(Collider other)
    {
        // Ελέγχουμε αν το αντικείμενο του trigger έχει την ετικέτα (Tag) "CatchZoneTag".
        if (other.CompareTag("CatchZoneTag"))
        {
            // Αν ναι, ψάχνουμε προς τα "πάνω" στην ιεραρχία για να βρούμε το script του παίκτη.
            // Αυτό μας επιτρέπει να βρούμε τον παίκτη, ακόμα κι αν το CatchZone είναι "παιδί" του.
            PlayerController player = other.GetComponentInParent<PlayerController>();

            // Αν βρούμε το script του παίκτη...
            if (player != null)
            {
                // ... του "φωνάζουμε" να εκτελέσει τη συνάρτηση CatchBall, δίνοντάς του τον εαυτό μας (αυτό το GameObject της μπάλας).
                player.CatchBall(this.gameObject);
            }
        }
    }

    // --- OnCollisionEnter ---
    // Αυτή η συνάρτηση καλείται όταν ο Collider της μπάλας συγκρουστεί φυσικά (όχι ως trigger) με έναν άλλο Collider.
    // Τη χρησιμοποιούμε για να ξέρουμε πότε η μπάλα χτύπησε έναν εχθρό.
    private void OnCollisionEnter(Collision collision)
    {
        // Ελέγχουμε αν το αντικείμενο που χτυπήσαμε έχει το Tag "Zombie".
        if (collision.gameObject.CompareTag("Zombie"))
        {
            // Αν ναι, η μπάλα έχει εκπληρώσει τον σκοπό της.
            // Η λογική για το τι θα πάθει το ζόμπι (catch, dodge, destroy) βρίσκεται στο script του Zombie.
            // Εμείς εδώ απλά καταστρέφουμε την μπάλα.
            Destroy(this.gameObject);
        }
    }
}