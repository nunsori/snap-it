using System;
using UnityEngine;

public class Networking : MonoBehaviour
{
    public static Networking Instance;

    public static event Action<bool> NetworkChangeAction;

    private bool IsConnect = false;
    private NetworkReachability lastStatus;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        lastStatus = Application.internetReachability;

        DontDestroyOnLoad(gameObject);
    }

    public static void NetworkChangeInvoke(bool isOn)
    {
        NetworkChangeAction?.Invoke(isOn);
    }
    
    public bool IsConnected() {
        return IsConnect;
    }

    private void CheckNetwork()
    {

        if (lastStatus != Application.internetReachability)
        {
            lastStatus = Application.internetReachability;
            NetworkChangeInvoke(lastStatus != NetworkReachability.NotReachable);
        }
        
        IsConnect = (lastStatus != NetworkReachability.NotReachable);
        
    }

    void Update()
    {
        CheckNetwork();
    }
}
