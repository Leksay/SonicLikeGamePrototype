using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditorInternal;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UObject = UnityEngine.Object;
using SObject = System.Object;

// проверка наличия слоя в маске
public static class LayerExtension
{
	public static bool Contains(this LayerMask layerMask, int layer)
	{
		return layerMask == (layerMask | (1 << layer));
	}
}

public static class Vector3Extend
{
	public static float GetXZDistance(this Vector3 from, Vector3 to)
	{
		to.y = from.y;
		return Vector3.Distance(from, to);
	}

	public static Vector3 GetXZVector(this Vector3 from, Vector3 to)
	{
		to.y = from.y;
		return to - from;
	}

	public static float GetXZAngle(this Vector3 from, Vector3 to)
	{
		to.y = from.y;
		return Vector3.Angle(from, to);
	}

	public static float GetXZAngleSigned(this Vector3 from, Vector3 to)
	{
		to.y = from.y;
		return Vector3.SignedAngle(from, to, Vector3.up);
	}

	public static Vector3 GetAngleVector3(this Vector3 from, float angle)
	{
		return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad), 0f);
	}

	public static Vector3 GetVector3(this Vector2Int from)
	{
		return new Vector3(from.x, from.y, 0f);
	}

	public static Vector2Int GetVector2Int(this Vector3 from)
	{
		return new Vector2Int((int)from.x, (int)from.y);
	}
}

public static class RectIntExtend
{
	public static Vector2 VectorCenter(this RectInt from)
	{
		return new Vector2(from.x + from.width * 0.5f, from.y + from.height * 0.5f);
	}

	public static Vector3 VectorSize(this RectInt from)
	{
		return new Vector3(from.width, from.height, 0f);
	}

	public static Vector3 VectorPos(this RectInt from)
	{
		return new Vector3(from.x, from.y, 0f);
	}
}

public static class MonoBehaviourExtension
{
	/// <summary>
	///     Call delegate after pause (time scaled)
	/// </summary>
	/// <param name="mn"></param>
	/// <param name="func">Delegate</param>
	/// <param name="time">Pause time</param>
	public static Coroutine InvokeDelegate(this MonoBehaviour mn, Action func, float time)
	{
		if (!mn.isActiveAndEnabled) return null;
		return mn.StartCoroutine(InvokeDelegateCor(func, time));
	}

	private static IEnumerator InvokeDelegateCor(Action func, float time)
	{
		yield return new WaitForSeconds(time);
		func();
	}

	/// <summary>
	///     Call delegate after pause (time unscaled)
	/// </summary>
	/// <param name="mn"></param>
	/// <param name="func">Delegate</param>
	/// <param name="time">Pause time</param>
	public static Coroutine InvokeDelegateUnscaled(this MonoBehaviour mn, Action func, float time) => mn.StartCoroutine(InvokeDelegateUnscaledCor(func, time));
	private static IEnumerator InvokeDelegateUnscaledCor(Action func, float time)
	{
		yield return new WaitForSecondsRealtime(time);
		func();
	}

	/// <summary>
	///     Call delegate by time with normalized time in parameter (time scaled)
	/// </summary>
	/// <param name="mn"></param>
	/// <param name="func">Delegate with parameter float [0..1]</param>
	/// <param name="time">Time to work</param>
	/// <param name="endFunc">Delegate after end</param>
	public static Coroutine InvokeDelegate(this MonoBehaviour mn, Action<float> func, float time, Action endFunc = null) => mn.StartCoroutine(InvokeDelegateCor(func, time, endFunc));
	private static IEnumerator InvokeDelegateCor(Action<float> func, float time, Action endFunc)
	{
		var timer = 0f;
		while (timer <= time)
		{
			func(timer / time);
			yield return null;
			timer += Time.deltaTime;
		}
		func(1f);
		endFunc?.Invoke();
	}

	/// <summary>
	///     Call delegate by time with normalized time in parameter (time unscaled)
	/// </summary>
	/// <param name="mn"></param>
	/// <param name="func">Delegate with parameter float [0..1]</param>
	/// <param name="time">Time to work</param>
	/// <param name="endFunc">Delegate after end</param>
	public static Coroutine InvokeDelegateUnscaled(this MonoBehaviour mn, Action<float> func, float time, Action endFunc = null) => mn.StartCoroutine(InvokeDelegateUnscaledCor(func, time, endFunc));
	private static IEnumerator InvokeDelegateUnscaledCor(Action<float> func, float time, Action endFunc)
	{
		var timer = 0f;
		while (timer <= time)
		{
			func(timer / time);
			yield return null;
			timer += Time.unscaledDeltaTime;
		}
		endFunc?.Invoke();
	}

	/// <summary>
	/// Call delegate once when func is TRUE
	/// </summary>
	/// <param name="mn"></param>
	/// <param name="func"></param>
	/// <param name="action"></param>
	public static void InvokeCond(this MonoBehaviour mn, Func<bool> func, Action action) => mn.StartCoroutine(InvokeCondCor(func, action));
	private static IEnumerator InvokeCondCor(Func<bool> func, Action action)
	{
		while (!func.Invoke())
			yield return null;
		action.Invoke();
	}

	/// <summary>
	///     TRUE if cursor (touch) is over UI element
	/// </summary>
	/// <returns>TRUE if cursor (touch) is over UI element</returns>
	public static bool CursorOverUI(this MonoBehaviour mn)
	{
#if (UNITY_ANDROID || UNITY_IOS) && (!UNITY_EDITOR)
        int cursorID = Input.GetTouch(0).fingerId;
        return EventSystem.current.IsPointerOverGameObject(cursorID);
#else
		return EventSystem.current.IsPointerOverGameObject();
#endif
	}
}

#if UNITY_EDITOR
namespace EasyEditorGUI
{
	public static class eGUI
	{
        #region Labels size + shift by ident
		private const  float FixedIdentDefault = 60f;
		private static float _fixedIdent       = 60f;

		/// <summary>
		///     Size of indent
		/// </summary>
		public static float IndentSize => EditorGUI.indentLevel * 15f;

		/// <summary>
		///     Size of work area without indent
		/// </summary>
		public static float IndentWidth => EditorGUIUtility.currentViewWidth - IndentSize - 5f;

		/// <summary>
		///     Set label width to fixedIdent
		/// </summary>
		/// <param name="value">FALSE - set to default</param>
		public static void SetLabelWidth(bool value)
		{
			EditorGUIUtility.labelWidth = value ? IndentSize + _fixedIdent : 0f;
		}

		/// <summary>
		///     Set label width to fixedIdent with defined size
		/// </summary>
		/// <param name="size">defined size (less zero to set default)</param>
		/// <param name="value">FALSE - set to default</param>
		public static void SetLabelWidth(float size, bool value)
		{
			if (size < 0f)
				_fixedIdent = FixedIdentDefault;
			_fixedIdent = size;
			SetLabelWidth(value);
		}
        #endregion

        #region Type conversions
		/// <summary>
		///     Convert Quaternion to Vector4
		/// </summary>
		/// <param name="rot"></param>
		/// <returns></returns>
		public static Vector4 QuaternionToVector4(Quaternion rot)
		{
			return new Vector4(rot.x, rot.y, rot.z, rot.w);
		}

		/// <summary>
		///     Convert Vector4 to Quaternion
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		public static Quaternion Vector4ToQuaternion(Vector4 vec)
		{
			return new Quaternion(vec.x, vec.y, vec.z, vec.w);
		}
        #endregion

        #region Vertical scroll in EditorWindow
		private static Dictionary<int, Vector2> _scrollPoss;

		/// <summary>
		/// Start vertical scroll
		/// </summary>
		/// <param name="value">Editor window (for scroll state))</param>
		public static void StartScrollVertical(EditorWindow value)
		{
			var hash = value.GetHashCode();
			if (_scrollPoss == null)
				_scrollPoss = new Dictionary<int, Vector2>();
			if (!_scrollPoss.ContainsKey(hash))
				_scrollPoss.Add(hash, Vector2.zero);
			_scrollPoss[hash] = GUILayout.BeginScrollView(_scrollPoss[hash], false, true, GUIStyle.none, GUI.skin.verticalScrollbar);
			GUILayout.BeginVertical();
		}

		/// <summary>
		/// Start both scroll
		/// </summary>
		/// <param name="value">Editor window (for scroll state))</param>
		public static void StartScrollVertHoriz(EditorWindow value)
		{
			var hash = value.GetHashCode();
			if (_scrollPoss == null)
				_scrollPoss = new Dictionary<int, Vector2>();
			if (!_scrollPoss.ContainsKey(hash))
				_scrollPoss.Add(hash, Vector2.zero);
			_scrollPoss[hash] = GUILayout.BeginScrollView(_scrollPoss[hash], true, true, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar);
			GUILayout.BeginVertical();
		}

		/// <summary>
		/// End scroll (any of them)
		/// </summary>
		public static void EndScroll()
		{
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
		}
        #endregion

        #region Universal folding
		private static readonly Dictionary<object, bool> Foldouts2 = new Dictionary<object, bool>();

		/// <summary>
		/// Show fold
		/// </summary>
		/// <param name="text">Caption</param>
		/// <param name="object">Key for state</param>
		/// <returns></returns>
		public static bool ShowFold(string text, object @object)
		{
			var result = false;
			if (!Foldouts2.ContainsKey(@object))
				Foldouts2.Add(@object, false);
			else
				result = Foldouts2[@object];
			result             = EditorGUILayout.Foldout(result, text, true);
			Foldouts2[@object] = result;
			return result;
		}

		/// <summary>
		/// Show fold
		/// </summary>
		/// <param name="rect">View rect</param>
		/// <param name="text">Caption</param>
		/// <param name="object">Key for state</param>
		/// <param name="simply">Open by header click</param>
		/// <returns></returns>
		public static bool ShowFold(Rect rect, string text, object @object, bool simply = true)
		{
			var result = false;
			if (!Foldouts2.ContainsKey(@object))
				Foldouts2.Add(@object, false);
			else
				result = Foldouts2[@object];
			result             = EditorGUI.Foldout(rect, result, text, simply);
			Foldouts2[@object] = result;
			return result;
		}

		/// <summary>
		/// Return state of fold by key
		/// </summary>
		/// <param name="object"></param>
		/// <returns></returns>
		public static bool GetFoldState(object @object)
		{
			if (!Foldouts2.ContainsKey(@object)) return false;
			return Foldouts2[@object];
		}
        #endregion

        #region Field view
		/// <summary>
		///     Compact view Vector3 in EditorWindow
		/// </summary>
		/// <param name="prop">Property Vector3</param>
		/// <param name="width">Total width</param>
		/// <param name="gui">Custom label & hint</param>
		public static void ShowVector3(SerializedProperty prop, GUIContent gui, float width = 0f)
		{
			var lw = EditorGUIUtility.labelWidth;
			var v  = prop.vector3Value;
			var w  = GUILayout.Width(width > 0f ? width * 0.25f : 90f);
			GUILayout.BeginHorizontal();
			{
				SetLabelWidth(12f, true);
				if (gui == null) EditorGUILayout.LabelField(prop.name, w);
				else EditorGUILayout.LabelField(gui,                   w);
				v.x               = EditorGUILayout.FloatField("X", v.x, w);
				v.y               = EditorGUILayout.FloatField("Y", v.y, w);
				v.z               = EditorGUILayout.FloatField("Z", v.z, w);
				prop.vector3Value = v;
			}
			SetLabelWidth(lw, false);
			GUILayout.EndHorizontal();
		}

		/// <summary>
		///     Compact view Vector3 in EditorWindow
		/// </summary>
		/// <param name="prop">Property Vector3</param>
		/// <param name="width">Total width</param>
		public static void ShowVector3(SerializedProperty prop, float width = 0f)
		{
			ShowVector3(prop, null, width);
		}

		private static GUIStyle _toggleOff;
		private static GUIStyle _toggleOn;

		/// <summary>
		/// Show toggle as button
		/// </summary>
		/// <param name="name">Caption text</param>
		/// <param name="state">state</param>
		/// <param name="option">additional option</param>
		/// <returns></returns>
		public static bool ToggleButton(string name, bool state, GUILayoutOption option = null)
		{
			if (_toggleOff == null)
			{
				_toggleOff       = new GUIStyle(GUI.skin.button);
				_toggleOn        = new GUIStyle(GUI.skin.button);
				_toggleOn.normal = _toggleOn.onActive;
			}

			if (option == null)
			{
				if (GUILayout.Button(name, state ? _toggleOn : _toggleOff))
					return !state;
			}
			else
			{
				if (GUILayout.Button(name, state ? _toggleOn : _toggleOff, option))
					return !state;
			}

			return state;
		}

		/// <summary>
		/// Show toggle as button
		/// </summary>
		/// <param name="position"></param>
		/// <param name="name">Caption text</param>
		/// <param name="state">state</param>
		/// <returns></returns>
		public static bool ToggleButton(Rect position, string name, bool state)
		{
			if (_toggleOff == null)
			{
				_toggleOff       = new GUIStyle(GUI.skin.button);
				_toggleOn        = new GUIStyle(GUI.skin.button);
				_toggleOn.normal = _toggleOn.onActive;
			}

			if (GUI.Button(position, name, state ? _toggleOn : _toggleOff))
				return !state;
			return state;
		}

		/// <summary>
		/// Show text as box with label (readonly)
		/// </summary>
		/// <param name="position"></param>
		/// <param name="label"></param>
		/// <param name="text"></param>
		public static void TextBox(Rect position, string label, string text)
		{
			EditorGUI.TextField(position, label, text, GUI.skin.box);
		}

		/// <summary>
		/// Show text as box (readonly)
		/// </summary>
		/// <param name="position"></param>
		/// <param name="text"></param>
		public static void TextBox(Rect position, string text)
		{
			EditorGUI.TextField(position, text, GUI.skin.box);
		}
        #endregion

		/// <summary>
		/// Show divider line
		/// </summary>
		/// <param name="position"></param>
		public static void Divider(Rect position)
		{
			EditorGUI.LabelField(position, "", GUI.skin.horizontalSlider);
		}

        #region Reorderable list
		private static readonly Dictionary<string, ReorderableList> Rlist      = new Dictionary<string, ReorderableList>();
		public static readonly  Dictionary<string, string>          RlistTitle = new Dictionary<string, string>();
		private static readonly Dictionary<string, object>          RlistFold  = new Dictionary<string, object>();

		/// <summary>
		/// Add array property to Reorderable array
		/// </summary>
		/// <param name="prop"></param>
		/// <param name="title"></param>
		/// <param name="draggable">Is draggable items</param>
		/// <param name="add">Can add items</param>
		public static void AddArray(SerializedProperty prop, string title, bool draggable = true, bool add = true)
		{
			if (prop == null) return;
			if (!Rlist.ContainsKey(GetPropPath(prop)) || Rlist[GetPropPath(prop)].serializedProperty != prop)
			{
				string s = "{0:D1}";
				var rl = new ReorderableList(prop.serializedObject, prop, draggable, false, add, add)
				{
					drawHeaderCallback =
						rect => { EditorGUI.LabelField(rect, title); },
					drawElementCallback =
						(rect, index, isActive, isFocused) =>
						{
							EditorGUI.PropertyField(rect,
								prop
									.GetArrayElementAtIndex(index),
								new
									GUIContent(string
										.Format(s,
											index)));
						}
				};
				Rlist[GetPropPath(prop)]      = rl;
				RlistTitle[GetPropPath(prop)] = title;
				if (!RlistFold.ContainsKey(GetPropPath(prop)))
					RlistFold[GetPropPath(prop)] = new object();
			}
		}

		/// <summary>
		/// Show array property at Rect
		/// </summary>
		/// <param name="prop"></param>
		/// <param name="rect"></param>
		public static void ShowArray(SerializedProperty prop, Rect rect)
		{
			if (Rlist.ContainsKey(GetPropPath(prop)))
			{
				rect.height = EditorGUIUtility.singleLineHeight;
				if (ShowFold(rect, RlistTitle[GetPropPath(prop)], RlistFold[GetPropPath(prop)]))
				{
					rect.height = GetArrayHeight(prop);
					var lw = EditorGUIUtility.labelWidth;
					SetLabelWidth(50f, true);
					Rlist[GetPropPath(prop)].DoList(rect);
					EditorGUIUtility.labelWidth = lw;
				}
			}
		}

		private static readonly Dictionary<object, ReorderableList> RList2 = new Dictionary<object, ReorderableList>();

		public static void ShowArray<T>(string title, T[] array)
		{
			if (!RList2.ContainsKey(array)) RList2.Add(array, new ReorderableList(array, typeof(T)));
			RList2[array].drawHeaderCallback = rect => EditorGUI.LabelField(rect, title);
			RList2[array].DoLayoutList();
		}

		public static void ShowArray<T>(string title, List<T> array)
		{
			if (!RList2.ContainsKey(array))
				RList2.Add(array, new ReorderableList(array, typeof(T), true, true, true, true));
			RList2[array].drawHeaderCallback = rect => EditorGUI.LabelField(rect, title);
			RList2[array].DoLayoutList();
		}

		/// <summary>
		/// Return array property rect height
		/// </summary>
		/// <param name="prop"></param>
		/// <returns></returns>
		public static float GetArrayHeight(SerializedProperty prop)
		{
			if (Rlist.ContainsKey(GetPropPath(prop)))
			{
				if (!GetFoldState(RlistFold[GetPropPath(prop)]))
					return EditorGUIUtility.singleLineHeight;
				return Rlist[GetPropPath(prop)].GetHeight();
			}

			return 0f;
		}

		public static string GetPropPath(SerializedProperty prop)
		{
			return $"{prop.serializedObject.targetObject.name}.{prop.propertyPath}";
		}
        #endregion

        #region Mouse tools
		/// <summary>
		///     Invoke method after click (default: RMB)
		/// </summary>
		/// <param name="mouseButton">Mouse button</param>
		/// <returns>Is clicked</returns>
		public static bool CheckMouseClick(int mouseButton = 1)
		{
			var w = EditorWindow.mouseOverWindow;
			if (w == null) return false;
			var current = Event.current;
			if (!current.isMouse || current.button != mouseButton) return false;
			return GUILayoutUtility.GetLastRect().Contains(current.mousePosition);
		}

		/// <summary>
		///     Invoke method if mouse over element
		/// </summary>
		/// <returns>Is over element</returns>
		public static bool CheckMouseOver()
		{
			var w = EditorWindow.mouseOverWindow;
			if (w == null) return false;
			var current = Event.current;
			return GUILayoutUtility.GetLastRect().Contains(current.mousePosition);
		}

		public static bool CheckMouseButton(int button, bool ctrl = false, bool shift = false, bool alt = false)
		{
			var ev = Event.current;
			return ev.button == button && ev.type == EventType.MouseDown && ev.control == ctrl && ev.shift == shift && ev.alt == alt;
		}
        #endregion

        #region Get color by object
		public static Color GetColorByInstanceID(UObject @object, int step = 2)
		{
			var id         = @object.GetInstanceID();
			if (id < 0) id += int.MaxValue;
			Random.InitState(id);
			var c = new Color(Random.Range(0, step + 1) / (float)step, Random.Range(0, step + 1) / (float)step,
				Random.Range(0,               step + 1) / (float)step, 1f);
			return c;
		}

		public static Color GetColorByInstanceID(object @object, int step = 2)
		{
			var id         = @object.GetHashCode();
			if (id < 0) id += int.MaxValue;
			Random.InitState(id);
			var c = new Color(Random.Range(0, step + 1) / (float)step, Random.Range(0, step + 1) / (float)step,
				Random.Range(0,               step + 1) / (float)step, 1f);
			return c;
		}
        #endregion

        #region Get IDs
		public static int GetID(object @object)
		{
			return @object.GetHashCode();
		}

		public static int GetID(UObject @object)
		{
			return @object.GetInstanceID();
		}

		public static string GetFullPath(Transform target, string divider = " → ")
		{
			if (target == null) return "< Missing object >";
			var s = target.gameObject.name;
			var p = target;
			while (p.parent != null)
			{
				var parent = p.parent;
				s = $"{parent.gameObject.name}{divider}{s}";
				p = parent;
			}
			return s;
		}
        #endregion

		public static T GetObjectFromProperty<T>(FieldInfo fieldInfo, SerializedProperty property) where T : class
		{
			var obj = fieldInfo.GetValue(property.serializedObject.targetObject);
			if (obj == null) return null;
			T actualObject;
			if (obj.GetType().IsArray)
			{
				var index = Convert.ToInt32(new string(property.propertyPath.Where(c => char.IsDigit(c)).ToArray()));
				actualObject = ((T[])obj)[index];
			}
			else
			{
				actualObject = obj as T;
			}

			return actualObject;
		}

		/// <summary>
		///     Mark SerializedObject as changed + mark scene as changed
		/// </summary>
		/// <param name="object"></param>
		public static void MarkAsDirty(SerializedObject @object = null)
		{
			@object?.ApplyModifiedProperties();
			EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
		}

		/// <summary>
		///     Mark object as changed + mark scene as changed
		/// </summary>
		/// <param name="object"></param>
		public static void SetDirty(UObject @object)
		{
			if (@object != null)
				EditorUtility.SetDirty(@object);
			EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
		}
	}
}

#endif
