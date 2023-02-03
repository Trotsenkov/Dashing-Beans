using UnityEngine;
using Mirror;

public class DashController : NetworkBehaviour
{
    [SerializeField] public float dashTime = 0.3f;
    [SerializeField] public float dash;
    public float dashing { get { return timer < dashTime ? timer : -1; } }

    [SyncVar]
    public float timer;
}