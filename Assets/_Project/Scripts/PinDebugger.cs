using UnityEngine;

public class PinDebugger : MonoBehaviour
{
    void Start()
    {
        SRDebug.Instance.DockConsole.IsVisible = true;
        SRDebug.Instance.PinOption("DeviceIndex");
        SRDebug.Instance.PinOption("ComPort");
        SRDebug.Instance.PinOption("FadeInSpeed");
        SRDebug.Instance.PinOption("FadeOutSpeed");
    }
}
