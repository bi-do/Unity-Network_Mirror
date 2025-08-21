using Mirror;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [SerializeField] private Transform coin;

    //SyncVar = 대상이 변경될떄 클라이언트 Alart
    [SyncVar(hook = nameof(OnCoinPositionChanged))] // 값이 변경되면 함수 호출
    public Vector3 coin_pos;

    void Awake()
    {
        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        MoveCoin();
    }

    [Server]
    public void MoveCoin()
    {
        float ran_x = Random.Range(-20f, 20f);
        float ran_y = Random.Range(-10f, 11f);

        coin_pos = new Vector3(ran_x, ran_y, 0);
    }

    private void OnCoinPositionChanged(Vector3 prev_pos, Vector3 new_pos)
    {
        coin.position = new_pos;
    }
}
