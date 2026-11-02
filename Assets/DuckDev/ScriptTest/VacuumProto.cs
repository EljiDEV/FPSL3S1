using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

public class VacuumProto : MonoBehaviour
{
    public GameObject playerRoot;
    [Header("Filtres & Portée")]
    public LayerMask suckableLayer;
    public LayerMask biteLayer;
    [Tooltip("Mettez les layers où le joueur ne doit pas pouvoir aspirer l'objet (murs, sols, etc.)")]
    public LayerMask obstacleLayer;
    public bool biteObjectsBlockSuction = true;
    public float reach = 10f;
    public float angle = 45f;
    public float absorbDistance = 1.5f;

    [Header("Aspiration")]
    public Transform suctionPoint;
    public float speed = 15f;
    public int capacity = 10;
    private Stack<GameObject> storedObjects = new Stack<GameObject>();

    private GameObject heldBiteObject = null;
    private Rigidbody heldBiteRb = null;
    private Collider[] heldBiteColliders = null;
    private Collider[] playerColliders = null;
    private Quaternion heldRotationOffset = Quaternion.identity;

    private Vector3 suctionInitialLocalPos;
    private Quaternion suctionInitialLocalRot;
    private Vector3 launchInitialLocalPos;
    private Quaternion launchInitialLocalRot;

    private bool originalUseGravity;
    private float originalLinearDamping;
    private float originalAngularDamping;
    private float originalMass;
    private CollisionDetectionMode originalCollisionMode;
    private RigidbodyInterpolation originalInterpolation;

    [Header("Tir & Maintien")]
    public Transform launchPoint;
    public float launchVelocity = 25f;
    [Tooltip("Fermeté du maintien d'objet pour les gros objets : plus bas = flotte devant l'aspi, plus haut = reste collé au canon")]
    public float holdSpringForce = 15f;
    [Tooltip("Sécurité rappel objet contre les bugs de collision")]
    public float maxHoldSpeed = 25f;
    [Tooltip("Garde la rotation initiale de l'objet par rapport au joueur")]
    public bool keepInitialRelativeRotation = true;

    [Header("Apparence de l'objet tenu (Bite)")]
    [Range(0f, 1f)]
    public float heldObjectAlpha = 0.5f;
    public Material customTransparentMaterial = null;

    private struct RendererMaterialBackup
    {
        public Renderer renderer;
        public Material[] originalSharedMaterials;
        public Material[] instancedMaterials;
    }
    private List<RendererMaterialBackup> heldRendererBackups = new List<RendererMaterialBackup>();

    private int suckableLayerIndex;
    private int biteLayerIndex;

    private float nextActionTime = 0f;
    private float actionCooldown = 0.2f;

    void Start()
    {
        suckableLayerIndex = (int)Mathf.Log(suckableLayer.value, 2);
        biteLayerIndex = (int)Mathf.Log(biteLayer.value, 2);

        if (suctionPoint != null)
        {
            suctionInitialLocalPos = suctionPoint.localPosition;
            suctionInitialLocalRot = suctionPoint.localRotation;
        }

        if (launchPoint != null)
        {
            launchInitialLocalPos = launchPoint.localPosition;
            launchInitialLocalRot = launchPoint.localRotation;
        }

        GatherPlayerColliders();
    }

    private void GatherPlayerColliders()
    {
        HashSet<Collider> cols = new HashSet<Collider>();
        HashSet<Rigidbody> playerRbs = new HashSet<Rigidbody>();

        void CollectFromRoot(Transform rootTransform)
        {
            if (rootTransform == null) return;
            foreach (var c in rootTransform.GetComponentsInChildren<Collider>(true)) cols.Add(c);
            foreach (var r in rootTransform.GetComponentsInChildren<Rigidbody>(true)) playerRbs.Add(r);
        }

        
        if (playerRoot != null) CollectFromRoot(playerRoot.transform.root);
        CollectFromRoot(transform.root);

        
        if (Camera.main != null) CollectFromRoot(Camera.main.transform.root);

        
        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null) CollectFromRoot(taggedPlayer.transform.root);

        CharacterController cc = FindFirstObjectByType<CharacterController>();
        if (cc != null) CollectFromRoot(cc.transform.root);

        
        if (suctionPoint != null)
        {
            foreach (var c in suctionPoint.GetComponentsInChildren<Collider>(true)) c.enabled = false;
        }
        if (launchPoint != null)
        {
            foreach (var c in launchPoint.GetComponentsInChildren<Collider>(true)) c.enabled = false;
        }
        
        foreach (Rigidbody rb in playerRbs)
        {
            if (rb != null)
            {
                rb.interpolation = RigidbodyInterpolation.None;
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }
        }

        playerColliders = new Collider[cols.Count];
        cols.CopyTo(playerColliders);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot(launchVelocity);
            return;
        }

        if (heldBiteObject != null)
        {
            if (Input.GetMouseButtonDown(1) && Time.time >= nextActionTime)
            {
                Shoot(0f);
                nextActionTime = Time.time + actionCooldown;
            }
        }
        else
        {
            if (Input.GetMouseButton(1) && Time.time >= nextActionTime)
            {
                Suck();
            }
        }
    }

    void LateUpdate()
    {
        
        if (suctionPoint != null)
        {
            suctionPoint.localPosition = suctionInitialLocalPos;
            suctionPoint.localRotation = suctionInitialLocalRot;
        }

        if (launchPoint != null)
        {
            launchPoint.localPosition = launchInitialLocalPos;
            launchPoint.localRotation = launchInitialLocalRot;
        }
    }

    void FixedUpdate()
    {
        if (heldBiteObject != null && heldBiteRb != null && suctionPoint != null)
        {
            Vector3 direction = suctionPoint.position - heldBiteRb.position;
            heldBiteRb.linearVelocity = Vector3.ClampMagnitude(direction * holdSpringForce, maxHoldSpeed);

            heldBiteRb.angularVelocity = Vector3.zero;
            Quaternion targetRotation = keepInitialRelativeRotation
                ? suctionPoint.rotation * heldRotationOffset
                : suctionPoint.rotation;

            heldBiteRb.MoveRotation(targetRotation);
        }
    }

    void Suck()
    {
        if (suctionPoint == null) return;

        LayerMask combinedLayers = suckableLayer | biteLayer;
        Collider[] hits = Physics.OverlapSphere(transform.position, reach, combinedLayers);

        LayerMask blockingMask = biteObjectsBlockSuction ? (obstacleLayer | biteLayer) : obstacleLayer;

        foreach (Collider hit in hits)
        {
            Rigidbody rb = hit.attachedRigidbody;
            if (rb == null) continue;

            GameObject rootObj = rb.gameObject;

            Vector3 targetCenter = hit.bounds.center;
            Vector3 directionToTarget = (targetCenter - suctionPoint.position).normalized;

            if (Vector3.Angle(suctionPoint.forward, directionToTarget) <= angle)
            {
                if (Physics.Linecast(suctionPoint.position, targetCenter, out RaycastHit lineHit, blockingMask, QueryTriggerInteraction.Ignore))
                {
                    if (lineHit.rigidbody != rb && lineHit.transform.root != rootObj.transform.root)
                    {
                        continue;
                    }
                }

                Vector3 pullDirection = (suctionPoint.position - hit.transform.position).normalized;
                float distance = Vector3.Distance(hit.ClosestPoint(suctionPoint.position), suctionPoint.position);

                if (distance <= absorbDistance)
                {
                    if (rootObj.layer == suckableLayerIndex)
                    {
                        if (storedObjects.Count < capacity)
                        {
                            AbsorbSuckable(rootObj, rb);
                            break;
                        }
                    }
                    else if (rootObj.layer == biteLayerIndex && heldBiteObject == null)
                    {
                        HoldBiteObject(rootObj, rb);
                        break;
                    }
                }
                else
                {
                    rb.linearVelocity = pullDirection * speed;
                }
            }
        }
    }

    void AbsorbSuckable(GameObject target, Rigidbody rb)
    {
        if (storedObjects.Contains(target)) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        target.SetActive(false);
        storedObjects.Push(target);
    }

    void HoldBiteObject(GameObject target, Rigidbody rb)
    {
        heldBiteObject = target;
        heldBiteRb = rb;

        originalUseGravity = heldBiteRb.useGravity;
        originalLinearDamping = heldBiteRb.linearDamping;
        originalAngularDamping = heldBiteRb.angularDamping;
        originalMass = heldBiteRb.mass;
        originalCollisionMode = heldBiteRb.collisionDetectionMode;
        originalInterpolation = heldBiteRb.interpolation;

        heldRotationOffset = Quaternion.Inverse(suctionPoint.rotation) * heldBiteRb.rotation;

        heldBiteRb.isKinematic = false;
        heldBiteRb.useGravity = false;
        heldBiteRb.mass = 0.01f;
        heldBiteRb.linearDamping = 5f;
        heldBiteRb.angularDamping = 5f;
        heldBiteRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        heldBiteRb.interpolation = RigidbodyInterpolation.Interpolate;

        heldBiteColliders = target.GetComponentsInChildren<Collider>(true);
        SetIgnorePlayerCollisions(heldBiteColliders, true);

        ApplyHeldTransparency(target);

        nextActionTime = Time.time + actionCooldown;
    }

    void Shoot(float appliedForce)
    {
        if (launchPoint == null) return;

        if (heldBiteObject != null && heldBiteRb != null)
        {
            RestoreOriginalMaterials();

            Collider[] colsToRestore = heldBiteColliders;
            heldBiteColliders = null;

            heldBiteRb.useGravity = originalUseGravity;
            heldBiteRb.linearDamping = originalLinearDamping;
            heldBiteRb.angularDamping = originalAngularDamping;
            heldBiteRb.mass = originalMass;
            heldBiteRb.interpolation = originalInterpolation;
            heldBiteRb.collisionDetectionMode = appliedForce > 0f
                ? CollisionDetectionMode.ContinuousDynamic
                : originalCollisionMode;

            heldBiteRb.linearVelocity = Vector3.zero;
            heldBiteRb.angularVelocity = Vector3.zero;

            float finalForce = appliedForce > 0f ? appliedForce : 2f;
            heldBiteRb.AddForce(launchPoint.forward * finalForce, ForceMode.Impulse);

            if (colsToRestore != null)
            {
                StartCoroutine(RestoreCollisionWhenClearCoroutine(colsToRestore));
            }

            heldBiteObject = null;
            heldBiteRb = null;
            return;
        }

        if (appliedForce > 0f && storedObjects.Count > 0)
        {
            GameObject obj = storedObjects.Pop();
            obj.transform.position = launchPoint.position;
            obj.transform.rotation = launchPoint.rotation;
            obj.SetActive(true);

            Collider[] objCols = obj.GetComponentsInChildren<Collider>(true);
            SetIgnorePlayerCollisions(objCols, true);
            StartCoroutine(RestoreCollisionWhenClearCoroutine(objCols));

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.AddForce(launchPoint.forward * appliedForce, ForceMode.Impulse);
            }
        }
    }

    private void SetIgnorePlayerCollisions(Collider[] objectColliders, bool ignore)
    {
        if (objectColliders == null || playerColliders == null) return;

        foreach (Collider col in objectColliders)
        {
            if (col == null) continue;
            foreach (Collider pCol in playerColliders)
            {
                if (pCol != null && col != pCol)
                {
                    Physics.IgnoreCollision(col, pCol, ignore);
                }
            }
        }
    }

    private IEnumerator RestoreCollisionWhenClearCoroutine(Collider[] objectColliders)
    {
        yield return new WaitForSeconds(0.15f);

        float timeout = 3f;
        float timer = 0f;

        while (timer < timeout && IsOverlappingPlayer(objectColliders))
        {
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        SetIgnorePlayerCollisions(objectColliders, false);
    }

    private bool IsOverlappingPlayer(Collider[] objectColliders)
    {
        if (objectColliders == null || playerColliders == null) return false;

        foreach (Collider col in objectColliders)
        {
            if (col == null || !col.enabled || !col.gameObject.activeInHierarchy) continue;

            foreach (Collider pCol in playerColliders)
            {
                if (pCol == null || !pCol.enabled || !pCol.gameObject.activeInHierarchy) continue;

                if (col.bounds.Intersects(pCol.bounds))
                {
                    if (Physics.ComputePenetration(
                        col, col.transform.position, col.transform.rotation,
                        pCol, pCol.transform.position, pCol.transform.rotation,
                        out _, out _))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void ApplyHeldTransparency(GameObject target)
    {
        RestoreOriginalMaterials();

        if (heldObjectAlpha >= 1f && customTransparentMaterial == null) return;

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            RendererMaterialBackup backup = new RendererMaterialBackup
            {
                renderer = rend,
                originalSharedMaterials = rend.sharedMaterials
            };

            Material[] currentMats = rend.materials;
            for (int i = 0; i < currentMats.Length; i++)
            {
                if (customTransparentMaterial != null)
                {
                    Material customInstance = new Material(customTransparentMaterial);
                    SetMaterialAlpha(customInstance, heldObjectAlpha);
                    Destroy(currentMats[i]);
                    currentMats[i] = customInstance;
                }
                else
                {
                    MakeMaterialTransparent(currentMats[i], heldObjectAlpha);
                }
            }

            rend.materials = currentMats;
            backup.instancedMaterials = currentMats;
            heldRendererBackups.Add(backup);
        }
    }

    private void RestoreOriginalMaterials()
    {
        foreach (var backup in heldRendererBackups)
        {
            if (backup.renderer != null)
            {
                backup.renderer.sharedMaterials = backup.originalSharedMaterials;
            }
            if (backup.instancedMaterials != null)
            {
                foreach (Material mat in backup.instancedMaterials)
                {
                    if (mat != null) Destroy(mat);
                }
            }
        }
        heldRendererBackups.Clear();
    }

    private void MakeMaterialTransparent(Material mat, float alpha)
    {
        if (mat.HasProperty("_Surface"))
        {
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
        }

        if (mat.HasProperty("_Mode"))
        {
            mat.SetFloat("_Mode", 3f);
        }

        mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = (int)RenderQueue.Transparent;

        SetMaterialAlpha(mat, alpha);
    }

    private void SetMaterialAlpha(Material mat, float alpha)
    {
        if (mat.HasProperty("_BaseColor"))
        {
            Color c = mat.GetColor("_BaseColor");
            c.a = alpha;
            mat.SetColor("_BaseColor", c);
        }
        if (mat.HasProperty("_Color"))
        {
            Color c = mat.GetColor("_Color");
            c.a = alpha;
            mat.SetColor("_Color", c);
        }
    }

    void OnDisable()
    {
        RestoreOriginalMaterials();
    }
}