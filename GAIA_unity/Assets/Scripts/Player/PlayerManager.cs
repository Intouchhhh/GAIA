using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    private Rigidbody2D rb;
	private AgentController agentController;

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
			collision.gameObject.SetActive(false);
		}
	}

	public void TakeDamage(int damage)
	{
		currentHealth -= damage;
		if (currentHealth <= 0)
		{
			Die();
		}
	}

	public void Die()
    {
        Debug.Log("Player Died!");
		if (!Academy.Instance.IsCommunicatorOn)
		{
			if (agentController == null)
				agentController = GetComponent<AgentController>();

			agentController.HandleCompletionRewards();
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}
	}
}
