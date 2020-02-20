using UnityEditor;
using UnityEngine.UIElements;

namespace PiRhoSoft.Expressions.Editor
{
	[CustomPropertyDrawer(typeof(Expression), true)]
	public class ExpressionDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			return new ExpressionField(property);
		}
	}
}
