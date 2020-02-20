using PiRhoSoft.Utilities.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace PiRhoSoft.Expressions.Editor
{
	[CustomPropertyDrawer(typeof(VariableSource))]
	public class VariableSourceDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var frame = new Frame
			{
				Label = property.displayName,
				Tooltip = this.GetTooltip()
			};

			var typeProperty = property.FindPropertyRelative(nameof(VariableSource.Type));
			var expressionProperty = property.FindPropertyRelative(nameof(VariableSource.Expression));
			var valueProperty = property.FindPropertyRelative(nameof(VariableSource.Value));

			var typeField = new PropertyField(typeProperty);
			var expressionField = new PropertyField(expressionProperty);
			var valueField = new PropertyField(valueProperty);

			frame.Content.Add(typeField);
			frame.Content.Add(expressionField);
			frame.Content.Add(valueField);

			return frame;
		}
	}
}
