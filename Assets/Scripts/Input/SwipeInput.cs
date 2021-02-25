using UnityEngine;
using UnityEngine.UI;
public class SwipeInput : MonoBehaviour
{
    public delegate void PlayerSwipe(SwipeType swipeType);
    public static event PlayerSwipe OnPlayerSwiped;

    [SerializeField] private float swipeRange;
    [SerializeField] private float tapRange;
    [SerializeField] private Text debugText;

    private Vector2 startPosition;
    private Vector2 currentPosition;
    private Vector2 endPosition;
    private bool stopTouch;
    private SwipeType swipeType;

    public enum SwipeType
    {
        Left, Right, Up, Down, Tap
    }

    private void Update()
    {
        Swipe();
    }

    private void Swipe()
    {
        Vector2 distance = Vector2.zero; ;
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            startPosition = Input.mousePosition;
            stopTouch = false;
        }
        else if (stopTouch == false && Input.GetMouseButton(0))
        {
            currentPosition = Input.mousePosition;
            distance = currentPosition - startPosition;
            swipeType = ChekSwipe(distance);
            if (stopTouch)
                NotifiyListeners(swipeType);
        }
        else if (stopTouch == false && Input.GetMouseButtonUp(0))
        {
            stopTouch = true;
            swipeType = ChekSwipe(startPosition - endPosition);
            debugText.text = "Tap";
        }
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            startPosition = Input.GetTouch(0).position;
            stopTouch = false;
        }
        else if (stopTouch == false && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            currentPosition = Input.GetTouch(0).position;
            distance = currentPosition - startPosition;
            swipeType = ChekSwipe(distance);
            if (stopTouch)
                NotifiyListeners(swipeType);
        }
        else if (stopTouch == false && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            stopTouch = true;
            swipeType = ChekSwipe(startPosition - endPosition);
            debugText.text = "Tap";
        }
#endif


    }

    private SwipeType ChekSwipe(Vector2 distance)
    {

        if (distance.x < -swipeRange)
        {
            stopTouch = true;
            debugText.text = "Left";
            return SwipeType.Left;
        }
        else if (distance.x > swipeRange)
        {
            stopTouch = true;
            debugText.text = "Right";
            return SwipeType.Right;
        }
        else if (distance.y < -swipeRange)
        {
            stopTouch = true;
            debugText.text = "Down";
            return SwipeType.Down;
        }
        else if (distance.y > swipeRange)
        {
            stopTouch = true;
            debugText.text = "Up";
            return SwipeType.Up;
        }
        else
        {
            return SwipeType.Tap;
        }

    }

    private void NotifiyListeners(SwipeType swipeType)
    {
        OnPlayerSwiped?.Invoke(swipeType);
    }

}
