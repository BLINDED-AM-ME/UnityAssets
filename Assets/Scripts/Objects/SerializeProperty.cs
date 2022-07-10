using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BLINDED_AM_ME
{
	// Thanks Unity for making this simple

	/// <summary>
	/// https://docs.unity3d.com/ScriptReference/PropertyAttribute.htmlhttps://docs.unity3d.com/ScriptReference/PropertyAttribute.html
	/// <br/> A custom attributes can be hooked up with a custom PropertyDrawer class 
	/// <br/> to control how a script variable with that attribute is shown in the Inspector.
	/// </summary>
	/// <remarks> 
	/// 
	/// <br/> [SerializeField]
	/// <br/> [SerializeProperty(nameof(Id))]
	/// <br/> private string _id;
	/// <br/> public string Id
	/// <br/> {
	/// <br/> get => _id;
	/// <br/> set => SetProperty(ref _id, value);
	/// <br/> }
	/// 
	/// </remarks> 

	public class SerializeProperty : PropertyAttribute
	{
		public string PropertyName { get; private set; }

		public SerializeProperty(string propertyName)
		{
			PropertyName = propertyName;
		}
	}

#if UNITY_EDITOR

	/// <summary>
	///  https://docs.unity3d.com/ScriptReference/PropertyDrawer.html
	/// <br/> Use this to create custom drawers for your own Serializable classes 
	/// <br/> or for script variables with custom PropertyAttributes.
	///	<br/> PropertyDrawers have two uses:
	/// <br/> Customize the GUI of every instance of a Serializable class.
	/// <br/> Customize the GUI of script members with custom PropertyAttributes.
	/// <br/> If you have a custom Serializable class, 
	/// <br/> you can use a PropertyDrawer to control how it looks in the Inspector.
	/// <br/> Consider the Serializable class Ingredient in the script below:
	/// </summary>
	[CustomPropertyDrawer(typeof(SerializeProperty))]
	public class SerializePropertyDrawer : PropertyDrawer
	{
		// https://docs.unity3d.com/ScriptReference/SerializedObject.html
		// https://docs.unity3d.com/ScriptReference/SerializedProperty.html
		// https://docs.unity3d.com/ScriptReference/SerializeField.html
		// https://docs.unity3d.com/ScriptReference/EditorGUI.html

		public override bool CanCacheInspectorGUI(SerializedProperty property)
		{
			return base.CanCacheInspectorGUI(property);
		}
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, true);
			//return base.GetPropertyHeight(property, label);
		}
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			return base.CreatePropertyGUI(property);
		}

		// Draw the property inside the given rect
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.

			label = EditorGUI.BeginProperty(position, label, property);

			// Starts a new code block to check for GUI changes.
			EditorGUI.BeginChangeCheck();

			var targetObject = property.serializedObject.targetObject;
			var targetAttribute = (SerializeProperty)attribute;
			var properties = targetObject.GetType().GetProperties();
			var propertyInfo = properties.FirstOrDefault(p => p.Name == targetAttribute.PropertyName);

			if (propertyInfo == null)
				return;

			// get
			var value = propertyInfo.GetValue(targetObject);

			// show
			switch (property.propertyType)
			{
				case SerializedPropertyType.Integer:
					value = EditorGUI.IntField(position, label, (int)value);
					break;
				case SerializedPropertyType.Float:
					value = EditorGUI.FloatField(position, label, (float)value);
					break;
				case SerializedPropertyType.Boolean:
					value = EditorGUI.Toggle(position, label, (bool)value);
					break;
				case SerializedPropertyType.String:
					value = EditorGUI.TextField(position, label, (string)value);
					break;
				case SerializedPropertyType.Vector2:
					value = EditorGUI.Vector2Field(position, label, (Vector2)value);
					break;
				case SerializedPropertyType.Vector3:
					value = EditorGUI.Vector3Field(position, label, (Vector3)value);
					break;
				case SerializedPropertyType.Vector4:
					value = EditorGUI.Vector4Field(position, label, (Vector4)value);
					break;
				case SerializedPropertyType.Color:
					value = EditorGUI.ColorField(position, label, (Color)value);
					break;
				case SerializedPropertyType.Enum:
					value = EditorGUI.EnumPopup(position, label, (Enum)value);
					break;
				case SerializedPropertyType.ObjectReference:
					value = EditorGUI.ObjectField(position, label, (UnityEngine.Object)value, propertyInfo.PropertyType, true);
					break;
				case SerializedPropertyType.ExposedReference:
					value = EditorGUI.ObjectField(position, label, (UnityEngine.Object)value, propertyInfo.PropertyType, true);
					break;
				case SerializedPropertyType.Rect:
					value = EditorGUI.RectField(position, label, (Rect)value);
					break;
				default:
					throw new NotImplementedException(
						$"Property type {Enum.GetName(typeof(SerializedPropertyType), property.propertyType)} " +
						$"needs to be added check out https://docs.unity3d.com/ScriptReference/EditorGUI.html");
			}

			// Only assign the value back if it was actually changed by the user.
			// Otherwise a single value will be assigned to all objects when multi-object editing,
			// even when the user didn't touch the control.
			if (EditorGUI.EndChangeCheck())
			{
				// TODO handle Undo (ctrl + z)

				// Set
				propertyInfo.SetValue(targetObject, value);

				//Canvas.ForceUpdateCanvases();
				//EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Repaint();
				//SceneView.RepaintAll();
				//EditorWindow.GetWindow<SceneView>().Repaint();
				//HandleUtility.Repaint();
				EditorUtility.SetDirty(targetObject);
			}

			EditorGUI.EndProperty();
		}
	}
#endif
}

