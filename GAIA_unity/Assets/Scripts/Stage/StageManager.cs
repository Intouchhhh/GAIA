using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class StageManager : MonoBehaviour
{
    private Camera _cam;
    private PlayerMovement _player;
    [SerializeField] private Tilemap[] levels;
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private TextMeshProUGUI nameText;

    private int _currentPlayerTypeIndex;
    private int _currentTilemapIndex;
    private Color _currentForegroundColor;

    public StageData stageData;

    private void Awake()
    {
        _cam = FindFirstObjectByType<Camera>();
        _player = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        SetSceneData(stageData);
		_player.transform.position = spawnPoint.position;
	}

    public void SetSceneData(StageData data)
    {
		stageData = data;

        _cam.orthographicSize = data.camSize;
        _cam.backgroundColor = data.backgroundColor;
        //levels[_currentTilemapIndex].color = data.foregroundColor;

        _currentForegroundColor = data.foregroundColor;
    }
}
