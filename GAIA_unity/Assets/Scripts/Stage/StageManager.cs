using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class StageManager : MonoBehaviour
{
    private Camera _cam;
    private BasicPlayerMovement _player;
    [SerializeField] private Tilemap[] levels;
    [SerializeField] public Transform spawnPoint;

    [SerializeField] private TextMeshProUGUI nameText;

    private int _currentPlayerTypeIndex;
    private int _currentTilemapIndex;
    private Color _currentForegroundColor;

    public StageData stageData;

    private void Awake()
    {
        _cam = FindFirstObjectByType<Camera>();
        _player = GameObject.FindWithTag("Player").GetComponent<BasicPlayerMovement>();
    }

    private void Start()
    {
        SetSceneData(stageData);
		//_player.transform.position = spawnPoint.position;
	}

    public void SetSceneData(StageData data)
    {
		stageData = data;

        _cam.orthographicSize = data.camSize;
        _cam.backgroundColor = data.backgroundColor;

        _currentForegroundColor = data.foregroundColor;
    }
}
