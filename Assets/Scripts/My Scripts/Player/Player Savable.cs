using UnityEngine;

public class PlayerSavable : MonoBehaviour, ISaveable
{

    public void Capture(GameSaveData data)
    {
        data.player.position = transform.position;        
        data.player.yRotation = transform.rotation.y;

    }

    public void Restore(GameSaveData data)
    {
        if (TryGetComponent(out CharacterController controller))
        {
            controller.enabled = false;
            transform.SetPositionAndRotation(data.player.position, Quaternion.Euler(0, data.player.yRotation, 0));
            controller.enabled = true;
        }
    }
}
