using System.Collections.Immutable;
using GuildScript.Analysis.Semantics.Symbols;
using GuildScript.Analysis.Syntax;

namespace GuildScript.Analysis.Semantics;

public abstract class ResolvedStatement : ResolvedNode
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
		void VisitAccessorAutoStatement(AccessorAuto statement);
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
	}
	
	public abstract void AcceptVisitor(IVisitor visitor);
	
	public sealed class Program : ResolvedStatement
	{
		public ImmutableArray<ResolvedStatement> Statements { get; }
		public ModuleSymbol ModuleSymbol { get; }

		public Program(IEnumerable<ResolvedStatement> statements, ModuleSymbol moduleSymbol)
		{
			Statements = statements.ToImmutableArray();
			ModuleSymbol = moduleSymbol;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitProgramStatement(this);
		}
	}

	public sealed class Define : ResolvedStatement
	{
		public string Identifier { get; }
		public ResolvedType Type { get; }

		public Define(string identifier, ResolvedType type)
		{
			Identifier = identifier;
			Type = type;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitDefineStatement(this);
		}
	}

	public sealed class EntryPoint : ResolvedStatement
	{
		public ResolvedType? ReturnType { get; }
		public MethodSymbol MethodSymbol { get; }
		public ImmutableArray<ParameterSymbol> ParameterList { get; }
		public ResolvedStatement Body { get; }

		public EntryPoint(ResolvedType? returnType, MethodSymbol methodSymbol,
						  IEnumerable<ParameterSymbol> parameterList, ResolvedStatement body)
		{
			ReturnType = returnType;
			MethodSymbol = methodSymbol;
			ParameterList = parameterList.ToImmutableArray();
			Body = body;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitEntryPointStatement(this);
		}
	}

	public sealed class Block : ResolvedStatement
	{
		public ImmutableArray<ResolvedStatement> Statements { get; }
		
		public Block(IEnumerable<ResolvedStatement> statements)
		{
			Statements = statements.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitBlockStatement(this);
		}
	}

	public sealed class Class : ResolvedStatement
	{
		public AccessModifier AccessModifier { get; }
		public ClassModifier ClassModifier { get; }
		public ClassSymbol ClassSymbol { get; }
		public ImmutableArray<string> TypeParameters { get; }
		public ClassSymbol? BaseClass { get; }
		public ImmutableArray<ResolvedStatement> Members { get; }

		public Class(AccessModifier accessModifier, ClassModifier classModifier, ClassSymbol classSymbol,
					 IEnumerable<string> typeParameters, ClassSymbol? baseClass, IEnumerable<ResolvedStatement> members)
		{
			AccessModifier = accessModifier;
			ClassModifier = classModifier;
			ClassSymbol = classSymbol;
			TypeParameters = typeParameters.ToImmutableArray();
			BaseClass = baseClass;
			Members = members.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitClassStatement(this);
		}
	}

	public sealed class Struct : ResolvedStatement
	{
		public AccessModifier AccessModifier { get; }
		public StructModifier StructModifier { get; }
		public StructSymbol StructSymbol { get; }
		public ImmutableArray<ResolvedStatement> Members { get; }

		public Struct(AccessModifier accessModifier, StructModifier structModifier, StructSymbol structSymbol,
					  IEnumerable<ResolvedStatement> members)
		{
			AccessModifier = accessModifier;
			StructModifier = structModifier;
			StructSymbol = structSymbol;
			Members = members.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitStructStatement(this);
		}
	}

	public sealed class Interface : ResolvedStatement
	{
		public AccessModifier AccessModifier { get; }
		public InterfaceSymbol InterfaceSymbol { get; }
		public ImmutableArray<ResolvedStatement> Members { get; }

		public Interface(AccessModifier accessModifier, InterfaceSymbol interfaceSymbol,
						 IEnumerable<ResolvedStatement> members)
		{
			AccessModifier = accessModifier;
			InterfaceSymbol = interfaceSymbol;
			Members = members.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitInterfaceStatement(this);
		}
	}

	public sealed class Enum : ResolvedStatement
	{
		public class Member
		{
			public string Identifier { get; }
			public ResolvedExpression Expression { get; }

			public Member(string identifier, ResolvedExpression expression)
			{
				Identifier = identifier;
				Expression = expression;
			}
		}
		
		public AccessModifier AccessModifier { get; }
		public EnumSymbol EnumSymbol { get; }
		public ImmutableArray<Member> Members { get; }
		public ResolvedType Type { get; }

		public Enum(AccessModifier accessModifier, EnumSymbol enumSymbol, IEnumerable<Member> members,
					ResolvedType type)
		{
			AccessModifier = accessModifier;
			EnumSymbol = enumSymbol;
			Type = type;
			Members = members.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitEnumStatement(this);
		}
	}

	public sealed class Destructor : ResolvedStatement
	{
		public ResolvedStatement Body { get; }
		public DestructorSymbol DestructorSymbol { get; }
		
		public Destructor(ResolvedStatement body, DestructorSymbol destructorSymbol)
		{
			Body = body;
			DestructorSymbol = destructorSymbol;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitDestructorStatement(this);
		}
	}

	public sealed class ExternalMethod : ResolvedStatement
	{
		public ResolvedType? ReturnType { get; }
		public ExternalMethodSymbol ExternalMethodSymbol { get; }
		public ImmutableArray<ParameterSymbol> ParameterList { get; }

		public ExternalMethod(ResolvedType? returnType, ExternalMethodSymbol externalMethodSymbol,
							  IEnumerable<ParameterSymbol> parameterList)
		{
			ReturnType = returnType;
			ExternalMethodSymbol = externalMethodSymbol;
			ParameterList = parameterList.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitExternalMethodStatement(this);
		}
	}

	public sealed class Constructor : ResolvedStatement
	{
		public AccessModifier AccessModifier { get; }
		public ConstructorSymbol ConstructorSymbol { get; }
		public ImmutableArray<ParameterSymbol> ParameterList { get; }
		public ResolvedStatement Body { get; }
		public ConstructorSymbol? Initializer { get; }
		public ImmutableArray<ResolvedExpression> ArgumentList { get; }

		public Constructor(AccessModifier accessModifier, ConstructorSymbol constructorSymbol,
						   IEnumerable<ParameterSymbol> parameterList, ResolvedStatement body,
						   ConstructorSymbol? initializer, IEnumerable<ResolvedExpression> argumentList)
		{
			AccessModifier = accessModifier;
			ConstructorSymbol = constructorSymbol;
			ParameterList = parameterList.ToImmutableArray();
			Body = body;
			Initializer = initializer;
			ArgumentList = argumentList.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitConstructorStatement(this);
		}
	}

	public sealed class Indexer : ResolvedStatement
	{
		public AccessModifier AccessModifier { get; }
		public ResolvedType Type { get; }
		public ImmutableArray<ParameterSymbol> ParameterList { get; }
		public ImmutableArray<ResolvedStatement> Body { get; }

		public Indexer(AccessModifier accessModifier, ResolvedType type, IEnumerable<ParameterSymbol> parameterList,
					   IEnumerable<ResolvedStatement> body)
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

	public sealed class Event : ResolvedStatement
	{
		public AccessModifier AccessModifier { get; }
		public EventModifier EventModifier { get; }
		public EventSymbol EventSymbol { get; }
		public ImmutableArray<ParameterSymbol> ParameterList { get; }

		public Event(AccessModifier accessModifier, EventModifier eventModifier, EventSymbol eventSymbol,
					 IEnumerable<ParameterSymbol> parameterList)
		{
			AccessModifier = accessModifier;
			EventModifier = eventModifier;
			EventSymbol = eventSymbol;
			ParameterList = parameterList.ToImmutableArray();
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitEventStatement(this);
		}
	}
	
	public sealed class EventSignature : ResolvedStatement
	{
		public EventModifier EventModifier { get; }
		public EventSymbol EventSymbol { get; }
		public ImmutableArray<ParameterSymbol> ParameterList { get; }

		public EventSignature(EventModifier eventModifier, EventSymbol eventSymbol,
							  IEnumerable<ParameterSymbol> parameterList)
		{
			EventModifier = eventModifier;
			EventSymbol = eventSymbol;
			ParameterList = parameterList.ToImmutableArray();
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitEventSignatureStatement(this);
		}
	}

	public abstract class Accessor : ResolvedStatement
	{
		
	}

	public sealed class AccessorAuto : Accessor
	{
		public AccessModifier AccessModifier { get; }
		public AccessorAutoType AutoType { get; }

		public AccessorAuto(AccessModifier accessModifier, AccessorAutoType autoType)
		{
			AccessModifier = accessModifier;
			AutoType = autoType;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitAccessorAutoStatement(this);
		}
	}

	public sealed class AccessorLambda : Accessor
	{
		public AccessModifier AccessModifier { get; }
		public ResolvedExpression.Lambda LambdaExpression { get; }

		public AccessorLambda(AccessModifier accessModifier, ResolvedExpression.Lambda lambdaExpression)
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
		public AccessModifier AccessModifier { get; }
		public LambdaTypeSymbol LambdaTypeSymbol { get; }

		public AccessorLambdaSignature(AccessModifier accessModifier, LambdaTypeSymbol lambdaTypeSymbol)
		{
			AccessModifier = accessModifier;
			LambdaTypeSymbol = lambdaTypeSymbol;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitAccessorLambdaSignatureStatement(this);
		}
	}

	public sealed class Property : ResolvedStatement
	{
		public AccessModifier AccessModifier { get; }
		public ImmutableArray<SyntaxToken> Modifiers { get; }
		public TypeSyntax Type { get; }
		public SyntaxToken NameToken { get; }
		public ImmutableArray<Statement> Body { get; }

		public Property(AccessModifier accessModifier, IEnumerable<SyntaxToken> modifiers, TypeSyntax type,
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

	public sealed class PropertySignature : ResolvedStatement
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

	public sealed class Method : ResolvedStatement
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

	public sealed class MethodSignature : ResolvedStatement
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

	public sealed class OperatorOverload : ResolvedStatement
	{
		public TypeSyntax ReturnType { get; }
		public BinaryOperator BinaryOperator { get; }
		public ImmutableArray<Variable> ParameterList { get; }
		public Statement Body { get; }

		public OperatorOverload(TypeSyntax returnType, BinaryOperator binaryOperator, IEnumerable<Variable> parameterList,
								Statement body)

		{
			ReturnType = returnType;
			BinaryOperator = binaryOperator;
			ParameterList = parameterList.ToImmutableArray();
			Body = body;
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitOperatorOverloadStatement(this);
		}
	}

	public sealed class Field : ResolvedStatement
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

	public sealed class Break : ResolvedStatement
	{
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitBreakStatement(this);
		}
	}

	public sealed class Continue : ResolvedStatement
	{
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitContinueStatement(this);
		}
	}

	public sealed class Control : ResolvedStatement
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

	public sealed class While : ResolvedStatement
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

	public sealed class DoWhile : ResolvedStatement
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

	public sealed class For : ResolvedStatement
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

	public sealed class ForEach : ResolvedStatement
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

	public sealed class Repeat : ResolvedStatement
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

	public sealed class Return : ResolvedStatement
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

	public sealed class Throw : ResolvedStatement
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

	public sealed class Seal : ResolvedStatement
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

	public sealed class Try : ResolvedStatement
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

	public sealed class VariableDeclaration : ResolvedStatement
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

	public sealed class Lock : ResolvedStatement
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

	public sealed class Switch : ResolvedStatement
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

	public sealed class ExpressionStatement : ResolvedStatement
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