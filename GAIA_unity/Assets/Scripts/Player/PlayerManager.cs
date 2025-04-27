using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    private Rigidbody2D rb;

    public int maxHealth = 3;
    public int currentHealth = 3;
    public int attackDamage = 1;

	[SerializeField] private List<GameObject> spawnPointsList;

	private void Start()
	{
		spawnPointsList = new List<GameObject>(GameObject.FindGameObjectsWithTag("Spawn"));

		transform.position = spawnPointsList[Random.Range(0, spawnPointsList.Count)].transform.position;
	}

	private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hazard"))
        {
            Debug.Log("Player hit the spike (trigger)!");
            Die();
        }
        if (collision.CompareTag("Coins"))
        {
            Destroy(collision.gameObject);
        }
    }

    public void Die()
    {
        Debug.Log("Player Died!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
