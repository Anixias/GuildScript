using GuildScript.Analysis.Syntax;

namespace GuildScript.Analysis.Semantics;

public sealed class Resolver : Statement.IVisitor
{
	private readonly SemanticModel semanticModel;

	public Resolver(SemanticModel semanticModel)
	{
		this.semanticModel = semanticModel;
	}

	public void Resolve()
	{
		
	}

	private ResolvedType? ResolveType(TypeSyntax typeSyntax)
	{
		switch (typeSyntax)
		{
			case BaseTypeSyntax baseTypeSyntax:
				var nativeResolvedType = SimpleResolvedType.FindNativeType(baseTypeSyntax.ToString());
				if (nativeResolvedType is null)
					throw new Exception($"The native type '{baseTypeSyntax}' could not be resolved.");

				return nativeResolvedType;
			//case ExpressionTypeSyntax expressionTypeSyntax:
				
		}

		return null;
	}

	public void VisitProgramStatement(Statement.Program statement)
	{
		foreach (var name in statement.Module.Names)
		{
			var module = semanticModel.GetModule(name);
			if (module is null)
				throw new Exception($"Missing module '{name}' in '{statement.Module}'.");
			
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
		throw new NotImplementedException();
	}

	public void VisitDefineStatement(Statement.Define statement)
	{
		throw new NotImplementedException();
	}

	public void VisitBlockStatement(Statement.Block statement)
	{
		throw new NotImplementedException();
	}

	public void VisitClassStatement(Statement.Class statement)
	{
		throw new NotImplementedException();
	}

	public void VisitStructStatement(Statement.Struct statement)
	{
		throw new NotImplementedException();
	}

	public void VisitInterfaceStatement(Statement.Interface statement)
	{
		throw new NotImplementedException();
	}

	public void VisitEnumStatement(Statement.Enum statement)
	{
		throw new NotImplementedException();
	}

	public void VisitDestructorStatement(Statement.Destructor statement)
	{
		throw new NotImplementedException();
	}

	public void VisitExternalMethodStatement(Statement.ExternalMethod statement)
	{
		throw new NotImplementedException();
	}

	public void VisitConstructorStatement(Statement.Constructor statement)
	{
		throw new NotImplementedException();
	}

	public void VisitIndexerStatement(Statement.Indexer statement)
	{
		throw new NotImplementedException();
	}

	public void VisitAccessorTokenStatement(Statement.AccessorToken statement)
	{
		throw new NotImplementedException();
	}

	public void VisitAccessorLambdaStatement(Statement.AccessorLambda statement)
	{
		throw new NotImplementedException();
	}

	public void VisitAccessorLambdaSignatureStatement(Statement.AccessorLambdaSignature statement)
	{
		throw new NotImplementedException();
	}

	public void VisitEventStatement(Statement.Event statement)
	{
		throw new NotImplementedException();
	}

	public void VisitEventSignatureStatement(Statement.EventSignature statement)
	{
		throw new NotImplementedException();
	}

	public void VisitPropertyStatement(Statement.Property statement)
	{
		throw new NotImplementedException();
	}

	public void VisitPropertySignatureStatement(Statement.PropertySignature statement)
	{
		throw new NotImplementedException();
	}

	public void VisitMethodStatement(Statement.Method statement)
	{
		throw new NotImplementedException();
	}

	public void VisitMethodSignatureStatement(Statement.MethodSignature statement)
	{
		throw new NotImplementedException();
	}

	public void VisitFieldStatement(Statement.Field statement)
	{
		throw new NotImplementedException();
	}

	public void VisitBreakStatement(Statement.Break statement)
	{
		throw new NotImplementedException();
	}

	public void VisitContinueStatement(Statement.Continue statement)
	{
		throw new NotImplementedException();
	}

	public void VisitControlStatement(Statement.Control statement)
	{
		throw new NotImplementedException();
	}

	public void VisitWhileStatement(Statement.While statement)
	{
		throw new NotImplementedException();
	}

	public void VisitDoWhileStatement(Statement.DoWhile statement)
	{
		throw new NotImplementedException();
	}

	public void VisitForStatement(Statement.For statement)
	{
		throw new NotImplementedException();
	}

	public void VisitForEachStatement(Statement.ForEach statement)
	{
		throw new NotImplementedException();
	}

	public void VisitRepeatStatement(Statement.Repeat statement)
	{
		throw new NotImplementedException();
	}

	public void VisitReturnStatement(Statement.Return statement)
	{
		throw new NotImplementedException();
	}

	public void VisitThrowStatement(Statement.Throw statement)
	{
		throw new NotImplementedException();
	}

	public void VisitSealStatement(Statement.Seal statement)
	{
		throw new NotImplementedException();
	}

	public void VisitTryStatement(Statement.Try statement)
	{
		throw new NotImplementedException();
	}

	public void VisitVariableDeclarationStatement(Statement.VariableDeclaration statement)
	{
		throw new NotImplementedException();
	}

	public void VisitLockStatement(Statement.Lock statement)
	{
		throw new NotImplementedException();
	}

	public void VisitSwitchStatement(Statement.Switch statement)
	{
		throw new NotImplementedException();
	}

	public void VisitExpressionStatement(Statement.ExpressionStatement statement)
	{
		throw new NotImplementedException();
	}

	public void VisitOperatorOverloadStatement(Statement.OperatorOverload statement)
	{
		throw new NotImplementedException();
	}
}