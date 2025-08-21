using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Random = UnityEngine.Random;

public class SnakeController : NetworkBehaviour
{
    [SerializeField] private GameObject tail_prefab;

    // SyncVar : 대상이 변경되면 동기화해주는 기능
    [SyncVar]
    private Transform coin_tf;

    [SerializeField] private float move_spd = 5f;
    [SerializeField] private float turn_spd = 120f;
    [SerializeField] private float lerp_spd = 7f;

    // SyncList : 추가 / 삽입 / 삭제할 때 동기화 해주는 기능
    private SyncList<Transform> tails = new SyncList<Transform>();

    [Server] //[Server] = 서버에서만 호출되는 함수
    public override void OnStartServer()
    {
        coin_tf = GameObject.FindGameObjectWithTag("Coin").transform;
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (isLocalPlayer)
            MoveHead();
    }

    void LateUpdate()
    {
        MoveTail();
    }

    private void MoveHead()
    {
        this.transform.Translate(Vector3.up * this.move_spd * Time.deltaTime);

        float h = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.forward * -h * turn_spd * Time.deltaTime);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            AddTail();
            MoveCoin();
        }
    }

    [Server]
    private void MoveCoin()
    {
        if (this.coin_tf == null)
            return;

        float ran_x = Random.Range(-20f, 20f);
        float ran_y = Random.Range(-10f, 11f);

        coin_tf.position = new Vector3(ran_x, ran_y, 0);
    }

    [Server]
    private void AddTail()
    {
        GameObject new_tail = Instantiate(tail_prefab);
        new_tail.transform.position = this.transform.position;

        NetworkServer.Spawn(new_tail, connectionToServer);

        tails.Add(new_tail.transform);
    }

    private void MoveTail()
    {
        Transform target = this.transform;

        foreach (Transform element in tails)
        {
            element.position = Vector3.Lerp(element.position, target.position, this.lerp_spd * Time.deltaTime);
            element.rotation = Quaternion.Lerp(element.rotation, target.rotation, this.lerp_spd * Time.deltaTime);

            target = element;
        }
    }
}
