using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSlider : MonoBehaviour
{
  const float _speed = 2f;

  [SerializeField] Transform _gamePos;
  [SerializeField] Transform _titlePos;
  [SerializeField] Transform _menuPos;

  Vector3 _goalPos;
  Quaternion _goalRot;

  void Start()
  {
    InstantJumpTo(_titlePos);
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Alpha0))
    {
      SlideTo(_titlePos);
    }
    if (Input.GetKeyDown(KeyCode.Alpha1))
    {
      SlideTo(_menuPos);
    }
    if(Input.GetKeyDown(KeyCode.Alpha2))
    {
      SlideTo(_gamePos);
    }

    transform.position = Vector3.Slerp(transform.position, _goalPos, _speed * Time.deltaTime);
    transform.rotation = Quaternion.Slerp(transform.rotation, _goalRot, _speed * Time.deltaTime);
  }

  void SlideTo(Transform goal)
  {
    _goalPos = goal.position;
    _goalRot = goal.rotation;
  }

  void InstantJumpTo(Transform goal)
  {
    _goalPos = goal.position;
    _goalRot = goal.rotation;
    transform.position = goal.position;
    transform.rotation = goal.rotation;
  }
}
