using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// --- SimpleAISpawner ---
// Αυτό το script είναι υπεύθυνο για τη δημιουργία (spawn) των Zombies στη σκηνή.
// Λειτουργεί ως "βοηθός" του GameManager.
public class SimpleAISpawner : MonoBehaviour
{
    // --- Ρυθμίσεις Δημιουργίας (εμφανίζονται στον Inspector) ---
    [Header("Spawn Settings")]
    public GameObject zombiePrefab; // Το Prefab του Zombie που θέλουμε να δημιουργήσουμε.
    public int numberOfZombies = 5; // Πόσα ζόμπι θα δημιουργηθούν. Ο GameManager μπορεί να αλλάξει αυτή την τιμή.

    // Η συνάρτηση Start() έχει αφαιρεθεί, καθώς ο GameManager έχει πλέον τον έλεγχο
    // και θα καλέσει τη SpawnZombies() όποτε χρειάζεται (στην αρχή κάθε level).

    // --- SpawnZombies ---
    // Αυτή η συνάρτηση είναι public ώστε να μπορεί να την καλέσει ο GameManager.
    // Είναι η κύρια λειτουργία αυτού του script.
    public void SpawnZombies()
    {
        // Έλεγχος ασφαλείας: αν δεν έχουμε συνδέσει το prefab, εμφανίζει σφάλμα και σταματάει.
        if (zombiePrefab == null)
        {
            Debug.LogError("Zombie Prefab is not assigned in the Spawner!");
            return;
        }

        int spawnedCount = 0; // Μετρητής για τα ζόμπι που δημιουργήθηκαν.

        // Μια επανάληψη (for loop) που εκτελείται όσες φορές έχουμε ορίσει στο numberOfZombies.
        for (int i = 0; i < numberOfZombies; i++)
        {
            // Παίρνουμε μια τυχαία θέση από τη βοηθητική συνάρτηση.
            Vector3 spawnPosition = GetRandomSpawnPosition();
            // Δημιουργούμε ένα νέο ζόμπι από το prefab, στην τυχαία θέση, χωρίς περιστροφή.
            GameObject zombie = Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);
            // Δίνουμε ένα μοναδικό όνομα σε κάθε ζόμπι για ευκολότερο debugging.
            zombie.name = "Zombie " + (i + 1);
            spawnedCount++; // Αυξάνουμε τον μετρητή.
        }

        // Εμφανίζουμε ένα μήνυμα στην κονσόλα για να επιβεβαιώσουμε τη δημιουργία.
        Debug.Log($"Spawner created {spawnedCount} zombies!");

        // Ενημερώνουμε τον GameManager για το πόσα ακριβώς ζόμπι δημιουργήσαμε.
        if (GameManager.instance != null)
        {
            // Καλούμε τη δημόσια συνάρτηση SetInitialZombieCount του GameManager.
            GameManager.instance.SetInitialZombieCount(spawnedCount);
        }
    }

    // --- GetRandomSpawnPosition ---
    // Μια βοηθητική συνάρτηση που επιστρέφει μια τυχαία θέση μέσα στην αρένα.
    Vector3 GetRandomSpawnPosition()
    {
        // Διαλέγει έναν τυχαίο αριθμό για τη συντεταγμένη Χ, από -8 έως 8.
        float x = Random.Range(-8f, 8f);
        // Διαλέγει έναν τυχαίο αριθμό για τη συντεταγμένη Ζ, από -8 έως 8.
        float z = Random.Range(-8f, 8f);

        // Επιστρέφει τη νέα τυχαία θέση. Το Υ είναι σταθερό στο 0.5f για να είναι λίγο πάνω από το έδαφος.
        return new Vector3(x, 0.5f, z);
    }
}