﻿using PiRhoSoft.Variables;
using UnityEngine;

namespace PiRhoSoft.Expressions.Samples
{
	public class ExpressionSample : MonoBehaviour
	{
		[VariableConstraint(VariableType.Asset)]
		public VariableSource Source = new VariableSource();

		public Expression Expression = new Expression();
		public ReadOnlyExpression ReadOnly = new ReadOnlyExpression();
		public AssignmentExpression Assignment = new AssignmentExpression();
		public StringExpression StringExpressions = new StringExpression();
	}
}