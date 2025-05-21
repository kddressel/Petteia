using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSlider : MonoBehaviour
{
    const float _speed = 2f;

    public static Position StartPosition = Position.Title;

    public enum Position
    {
        Title,
        Menu,
        Game
    }

    [SerializeField] Transform _gamePos;
    [SerializeField] Transform _titlePos;
    [SerializeField] Transform _menuPos;

    Vector3 _goalPos;
    Quaternion _goalRot;

    void Start()
    {
        InstantJumpTo(StartPosition);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SlideToTitlePos();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SlideToMenuPos();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SlideToGamePos();
        }

        transform.position = Vector3.Slerp(transform.position, _goalPos, _speed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, _goalRot, _speed * Time.deltaTime);
    }

    public void SlideToTitlePos()
    {
        SlideTo(_titlePos);
    }
    public void SlideToMenuPos()
    {
        SlideTo(_menuPos);
    }
    public void SlideToGamePos()
    {
        SlideTo(_gamePos);
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

    public void InstantJumpTo(Position pos)
    {
        switch (pos)
        {
            case Position.Title:
                InstantJumpTo(_titlePos);
                break;
            case Position.Menu:
                InstantJumpTo(_menuPos);
                break;
            case Position.Game:
                InstantJumpTo(_gamePos);
                break;
        }
    }
}
