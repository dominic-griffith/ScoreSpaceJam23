using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] private Transform _cameraPosition;
    [SerializeField] private PlayerManager _pm;
    private void Update()
    {
        if (!_pm.NameReady()) return;
        transform.position = _cameraPosition.position;
    }
}
