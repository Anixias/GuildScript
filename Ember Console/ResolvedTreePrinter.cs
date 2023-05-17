using System.Text;
using GuildScript.Analysis.Semantics;

namespace EmberConsole;

public class ResolvedTreePrinter : ResolvedStatement.IVisitor, ResolvedExpression.IVisitor
{
	private readonly TextWriter writer;
	private StringBuilder stringBuilder = new();
	private int indent;
	private readonly List<bool> childStatus = new();

	private bool IsLastChild
	{
		get => childStatus[indent - 1];
		set => childStatus[indent - 1] = value;
	}

	public ResolvedTreePrinter(TextWriter writer)
	{
		this.writer = writer;
	}

	private void PushIndent()
	{
		indent++;
		childStatus.Add(true);
	}

	private void PopIndent()
	{
		indent--;
		childStatus.RemoveAt(indent);
	}

	private void Write(string text)
	{
		for (var i = 0; i < indent - 1; i++)
		{
			stringBuilder.Append(childStatus[i] ? "    " : "│   ");
		}

		if (indent > 0)
		{
			stringBuilder.Append(IsLastChild ? "└── " : "├── ");
		}

		stringBuilder.Append(text);
		stringBuilder.AppendLine();
	}

	private void Print(ResolvedNode node)
	{
		switch (node)
		{
			case ResolvedStatement statement:
				statement.AcceptVisitor(this);
				break;
			case ResolvedExpression expression:
				expression.AcceptVisitor(this);
				break;
		}
	}
	
	public void PrintTree(ResolvedTree tree)
	{
		Print(tree.Root);

		writer.WriteLine(stringBuilder.ToString());
		stringBuilder = new StringBuilder();
	}

	public void VisitAwaitExpression(ResolvedExpression.Await expression)
	{
		Write($"Await ({expression.Type})");
		PushIndent();
		IsLastChild = true;
		Print(expression.Expression);
		PopIndent();
	}

	public void VisitConditionalExpression(ResolvedExpression.Conditional expression)
	{
		Write($"Conditional ({expression.Type})");
		PushIndent();
		
		IsLastChild = false;
		Write("Condition:");
		
		PushIndent();
		IsLastChild = true;
		Print(expression.Condition);
		PopIndent();
		
		Write("True Expression:");
		PushIndent();
		IsLastChild = true;
		Print(expression.TrueExpression);
		PopIndent();
		
		Write("False Expression:");
		IsLastChild = true;
		PushIndent();
		IsLastChild = true;
		Print(expression.FalseExpression);
		PopIndent();
		
		PopIndent();
	}

	public void VisitBinaryExpression(ResolvedExpression.Binary expression)
	{
		Write($"Binary ({expression.Type})");
		PushIndent();
		
		IsLastChild = false;
		Write("Left:");
		
		PushIndent();
		IsLastChild = true;
		Print(expression.Left);
		PopIndent();
		
		IsLastChild = false;
		Write("Operator: " + expression.Operator);
		
		IsLastChild = true;
		Write("Right:");
		
		PushIndent();
		IsLastChild = true;
		Print(expression.Right);
		PopIndent();
		
		PopIndent();
	}

	public void VisitTypeRelationExpression(ResolvedExpression.TypeRelation expression)
	{
		Write($"Type Relation ({expression.Type})");
		PushIndent();
		
		IsLastChild = false;
		Write("Operand:");
		
		PushIndent();
		IsLastChild = true;
		Print(expression.Operand);
		PopIndent();
		
		IsLastChild = false;
		Write("Operator: " + expression.Operator);
		
		IsLastChild = expression.VariableSymbol is null;
		Write("Type:");
		
		PushIndent();
		IsLastChild = true;
		Write(expression.TypeQuery?.ToString() ?? "Null");
		PopIndent();

		if (expression.VariableSymbol is not null)
		{
			IsLastChild = true;
			Write("Variable:");
		
			PushIndent();
			IsLastChild = true;
			Write(expression.VariableSymbol.Name);
			PopIndent();
		}
		
		PopIndent();
	}

	public void VisitUnaryExpression(ResolvedExpression.Unary expression)
	{
		Write($"Unary ({expression.Type})");
		PushIndent();
		
		IsLastChild = false;
		Write("Operand:");
		
		PushIndent();
		IsLastChild = true;
		Print(expression.Operand);
		PopIndent();
		
		IsLastChild = false;
		Write("Operator: " + expression.Operator.TokenSpan);
		
		IsLastChild = true;
		Write(expression.Operator.IsPostfix ? "Postfix" : "Prefix");
		
		PopIndent();
	}

	public void VisitIdentifierExpression(ResolvedExpression.Identifier expression)
	{
		Write($"Identifier: ({expression.Type}) {expression.Symbol}");
	}

	public void VisitQualifierExpression(ResolvedExpression.Qualifier expression)
	{
		Write($"Qualifier: ({expression.Type}) {expression.Symbol}");
	}

	public void VisitCallExpression(ResolvedExpression.Call expression)
	{
		Write($"Call ({expression.Type})");
		PushIndent();

		IsLastChild = expression.Arguments.Length == 0;
		Write("Function:");
		PushIndent();
		IsLastChild = true;
		Write(expression.Function.ToString() ?? "ERROR");
		PopIndent();

		if (expression.TemplateArguments.Length > 0)
		{
			IsLastChild = expression.Arguments.Length == 0;
			Write("Template Arguments:");
			PushIndent();

			for (var i = 0; i < expression.TemplateArguments.Length; i++)
			{
				IsLastChild = i == expression.TemplateArguments.Length - 1;
				Write(expression.TemplateArguments[i].ToString() ?? "ERROR");
			}
			
			PopIndent();
		}

		if (expression.Arguments.Length > 0)
		{
			IsLastChild = true;
			Write("Arguments:");
			PushIndent();

			for (var i = 0; i < expression.Arguments.Length; i++)
			{
				IsLastChild = i == expression.Arguments.Length - 1;
				Print(expression.Arguments[i]);
			}
			
			PopIndent();
		}

		PopIndent();
	}

	public void VisitLiteralExpression(ResolvedExpression.Literal expression)
	{
		Write($"{expression.Token.Text} ({expression.Type})");
	}

	public void VisitInstantiateExpression(ResolvedExpression.Instantiate expression)
	{
		Write($"Instantiate ({expression.Type})");
		PushIndent();

		IsLastChild = false;
		if (expression.Arguments.Length > 0)
		{
			IsLastChild = expression.Initializers.Length == 0;
			Write("Arguments:");
			PushIndent();

			for (var i = 0; i < expression.Arguments.Length; i++)
			{
				IsLastChild = i == expression.Arguments.Length - 1;
				Print(expression.Arguments[i]);
			}
			
			PopIndent();
		}

		if (expression.Initializers.Length > 0)
		{
			IsLastChild = true;
			Write("Initializers:");
			PushIndent();

			for (var i = 0; i < expression.Initializers.Length; i++)
			{
				IsLastChild = i == expression.Initializers.Length - 1;
				Print(expression.Initializers[i]);
			}
			
			PopIndent();
		}

		PopIndent();
	}

	public void VisitCastExpression(ResolvedExpression.Cast expression)
	{
		Write($"Cast ({expression.Type})");
		
		PushIndent();
		IsLastChild = false;
		Write("Expression:");
		PushIndent();
		IsLastChild = true;
		Print(expression.Expression);
		PopIndent();

		IsLastChild = false;
		Write("Target Type: " + expression.TargetType);

		IsLastChild = true;
		Write(expression.IsConditional ? "Conditional" : "Standard");
		
		PopIndent();
	}

	public void VisitIndexExpression(ResolvedExpression.Index expression)
	{
		Write($"Index ({expression.Type})");
		
		PushIndent();
		IsLastChild = false;
		Write("Indexer:");
		PushIndent();
		IsLastChild = true;
		Write(expression.IndexerSymbol?.Name ?? "Null");
		PopIndent();

		IsLastChild = false;
		Write("Key:");
		PushIndent();
		IsLastChild = true;
		Print(expression.Key);
		PopIndent();
		
		IsLastChild = true;
		Write(expression.IsConditional ? "Conditional" : "Standard");
		
		PopIndent();
	}

	public void VisitLambdaExpression(ResolvedExpression.Lambda expression)
	{
		Write($"Lambda ({expression.Type})");
		
		/*PushIndent();
		IsLastChild = false;
		
		Write("Parameters:");
		
		PushIndent();
		for (var i = 0; i < expression.ParameterList.Length; i++)
		{
			var parameter = expression.ParameterList[i];
			IsLastChild = i == expression.ParameterList.Length - 1;
			
			Write((parameter.IsReference ? "ref " : "") + parameter.Type + " " + parameter.Name.Text);
		}
		PopIndent();
		
		Write("Return Type: " + expression.ReturnType);

		IsLastChild = true;
		Print(expression.Body);
		
		PopIndent();*/
	}

	public void VisitProgramStatement(ResolvedStatement.Program statement)
	{
		Write("Program");
		
		PushIndent();

		if (statement.Statements.Length > 0)
		{
			IsLastChild = true;
			Write("Statements:");
			
			PushIndent();
			for (var i = 0; i < statement.Statements.Length; i++)
			{
				IsLastChild = i == statement.Statements.Length - 1;

				Print(statement.Statements[i]);
			}
			PopIndent();
		}

		PopIndent();
	}

	public void VisitEntryPointStatement(ResolvedStatement.EntryPoint statement)
	{
		Write("Entry Point: " + statement.MethodSymbol.Name);
		
		PushIndent();
		IsLastChild = false;
		Write("Return Type: " + statement.ReturnType);
		Write("Parameters:");
		
		PushIndent();
		for (var i = 0; i < statement.ParameterList.Length; i++)
		{
			var parameter = statement.ParameterList[i];
			IsLastChild = i == statement.ParameterList.Length - 1;
			
			Write((parameter.IsReference ? "ref " : "") + parameter.Type + " " + parameter.Name);
		}
		PopIndent();

		IsLastChild = true;
		Print(statement.Body);
		
		PopIndent();
	}

	public void VisitDefineStatement(ResolvedStatement.Define statement)
	{
		Write("Define");
		
		PushIndent();
		
		IsLastChild = false;
		Write("Identifier: " + statement.Identifier);
		IsLastChild = true;
		Write("Type: " + statement.Type);
		
		PopIndent();
	}

	public void VisitBlockStatement(ResolvedStatement.Block statement)
	{
		Write("Block");
		
		PushIndent();

		for (var i = 0; i < statement.Statements.Length; i++)
		{
			IsLastChild = i == statement.Statements.Length - 1;
			Print(statement.Statements[i]);
		}
		
		PopIndent();
	}

	public void VisitClassStatement(ResolvedStatement.Class statement)
	{
		Write("Class: " + statement.ClassSymbol.Name);
		
		PushIndent();
		IsLastChild = false;
		
		Write("Access: " + statement.AccessModifier);
		Write("Modifier: " + statement.ClassModifier);

		if (statement.TypeParameters.Length > 0)
		{
			Write("Template Parameters:");
			PushIndent();

			for (var i = 0; i < statement.TypeParameters.Length; i++)
			{
				var typeParameter = statement.TypeParameters[i];
				IsLastChild = i == statement.TypeParameters.Length - 1;

				Write(typeParameter.Name);
			}

			PopIndent();
		}

		if (statement.BaseClass != null)
		{
			Write("BaseClass: " + statement.BaseClass.Name);
		}

		IsLastChild = true;
		
		Write("Members: ");
		
		PushIndent();

		for (var i = 0; i < statement.Members.Length; i++)
		{
			IsLastChild = i == statement.Members.Length - 1;
			Print(statement.Members[i]);
		}
		
		PopIndent();
		
		PopIndent();
	}

	public void VisitStructStatement(ResolvedStatement.Struct statement)
	{
		Write("Struct: " + statement.StructSymbol.Name);
		
		PushIndent();
		IsLastChild = false;
		
		Write("Access: " + statement.AccessModifier);
		Write("Modifier: " + statement.StructModifier);

		IsLastChild = true;
		
		Write("Members: ");
		
		PushIndent();
		
		for (var i = 0; i < statement.Members.Length; i++)
		{
			IsLastChild = i == statement.Members.Length - 1;
			Print(statement.Members[i]);
		}
		
		PopIndent();
		
		PopIndent();
	}

	public void VisitInterfaceStatement(ResolvedStatement.Interface statement)
	{
		Write("Interface: " + statement.InterfaceSymbol.Name);
		
		PushIndent();
		
		IsLastChild = false;
		Write("Access: " + statement.AccessModifier);

		IsLastChild = true;
		Write("Members: ");
		
		PushIndent();
		
		for (var i = 0; i < statement.Members.Length; i++)
		{
			IsLastChild = i == statement.Members.Length - 1;
			Print(statement.Members[i]);
		}
		
		PopIndent();
		
		PopIndent();
	}

	public void VisitEnumStatement(ResolvedStatement.Enum statement)
	{
		Write("Enum: " + statement.EnumSymbol.Name);
		
		PushIndent();
		IsLastChild = false;
		
		Write("Access: " + statement.AccessModifier);
		Write("Type: " + statement.Type);

		IsLastChild = true;
		
		Write("Members: ");
		
		PushIndent();
		
		for (var i = 0; i < statement.Members.Length; i++)
		{
			IsLastChild = i == statement.Members.Length - 1;
			var member = statement.Members[i];
			Write(member.Identifier);

			if (member.Expression is null)
				continue;
			
			PushIndent();
			IsLastChild = true;
			Print(member.Expression);
			PopIndent();
		}
		
		PopIndent();
		
		PopIndent();
	}

	public void VisitCastOverloadStatement(ResolvedStatement.CastOverload statement)
	{
		Write("Cast Overload: " + statement.OverloadType);
		
		PushIndent();

		IsLastChild = false;
		Write(statement.MethodSymbol.ToString());

		IsLastChild = true;
		Print(statement.Body);
		
		PopIndent();
	}

	public void VisitDestructorStatement(ResolvedStatement.Destructor statement)
	{
		Write("Destructor");
		
		PushIndent();

		IsLastChild = true;
		Print(statement.Body);
		
		PopIndent();
	}

	public void VisitExternalMethodStatement(ResolvedStatement.ExternalMethod statement)
	{
		Write("External Method: " + statement.ExternalMethodSymbol.Name);
		
		PushIndent();
		IsLastChild = false;
		Write("Return Type: " + statement.ReturnType);
		
		IsLastChild = true;
		Write("Parameters:");
		
		PushIndent();
		for (var i = 0; i < statement.ParameterList.Length; i++)
		{
			var parameter = statement.ParameterList[i];
			IsLastChild = i == statement.ParameterList.Length - 1;
			
			Write((parameter.IsReference ? "ref " : "") + parameter.Type + " " + parameter.Name);
		}
		PopIndent();
		PopIndent();
	}

	public void VisitConstructorStatement(ResolvedStatement.Constructor statement)
	{
		Write("Constructor");
		
		PushIndent();
		IsLastChild = false;
		
		Write("Access: " + statement.AccessModifier);
		
		Write("Parameters:");
		
		PushIndent();
		var parameters = statement.ConstructorSymbol.GetParameters().ToArray();
		for (var i = 0; i < parameters.Length; i++)
		{
			var parameter = parameters[i];
			IsLastChild = i == parameters.Length - 1;
			
			Write((parameter.IsReference ? "ref " : "") + parameter.Type + " " + parameter.Name);
		}
		PopIndent();

		if (statement.ConstructorSymbol.Initializer is not null)
		{
			Write($"{statement.ConstructorSymbol.Initializer.Name}:");

			PushIndent();
			var arguments = statement.ConstructorSymbol.InitializerArguments.ToArray();
			for (var i = 0; i < arguments.Length; i++)
			{
				var argument = arguments[i];
				IsLastChild = i == arguments.Length - 1;

				Print(argument);
			}

			PopIndent();
		}

		IsLastChild = true;
		Print(statement.Body);
		
		PopIndent();
	}

	public void VisitIndexerStatement(ResolvedStatement.Indexer statement)
	{
		Write("Indexer");
		
		PushIndent();
		IsLastChild = false;
		
		Write("Access: " + statement.AccessModifier);
		
		Write("Parameters:");
		
		PushIndent();
		for (var i = 0; i < statement.ParameterList.Length; i++)
		{
			var parameter = statement.ParameterList[i];
			IsLastChild = i == statement.ParameterList.Length - 1;
			
			Write((parameter.IsReference ? "ref " : "") + parameter.Type + " " + parameter.Name);
		}
		PopIndent();

		IsLastChild = true;
		Write("Accessors:");
		PushIndent();
		for (var i = 0; i < statement.Body.Length; i++)
		{
			var accessor = statement.Body[i];
			IsLastChild = i == statement.Body.Length - 1;
			
			Print(accessor);
		}
		PopIndent();
		
		PopIndent();
	}

	public void VisitAccessorAutoStatement(ResolvedStatement.AccessorAuto statement)
	{
		Write("Accessor: " + statement.AutoType);
		
		PushIndent();
		IsLastChild = true;
		Write("Access: " + statement.AccessModifier);
		PopIndent();
	}

	public void VisitAccessorLambdaStatement(ResolvedStatement.AccessorLambda statement)
	{
		Write("Accessor: Lambda");
		
		PushIndent();
		IsLastChild = false;
		Write("Access: " + statement.AccessModifier);
		IsLastChild = true;
		Print(statement.LambdaExpression);
		PopIndent();
	}

	public void VisitAccessorLambdaSignatureStatement(ResolvedStatement.AccessorLambdaSignature statement)
	{
		Write("Accessor: Lambda Signature");
		
		PushIndent();
		IsLastChild = false;
		Write("Access: " + statement.AccessModifier);
		IsLastChild = true;
		Write("Signature: " + statement.LambdaTypeSymbol);
		PopIndent();
	}

	public void VisitEventStatement(ResolvedStatement.Event statement)
	{
		Write("Event: " + statement.EventSymbol.Name);
		
		PushIndent();
		
		IsLastChild = false;
		Write("Access: " + statement.AccessModifier);
		Write("Modifier: " + statement.EventModifier);

		IsLastChild = true;
		Write("Parameters:");
		
		PushIndent();
		for (var i = 0; i < statement.ParameterList.Length; i++)
		{
			var parameter = statement.ParameterList[i];
			IsLastChild = i == statement.ParameterList.Length - 1;
			
			Write((parameter.IsReference ? "ref " : "") + parameter.Type + " " + parameter.Name);
		}
		PopIndent();

		PopIndent();
	}

	public void VisitEventSignatureStatement(ResolvedStatement.EventSignature statement)
	{
		Write("Event: " + statement.EventSymbol.Name);
		
		PushIndent();
		
		IsLastChild = false;
		Write("Modifier: " + statement.EventModifier);
		
		IsLastChild = true;
		Write("Parameters:");
		
		PushIndent();
		for (var i = 0; i < statement.ParameterList.Length; i++)
		{
			var parameter = statement.ParameterList[i];
			IsLastChild = i == statement.ParameterList.Length - 1;
			
			Write((parameter.IsReference ? "ref " : "") + parameter.Type + " " + parameter.Name);
		}
		PopIndent();

		PopIndent();
	}

	public void VisitPropertyStatement(ResolvedStatement.Property statement)
	{
		Write("Property: " + statement.PropertySymbol.Name);
		
		PushIndent();

		IsLastChild = false;
		Write("Type: " + statement.Type);
		Write("Access: " + statement.AccessModifier);

		foreach (var modifier in statement.Modifiers)
		{
			Write("Modifier: " + modifier);
		}
		
		IsLastChild = true;
		Write("Accessors:");
		PushIndent();
		for (var i = 0; i < statement.Body.Length; i++)
		{
			var accessor = statement.Body[i];
			IsLastChild = i == statement.Body.Length - 1;
			
			Print(accessor);
		}
		PopIndent();

		PopIndent();
	}

	public void VisitPropertySignatureStatement(ResolvedStatement.PropertySignature statement)
	{
		Write("Property Signature: " + statement.PropertySymbol.Name);
		
		PushIndent();

		IsLastChild = false;
		Write("Type: " + statement.Type);

		foreach (var modifier in statement.Modifiers)
		{
			Write("Modifier: " + modifier);
		}
		
		IsLastChild = true;
		Write("Accessors:");
		PushIndent();
		for (var i = 0; i < statement.Body.Length; i++)
		{
			var accessor = statement.Body[i];
			IsLastChild = i == statement.Body.Length - 1;
			
			Print(accessor);
		}
		PopIndent();

		PopIndent();
	}

	public void VisitMethodStatement(ResolvedStatement.Method statement)
	{
		Write("Method: " + statement.MethodSymbol.Name);
		
		PushIndent();

		IsLastChild = false;
		Write("Return Type: " + statement.ReturnType);
		Write("Async: " + (statement.IsAsync ? "True" : "False"));
		Write("Access: " + statement.AccessModifier);
		
		if (statement.TypeParameters.Length > 0)
		{
			Write("Template Parameters:");
			PushIndent();

			for (var i = 0; i < statement.TypeParameters.Length; i++)
			{
				var typeParameter = statement.TypeParameters[i];
				IsLastChild = i == statement.TypeParameters.Length - 1;

				Write(typeParameter.Name);
			}

			PopIndent();
		}

		foreach (var modifier in statement.Modifiers)
		{
			Write("Modifier: " + modifier);
		}
		
		Write("Parameters:");
		
		PushIndent();
		for (var i = 0; i < statement.ParameterList.Length; i++)
		{
			var parameter = statement.ParameterList[i];
			IsLastChild = i == statement.ParameterList.Length - 1;
			
			Write((parameter.IsReference ? "ref " : "") + parameter.Type + " " + parameter.Name);
		}
		PopIndent();
		
		IsLastChild = true;
		Print(statement.Body);

		PopIndent();
	}

	public void VisitMethodSignatureStatement(ResolvedStatement.MethodSignature statement)
	{
		Write("Method Signature: " + statement.MethodSymbol.Name);
		
		PushIndent();

		IsLastChild = false;
		Write("Return Type: " + statement.ReturnType);
		Write("Async: " + (statement.IsAsync ? "True" : "False"));
		
		if (statement.TypeParameters.Length > 0)
		{
			Write("Template Parameters:");
			PushIndent();

			for (var i = 0; i < statement.TypeParameters.Length; i++)
			{
				var typeParameter = statement.TypeParameters[i];
				IsLastChild = i == statement.TypeParameters.Length - 1;

				Write(typeParameter.Name);
			}

			PopIndent();
		}

		foreach (var modifier in statement.Modifiers)
		{
			Write("Modifier: " + modifier);
		}

		IsLastChild = true;
		Write("Parameters:");
		
		PushIndent();
		for (var i = 0; i < statement.ParameterList.Length; i++)
		{
			var parameter = statement.ParameterList[i];
			IsLastChild = i == statement.ParameterList.Length - 1;
			
			Write((parameter.IsReference ? "ref " : "") + parameter.Type + " " + parameter.Name);
		}
		PopIndent();

		PopIndent();
	}

	public void VisitFieldStatement(ResolvedStatement.Field statement)
	{
		Write("Field: " + statement.FieldSymbol.Name);
		
		PushIndent();

		IsLastChild = false;
		Write("Type: " + statement.Type);
		Write("Access: " + statement.AccessModifier);

		foreach (var modifier in statement.Modifiers)
		{
			Write("Modifier: " + modifier);
		}
		
		IsLastChild = true;
		if (statement.Initializer is { } initializer)
		{
			Write("Initializer:");
			PushIndent();
			IsLastChild = true;
			Print(initializer);
			PopIndent();
		}

		PopIndent();
	}

	public void VisitBreakStatement(ResolvedStatement.Break statement)
	{
		Write("Break");
	}

	public void VisitContinueStatement(ResolvedStatement.Continue statement)
	{
		Write("Continue");
	}

	public void VisitControlStatement(ResolvedStatement.Control statement)
	{
		Write("Control");
		
		PushIndent();

		IsLastChild = false;
		Write("If: ");
		
		PushIndent();
		IsLastChild = true;
		Print(statement.IfExpression);
		PopIndent();
		
		IsLastChild = statement.ElseStatement is null;
		Write("Then: ");
		
		PushIndent();
		IsLastChild = true;
		Print(statement.IfStatement);
		PopIndent();

		if (statement.ElseStatement is not null)
		{
			IsLastChild = true;
			Write("Else: ");

			PushIndent();
			IsLastChild = true;
			Print(statement.ElseStatement);
			PopIndent();
		}

		PopIndent();
	}

	public void VisitWhileStatement(ResolvedStatement.While statement)
	{
		Write("While");
		
		PushIndent();
		
		IsLastChild = false;
		Write("Condition:");
		PushIndent();
		
		IsLastChild = true;
		Print(statement.Condition);
		
		PopIndent();

		IsLastChild = true;
		Write("Body:");
		PushIndent();

		IsLastChild = true;
		Print(statement.Body);
		
		PopIndent();
		
		PopIndent();
	}

	public void VisitDoWhileStatement(ResolvedStatement.DoWhile statement)
	{
		Write("While");
		
		PushIndent();
		
		IsLastChild = false;
		Write("Body:");
		PushIndent();

		IsLastChild = true;
		Print(statement.Body);
		
		PopIndent();

		IsLastChild = true;
		Write("Condition:");
		PushIndent();
		
		IsLastChild = true;
		Print(statement.Condition);
		
		PopIndent();
		
		PopIndent();
	}

	public void VisitForStatement(ResolvedStatement.For statement)
	{
		Write("For");
		
		PushIndent();
		
		// Initializer
		if (statement.Initializer is not null)
		{
			IsLastChild = false;
			Write("Initializer:");
			PushIndent();

			IsLastChild = true;
			Print(statement.Initializer);

			PopIndent();
		}
		
		// Condition
		if (statement.Condition is not null)
		{
			IsLastChild = false;
			Write("Condition:");
			PushIndent();

			IsLastChild = true;
			Print(statement.Condition);

			PopIndent();
		}
		
		// Increment
		if (statement.Increment is not null)
		{
			IsLastChild = false;
			Write("Increment:");
			PushIndent();

			IsLastChild = true;
			Print(statement.Increment);

			PopIndent();
		}
		
		// Body
		IsLastChild = true;
		Write("Body:");
		PushIndent();

		IsLastChild = true;
		Print(statement.Body);

		PopIndent();

		PopIndent();
	}

	public void VisitForEachStatement(ResolvedStatement.ForEach statement)
	{
		Write("For Each:");
		
		PushIndent();

		IsLastChild = false;
		Write("Iterator Type: " + statement.Iterator.Type);
		Write("Iterator: " + statement.Iterator.Name);
		
		Write("Enumerable:");
		PushIndent();
		IsLastChild = true;
		Print(statement.Enumerable);
		PopIndent();

		IsLastChild = true;
		Write("Body:");
		PushIndent();
		IsLastChild = true;
		Print(statement.Body);
		PopIndent();

		PopIndent();
	}

	public void VisitRepeatStatement(ResolvedStatement.Repeat statement)
	{
		Write("Repeat");
		
		PushIndent();
		
		IsLastChild = false;
		Write("Repetitions:");
		PushIndent();
		
		IsLastChild = true;
		Print(statement.Repetitions);
		
		PopIndent();

		IsLastChild = true;
		Write("Body:");
		PushIndent();

		IsLastChild = true;
		Print(statement.Body);
		
		PopIndent();
		
		PopIndent();
	}

	public void VisitReturnStatement(ResolvedStatement.Return statement)
	{
		Write("Return");

		if (statement.Expression is null)
			return;
		
		PushIndent();
		IsLastChild = true;
		Print(statement.Expression);
		PopIndent();
	}

	public void VisitThrowStatement(ResolvedStatement.Throw statement)
	{
		Write("Throw");

		if (statement.Expression is null)
			return;
		
		PushIndent();
		IsLastChild = true;
		Print(statement.Expression);
		PopIndent();
	}

	public void VisitSealStatement(ResolvedStatement.Seal statement)
	{
		Write("Seal");
		
		PushIndent();
		IsLastChild = true;
		Write(statement.Symbol.Name);
		PopIndent();
	}

	public void VisitTryStatement(ResolvedStatement.Try statement)
	{
		Write("Try");

		var children = 1;
		if (statement.CatchStatement is not null)
			children++;
		if (statement.FinallyStatement is not null)
			children++;
		
		PushIndent();
		IsLastChild = --children == 0;
		Write("Try:");
		
		PushIndent();
		IsLastChild = true;
		Print(statement.TryStatement);
		PopIndent();

		if (statement.CatchStatement is not null)
		{
			IsLastChild = --children == 0;
			Write("Catch:");
		
			PushIndent();
			IsLastChild = false;
			Write(statement.CatchSymbol?.Type + " " + statement.CatchSymbol?.Name);
			
			IsLastChild = true;
			Print(statement.CatchStatement);
			PopIndent();
		}

		if (statement.FinallyStatement is not null)
		{
			IsLastChild = --children == 0;
			Write("Finally:");
		
			PushIndent();
			IsLastChild = true;
			Print(statement.FinallyStatement);
			PopIndent();
		}
		
		PopIndent();
	}

	public void VisitVariableDeclarationStatement(ResolvedStatement.VariableDeclaration statement)
	{
		Write("Variable Declaration");
		
		PushIndent();
		IsLastChild = statement.Initializer is null;
		Write("Identifier: " + statement.Symbol.Name);

		if (statement.Initializer is not null)
		{
			IsLastChild = true;
			Write("Initializer:");
			PushIndent();
			IsLastChild = true;
			Print(statement.Initializer);
			PopIndent();
		}
		
		PopIndent();
	}

	public void VisitLockStatement(ResolvedStatement.Lock statement)
	{
		Write("Lock");
		
		PushIndent();
		
		IsLastChild = false;
		Write(statement.Symbol.Name);

		IsLastChild = true;
		Print(statement.Body);
		
		PopIndent();
	}

	public void VisitSwitchStatement(ResolvedStatement.Switch statement)
	{
		Write("Switch");
		
		PushIndent();
		
		IsLastChild = false;
		Print(statement.Expression);

		IsLastChild = true;
		Write("Sections:");
		
		PushIndent();
		for (var i = 0; i < statement.Sections.Length; i++)
		{
			IsLastChild = i == statement.Sections.Length - 1;
			var section = statement.Sections[i];
			
			Write("Section:");
			PushIndent();
			IsLastChild = false;

			Write("Labels:");
			PushIndent();
			for (var j = 0; j < section.Labels.Length; j++)
			{
				IsLastChild = j == section.Labels.Length - 1;

				switch (section.Labels[j])
				{
					case ResolvedStatement.Switch.ExpressionLabel expressionLabel:
						Write("ResolvedExpression:");
						PushIndent();
						IsLastChild = true;
						Print(expressionLabel.Expression);
						PopIndent();
						break;
					case ResolvedStatement.Switch.PatternLabel patternLabel:
						Write("Pattern: " + patternLabel.Symbol.Type + " " + patternLabel.Symbol.Name);
						break;
					default:
						Write("Default");
						break;
				}
			}
			PopIndent();
			
			IsLastChild = true;
			Write("Body:");
			PushIndent();
			IsLastChild = true;
			Print(section.Body);
			PopIndent();
			
			PopIndent();
		}
		PopIndent();
		
		PopIndent();
	}

	public void VisitExpressionStatement(ResolvedStatement.ExpressionStatement statement)
	{
		Write("Expression Statement");
		
		PushIndent();
		IsLastChild = true;
		Print(statement.Expression);
		PopIndent();
	}

	public void VisitOperatorOverloadStatement(ResolvedStatement.OperatorOverload statement)
	{
		Write("Operator Overload");
		
		PushIndent();
		IsLastChild = false;
		Write("Return Type: " + statement.ReturnType);
		Write("Operator: " + statement.OperatorSymbol.Name);
		
		Write("Parameters:");
		
		PushIndent();
		var parameters = statement.MethodSymbol.GetParameters().ToArray();
		for (var i = 0; i < parameters.Length; i++)
		{
			var parameter = parameters[i];
			IsLastChild = i == parameters.Length - 1;
			
			Write((parameter.IsReference ? "ref " : "") + parameter.Type + " " + parameter.Name);
		}
		PopIndent();

		IsLastChild = true;
		Write("Body:");
		PushIndent();
		Print(statement.Body);
		PopIndent();
		
		PopIndent();
	}

	public void VisitOperatorOverloadSignatureStatement(ResolvedStatement.OperatorOverloadSignature statement)
	{
		Write("Operator Overload");
		
		PushIndent();
		IsLastChild = false;
		Write("Return Type: " + statement.ReturnType);
		Write("Operator: " + statement.OperatorSymbol.Name);
		
		IsLastChild = true;
		Write("Parameters:");
		
		PushIndent();
		var parameters = statement.MethodSymbol.GetParameters().ToArray();
		for (var i = 0; i < parameters.Length; i++)
		{
			var parameter = parameters[i];
			IsLastChild = i == parameters.Length - 1;
			
			Write((parameter.IsReference ? "ref " : "") + parameter.Type + " " + parameter.Name);
		}
		PopIndent();
		
		PopIndent();
	}
}