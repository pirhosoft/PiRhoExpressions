using PiRhoSoft.Utilities;
using PiRhoSoft.Utilities.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace PiRhoSoft.Expressions.Editor
{
	public class AssignmentField : BaseField<string>
	{
		public const string ValueSuffix = " = value";

		private readonly AssignmentControl _control;

		public AssignmentField(SerializedProperty property) : base(property.displayName, null)
		{
			var expression = property.GetObject<AssignmentExpression>();
			var statementProperty = property.FindPropertyRelative("_content");

			_control = new AssignmentControl(expression);
			_control.AddToClassList(ExpressionField.InputUssClassName);
			_control.RegisterCallback<ChangeEvent<string>>(evt =>
			{
				base.value = $"{evt.newValue}{ValueSuffix}";
				evt.StopImmediatePropagation();
			});

			labelElement.AddToClassList(ExpressionField.LabelUssClassName);

			AddToClassList(ExpressionField.UssClassName);

			this.SetVisualInput(_control);
			this.AddStyleSheet(ExpressionField.Stylesheet);
			this.ConfigureProperty(statementProperty);

			label = property.displayName;
		}

		public override void SetValueWithoutNotify(string newValue)
		{
			base.SetValueWithoutNotify(newValue);
			_control.SetValueWithoutNotify(newValue);
		}

		#region Visual Input

		private class AssignmentControl : VisualElement
		{
			public AssignmentExpression Value { get; private set; }

			private readonly TextField _textField;
			private readonly MessageBox _message;

			public AssignmentControl(AssignmentExpression value)
			{
				Value = value;

				_textField = new TextField { multiline = true, isDelayed = true };
				_textField.AddToClassList(ExpressionField.TextUssClassName);

				_message = new MessageBox(MessageBoxType.Error, "Expression is invalid");
				_message.AddToClassList(ExpressionField.MessageUssClassName);

				Add(_textField);
				Add(_message);
			}

			public void SetValueWithoutNotify(string expression)
			{
				Value.Content = expression;
				Refresh();
			}

			private void Refresh()
			{
				_textField.SetValueWithoutNotify(Value.Content.Substring(0, Value.Content.Length - ValueSuffix.Length));

				EnableInClassList(ExpressionField.InvalidUssClassName, !Value.IsValid);
			}
		}

		#endregion
	}
}