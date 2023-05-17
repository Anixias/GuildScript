using System.Collections.Immutable;

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
		void VisitCastOverloadStatement(CastOverload statement);
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
	
	public interface IVisitor<out T>
	{
		T VisitProgramStatement(Program statement);
		T VisitEntryPointStatement(EntryPoint statement);
		T VisitDefineStatement(Define statement);
		T VisitBlockStatement(Block statement);
		T VisitClassStatement(Class statement);
		T VisitStructStatement(Struct statement);
		T VisitInterfaceStatement(Interface statement);
		T VisitEnumStatement(Enum statement);
		T VisitCastOverloadStatement(CastOverload statement);
		T VisitDestructorStatement(Destructor statement);
		T VisitExternalMethodStatement(ExternalMethod statement);
		T VisitConstructorStatement(Constructor statement);
		T VisitIndexerStatement(Indexer statement);
		T VisitAccessorTokenStatement(AccessorToken statement);
		T VisitAccessorLambdaStatement(AccessorLambda statement);
		T VisitAccessorLambdaSignatureStatement(AccessorLambdaSignature statement);
		T VisitEventStatement(Event statement);
		T VisitEventSignatureStatement(EventSignature statement);
		T VisitPropertyStatement(Property statement);
		T VisitPropertySignatureStatement(PropertySignature statement);
		T VisitMethodStatement(Method statement);
		T VisitMethodSignatureStatement(MethodSignature statement);
		T VisitFieldStatement(Field statement);
		T VisitBreakStatement(Break statement);
		T VisitContinueStatement(Continue statement);
		T VisitControlStatement(Control statement);
		T VisitWhileStatement(While statement);
		T VisitDoWhileStatement(DoWhile statement);
		T VisitForStatement(For statement);
		T VisitForEachStatement(ForEach statement);
		T VisitRepeatStatement(Repeat statement);
		T VisitReturnStatement(Return statement);
		T VisitThrowStatement(Throw statement);
		T VisitSealStatement(Seal statement);
		T VisitTryStatement(Try statement);
		T VisitVariableDeclarationStatement(VariableDeclaration statement);
		T VisitLockStatement(Lock statement);
		T VisitSwitchStatement(Switch statement);
		T VisitExpressionStatement(ExpressionStatement statement);
		T VisitOperatorOverloadStatement(OperatorOverload statement);
		T VisitOperatorOverloadSignatureStatement(OperatorOverloadSignature statement);
	}
	
	public abstract void AcceptVisitor(IVisitor visitor);
	public abstract T AcceptVisitor<T>(IVisitor<T> visitor);

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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitProgramStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitDefineStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitEntryPointStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitBlockStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitClassStatement(this);
		}
	}

	public sealed class Struct : Statement
	{
		public SyntaxToken? AccessModifier { get; }
		public SyntaxToken? StructModifier { get; }
		public SyntaxToken NameToken { get; }
		public ImmutableArray<Statement> Members { get; }
		public ImmutableArray<SyntaxToken> TypeParameters { get; }

		public Struct(SyntaxToken? accessModifier, SyntaxToken? structModifier, SyntaxToken nameToken,
					  IEnumerable<SyntaxToken> typeParameters, IEnumerable<Statement> members)
		{
			AccessModifier = accessModifier;
			StructModifier = structModifier;
			NameToken = nameToken;
			Members = members.ToImmutableArray();
			TypeParameters = typeParameters.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitStructStatement(this);
		}

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitStructStatement(this);
		}
	}

	public sealed class Interface : Statement
	{
		public SyntaxToken? AccessModifier { get; }
		public SyntaxToken NameToken { get; }
		public ImmutableArray<Statement> Members { get; }
		public ImmutableArray<SyntaxToken> TypeParameters { get; }

		public Interface(SyntaxToken? accessModifier, SyntaxToken nameToken, IEnumerable<SyntaxToken> typeParameters,
						 IEnumerable<Statement> members)
		{
			AccessModifier = accessModifier;
			NameToken = nameToken;
			Members = members.ToImmutableArray();
			TypeParameters = typeParameters.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitInterfaceStatement(this);
		}

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitInterfaceStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitEnumStatement(this);
		}
	}

	public sealed class CastOverload : Statement
	{
		public SyntaxToken CastTypeToken { get; }
		public TypeSyntax TargetType { get; }
		public Statement Body { get; }
		
		public CastOverload(SyntaxToken castTypeToken, TypeSyntax targetType, Statement body)
		{
			CastTypeToken = castTypeToken;
			TargetType = targetType;
			Body = body;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitCastOverloadStatement(this);
		}

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitCastOverloadStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitDestructorStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitExternalMethodStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitConstructorStatement(this);
		}
	}

	public sealed class Indexer : Statement
	{
		public SyntaxToken ThisToken { get; }
		public SyntaxToken? AccessModifier { get; }
		public TypeSyntax Type { get; }
		public ImmutableArray<Variable> ParameterList { get; }
		public ImmutableArray<Statement> Body { get; }

		public Indexer(SyntaxToken thisToken, SyntaxToken? accessModifier, TypeSyntax type,
					   IEnumerable<Variable> parameterList, IEnumerable<Statement> body)
		{
			ThisToken = thisToken;
			AccessModifier = accessModifier;
			Type = type;
			ParameterList = parameterList.ToImmutableArray();
			Body = body.ToImmutableArray();
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitIndexerStatement(this);
		}

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitIndexerStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitEventStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitEventSignatureStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitAccessorTokenStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitAccessorLambdaStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitAccessorLambdaSignatureStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitPropertyStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitPropertySignatureStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitMethodStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitMethodSignatureStatement(this);
		}
	}

	public sealed class OperatorOverload : Statement
	{
		public bool Immutable { get; }
		public TypeSyntax ReturnType { get; }
		public Operator Operator { get; }
		public ImmutableArray<Variable> ParameterList { get; }
		public Statement Body { get; }

		public OperatorOverload(TypeSyntax returnType, Operator @operator,
								IEnumerable<Variable> parameterList, Statement body, bool immutable)

		{
			ReturnType = returnType;
			Operator = @operator;
			ParameterList = parameterList.ToImmutableArray();
			Body = body;
			Immutable = immutable;
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitOperatorOverloadStatement(this);
		}

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitOperatorOverloadStatement(this);
		}
	}

	public sealed class OperatorOverloadSignature : Statement
	{
		public bool Immutable { get; }
		public TypeSyntax ReturnType { get; }
		public Operator Operator { get; }
		public ImmutableArray<Variable> ParameterList { get; }

		public OperatorOverloadSignature(TypeSyntax returnType, Operator @operator, 
										 IEnumerable<Variable> parameterList, bool immutable)

		{
			ReturnType = returnType;
			Operator = @operator;
			Immutable = immutable;
			ParameterList = parameterList.ToImmutableArray();
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitOperatorOverloadSignatureStatement(this);
		}

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitOperatorOverloadSignatureStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitFieldStatement(this);
		}
	}

	public sealed class Break : Statement
	{
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitBreakStatement(this);
		}

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitBreakStatement(this);
		}
	}

	public sealed class Continue : Statement
	{
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitContinueStatement(this);
		}

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitContinueStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitControlStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitWhileStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitDoWhileStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitForStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitForEachStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitRepeatStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitReturnStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitThrowStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitSealStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitTryStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitVariableDeclarationStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitLockStatement(this);
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
		public SyntaxToken SwitchToken { get; }

		public Switch(Expression expression, IEnumerable<Section> sections, SyntaxToken switchToken)
		{
			Expression = expression;
			SwitchToken = switchToken;
			Sections = sections.ToImmutableArray();
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitSwitchStatement(this);
		}

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitSwitchStatement(this);
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

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitExpressionStatement(this);
		}
	}
}