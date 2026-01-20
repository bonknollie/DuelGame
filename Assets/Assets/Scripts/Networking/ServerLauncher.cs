using UnityEngine;
#if UNITY_NETCODE
using Unity.Netcode;
#endif

/// <summary>
/// Automatically starts the server when running in headless mode (no graphics).
/// Use this for dedicated server builds.
/// </summary>
public class ServerLauncher : MonoBehaviour
{
    [SerializeField] private bool autoStartInHeadless = true;

    private void Start()
    {
        // Check if running in headless mode (server build with -batchmode -nographics)
        if (autoStartInHeadless && SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            StartServer();
        }
    }

    private void StartServer()
    {
#if UNITY_NETCODE
        if (NetworkManager.Singleton != null)
        {
            Debug.Log("[ServerLauncher] Starting dedicated server...");
            NetworkManager.Singleton.StartServer();
            Debug.Log("[ServerLauncher] Server started successfully!");
        }
        else
        {
            Debug.LogError("[ServerLauncher] NetworkManager.Singleton is null! Cannot start server.");
        }
#else
        Debug.LogWarning("[ServerLauncher] UNITY_NETCODE not defined. Server mode unavailable.");
#endif
    }

    // Manual server start (for testing in editor)
    [ContextMenu("Start Server")]
    public void StartServerManual()
    {
        StartServer();
    }
}
