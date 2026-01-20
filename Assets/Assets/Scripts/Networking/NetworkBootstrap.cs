#if UNITY_NETCODE
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class NetworkBootstrap : MonoBehaviour
{
    public string address = "127.0.0.1";
    public ushort port = 7777;

    void Awake()
    {
        var transport = FindObjectOfType<UnityTransport>();
        if (transport != null)
        {
            transport.ConnectionData.Address = address;
            transport.ConnectionData.Port = port;
        }
    }

    void OnGUI()
    {
        if (NetworkManager.Singleton == null)
        {
            GUI.Label(new Rect(10, 10, 300, 25), "ERROR: NetworkManager not found in scene!", new GUIStyle(GUI.skin.label) { normal = { textColor = Color.red } });
            return;
        }

        const int w = 200;
        const int h = 30;
        int y = 10;
        GUI.Label(new Rect(10, y, w, h), $"Address: {address}:{port}"); y += h + 5;
        if (GUI.Button(new Rect(10, y, w, h), "Start Host"))
        {
            NetworkManager.Singleton.StartHost();
        }
        y += h + 5;
        if (GUI.Button(new Rect(10, y, w, h), "Start Client"))
        {
            var transport = FindObjectOfType<UnityTransport>();
            if (transport != null)
            {
                transport.ConnectionData.Address = address;
                transport.ConnectionData.Port = port;
            }
            NetworkManager.Singleton.StartClient();
        }
        y += h + 5;
        if (GUI.Button(new Rect(10, y, w, h), "Stop"))
        {
            if (NetworkManager.Singleton.IsListening)
                NetworkManager.Singleton.Shutdown();
        }
    }
}
#endif
