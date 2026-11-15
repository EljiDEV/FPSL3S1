using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TheDoorsRenforced : MonoBehaviour
{
    public string keyTag = "RenforcedKey";
    public float minImpactSpeed = 10f;
    public float breakForceMultiplier = 1.5f;
    public float breakTorque = 10f;
    private GameObject child;
    [SerializeField] public bool NeedKey = true;

    [Header("Visuel du Bouton (FBX)")]
    [Tooltip("Assigne le renderer connard")]
    public Renderer buttonRenderer;
    [Tooltip("index du material 1 pour le bouton standard")]
    public int materialIndex = 0;
    [Tooltip("Material custom")]
    public Material activatedMaterial;
    [Tooltip("Si tu es con je le met en vert par défaut")]
    public Color activatedColor = Color.green;

    private Rigidbody doorRb;
    private bool isBroken = false;

    void Start()
    {
        doorRb = GetComponent<Rigidbody>();
        doorRb.isKinematic = true;
        child = this.transform.GetChild(0).gameObject;

        
        if (buttonRenderer == null)
        {
            buttonRenderer = GetComponent<Renderer>();
            if (buttonRenderer == null)
            {
                foreach (Renderer r in GetComponentsInChildren<Renderer>())
                {
                    if (child == null || !r.transform.IsChildOf(child.transform))
                    {
                        buttonRenderer = r;
                        break;
                    }
                }
            }
        }

        if (child != null)
        {
            Debug.Log($"{child.name} a été assigné correctement au bouton {this.name}");
        }
        else
        {
            Debug.Log("pas d'enfant dans la cave");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collision");

        if (NeedKey && collision.gameObject.CompareTag(keyTag))
        {
            Debug.Log("clé détectée");
            disabledChild();
        }

        if (NeedKey == false && collision.gameObject)
        {
            disabledChild();
            Debug.Log("pas besoin de clé");
        }
    }

    void disabledChild()
    {
        if (child != null)
        {
            child.SetActive(false);
        }

        SwitchButtonToGreen();
    }

    private void SwitchButtonToGreen()
    {
        if (buttonRenderer == null) return;

        Material[] mats = buttonRenderer.materials;

        if (materialIndex < 0 || materialIndex >= mats.Length)
        {
            Debug.LogWarning($"[TheDoorsRenforced] L'index de matériau {materialIndex} n'existe pas sur {buttonRenderer.name} (Total: {mats.Length}).");
            return;
        }

        
        if (activatedMaterial != null)
        {
            mats[materialIndex] = activatedMaterial;
            buttonRenderer.materials = mats;
        }
        else
        {
            
            if (mats[materialIndex].HasProperty("_BaseColor"))
                mats[materialIndex].SetColor("_BaseColor", activatedColor);
            if (mats[materialIndex].HasProperty("_Color"))
                mats[materialIndex].SetColor("_Color", activatedColor);
        }
    }
}