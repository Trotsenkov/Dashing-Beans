using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(DashController))]
public class PlayerMovement : NetworkBehaviour
{
    internal static readonly Dictionary<string, int> playerNames = new Dictionary<string, int>();
    [SerializeField, SyncVar] public new string name;

    [SerializeField] float speed;
    Rigidbody rb;
    [SerializeField] float mouseSpeed;
    float rotationX;
    float rotationY;
    [SerializeField] new Transform camera;
    [SerializeField] float invisibleTime = 3;

    DashController dashController;
    float timer { get { return dashController.timer; } set { dashController.timer = value; } }
    float dashTime { get { return dashController.dashTime; } set { dashController.dashTime = value; } }
    float dashing => dashController.dashing;
    float dash => dashController.dash;

    Material regMat, hitMat;
    [SerializeField, SyncVar]
    bool invisible;
    public bool Invisible => invisible;

    //[SyncVar]
    //int _score = 0;
    public int score
    {
        get { return playerNames[name]; }
        set
        {
            //_score = value;
            playerNames[name] = value;
            Leaderboard.CheckWin(name);
        }
    }

    public override void OnStartServer()
    {
        name = (string)connectionToClient.authenticationData;
    }

    public override void OnStartLocalPlayer()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rb.mass = 0.1f;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        dashController = GetComponent<DashController>();
        rotationY = camera.localRotation.y;
        rotationX = transform.localRotation.x;
    }
    private void Start()
    {
        regMat = Resources.Load<Material>("Regular");
        hitMat = Resources.Load<Material>("Invisible");
        StartCoroutine(ClientInvisible());

        if (!isLocalPlayer)
        {
            timer = dashTime;
            Destroy(camera.gameObject);
        }
    }

    [ServerCallback]
    void OnCollisionEnter(Collision col)
    {
        PlayerMovement player = col.transform.GetComponent<PlayerMovement>();
        if (player)
        {
            StartCoroutine(GetInvisible(player));
        }
    }

    [ServerCallback]
    IEnumerator GetInvisible(PlayerMovement player)
    {
        if (player.Invisible)
            yield break;
        if (player.dashing == -1 && dashing == -1) //both aren`t dashing
            yield break;

        if (player.dashing > dashing) //another`s dash started earlier
            yield break;

        score++;

        player.invisible = true;
        yield return new WaitForSeconds(invisibleTime);
        player.invisible = false;
    }

    IEnumerator ClientInvisible()
    {
        yield return new WaitUntil(() => invisible);
        GetComponent<MeshRenderer>().material = hitMat;
        yield return new WaitWhile(() => invisible);
        GetComponent<MeshRenderer>().material = regMat;
        StartCoroutine(ClientInvisible());
    }

    Vector3 direction;
    void Update()
    {
        if (!isLocalPlayer)
            return;

        if (Input.GetKeyUp(KeyCode.LeftAlt))
            Cursor.lockState = CursorLockMode.Locked;
        if (Input.GetKeyDown(KeyCode.LeftAlt))
            Cursor.lockState = CursorLockMode.None;
        
        int fw = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0; //можно заменить на Vector3.GetAxis, 
        int rt = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0; //или настроить GetAxis в Project Settings
        rb.velocity = speed * (transform.forward * fw + transform.right * rt) + rb.velocity.y * transform.up;

        if (Input.GetMouseButtonDown(1) && timer >= dashTime) {
            timer = 0;
            direction = (transform.forward * fw + transform.right * rt);
        }

        if (timer < dashTime)
        {
            timer += Time.deltaTime;
            rb.velocity += dash / dashTime * direction;
        }

        rotationX += Input.GetAxis("Mouse X") * mouseSpeed * Time.deltaTime;
        rotationY -= Input.GetAxis("Mouse Y") * mouseSpeed * Time.deltaTime;

        transform.localRotation = Quaternion.Euler(0, rotationX, 0);
        camera.localRotation = Quaternion.Euler(rotationY, 0, 0);
    }
}