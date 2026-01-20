#if UNITY_NETCODE
using UnityEngine;
using Unity.Netcode;

public class NetStatusHUD : MonoBehaviour
{
    GUIStyle style;

    void Awake()
    {
        style = new GUIStyle(GUI.skin.label) { fontSize = 14, normal = { textColor = Color.white } };
    }

    void OnGUI()
    {
        if (NetworkManager.Singleton == null) { GUI.Label(new Rect(10, 90, 300, 25), "NGO: Not present", style); return; }
        var nm = NetworkManager.Singleton;
        string role = nm.IsServer ? (nm.IsHost ? "Host" : "Server") : (nm.IsClient ? "Client" : "Offline");
        int clients = nm.ConnectedClients != null ? nm.ConnectedClients.Count : 0;
        GUI.Label(new Rect(10, 90, 500, 25), $"NGO: {role} | Listening={nm.IsListening} | Clients={clients}", style);
    }
}
#endif
