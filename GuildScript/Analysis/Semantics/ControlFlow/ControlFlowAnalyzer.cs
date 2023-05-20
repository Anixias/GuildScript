namespace GuildScript.Analysis.Semantics.ControlFlow;

public sealed class ControlFlowAnalyzer : ResolvedStatement.IVisitor
{
	private Block currentBlock = new();
	private readonly Stack<Block> blockStack = new();
	private readonly List<ControlFlowGraph> graphs = new();
	
	public void Analyze(ResolvedTree tree)
	{
		try
		{
			if (tree.Root is ResolvedStatement statement)
				statement.AcceptVisitor(this);
		}
		catch (Exception e)
		{
			Console.ForegroundColor = ConsoleColor.DarkRed;
			Console.WriteLine(e.Message);
			Console.ResetColor();
		}
	}

	private void BeginGraph()
	{
		currentBlock = new Block();
		var graph = new ControlFlowGraph();
		graph.Add(currentBlock);
		graphs.Add(graph);
	}

	public void VisitProgramStatement(ResolvedStatement.Program statement)
	{
		foreach (var topLevelStatement in statement.Statements)
		{
			topLevelStatement.AcceptVisitor(this);
		}
	}

	public void VisitEntryPointStatement(ResolvedStatement.EntryPoint statement)
	{
		BeginGraph();
		statement.Body.AcceptVisitor(this);
	}

	public void VisitDefineStatement(ResolvedStatement.Define statement)
	{
		
	}

	public void VisitBlockStatement(ResolvedStatement.Block statement)
	{
		foreach (var stmt in statement.Statements)
		{
			stmt.AcceptVisitor(this);
		}
	}

	public void VisitClassStatement(ResolvedStatement.Class statement)
	{
		
	}

	public void VisitStructStatement(ResolvedStatement.Struct statement)
	{
		
	}

	public void VisitInterfaceStatement(ResolvedStatement.Interface statement)
	{
		
	}

	public void VisitEnumStatement(ResolvedStatement.Enum statement)
	{
		
	}

	public void VisitCastOverloadStatement(ResolvedStatement.CastOverload statement)
	{
		
	}

	public void VisitDestructorStatement(ResolvedStatement.Destructor statement)
	{
		
	}

	public void VisitExternalMethodStatement(ResolvedStatement.ExternalMethod statement)
	{
		
	}

	public void VisitConstructorStatement(ResolvedStatement.Constructor statement)
	{
		
	}

	public void VisitIndexerStatement(ResolvedStatement.Indexer statement)
	{
		
	}

	public void VisitAccessorAutoStatement(ResolvedStatement.AccessorAuto statement)
	{
		
	}

	public void VisitAccessorLambdaStatement(ResolvedStatement.AccessorLambda statement)
	{
		
	}

	public void VisitAccessorLambdaSignatureStatement(ResolvedStatement.AccessorLambdaSignature statement)
	{
		
	}

	public void VisitEventStatement(ResolvedStatement.Event statement)
	{
		
	}

	public void VisitEventSignatureStatement(ResolvedStatement.EventSignature statement)
	{
		
	}

	public void VisitPropertyStatement(ResolvedStatement.Property statement)
	{
		
	}

	public void VisitPropertySignatureStatement(ResolvedStatement.PropertySignature statement)
	{
		
	}

	public void VisitMethodStatement(ResolvedStatement.Method statement)
	{
		
	}

	public void VisitMethodSignatureStatement(ResolvedStatement.MethodSignature statement)
	{
		
	}

	public void VisitFieldStatement(ResolvedStatement.Field statement)
	{
		
	}

	public void VisitBreakStatement(ResolvedStatement.Break statement)
	{
		currentBlock.ControlExits = true;
		currentBlock.AddStatement(statement);
	}

	public void VisitContinueStatement(ResolvedStatement.Continue statement)
	{
		currentBlock.ControlExits = true;
		currentBlock.AddStatement(statement);
	}

	public void VisitControlStatement(ResolvedStatement.Control statement)
	{
		currentBlock.AddStatement(statement);
		var afterIfBlock = new Block();
		
		var thenBlock = new Block();
		currentBlock.Successors.Add(thenBlock);
		
		blockStack.Push(currentBlock);
		currentBlock = thenBlock;
		
		statement.ThenStatement.AcceptVisitor(this);
		if (!thenBlock.ControlExits)
			thenBlock.Successors.Add(afterIfBlock);

		currentBlock = blockStack.Pop();

		if (statement.ElseStatement is not null)
		{
			var elseBlock = new Block();
			currentBlock.Successors.Add(elseBlock);
			
			blockStack.Push(currentBlock);
			currentBlock = elseBlock;
			
			statement.ElseStatement.AcceptVisitor(this);
			
			if (!elseBlock.ControlExits)
				elseBlock.Successors.Add(afterIfBlock);

			currentBlock = blockStack.Pop();
		}
		else
		{
			currentBlock.Successors.Add(afterIfBlock);
		}
		
		currentBlock = afterIfBlock;
	}

	public void VisitWhileStatement(ResolvedStatement.While statement)
	{
		
	}

	public void VisitDoWhileStatement(ResolvedStatement.DoWhile statement)
	{
		
	}

	public void VisitForStatement(ResolvedStatement.For statement)
	{
		
	}

	public void VisitForEachStatement(ResolvedStatement.ForEach statement)
	{
		
	}

	public void VisitRepeatStatement(ResolvedStatement.Repeat statement)
	{
		
	}

	public void VisitReturnStatement(ResolvedStatement.Return statement)
	{
		currentBlock.ControlExits = true;
		currentBlock.AddStatement(statement);
	}

	public void VisitThrowStatement(ResolvedStatement.Throw statement)
	{
		
	}

	public void VisitSealStatement(ResolvedStatement.Seal statement)
	{
		
	}

	public void VisitTryStatement(ResolvedStatement.Try statement)
	{
		
	}

	public void VisitVariableDeclarationStatement(ResolvedStatement.VariableDeclaration statement)
	{
		currentBlock.AddStatement(statement);
	}

	public void VisitLockStatement(ResolvedStatement.Lock statement)
	{
		
	}

	public void VisitSwitchStatement(ResolvedStatement.Switch statement)
	{
		
	}

	public void VisitExpressionStatement(ResolvedStatement.ExpressionStatement statement)
	{
		
	}

	public void VisitOperatorOverloadStatement(ResolvedStatement.OperatorOverload statement)
	{
		
	}

	public void VisitOperatorOverloadSignatureStatement(ResolvedStatement.OperatorOverloadSignature statement)
	{
		
	}
}