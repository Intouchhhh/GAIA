using System.IO;
using UnityEngine;

public class RewardLogger : MonoBehaviour
{
	[SerializeField] private AgentController agent;
	private string filePath;
	private int episodeCounter = 0;

	private void Start()
	{
		string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
		filePath = Path.Combine(Application.dataPath, $"Logging/RewardLog_{timestamp}.csv");

		if (!File.Exists(filePath))
		{
			File.WriteAllText(filePath, "Episode,Steps,Reward,CoinsCollected,TotalCoins,CheckpointsCollected,TotalCheckpoints,JumpCount,JumpPenalty,DashCount,DashPenalty,CumulativeAvgSteps,CumulativeAvgRewards\n");
		}

		agent.OnEpisodeEnded += LogEpisodeData; // Subscribe to custom event
	}

	private void LogEpisodeData()
	{
		episodeCounter++;

		var reward = agent.GetCumulativeReward();
		var jumpPenalty = agent.Config.jumpCountTax * agent.JumpCount;
		var dashPenalty = agent.Config.dashCountTax * agent.DashCount;

		string entry = $"{episodeCounter}," +
					   $"{agent.StepCount}," +
					   $"{reward:F2}," +
					   $"{agent.CollectedCoins}," +
					   $"{agent.TotalCoins}," +
					   $"{agent.CollectedCheckpoints}," +
					   $"{agent.TotalCheckpoints}," +
					   $"{agent.JumpCount}," +
					   $"{jumpPenalty:F2}," +
					   $"{agent.DashCount}," +
					   $"{dashPenalty:F2}" +
					   $"{agent.AverageSteps}" +
                       $"{agent.AverageRewards}";

		File.AppendAllText(filePath, entry + "\n");
	}

	private void OnDestroy()
	{
		agent.OnEpisodeEnded -= LogEpisodeData;
	}
}
