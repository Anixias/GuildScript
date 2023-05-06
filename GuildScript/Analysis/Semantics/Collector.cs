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

		foreach (var topLevelStatement in statement.Statements)
		{
			topLevelStatement.AcceptVisitor(this);
		}

		for (var i = 0; i < statement.Module.Names.Length; i++)
		{
			semanticModel.Return();
		}
	}

	public void VisitEntryPointStatement(Statement.EntryPoint statement)
	{
		semanticModel.AddEntryPoint(statement);
	}

	public void VisitDefineStatement(Statement.Define statement)
	{
		
	}

	public void VisitBlockStatement(Statement.Block statement)
	{
		foreach (var bodyStatement in statement.Statements)
		{
			bodyStatement.AcceptVisitor(this);
		}
	}

	public void VisitClassStatement(Statement.Class statement)
	{
		try
		{
			var declaration = new Declaration(statement.NameToken, statement);
			var symbol = semanticModel.AddClass(statement.NameToken.Text, declaration);
			semanticModel.VisitSymbol(symbol);

			foreach (var member in statement.Members)
			{
				member.AcceptVisitor(this);
			}
		
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

			foreach (var member in statement.Members)
			{
				member.AcceptVisitor(this);
			}

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
			semanticModel.AddEnum(statement.NameToken.Text, declaration);
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitDestructorStatement(Statement.Destructor statement)
	{
		
	}

	public void VisitExternalMethodStatement(Statement.ExternalMethod statement)
	{
		
	}

	public void VisitConstructorStatement(Statement.Constructor statement)
	{
		
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
		
	}

	public void VisitMethodStatement(Statement.Method statement)
	{
		try
		{
			var declaration = new Declaration(statement.NameToken, statement);
			var methodSymbol = semanticModel.AddMethod(statement.NameToken.Text, declaration);

			foreach (var parameter in statement.ParameterList)
			{
				var parameterDeclaration = new Declaration(parameter.Name, statement);
				methodSymbol.AddParameter(parameter.Name.Text, parameterDeclaration);
			}
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitMethodSignatureStatement(Statement.MethodSignature statement)
	{
		
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
		
	}

	public void VisitWhileStatement(Statement.While statement)
	{
		
	}

	public void VisitDoWhileStatement(Statement.DoWhile statement)
	{
		
	}

	public void VisitForStatement(Statement.For statement)
	{
		
	}

	public void VisitForEachStatement(Statement.ForEach statement)
	{
		
	}

	public void VisitRepeatStatement(Statement.Repeat statement)
	{
		
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
		
	}

	public void VisitVariableDeclarationStatement(Statement.VariableDeclaration statement)
	{
		
	}

	public void VisitLockStatement(Statement.Lock statement)
	{
		
	}

	public void VisitSwitchStatement(Statement.Switch statement)
	{
		
	}

	public void VisitExpressionStatement(Statement.ExpressionStatement statement)
	{
		
	}

	public void VisitOperatorOverloadStatement(Statement.OperatorOverload statement)
	{
		
	}
}