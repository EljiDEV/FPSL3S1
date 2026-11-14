using UnityEditor.Experimental.GraphView;
using UnityEngine;
using DG.Tweening;

public class PlayerCam : MonoBehaviour
{
    [Header("Sensibility Cam")]
    [SerializeField] private float _sensX;
    [SerializeField] private float _sensY;
    [SerializeField] private Transform _orientation;
    
    [Header("Camera Holder")]
    [SerializeField] private Transform _cameraHolder;
    
    private float _xRotation;
    private float _yRotation;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float _mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * _sensX;
        float _mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * _sensY;

        _yRotation += _mouseX;
        _xRotation -= _mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        
        transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        _orientation.rotation = Quaternion.Euler(0, _yRotation, 0);
    }
    
    public void WalkingEffect(float _effectAmount, float _effectDuration)
    {
        _cameraHolder.DOKill();
        Sequence dipSeq = DOTween.Sequence();
        dipSeq.Append(_cameraHolder.DOLocalMoveY(-_effectAmount, _effectDuration * 0.5f).SetEase(Ease.OutQuad));
        dipSeq.Append(_cameraHolder.DOLocalMoveY(_effectAmount, _effectDuration * 0.5f).SetEase(Ease.InQuad));
    }
}
