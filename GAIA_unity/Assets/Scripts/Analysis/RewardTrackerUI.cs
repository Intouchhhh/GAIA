using TMPro;
using UnityEngine;

public class RewardTrackerUI : MonoBehaviour
{
    [SerializeField] private AgentController agent;
    [SerializeField] private TextMeshProUGUI text;

    private void Update()
    {
        if (agent == null || text == null) return;

        text.text =
			$"<b>EPISODE:</b> {agent.EpisodeNumber}   <b>AVG STEPS:</b> {agent.AverageSteps}   <b>DEATHS:</b> {agent.DeathCount}\n" +
			$"<b>STEP:</b> {agent.StepCount}/{agent.MaxStep}\n" +
            $"<b>REWARD:</b> {agent.GetCumulativeReward():F2}\n" +
            $"<b>HEALTH:</b> {agent.GetComponent<PlayerManager>().currentHealth}/{agent.GetComponent<PlayerManager>().maxHealth}\n" +
            $"<b>POS:</b> ({agent.transform.position.x:F1}, {agent.transform.position.y:F1})\n\n" +

            $"<b>COINS:</b> {agent.CollectedCoins}/{agent.TotalCoins}   <b>CHECKPOINTS:</b> {agent.CollectedCheckpoints}/{agent.TotalCheckpoints}\n\n" +

            $"<b>JUMPS:</b> {agent.JumpCount} (Penalty: {agent.JumpCount * agent.Config.jumpCountTax:F2})\n" +
            $"<b>DASHES:</b> {agent.DashCount} (Penalty: {agent.DashCount * agent.Config.dashCountTax:F2})\n";
    }
}
