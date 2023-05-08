using System.Collections.Immutable;
using GuildScript.Analysis.Text;

namespace GuildScript.Analysis.Syntax;

public abstract class Statement : SyntaxNode
{
	public interface IVisitor
	{
		void VisitProgramStatement(Program statement);
		void VisitEntryPointStatement(EntryPoint statement);
		void VisitDefineStatement(Define statement);
		void VisitBlockStatement(Block statement);
		void VisitClassStatement(Class statement);
		void VisitStructStatement(Struct statement);
		void VisitInterfaceStatement(Interface statement);
		void VisitEnumStatement(Enum statement);
		void VisitDestructorStatement(Destructor statement);
		void VisitExternalMethodStatement(ExternalMethod statement);
		void VisitConstructorStatement(Constructor statement);
		void VisitIndexerStatement(Indexer statement);
		void VisitAccessorTokenStatement(AccessorToken statement);
		void VisitAccessorLambdaStatement(AccessorLambda statement);
		void VisitAccessorLambdaSignatureStatement(AccessorLambdaSignature statement);
		void VisitEventStatement(Event statement);
		void VisitEventSignatureStatement(EventSignature statement);
		void VisitPropertyStatement(Property statement);
		void VisitPropertySignatureStatement(PropertySignature statement);
		void VisitMethodStatement(Method statement);
		void VisitMethodSignatureStatement(MethodSignature statement);
		void VisitFieldStatement(Field statement);
		void VisitBreakStatement(Break statement);
		void VisitContinueStatement(Continue statement);
		void VisitControlStatement(Control statement);
		void VisitWhileStatement(While statement);
		void VisitDoWhileStatement(DoWhile statement);
		void VisitForStatement(For statement);
		void VisitForEachStatement(ForEach statement);
		void VisitRepeatStatement(Repeat statement);
		void VisitReturnStatement(Return statement);
		void VisitThrowStatement(Throw statement);
		void VisitSealStatement(Seal statement);
		void VisitTryStatement(Try statement);
		void VisitVariableDeclarationStatement(VariableDeclaration statement);
		void VisitLockStatement(Lock statement);
		void VisitSwitchStatement(Switch statement);
		void VisitExpressionStatement(ExpressionStatement statement);
		void VisitOperatorOverloadStatement(OperatorOverload statement);
		void VisitOperatorOverloadSignatureStatement(OperatorOverloadSignature statement);
	}
	
	public abstract void AcceptVisitor(IVisitor visitor);

	public sealed class Program : Statement
	{
		public ImmutableArray<ModuleName> ImportedModules { get; }
		public ModuleName Module { get; }
		public ImmutableArray<Statement> Statements { get; }

		public Program(IEnumerable<ModuleName> importedModules, ModuleName module, IEnumerable<Statement> statements)
		{
			ImportedModules = importedModules.ToImmutableArray();
			Module = module;
			Statements = statements.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitProgramStatement(this);
		}
	}

	public sealed class Define : Statement
	{
		public SyntaxToken Identifier { get; }
		public TypeSyntax? Type { get; }

		public Define(SyntaxToken identifier, TypeSyntax? type)
		{
			Identifier = identifier;
			Type = type;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitDefineStatement(this);
		}
	}

	public sealed class EntryPoint : Statement
	{
		public TypeSyntax? ReturnType { get; }
		public SyntaxToken Identifier { get; }
		public ImmutableArray<Variable> ParameterList { get; }
		public Statement Body { get; }

		public EntryPoint(TypeSyntax? returnType, SyntaxToken identifier, IEnumerable<Variable> parameterList,
						  Statement body)
		{
			ReturnType = returnType;
			Identifier = identifier;
			ParameterList = parameterList.ToImmutableArray();
			Body = body;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitEntryPointStatement(this);
		}
	}

	public sealed class Block : Statement
	{
		public ImmutableArray<Statement> Statements { get; }
		
		public Block(IEnumerable<Statement> statements)
		{
			Statements = statements.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitBlockStatement(this);
		}
	}

	public sealed class Class : Statement
	{
		public SyntaxToken? AccessModifier { get; }
		public SyntaxToken? ClassModifier { get; }
		public SyntaxToken NameToken { get; }
		public ImmutableArray<SyntaxToken> TypeParameters { get; }
		public TypeSyntax? BaseClass { get; }
		public ImmutableArray<Statement> Members { get; }

		public Class(SyntaxToken? accessModifier, SyntaxToken? classModifier, SyntaxToken nameToken,
					 IEnumerable<SyntaxToken> typeParameters, TypeSyntax? baseClass, IEnumerable<Statement> members)
		{
			AccessModifier = accessModifier;
			ClassModifier = classModifier;
			NameToken = nameToken;
			TypeParameters = typeParameters.ToImmutableArray();
			BaseClass = baseClass;
			Members = members.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitClassStatement(this);
		}
	}

	public sealed class Struct : Statement
	{
		public SyntaxToken? AccessModifier { get; }
		public SyntaxToken? StructModifier { get; }
		public SyntaxToken NameToken { get; }
		public ImmutableArray<Statement> Members { get; }

		public Struct(SyntaxToken? accessModifier, SyntaxToken? structModifier, SyntaxToken nameToken,
					  IEnumerable<Statement> members)
		{
			AccessModifier = accessModifier;
			StructModifier = structModifier;
			NameToken = nameToken;
			Members = members.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitStructStatement(this);
		}
	}

	public sealed class Interface : Statement
	{
		public SyntaxToken? AccessModifier { get; }
		public SyntaxToken NameToken { get; }
		public ImmutableArray<Statement> Members { get; }

		public Interface(SyntaxToken? accessModifier, SyntaxToken nameToken, IEnumerable<Statement> members)
		{
			AccessModifier = accessModifier;
			NameToken = nameToken;
			Members = members.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitInterfaceStatement(this);
		}
	}

	public sealed class Enum : Statement
	{
		public class Member
		{
			public SyntaxToken Identifier { get; }
			public Expression? Expression { get; }

			public Member(SyntaxToken identifier, Expression? expression)
			{
				Identifier = identifier;
				Expression = expression;
			}
		}
		
		public SyntaxToken? AccessModifier { get; }
		public SyntaxToken NameToken { get; }
		public ImmutableArray<Member> Members { get; }
		public TypeSyntax Type { get; }

		public Enum(SyntaxToken? accessModifier, SyntaxToken nameToken, IEnumerable<Member> members, TypeSyntax type)
		{
			AccessModifier = accessModifier;
			NameToken = nameToken;
			Type = type;
			Members = members.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitEnumStatement(this);
		}
	}

	public sealed class Destructor : Statement
	{
		public SyntaxToken DestructorToken { get; }
		public Statement Body { get; }
		
		public Destructor(Statement body, SyntaxToken destructorToken)
		{
			Body = body;
			DestructorToken = destructorToken;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitDestructorStatement(this);
		}
	}

	public sealed class ExternalMethod : Statement
	{
		public TypeSyntax? ReturnType { get; }
		public SyntaxToken Identifier { get; }
		public ImmutableArray<Variable> ParameterList { get; }
		
		public ExternalMethod(TypeSyntax? returnType, SyntaxToken identifier, IEnumerable<Variable> parameterList)
		{
			ReturnType = returnType;
			Identifier = identifier;
			ParameterList = parameterList.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitExternalMethodStatement(this);
		}
	}

	public sealed class Constructor : Statement
	{
		public SyntaxToken ConstructorToken { get; }
		public SyntaxToken? AccessModifier { get; }
		public ImmutableArray<Variable> ParameterList { get; }
		public Statement Body { get; }
		public SyntaxToken? Initializer { get; }
		public ImmutableArray<Expression> ArgumentList { get; }

		public Constructor(SyntaxToken? accessModifier, IEnumerable<Variable> parameterList, Statement body,
						   SyntaxToken? initializer, IEnumerable<Expression> argumentList, SyntaxToken constructorToken)
		{
			AccessModifier = accessModifier;
			ParameterList = parameterList.ToImmutableArray();
			Body = body;
			Initializer = initializer;
			ConstructorToken = constructorToken;
			ArgumentList = argumentList.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitConstructorStatement(this);
		}
	}

	public sealed class Indexer : Statement
	{
		public SyntaxToken? AccessModifier { get; }
		public TypeSyntax Type { get; }
		public ImmutableArray<Variable> ParameterList { get; }
		public ImmutableArray<Statement> Body { get; }

		public Indexer(SyntaxToken? accessModifier, TypeSyntax type, IEnumerable<Variable> parameterList,
					   IEnumerable<Statement> body)
		{
			AccessModifier = accessModifier;
			Type = type;
			ParameterList = parameterList.ToImmutableArray();
			Body = body.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitIndexerStatement(this);
		}
	}

	public sealed class Event : Statement
	{
		public SyntaxToken? AccessModifier { get; }
		public SyntaxToken? EventModifier { get; }
		public SyntaxToken NameToken { get; }
		public ImmutableArray<Variable> ParameterList { get; }

		public Event(SyntaxToken? accessModifier, SyntaxToken? eventModifier, SyntaxToken nameToken,
					 IEnumerable<Variable> parameterList)
		{
			AccessModifier = accessModifier;
			EventModifier = eventModifier;
			NameToken = nameToken;
			ParameterList = parameterList.ToImmutableArray();
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitEventStatement(this);
		}
	}
	
	public sealed class EventSignature : Statement
	{
		public SyntaxToken? EventModifier { get; }
		public SyntaxToken NameToken { get; }
		public ImmutableArray<Variable> ParameterList { get; }

		public EventSignature(SyntaxToken? eventModifier, SyntaxToken nameToken, IEnumerable<Variable> parameterList)
		{
			EventModifier = eventModifier;
			NameToken = nameToken;
			ParameterList = parameterList.ToImmutableArray();
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitEventSignatureStatement(this);
		}
	}

	public abstract class Accessor : Statement
	{
		
	}

	public sealed class AccessorToken : Accessor
	{
		public SyntaxToken? AccessModifier { get; }
		public SyntaxToken Token { get; }

		public AccessorToken(SyntaxToken? accessModifier, SyntaxToken token)
		{
			AccessModifier = accessModifier;
			Token = token;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitAccessorTokenStatement(this);
		}
	}

	public sealed class AccessorLambda : Accessor
	{
		public SyntaxToken? AccessModifier { get; }
		public Expression.Lambda LambdaExpression { get; }

		public AccessorLambda(SyntaxToken? accessModifier, Expression.Lambda lambdaExpression)
		{
			AccessModifier = accessModifier;
			LambdaExpression = lambdaExpression;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitAccessorLambdaStatement(this);
		}
	}

	public sealed class AccessorLambdaSignature : Accessor
	{
		public SyntaxToken? AccessModifier { get; }
		public LambdaTypeSyntax LambdaSignature { get; }

		public AccessorLambdaSignature(SyntaxToken? accessModifier, LambdaTypeSyntax lambdaSignature)
		{
			AccessModifier = accessModifier;
			LambdaSignature = lambdaSignature;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitAccessorLambdaSignatureStatement(this);
		}
	}

	public sealed class Property : Statement
	{
		public SyntaxToken? AccessModifier { get; }
		public ImmutableArray<SyntaxToken> Modifiers { get; }
		public TypeSyntax Type { get; }
		public SyntaxToken NameToken { get; }
		public ImmutableArray<Statement> Body { get; }

		public Property(SyntaxToken? accessModifier, IEnumerable<SyntaxToken> modifiers, TypeSyntax type,
						SyntaxToken nameToken, IEnumerable<Statement> body)
		{
			AccessModifier = accessModifier;
			Modifiers = modifiers.ToImmutableArray();
			Type = type;
			NameToken = nameToken;
			Body = body.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitPropertyStatement(this);
		}
	}

	public sealed class PropertySignature : Statement
	{
		public ImmutableArray<SyntaxToken> Modifiers { get; }
		public TypeSyntax Type { get; }
		public SyntaxToken NameToken { get; }
		public ImmutableArray<Statement> Body { get; }

		public PropertySignature(IEnumerable<SyntaxToken> modifiers, TypeSyntax type,
								 SyntaxToken nameToken, IEnumerable<Statement> body)
		{
			Modifiers = modifiers.ToImmutableArray();
			Type = type;
			NameToken = nameToken;
			Body = body.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitPropertySignatureStatement(this);
		}
	}

	public sealed class Method : Statement
	{
		public SyntaxToken? AccessModifier { get; }
		public ImmutableArray<SyntaxToken> Modifiers { get; }
		public TypeSyntax? ReturnType { get; }
		public SyntaxToken NameToken { get; }
		public Block Body { get; }
		public ImmutableArray<Variable> ParameterList { get; }
		public SyntaxToken? AsyncToken { get; }
		public ImmutableArray<SyntaxToken> TypeParameters { get; }

		public Method(SyntaxToken? accessModifier, IEnumerable<SyntaxToken> modifiers, TypeSyntax? returnType,
					  SyntaxToken nameToken, Block body, IEnumerable<Variable> parameterList, SyntaxToken? asyncToken,
					  IEnumerable<SyntaxToken> typeParameters)
		{
			AccessModifier = accessModifier;
			Modifiers = modifiers.ToImmutableArray();
			ReturnType = returnType;
			NameToken = nameToken;
			Body = body;
			ParameterList = parameterList.ToImmutableArray();
			AsyncToken = asyncToken;
			TypeParameters = typeParameters.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitMethodStatement(this);
		}
	}

	public sealed class MethodSignature : Statement
	{
		public ImmutableArray<SyntaxToken> Modifiers { get; }
		public TypeSyntax? ReturnType { get; }
		public SyntaxToken NameToken { get; }
		public ImmutableArray<Variable> ParameterList { get; }
		public SyntaxToken? AsyncToken { get; }
		public ImmutableArray<SyntaxToken> TypeParameters { get; }

		public MethodSignature(IEnumerable<SyntaxToken> modifiers, TypeSyntax? returnType,
							   SyntaxToken nameToken, IEnumerable<Variable> parameterList, SyntaxToken? asyncToken,
							   IEnumerable<SyntaxToken> typeParameters)
		{
			Modifiers = modifiers.ToImmutableArray();
			ReturnType = returnType;
			NameToken = nameToken;
			ParameterList = parameterList.ToImmutableArray();
			AsyncToken = asyncToken;
			TypeParameters = typeParameters.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitMethodSignatureStatement(this);
		}
	}

	public sealed class OperatorOverload : Statement
	{
		public TypeSyntax ReturnType { get; }
		public SyntaxTokenSpan OperatorTokens { get; }
		public ImmutableArray<Variable> ParameterList { get; }
		public Statement Body { get; }

		public OperatorOverload(TypeSyntax returnType, SyntaxTokenSpan operatorTokens,
								IEnumerable<Variable> parameterList, Statement body)

		{
			ReturnType = returnType;
			OperatorTokens = operatorTokens;
			ParameterList = parameterList.ToImmutableArray();
			Body = body;
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitOperatorOverloadStatement(this);
		}
	}

	public sealed class OperatorOverloadSignature : Statement
	{
		public TypeSyntax ReturnType { get; }
		public SyntaxTokenSpan OperatorTokens { get; }
		public ImmutableArray<Variable> ParameterList { get; }

		public OperatorOverloadSignature(TypeSyntax returnType, SyntaxTokenSpan operatorTokens, 
										 IEnumerable<Variable> parameterList)

		{
			ReturnType = returnType;
			OperatorTokens = operatorTokens;
			ParameterList = parameterList.ToImmutableArray();
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitOperatorOverloadSignatureStatement(this);
		}
	}

	public sealed class Field : Statement
	{
		public SyntaxToken? AccessModifier { get; }
		public ImmutableArray<SyntaxToken> Modifiers { get; }
		public TypeSyntax? Type { get; }
		public SyntaxToken NameToken { get; }
		public Expression? Initializer { get; }

		public Field(SyntaxToken? accessModifier, IEnumerable<SyntaxToken> modifiers, TypeSyntax? type,
					 SyntaxToken nameToken, Expression? initializer)
		{
			AccessModifier = accessModifier;
			Modifiers = modifiers.ToImmutableArray();
			Type = type;
			NameToken = nameToken;
			Initializer = initializer;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitFieldStatement(this);
		}
	}

	public sealed class Break : Statement
	{
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitBreakStatement(this);
		}
	}

	public sealed class Continue : Statement
	{
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitContinueStatement(this);
		}
	}

	public sealed class Control : Statement
	{
		public Expression IfExpression { get; }
		public Statement IfStatement { get; }
		public Statement? ElseStatement { get; }
		
		public Control(Expression ifExpression, Statement ifStatement, Statement? elseStatement)
		{
			IfExpression = ifExpression;
			IfStatement = ifStatement;
			ElseStatement = elseStatement;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitControlStatement(this);
		}
	}

	public sealed class While : Statement
	{
		public Expression Condition { get; }
		public Statement Body { get; }
		
		public While(Expression condition, Statement body)
		{
			Condition = condition;
			Body = body;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitWhileStatement(this);
		}
	}

	public sealed class DoWhile : Statement
	{
		public Statement Body { get; }
		public Expression Condition { get; }
		
		public DoWhile(Statement body, Expression condition)
		{
			Body = body;
			Condition = condition;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitDoWhileStatement(this);
		}
	}

	public sealed class For : Statement
	{
		public Statement? Initializer { get; }
		public Expression? Condition { get; }
		public Statement? Increment { get; }
		public Statement Body { get; }
		
		public For(Statement? initializer, Expression? condition, Statement? increment, Statement body)
		{
			Initializer = initializer;
			Condition = condition;
			Increment = increment;
			Body = body;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitForStatement(this);
		}
	}

	public sealed class ForEach : Statement
	{
		public TypeSyntax? IteratorType { get; }
		public SyntaxToken Iterator { get; }
		public Expression Enumerable { get; }
		public Statement Body { get; }
		
		public ForEach(TypeSyntax? iteratorType, SyntaxToken iterator, Expression enumerable, Statement body)
		{
			IteratorType = iteratorType;
			Iterator = iterator;
			Enumerable = enumerable;
			Body = body;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitForEachStatement(this);
		}
	}

	public sealed class Repeat : Statement
	{
		public Expression Repetitions { get; }
		public Statement Body { get; }
		
		public Repeat(Expression repetitions, Statement body)
		{
			Repetitions = repetitions;
			Body = body;
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitRepeatStatement(this);
		}
	}

	public sealed class Return : Statement
	{
		public Expression? Expression { get; }
		
		public Return(Expression? expression)
		{
			Expression = expression;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitReturnStatement(this);
		}
	}

	public sealed class Throw : Statement
	{
		public Expression? Expression { get; }

		public Throw(Expression? expression)
		{
			Expression = expression;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitThrowStatement(this);
		}
	}

	public sealed class Seal : Statement
	{
		public SyntaxToken Identifier { get; }
		
		public Seal(SyntaxToken identifier)
		{
			Identifier = identifier;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitSealStatement(this);
		}
	}

	public sealed class Try : Statement
	{
		public Statement TryStatement { get; }
		public TypeSyntax? CatchType { get; }
		public SyntaxToken? CatchNameToken { get; }
		public Statement? CatchStatement { get; }
		public Statement? FinallyStatement { get; }
		
		public Try(Statement tryStatement, TypeSyntax? catchType, SyntaxToken? catchNameToken,
				   Statement? catchStatement, Statement? finallyStatement)
		{
			TryStatement = tryStatement;
			CatchType = catchType;
			CatchNameToken = catchNameToken;
			CatchStatement = catchStatement;
			FinallyStatement = finallyStatement;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitTryStatement(this);
		}
	}

	public sealed class VariableDeclaration : Statement
	{
		public TypeSyntax? Type { get; }
		public SyntaxToken Identifier { get; }
		public Expression? Initializer { get; }
		
		public VariableDeclaration(TypeSyntax? type, SyntaxToken identifier, Expression? initializer)
		{
			Type = type;
			Identifier = identifier;
			Initializer = initializer;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitVariableDeclarationStatement(this);
		}
	}

	public sealed class Lock : Statement
	{
		public Expression Expression { get; }
		public Statement Body { get; }
		
		public Lock(Expression expression, Statement body)
		{
			Expression = expression;
			Body = body;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitLockStatement(this);
		}
	}

	public sealed class Switch : Statement
	{
		public sealed class Section
		{
			public ImmutableArray<Label> Labels { get; }
			public Statement Body { get; }
			
			public Section(IEnumerable<Label> labels, Statement body)
			{
				Labels = labels.ToImmutableArray();
				Body = body;
			}
		}

		public class Label
		{
			protected Label()
			{
				
			}
			
			public static readonly Label Default = new Label();
		}

		public sealed class ExpressionLabel : Label
		{
			public Expression Expression { get; }
			
			public ExpressionLabel(Expression expression)
			{
				Expression = expression;
			}
		}

		public sealed class PatternLabel : Label
		{
			public TypeSyntax Type { get; }
			public SyntaxToken? Identifier { get; }
			
			public PatternLabel(TypeSyntax type, SyntaxToken? identifier)
			{
				Type = type;
				Identifier = identifier;
			}
		}
		
		public Expression Expression { get; }
		public ImmutableArray<Section> Sections { get; }

		public Switch(Expression expression, IEnumerable<Section> sections)
		{
			Expression = expression;
			Sections = sections.ToImmutableArray();
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitSwitchStatement(this);
		}
	}

	public sealed class ExpressionStatement : Statement
	{
		public Expression Expression { get; }
		
		public ExpressionStatement(Expression expression)
		{
			Expression = expression;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitExpressionStatement(this);
		}
	}
}