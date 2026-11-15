using UnityEngine;

{
    [Header("Amplitude")]
    [Tooltip("Amplitude par axe (pas le déodorant)")]
    public bool mouvementOn = true;
    public Vector3 moveAmplitude = new Vector3(0f, 0.4f, 0f);
    [Tooltip("MoveSpeed")]
    public float moveSpeed = 1.5f;

    [Header("Rotation")]
    [Tooltip("Angle de rotation max par axe (pas non plus le gel douche de la meme marque)")]
    public Vector3 rotationAmplitude = new Vector3(2f, 0f, 2f);
    [Tooltip("RotationSpeed")]
    public float rotationSpeed = 1f;

    [Header("Offset")]
    [Tooltip("Offset Random au start")]
    public bool randomTiming = true;
    [Tooltip("Manual offset")]
    public float timeOffset = 0f;

    private Vector3 _startLocalPos;
    private Quaternion _startLocalRot;
    private CharacterController _playerCC;

    void Start()
    {
        _startLocalPos = transform.localPosition;
        _startLocalRot = transform.localRotation;

        if (randomTiming)
        {
            timeOffset = Random.Range(0f, 10f);
        }

        _playerCC = FindFirstObjectByType<CharacterController>();
    }

    void FixedUpdate()
    {
        float t = Time.time + timeOffset;

        
        Vector3 oldWorldPos = transform.position;
        Quaternion oldWorldRot = transform.rotation;
        bool playerOnPlatform = IsPlayerOnPlatform();
        
        Vector3 targetLocalPos = _startLocalPos + moveAmplitude * Mathf.Sin(t * moveSpeed);
        Quaternion targetLocalRot = _startLocalRot * Quaternion.Euler(rotationAmplitude * Mathf.Sin(t * rotationSpeed));

        Vector3 newWorldPos = transform.parent != null 
            ? transform.parent.TransformPoint(targetLocalPos) 
            : targetLocalPos;
        Quaternion newWorldRot = transform.parent != null 
            ? transform.parent.rotation * targetLocalRot 
            : targetLocalRot;

        
        if (playerOnPlatform && _playerCC != null)
        {
            Quaternion deltaRot = newWorldRot * Quaternion.Inverse(oldWorldRot);
            Vector3 playerOffset = _playerCC.transform.position - oldWorldPos;
            Vector3 totalDelta = (newWorldPos - oldWorldPos) + (deltaRot * playerOffset - playerOffset);

            
            if (totalDelta.y < 0f && !_playerCC.isGrounded)
            {
                totalDelta.y = 0f;
            }

            if (totalDelta.y > 0f)
            {
                
                _playerCC.Move(totalDelta);
                transform.SetLocalPositionAndRotation(targetLocalPos, targetLocalRot);
                Physics.SyncTransforms();
            }
            else
            {
                
                transform.SetLocalPositionAndRotation(targetLocalPos, targetLocalRot);
                Physics.SyncTransforms();
                _playerCC.Move(totalDelta);
            }
        }
        else
        {
            transform.SetLocalPositionAndRotation(targetLocalPos, targetLocalRot);
            Physics.SyncTransforms();
        }
    }

    private bool IsPlayerOnPlatform()
    {
        if (_playerCC == null || !_playerCC.gameObject.activeInHierarchy) return false;

        Bounds b = _playerCC.bounds;
        float radius = b.extents.x * 0.9f;
        float castDistance = (b.extents.y - radius) + 0.2f;

        RaycastHit[] hits = Physics.SphereCastAll(b.center, radius, Vector3.down, castDistance, ~0, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit hit in hits)
        {
            if (hit.normal.y > 0.4f && (hit.transform == transform || hit.transform.IsChildOf(transform)))
            {
                return true;
            }
        }

        return false;
    }
}