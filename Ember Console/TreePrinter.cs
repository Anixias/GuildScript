using System.Text;
using GuildScript.Analysis.Syntax;

namespace EmberConsole;

public class TreePrinter : Expression.IVisitor, Statement.IVisitor
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

	public TreePrinter(TextWriter writer)
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

	private void Print(SyntaxNode node)
	{
		switch (node)
		{
			case Statement statement:
				statement.AcceptVisitor(this);
				break;
			case Expression expression:
				expression.AcceptVisitor(this);
				break;
		}
	}
	
	public void PrintTree(SyntaxTree tree)
	{
		Print(tree.Root);

		writer.WriteLine(stringBuilder.ToString());
		stringBuilder = new StringBuilder();
	}

	public void VisitAwaitExpression(Expression.Await expression)
	{
		Write("Await");
		PushIndent();
		IsLastChild = true;
		Print(expression.Expression);
		PopIndent();
	}

	public void VisitConditionalExpression(Expression.Conditional expression)
	{
		Write("Conditional");
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

	public void VisitBinaryExpression(Expression.Binary expression)
	{
		Write("Binary");
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

	public void VisitTypeRelationExpression(Expression.TypeRelation expression)
	{
		Write("Type Relation");
		PushIndent();
		
		IsLastChild = false;
		Write("Operand:");
		
		PushIndent();
		IsLastChild = true;
		Print(expression.Operand);
		PopIndent();
		
		IsLastChild = false;
		Write("Operator: " + expression.Operator);
		
		IsLastChild = expression.IdentifierToken is null;
		Write("Type:");
		
		PushIndent();
		IsLastChild = true;
		Write(expression.Type?.ToString() ?? "Null");
		PopIndent();

		if (expression.IdentifierToken is not null)
		{
			IsLastChild = true;
			Write("Identifier:");
		
			PushIndent();
			IsLastChild = true;
			Write(expression.IdentifierToken.Text);
			PopIndent();
		}
		
		PopIndent();
	}

	public void VisitUnaryExpression(Expression.Unary expression)
	{
		Write("Unary");
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

	public void VisitIdentifierExpression(Expression.Identifier expression)
	{
		Write("Identifier: " + expression.NameToken.Text);
	}

	public void VisitQualifierExpression(Expression.Qualifier expression)
	{
		Write("Qualifier: " + expression.NameToken.Text);
	}

	public void VisitCallExpression(Expression.Call expression)
	{
		Write("Call");
		PushIndent();

		IsLastChild = expression.Arguments.Length == 0;
		Write("Function:");
		PushIndent();
		IsLastChild = true;
		Print(expression.Function);
		PopIndent();

		if (expression.TemplateArguments.Length > 0)
		{
			IsLastChild = expression.Arguments.Length == 0;
			Write("Template Arguments:");
			PushIndent();

			for (var i = 0; i < expression.TemplateArguments.Length; i++)
			{
				IsLastChild = i == expression.TemplateArguments.Length - 1;
				Print(expression.TemplateArguments[i]);
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

	public void VisitLiteralExpression(Expression.Literal expression)
	{
		Write(expression.Token.Text);
	}

	public void VisitInstantiateExpression(Expression.Instantiate expression)
	{
		Write("Instantiate");
		PushIndent();

		IsLastChild = false;
		Write("Instance Type:");
		PushIndent();
		IsLastChild = true;
		Write(expression.InstanceType.ToString());
		PopIndent();

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

	public void VisitCastExpression(Expression.Cast expression)
	{
		Write("Cast");
		
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

	public void VisitIndexExpression(Expression.Index expression)
	{
		Write("Index");
		
		PushIndent();
		IsLastChild = false;
		Write("Expression:");
		PushIndent();
		IsLastChild = true;
		Print(expression.Expression);
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

	public void VisitLambdaExpression(Expression.Lambda expression)
	{
		Write("Lambda");
		
		PushIndent();
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
		
		PopIndent();
	}

	public void VisitProgramStatement(Statement.Program statement)
	{
		Write("Program");
		
		PushIndent();

		IsLastChild = false;
		Write("Imports:");
		PushIndent();
		for (var i = 0; i < statement.ImportedModules.Length; i++)
		{
			IsLastChild = i == statement.ImportedModules.Length - 1;
			Write(statement.ImportedModules[i].ToString());
		}
		PopIndent();

		IsLastChild = statement.Statements.Length == 0;
		Write("Module: " + statement.Module);

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

	public void VisitEntryPointStatement(Statement.EntryPoint statement)
	{
		Write("Entry Point: " + statement.Identifier.Text);
		
		PushIndent();
		IsLastChild = false;
		Write("Return Type: " + statement.ReturnType);
		Write("Parameters:");
		
		PushIndent();
		for (var i = 0; i < statement.ParameterList.Length; i++)
		{
			var parameter = statement.ParameterList[i];
			IsLastChild = i == statement.ParameterList.Length - 1;
			
			Write((parameter.IsReference ? "ref " : "") + parameter.Type + " " + parameter.Name.Text);
		}
		PopIndent();

		IsLastChild = true;
		Print(statement.Body);
		
		PopIndent();
	}

	public void VisitDefineStatement(Statement.Define statement)
	{
		Write("Define");
		
		PushIndent();
		
		IsLastChild = false;
		Write("Identifier: " + statement.Identifier.Text);
		IsLastChild = true;
		Write("Type: " + statement.Type);
		
		PopIndent();
	}

	public void VisitBlockStatement(Statement.Block statement)
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

	public void VisitClassStatement(Statement.Class statement)
	{
		Write("Class: " + statement.NameToken.Text);
		
		PushIndent();
		IsLastChild = false;
		
		Write("Access: " + (statement.AccessModifier?.Text ?? "internal"));
		
		if (statement.ClassModifier is not null)
			Write("Modifier: " + statement.ClassModifier.Text);

		if (statement.TypeParameters.Length > 0)
		{
			Write("Template Parameters:");
			PushIndent();

			for (var i = 0; i < statement.TypeParameters.Length; i++)
			{
				var typeParameter = statement.TypeParameters[i];
				IsLastChild = i == statement.TypeParameters.Length - 1;

				Write(typeParameter.Text);
			}

			PopIndent();
		}

		if (statement.BaseClass != null)
		{
			Write("BaseClass: " + statement.BaseClass);
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

	public void VisitStructStatement(Statement.Struct statement)
	{
		Write("Struct: " + statement.NameToken.Text);
		
		PushIndent();
		IsLastChild = false;
		
		Write("Access: " + (statement.AccessModifier?.Text ?? "internal"));
		
		if (statement.StructModifier is not null)
			Write("Modifier: " + statement.StructModifier.Text);

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

	public void VisitInterfaceStatement(Statement.Interface statement)
	{
		Write("Interface: " + statement.NameToken.Text);
		
		PushIndent();
		
		IsLastChild = false;
		Write("Access: " + (statement.AccessModifier?.Text ?? "internal"));

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

	public void VisitEnumStatement(Statement.Enum statement)
	{
		Write("Enum: " + statement.NameToken.Text);
		
		PushIndent();
		IsLastChild = false;
		
		Write("Access: " + (statement.AccessModifier?.Text ?? "internal"));
		Write("Type: " + statement.Type);

		IsLastChild = true;
		
		Write("Members: ");
		
		PushIndent();
		
		for (var i = 0; i < statement.Members.Length; i++)
		{
			IsLastChild = i == statement.Members.Length - 1;
			var member = statement.Members[i];
			Write(member.Identifier.Text);

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

	public void VisitCastOverloadStatement(Statement.CastOverload statement)
	{
		Write("Cast Overload");
		
		PushIndent();

		IsLastChild = false;
		Write("Cast Type: " + statement.CastTypeToken.Text);
		
		Write("Target Type: " + statement.TargetType);
		
		IsLastChild = true;
		Print(statement.Body);
		
		PopIndent();
	}

	public void VisitDestructorStatement(Statement.Destructor statement)
	{
		Write("Destructor");
		
		PushIndent();

		IsLastChild = true;
		Print(statement.Body);
		
		PopIndent();
	}

	public void VisitExternalMethodStatement(Statement.ExternalMethod statement)
	{
		Write("External Method: " + statement.Identifier.Text);
		
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
			
			Write((parameter.IsReference ? "ref " : "") + parameter.Type + " " + parameter.Name.Text);
		}
		PopIndent();
		PopIndent();
	}

	public void VisitConstructorStatement(Statement.Constructor statement)
	{
		Write("Constructor");
		
		PushIndent();
		IsLastChild = false;
		
		Write("Access: " + (statement.AccessModifier?.Text ?? "public"));
		
		Write("Parameters:");
		
		PushIndent();
		for (var i = 0; i < statement.ParameterList.Length; i++)
		{
			var parameter = statement.ParameterList[i];
			IsLastChild = i == statement.ParameterList.Length - 1;
			
			Write((parameter.IsReference ? "ref " : "") + parameter.Type + " " + parameter.Name.Text);
		}
		PopIndent();

		if (statement.Initializer is not null)
		{
			Write($"{statement.Initializer.Text}:");

			PushIndent();
			for (var i = 0; i < statement.ArgumentList.Length; i++)
			{
				var argument = statement.ArgumentList[i];
				IsLastChild = i == statement.ArgumentList.Length - 1;

				Print(argument);
			}

			PopIndent();
		}

		IsLastChild = true;
		Print(statement.Body);
		
		PopIndent();
	}

	public void VisitIndexerStatement(Statement.Indexer statement)
	{
		Write("Indexer");
		
		PushIndent();
		IsLastChild = false;
		
		Write("Access: " + (statement.AccessModifier?.Text ?? "private"));
		
		Write("Parameters:");
		
		PushIndent();
		for (var i = 0; i < statement.ParameterList.Length; i++)
		{
			var parameter = statement.ParameterList[i];
			IsLastChild = i == statement.ParameterList.Length - 1;
			
			Write((parameter.IsReference ? "ref " : "") + parameter.Type + " " + parameter.Name.Text);
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

	public void VisitAccessorTokenStatement(Statement.AccessorToken statement)
	{
		Write("Accessor: " + statement.Token.Text);
		
		PushIndent();
		IsLastChild = true;
		Write("Access: " + (statement.AccessModifier?.Text ?? "public"));
		PopIndent();
	}

	public void VisitAccessorLambdaStatement(Statement.AccessorLambda statement)
	{
		Write("Accessor: Lambda");
		
		PushIndent();
		IsLastChild = false;
		Write("Access: " + (statement.AccessModifier?.Text ?? "public"));
		IsLastChild = true;
		Print(statement.LambdaExpression);
		PopIndent();
	}

	public void VisitAccessorLambdaSignatureStatement(Statement.AccessorLambdaSignature statement)
	{
		Write("Accessor: Lambda Signature");
		
		PushIndent();
		IsLastChild = false;
		Write("Access: " + (statement.AccessModifier?.Text ?? "public"));
		IsLastChild = true;
		Write("Signature: " + statement.LambdaSignature);
		PopIndent();
	}

	public void VisitEventStatement(Statement.Event statement)
	{
		Write("Event: " + statement.NameToken.Text);
		
		PushIndent();
		
		IsLastChild = false;
		Write("Access: " + (statement.AccessModifier?.Text ?? "private"));

		if (statement.EventModifier is { } eventModifier)
		{
			Write("Modifier: " + eventModifier.Text);
		}
		
		IsLastChild = true;
		Write("Parameters:");
		
		PushIndent();
		for (var i = 0; i < statement.ParameterList.Length; i++)
		{
			var parameter = statement.ParameterList[i];
			IsLastChild = i == statement.ParameterList.Length - 1;
			
			Write((parameter.IsReference ? "ref " : "") + parameter.Type + " " + parameter.Name.Text);
		}
		PopIndent();

		PopIndent();
	}

	public void VisitEventSignatureStatement(Statement.EventSignature statement)
	{
		Write("Event: " + statement.NameToken.Text);
		
		PushIndent();
		
		IsLastChild = false;

		if (statement.EventModifier is { } eventModifier)
		{
			Write("Modifier: " + eventModifier.Text);
		}
		
		IsLastChild = true;
		Write("Parameters:");
		
		PushIndent();
		for (var i = 0; i < statement.ParameterList.Length; i++)
		{
			var parameter = statement.ParameterList[i];
			IsLastChild = i == statement.ParameterList.Length - 1;
			
			Write((parameter.IsReference ? "ref " : "") + parameter.Type + " " + parameter.Name.Text);
		}
		PopIndent();

		PopIndent();
	}

	public void VisitPropertyStatement(Statement.Property statement)
	{
		Write("Property: " + statement.NameToken.Text);
		
		PushIndent();

		IsLastChild = false;
		Write("Type: " + statement.Type);
		Write("Access: " + (statement.AccessModifier?.Text ?? "private"));

		foreach (var modifier in statement.Modifiers)
		{
			Write("Modifier: " + modifier.Text);
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

	public void VisitPropertySignatureStatement(Statement.PropertySignature statement)
	{
		Write("Property Signature: " + statement.NameToken.Text);
		
		PushIndent();

		IsLastChild = false;
		Write("Type: " + statement.Type);

		foreach (var modifier in statement.Modifiers)
		{
			Write("Modifier: " + modifier.Text);
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

	public void VisitMethodStatement(Statement.Method statement)
	{
		Write("Method: " + statement.NameToken.Text);
		
		PushIndent();

		IsLastChild = false;
		Write("Return Type: " + statement.ReturnType);
		Write("Async: " + (statement.AsyncToken is null ? "False" : "True"));
		Write("Access: " + (statement.AccessModifier?.Text ?? "private"));
		
		if (statement.TypeParameters.Length > 0)
		{
			Write("Template Parameters:");
			PushIndent();

			for (var i = 0; i < statement.TypeParameters.Length; i++)
			{
				var typeParameter = statement.TypeParameters[i];
				IsLastChild = i == statement.TypeParameters.Length - 1;

				Write(typeParameter.Text);
			}

			PopIndent();
		}

		foreach (var modifier in statement.Modifiers)
		{
			Write("Modifier: " + modifier.Text);
		}
		
		Write("Parameters:");
		
		PushIndent();
		for (var i = 0; i < statement.ParameterList.Length; i++)
		{
			var parameter = statement.ParameterList[i];
			IsLastChild = i == statement.ParameterList.Length - 1;
			
			Write((parameter.IsReference ? "ref " : "") + parameter.Type + " " + parameter.Name.Text);
		}
		PopIndent();
		
		IsLastChild = true;
		Print(statement.Body);

		PopIndent();
	}

	public void VisitMethodSignatureStatement(Statement.MethodSignature statement)
	{
		Write("Method Signature: " + statement.NameToken.Text);
		
		PushIndent();

		IsLastChild = false;
		Write("Return Type: " + statement.ReturnType);
		Write("Async: " + (statement.AsyncToken is null ? "False" : "True"));
		
		if (statement.TypeParameters.Length > 0)
		{
			Write("Template Parameters:");
			PushIndent();

			for (var i = 0; i < statement.TypeParameters.Length; i++)
			{
				var typeParameter = statement.TypeParameters[i];
				IsLastChild = i == statement.TypeParameters.Length - 1;

				Write(typeParameter.Text);
			}

			PopIndent();
		}

		foreach (var modifier in statement.Modifiers)
		{
			Write("Modifier: " + modifier.Text);
		}

		IsLastChild = true;
		Write("Parameters:");
		
		PushIndent();
		for (var i = 0; i < statement.ParameterList.Length; i++)
		{
			var parameter = statement.ParameterList[i];
			IsLastChild = i == statement.ParameterList.Length - 1;
			
			Write((parameter.IsReference ? "ref " : "") + parameter.Type + " " + parameter.Name.Text);
		}
		PopIndent();

		PopIndent();
	}

	public void VisitFieldStatement(Statement.Field statement)
	{
		Write("Field: " + statement.NameToken.Text);
		
		PushIndent();

		IsLastChild = false;
		Write("Type: " + statement.Type);
		Write("Access: " + (statement.AccessModifier?.Text ?? "private"));

		foreach (var modifier in statement.Modifiers)
		{
			Write("Modifier: " + modifier.Text);
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

	public void VisitBreakStatement(Statement.Break statement)
	{
		Write("Break");
	}

	public void VisitContinueStatement(Statement.Continue statement)
	{
		Write("Continue");
	}

	public void VisitControlStatement(Statement.Control statement)
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

	public void VisitWhileStatement(Statement.While statement)
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

	public void VisitDoWhileStatement(Statement.DoWhile statement)
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

	public void VisitForStatement(Statement.For statement)
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

	public void VisitForEachStatement(Statement.ForEach statement)
	{
		Write("For Each:");
		
		PushIndent();

		IsLastChild = false;
		Write("Iterator Type: " + statement.IteratorType);
		Write("Iterator: " + statement.Iterator.Text);
		
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

	public void VisitRepeatStatement(Statement.Repeat statement)
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

	public void VisitReturnStatement(Statement.Return statement)
	{
		Write("Return");

		if (statement.Expression is null)
			return;
		
		PushIndent();
		IsLastChild = true;
		Print(statement.Expression);
		PopIndent();
	}

	public void VisitThrowStatement(Statement.Throw statement)
	{
		Write("Throw");

		if (statement.Expression is null)
			return;
		
		PushIndent();
		IsLastChild = true;
		Print(statement.Expression);
		PopIndent();
	}

	public void VisitSealStatement(Statement.Seal statement)
	{
		Write("Seal");
		
		PushIndent();
		IsLastChild = true;
		Write(statement.Identifier.Text);
		PopIndent();
	}

	public void VisitTryStatement(Statement.Try statement)
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
			Write(statement.CatchType + " " + statement.CatchNameToken?.Text);
			
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

	public void VisitVariableDeclarationStatement(Statement.VariableDeclaration statement)
	{
		Write("Variable Declaration");
		
		PushIndent();
		IsLastChild = statement.Initializer is null;
		Write("Identifier: " + statement.Identifier.Text);

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

	public void VisitLockStatement(Statement.Lock statement)
	{
		Write("Lock");
		
		PushIndent();
		
		IsLastChild = false;
		Print(statement.Expression);

		IsLastChild = true;
		Print(statement.Body);
		
		PopIndent();
	}

	public void VisitSwitchStatement(Statement.Switch statement)
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
					case Statement.Switch.ExpressionLabel expressionLabel:
						Write("Expression:");
						PushIndent();
						IsLastChild = true;
						Print(expressionLabel.Expression);
						PopIndent();
						break;
					case Statement.Switch.PatternLabel patternLabel:
						Write("Pattern: " + patternLabel.Type +
							  (patternLabel.Identifier is null ? "" : " " + patternLabel.Identifier.Text));
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

	public void VisitExpressionStatement(Statement.ExpressionStatement statement)
	{
		Write("Expression Statement");
		
		PushIndent();
		IsLastChild = true;
		Print(statement.Expression);
		PopIndent();
	}

	public void VisitOperatorOverloadStatement(Statement.OperatorOverload statement)
	{
		Write("Operator Overload");
		
		PushIndent();
		IsLastChild = false;
		Write("Return Type: " + statement.ReturnType);
		Write("Operator: " + statement.Operator.TokenSpan);
		
		Write("Parameters:");
		
		PushIndent();
		for (var i = 0; i < statement.ParameterList.Length; i++)
		{
			var parameter = statement.ParameterList[i];
			IsLastChild = i == statement.ParameterList.Length - 1;
			
			Write((parameter.IsReference ? "ref " : "") + parameter.Type + " " + parameter.Name.Text);
		}
		PopIndent();

		IsLastChild = true;
		Write("Body:");
		PushIndent();
		Print(statement.Body);
		PopIndent();
		
		PopIndent();
	}

	public void VisitOperatorOverloadSignatureStatement(Statement.OperatorOverloadSignature statement)
	{
		Write("Operator Overload");
		
		PushIndent();
		IsLastChild = false;
		Write("Return Type: " + statement.ReturnType);
		Write("Operator: " + statement.Operator.TokenSpan);
		
		IsLastChild = true;
		Write("Parameters:");
		
		PushIndent();
		for (var i = 0; i < statement.ParameterList.Length; i++)
		{
			var parameter = statement.ParameterList[i];
			IsLastChild = i == statement.ParameterList.Length - 1;
			
			Write((parameter.IsReference ? "ref " : "") + parameter.Type + " " + parameter.Name.Text);
		}
		PopIndent();
		
		PopIndent();
	}
}