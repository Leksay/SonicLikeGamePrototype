using UnityEngine;

public class SwipeInput : MonoBehaviour
{
	public delegate void            PlayerSwipe(SwipeType swipeType);
	public static event PlayerSwipe OnPlayerSwiped;

	[SerializeField] private float swipeRange;
	[SerializeField] private float tapRange;

	private Vector2   startPosition;
	private Vector2   currentPosition;
	private Vector2   endPosition;
	private bool      stopTouch;
	private SwipeType swipeType;
	public SwipeInput(Vector2 endPosition)
	{
		this.endPosition = endPosition;
	}

	public enum SwipeType
	{
		Left, Right, Up, Down, Tap
	}

	private void Update() => Swipe();

	private void Swipe()
	{
		var distance = Vector2.zero;

#if UNITY_EDITOR
		if (Input.GetMouseButtonDown(0))
		{
			startPosition = Input.mousePosition;
			stopTouch     = false;
		}
		else if (stopTouch == false && Input.GetMouseButton(0))
		{
			currentPosition = Input.mousePosition;
			distance         = currentPosition - startPosition;
			swipeType       = ChekSwipe(distance);
			if (stopTouch)
				NotifiyListeners(swipeType);
		}
		else if (stopTouch == false && Input.GetMouseButtonUp(0))
		{
			stopTouch = true;
			swipeType = ChekSwipe(startPosition - endPosition);
		}

		if (Input.GetKeyDown(KeyCode.LeftArrow)) OnPlayerSwiped?.Invoke(SwipeType.Left);
		if (Input.GetKeyDown(KeyCode.RightArrow)) OnPlayerSwiped?.Invoke(SwipeType.Right);
		if (Input.GetKeyDown(KeyCode.UpArrow)) OnPlayerSwiped?.Invoke(SwipeType.Up);
		if (Input.GetKeyDown(KeyCode.DownArrow)) OnPlayerSwiped?.Invoke(SwipeType.Down);
#elif UNITY_ANDROID || UNITY_IOS
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
        }
#endif


	}

	private SwipeType ChekSwipe(Vector2 distance)
	{

		//if (distance.x < -swipeRange)
		//{
		//    stopTouch = true;
		//    return SwipeType.Left;
		//}
		//else if (distance.x > swipeRange)
		//{
		//    stopTouch = true;
		//    return SwipeType.Right;
		//}
		//else if (distance.y < -swipeRange)
		//{
		//    stopTouch = true;
		//    return SwipeType.Down;
		//}
		//else if (distance.y > swipeRange)
		//{
		//    stopTouch = true;
		//    return SwipeType.Up;
		//}
		//else
		//{
		//    return SwipeType.Tap;
		//}

		if (distance.sqrMagnitude > swipeRange)
		{
			// x-axis swipe
			if (Mathf.Abs(distance.x) > Mathf.Abs(distance.y))
			{
				if (distance.x < 0)
				{
					stopTouch = true;
					return SwipeType.Left;
				}
				else
				{
					stopTouch = true;
					return SwipeType.Right;
				}
			}

			// y-axis swipe
			else
			{
				if (distance.y < 0)
				{
					stopTouch = true;
					return SwipeType.Down;
				}
				else
				{
					stopTouch = true;
					return SwipeType.Up;
				}
			}
		}
		else
		{
			return SwipeType.Tap;
		}

	}

	private void NotifiyListeners(SwipeType swipeType) => OnPlayerSwiped?.Invoke(swipeType);

	private void OnDestroy()
	{
		OnPlayerSwiped = null;
	}

}
