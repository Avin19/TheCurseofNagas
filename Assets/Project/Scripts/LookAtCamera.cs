
using UnityEditor.ShortcutManagement;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private enum Mode
    {
        LookAt,
        LookAtInverted,
        CameraFarword,
        CameraFarwordInverted,
    }
    [SerializeField] private Mode mode;
    void LateUpdate()
    {
        switch (mode)
        {
            case Mode.LookAt:
                transform.LookAt(Camera.main.transform);
                break;
            case Mode.LookAtInverted:
                Vector3 lookDir = transform.position - Camera.main.transform.position;
                transform.LookAt(transform.position + lookDir);
                break;

            case Mode.CameraFarword:
                transform.LookAt(Camera.main.transform.forward);
                break;
            case Mode.CameraFarwordInverted:
                transform.LookAt(-Camera.main.transform.forward);
                break;
        }
    }
}
