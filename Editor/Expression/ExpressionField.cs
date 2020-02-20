using PiRhoSoft.Utilities;
using PiRhoSoft.Utilities.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace PiRhoSoft.Expressions.Editor
{
	public class ExpressionField : BaseField<string>
	{
		public const string Stylesheet = "ExpressionStyle.uss";
		public const string UssClassName = "pirho-expression";
		public const string LabelUssClassName = UssClassName + "__label";
		public const string InputUssClassName = UssClassName + "__input";
		public const string InvalidUssClassName = UssClassName + "--invalid";
		public const string TextUssClassName = InputUssClassName + "__text";
		public const string MessageUssClassName = InputUssClassName + "__message";

		private readonly ExpressionControl _control;

		public ExpressionField(SerializedProperty property) : base(property.displayName, null)
		{
			var expression = property.GetObject<Expression>();
			var statementProperty = property.FindPropertyRelative("_content");

			_control = new ExpressionControl(expression);
			_control.AddToClassList(InputUssClassName);
			_control.RegisterCallback<ChangeEvent<string>>(evt => base.value = evt.newValue);

			labelElement.AddToClassList(LabelUssClassName);

			AddToClassList(UssClassName);

			this.SetVisualInput(_control);
			this.AddStyleSheet(Stylesheet);
			this.ConfigureProperty(statementProperty);

			label = property.displayName;
		}

		public override void SetValueWithoutNotify(string newValue)
		{
			base.SetValueWithoutNotify(newValue);
			_control.SetValueWithoutNotify(newValue);
		}

		#region Visual Input

		private class ExpressionControl : VisualElement
		{
			public Expression Value { get; private set; }

			private readonly TextField _textField;
			private readonly MessageBox _message;

			public ExpressionControl(Expression value)
			{
				Value = value;

				_textField = new TextField { multiline = true, isDelayed = true };
				_textField.AddToClassList(TextUssClassName);

				_message = new MessageBox(MessageBoxType.Error, "Expression is invalid");
				_message.AddToClassList(MessageUssClassName);

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
				_textField.SetValueWithoutNotify(Value.Content);

				EnableInClassList(InvalidUssClassName, !Value.IsValid);
			}
		}

		#endregion
	}
}