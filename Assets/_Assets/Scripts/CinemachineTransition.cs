using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CinemachineTransition : MonoBehaviour
{
    [SerializeField]
    private Misc ambianceManager;

    private CinemachineVirtualCamera cam;
    private bool isLive = false;

    private void Start()
    {
        cam = GetComponent<CinemachineVirtualCamera>();
    }

    private void Update()
    {
        bool newState = CinemachineCore.Instance.IsLive(cam);

        if (newState != isLive)
        {
            isLive = newState;
            if (newState)
                ambianceManager.CycleTheme();
        }
    }
}
