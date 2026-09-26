using UnityEngine;

public class NpcRandomAppearance : MonoBehaviour
{
    [Header("regular material")]
    [Tooltip("If this material exists, random material won't work")]
    public Material clothMaterial;

    [Header("Renderer Setting")]
    [Tooltip("Drag in SkinnnedMeshRenderer that contained npc's clothes")]
    public SkinnedMeshRenderer targetRenderer;

    public SkinnedMeshRenderer LODtargetRenderer;

    [Tooltip("Modify the index if clothes is not the first property(Element 0)")]
    public int materialIndex = 0;

    [Header("Property Repo")]
    [Tooltip("Put all kinds of material sphere into this array")]

    public Material[] clothMaterials;

    void Start()
    {
        if (targetRenderer != null && clothMaterial != null)
        {
            Material[] currentMaterials = targetRenderer.sharedMaterials;

            if (materialIndex < currentMaterials.Length)
            {
                currentMaterials[materialIndex] = clothMaterial;

                targetRenderer.sharedMaterials = currentMaterials;
                LODtargetRenderer.sharedMaterials = currentMaterials;
            }

            return;
        }

        if (targetRenderer != null && clothMaterials.Length > 0)
        {
            int randomIndex = Random.Range(0, clothMaterials.Length);
            Material selectedMat = clothMaterials[randomIndex];

            Material[] currentMaterials = targetRenderer.materials;

            if (materialIndex < currentMaterials.Length)
            {
                currentMaterials[materialIndex] = selectedMat;

                targetRenderer.materials = currentMaterials;
                LODtargetRenderer.materials = currentMaterials;
            }
        }        
    }

    
}
