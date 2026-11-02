using UnityEngine;

[ExecuteInEditMode] // Permet de voir l'effet sans lancer le jeu !
public class VHSPostProcess : MonoBehaviour
{
    public Material vhsMaterial;

    // Cette fonction est magique : elle intercepte le rendu de la caméra
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (vhsMaterial != null)
        {
            // Applique le matériel (et donc le shader) à l'image
            Graphics.Blit(source, destination, vhsMaterial);
        }
        else
        {
            // Si pas de matériel, on affiche l'image normale
            Graphics.Blit(source, destination);
        }
    }
}