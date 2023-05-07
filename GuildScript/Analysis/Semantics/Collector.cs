using GuildScript.Analysis.Syntax;
using GuildScript.Analysis.Text;

namespace GuildScript.Analysis.Semantics;

public sealed class Collector : Statement.IVisitor
{
	public DiagnosticCollection Diagnostics { get; } = new();
	private readonly SemanticModel semanticModel;

	public Collector(SemanticModel semanticModel)
	{
		this.semanticModel = semanticModel;
	}

	public void CollectSymbols(SyntaxTree tree)
	{
		if (tree.Root is Statement statement)
		{
			statement.AcceptVisitor(this);
		}
	}

	public void VisitProgramStatement(Statement.Program statement)
	{
		foreach (var name in statement.Module.Names)
		{
			var module = semanticModel.AddModule(name);
			semanticModel.VisitSymbol(module);
		}
		
		semanticModel.EnterScope(statement);
		foreach (var topLevelStatement in statement.Statements)
		{
			topLevelStatement.AcceptVisitor(this);
		}
		semanticModel.ExitScope();

		for (var i = 0; i < statement.Module.Names.Length; i++)
		{
			semanticModel.Return();
		}
	}

	public void VisitEntryPointStatement(Statement.EntryPoint statement)
	{
		var entryPointSymbol = semanticModel.AddEntryPoint(statement);
		semanticModel.VisitSymbol(entryPointSymbol);
		semanticModel.EnterScope(statement);
		
		foreach (var parameter in statement.ParameterList)
		{
			var parameterDeclaration = new Declaration(parameter.Name, statement);
			entryPointSymbol.AddParameter(parameter.Name.Text, parameterDeclaration);
		}
		
		statement.Body.AcceptVisitor(this);
		semanticModel.ExitScope();
		semanticModel.Return();
	}

	public void VisitDefineStatement(Statement.Define statement)
	{
		
	}

	public void VisitBlockStatement(Statement.Block statement)
	{
		semanticModel.EnterScope(statement);
		
		foreach (var bodyStatement in statement.Statements)
		{
			bodyStatement.AcceptVisitor(this);
		}
		
		semanticModel.ExitScope();
	}

	public void VisitClassStatement(Statement.Class statement)
	{
		try
		{
			var declaration = new Declaration(statement.NameToken, statement);
			var symbol = semanticModel.AddClass(statement.NameToken.Text, declaration);
			semanticModel.VisitSymbol(symbol);
			semanticModel.EnterScope(statement);

			foreach (var member in statement.Members)
			{
				member.AcceptVisitor(this);
			}
		
			semanticModel.ExitScope();
			semanticModel.Return();
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitStructStatement(Statement.Struct statement)
	{
		try
		{
			var declaration = new Declaration(statement.NameToken, statement);
			var symbol = semanticModel.AddStruct(statement.NameToken.Text, declaration);
			semanticModel.VisitSymbol(symbol);
			semanticModel.EnterScope(statement);

			foreach (var member in statement.Members)
			{
				member.AcceptVisitor(this);
			}
		
			semanticModel.ExitScope();
			semanticModel.Return();
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitInterfaceStatement(Statement.Interface statement)
	{
		try
		{
			var declaration = new Declaration(statement.NameToken, statement);
			semanticModel.AddInterface(statement.NameToken.Text, declaration);
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitEnumStatement(Statement.Enum statement)
	{
		try
		{
			var declaration = new Declaration(statement.NameToken, statement);
			var enumSymbol = semanticModel.AddEnum(statement.NameToken.Text, declaration);
			foreach (var member in statement.Members)
			{
				enumSymbol.AddMember(member.Identifier.Text);
			}
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitDestructorStatement(Statement.Destructor statement)
	{
		try
		{
			var declaration = new Declaration(statement.DestructorToken, statement);
			semanticModel.AddDestructor(statement.DestructorToken.Text, declaration);

			semanticModel.EnterScope(statement);
			statement.Body.AcceptVisitor(this);
			semanticModel.ExitScope();
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.DestructorToken, e.Message);
		}
	}

	public void VisitExternalMethodStatement(Statement.ExternalMethod statement)
	{
		try
		{
			var declaration = new Declaration(statement.Identifier, statement);
			var externalMethodSymbol = semanticModel.AddExternalMethod(statement.Identifier.Text, declaration);

			semanticModel.EnterScope(statement);
			foreach (var parameter in statement.ParameterList)
			{
				var parameterDeclaration = new Declaration(parameter.Name, statement);
				externalMethodSymbol.AddParameter(parameter.Name.Text, parameterDeclaration);
			}
			semanticModel.ExitScope();
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.Identifier, e.Message);
		}
	}

	public void VisitConstructorStatement(Statement.Constructor statement)
	{
		try
		{
			var declaration = new Declaration(statement.ConstructorToken, statement);
			var constructorSymbol = semanticModel.AddConstructor(statement.ConstructorToken.Text, declaration);

			semanticModel.EnterScope(statement);
			foreach (var parameter in statement.ParameterList)
			{
				var parameterDeclaration = new Declaration(parameter.Name, statement);
				constructorSymbol.AddParameter(parameter.Name.Text, parameterDeclaration);
			}
			
			statement.Body.AcceptVisitor(this);
			
			semanticModel.ExitScope();
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.ConstructorToken, e.Message);
		}
	}

	public void VisitIndexerStatement(Statement.Indexer statement)
	{
		
	}

	public void VisitAccessorTokenStatement(Statement.AccessorToken statement)
	{
		
	}

	public void VisitAccessorLambdaStatement(Statement.AccessorLambda statement)
	{
		
	}

	public void VisitAccessorLambdaSignatureStatement(Statement.AccessorLambdaSignature statement)
	{
		
	}

	public void VisitEventStatement(Statement.Event statement)
	{
		try
		{
			var declaration = new Declaration(statement.NameToken, statement);
			var eventSymbol = semanticModel.AddEvent(statement.NameToken.Text, declaration);

			foreach (var parameter in statement.ParameterList)
			{
				var parameterDeclaration = new Declaration(parameter.Name, statement);
				eventSymbol.AddParameter(parameter.Name.Text, parameterDeclaration);
			}
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitEventSignatureStatement(Statement.EventSignature statement)
	{
		try
		{
			var declaration = new Declaration(statement.NameToken, statement);
			var eventSymbol = semanticModel.AddEvent(statement.NameToken.Text, declaration);

			foreach (var parameter in statement.ParameterList)
			{
				var parameterDeclaration = new Declaration(parameter.Name, statement);
				eventSymbol.AddParameter(parameter.Name.Text, parameterDeclaration);
			}
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitPropertyStatement(Statement.Property statement)
	{
		try
		{
			var declaration = new Declaration(statement.NameToken, statement);
			semanticModel.AddProperty(statement.NameToken.Text, declaration);
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitPropertySignatureStatement(Statement.PropertySignature statement)
	{
		try
		{
			var declaration = new Declaration(statement.NameToken, statement);
			semanticModel.AddProperty(statement.NameToken.Text, declaration);
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitMethodStatement(Statement.Method statement)
	{
		try
		{
			var declaration = new Declaration(statement.NameToken, statement);
			var methodSymbol = semanticModel.AddMethod(statement.NameToken.Text, declaration);

			semanticModel.EnterScope(statement);
			foreach (var parameter in statement.ParameterList)
			{
				var parameterDeclaration = new Declaration(parameter.Name, statement);
				methodSymbol.AddParameter(parameter.Name.Text, parameterDeclaration);
			}
			
			statement.Body.AcceptVisitor(this);
			
			semanticModel.ExitScope();
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitMethodSignatureStatement(Statement.MethodSignature statement)
	{
		try
		{
			var declaration = new Declaration(statement.NameToken, statement);
			var methodSymbol = semanticModel.AddMethod(statement.NameToken.Text, declaration);

			semanticModel.EnterScope(statement);
			foreach (var parameter in statement.ParameterList)
			{
				var parameterDeclaration = new Declaration(parameter.Name, statement);
				methodSymbol.AddParameter(parameter.Name.Text, parameterDeclaration);
			}
			semanticModel.ExitScope();
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitFieldStatement(Statement.Field statement)
	{
		try
		{
			var declaration = new Declaration(statement.NameToken, statement);
			semanticModel.AddField(statement.NameToken.Text, declaration);
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitBreakStatement(Statement.Break statement)
	{
		
	}

	public void VisitContinueStatement(Statement.Continue statement)
	{
		
	}

	public void VisitControlStatement(Statement.Control statement)
	{
		semanticModel.EnterScope(statement);
		
		statement.IfStatement.AcceptVisitor(this);
		statement.ElseStatement?.AcceptVisitor(this);
		
		semanticModel.ExitScope();
	}

	public void VisitWhileStatement(Statement.While statement)
	{
		semanticModel.EnterScope(statement);
		statement.Body.AcceptVisitor(this);
		semanticModel.ExitScope();
	}

	public void VisitDoWhileStatement(Statement.DoWhile statement)
	{
		semanticModel.EnterScope(statement);
		statement.Body.AcceptVisitor(this);
		semanticModel.ExitScope();
	}

	public void VisitForStatement(Statement.For statement)
	{
		semanticModel.EnterScope(statement);
		
		statement.Initializer?.AcceptVisitor(this);
		statement.Increment?.AcceptVisitor(this);
		statement.Body.AcceptVisitor(this);
		
		semanticModel.ExitScope();
	}

	public void VisitForEachStatement(Statement.ForEach statement)
	{
		semanticModel.EnterScope(statement);

		semanticModel.AddLocalVariable(statement.Iterator.Text, new Declaration(statement.Iterator, statement),
			statement.IteratorType);
		statement.Body.AcceptVisitor(this);
		
		semanticModel.ExitScope();
	}

	public void VisitRepeatStatement(Statement.Repeat statement)
	{
		semanticModel.EnterScope(statement);
		statement.Body.AcceptVisitor(this);
		semanticModel.ExitScope();
	}

	public void VisitReturnStatement(Statement.Return statement)
	{
		
	}

	public void VisitThrowStatement(Statement.Throw statement)
	{
		
	}

	public void VisitSealStatement(Statement.Seal statement)
	{
		
	}

	public void VisitTryStatement(Statement.Try statement)
	{
		semanticModel.EnterScope(statement);
		
		statement.TryStatement.AcceptVisitor(this);
		statement.CatchStatement?.AcceptVisitor(this);
		statement.FinallyStatement?.AcceptVisitor(this);
		
		semanticModel.ExitScope();
	}

	public void VisitVariableDeclarationStatement(Statement.VariableDeclaration statement)
	{
		try
		{
			var declaration = new Declaration(statement.Identifier, statement);
			semanticModel.AddLocalVariable(statement.Identifier.Text, declaration, statement.Type);
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.Identifier, e.Message);
		}
	}

	public void VisitLockStatement(Statement.Lock statement)
	{
		
	}

	public void VisitSwitchStatement(Statement.Switch statement)
	{
		semanticModel.EnterScope(statement);

		foreach (var section in statement.Sections)
		{
			semanticModel.EnterScope(section.Body);
			foreach (var label in section.Labels)
			{
				if (label is not Statement.Switch.PatternLabel patternLabel)
					continue;

				if (patternLabel.Identifier is null)
					continue;
				
				semanticModel.AddLocalVariable(patternLabel.Identifier.Text,
					new Declaration(patternLabel.Identifier, statement), patternLabel.Type);
			}
			semanticModel.ExitScope();
		}

		semanticModel.ExitScope();
	}

	public void VisitExpressionStatement(Statement.ExpressionStatement statement)
	{
		
	}

	public void VisitOperatorOverloadStatement(Statement.OperatorOverload statement)
	{
		try
		{
			// @TODO Add support for TokenSpan in Declaration
			var declaration = new Declaration(statement.BinaryOperator.TokenSpan.Tokens[0], statement);
			var methodSymbol = semanticModel.AddMethod(statement.BinaryOperator.ToString(), declaration);

			semanticModel.EnterScope(statement);
			foreach (var parameter in statement.ParameterList)
			{
				var parameterDeclaration = new Declaration(parameter.Name, statement);
				methodSymbol.AddParameter(parameter.Name.Text, parameterDeclaration);
			}
			
			statement.Body.AcceptVisitor(this);
			
			semanticModel.ExitScope();
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.BinaryOperator.TokenSpan.Tokens[0], e.Message);
		}
	}
}