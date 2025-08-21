using Mirror;
using UnityEngine;

public class Snake : NetworkBehaviour
{
    [SerializeField] private GameObject tail_prefab;
    [SerializeField] private MeshRenderer head_rednerer;

    [SerializeField] private float move_spd = 5f;
    [SerializeField] private float turn_spd = 120f;
    [SerializeField] private float lerp_spd = 7f;

    private float tail_offset = 0.5f;

    // SyncList : 추가 / 삽입 / 삭제할 때 동기화 해주는 기능
    private SyncList<GameObject> tails = new SyncList<GameObject>();

    [SyncVar(hook = nameof(OnDeathStateChanged))]
    private bool isDead = false;

    public override void OnStartClient()
    {
        base.OnStartClient();
        tails.Callback += OnTailUpdated;
    }

    public override void OnStartLocalPlayer()
    {
        head_rednerer.material.color = new Color(0.8f, 1f, 0.8f);
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

    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer)
            return;

        if (other.CompareTag("Coin"))
        {
            GetCoin();
        }
        if (other.CompareTag("Tail"))
        {
            Tail tail = other.GetComponent<Tail>();
            if (tail.owner_ID != this.netIdentity)
            {
                Died();
            }
        }
    }

    private void MoveTail()
    {
        Transform target = this.transform;

        foreach (GameObject element in tails)
        {
            if (element != null)
            {
                element.transform.position = Vector3.Lerp(element.transform.position, target.position, this.lerp_spd * Time.deltaTime);
                element.transform.rotation = Quaternion.Lerp(element.transform.rotation, target.rotation, this.lerp_spd * Time.deltaTime);

                target = element.transform;
            }
        }
    }

    private void OnTailUpdated(SyncList<GameObject>.Operation op, int index, GameObject old_tail, GameObject new_tail)
    {
        if (op == SyncList<GameObject>.Operation.OP_ADD && isLocalPlayer)
        {
            Transform target = this.transform;

            if (index > 0)
            {
                target = tails[index - 1].transform;
            }

            new_tail.transform.position = target.position - (target.up * tail_offset);
            new_tail.transform.rotation = target.rotation;

        }
    }

    private void MoveHead()
    {
        this.transform.Translate(Vector3.up * this.move_spd * Time.deltaTime);

        float h = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.forward * -h * turn_spd * Time.deltaTime);
    }

    void OnDeathStateChanged(bool old_state, bool new_state)
    {
        if (new_state)
        {
            head_rednerer.material.color = Color.gray;
        }
    }

    [Command]
    private void Died()
    {
        this.isDead = true;
    }

    [Command]
    private void GetCoin()
    {
        Debug.Log("1");
        GameManager.Instance.MoveCoin();
        Debug.Log("2");

        AddTail();
        Debug.Log("3");
    }

    [Server]
    private void AddTail()
    {
        Transform spawn_target = this.transform;
        if (tails.Count > 0)
        {
            spawn_target = tails[tails.Count - 1].transform;
        }

        Vector3 spawn_pos = spawn_target.position - (spawn_target.up * tail_offset);
        Quaternion spawn_rot = spawn_target.rotation;

        GameObject new_tail = Instantiate(tail_prefab, spawn_pos, spawn_rot);
        new_tail.GetComponent<Tail>().owner_ID = netIdentity;

        NetworkServer.Spawn(new_tail, connectionToClient);
        tails.Add(new_tail);
    }



}