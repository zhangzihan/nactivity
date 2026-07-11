parser grammar FeelParser;

options { tokenVocab = FeelLexer; }

// ===================== Entry Points =====================

// Entry point for expressions (expression decisions, rule outputs)
expressionRoot
    : expression EOF
    ;

// Entry point for decision table input entries (unary tests)
unaryTestsRoot
    : simpleUnaryTests EOF
    ;

// ===================== Simple Unary Tests =====================

simpleUnaryTests
    : MINUS                                                 # positiveUnaryTestDash
    | NOT LPAREN simpleUnaryTests RPAREN                    # negatedUnaryTests
    | simpleUnaryTest (COMMA simpleUnaryTest)*              # positiveUnaryTests
    ;

simpleUnaryTest
    : intervalStartBracket endpoint DOTDOT endpoint intervalEndBracket  # unaryTestBracketedInterval
    | endpoint DOTDOT endpoint                              # unaryTestInterval
    | compOp endpoint                                       # unaryTestOp
    | comparison                                            # unaryTestComparison
    | NULL                                                  # unaryTestNull
    ;

// Brackets for interval notation (FEEL interval syntax)
// [  = inclusive start, (  = exclusive start, ]  = exclusive start (European)
// ]  = inclusive end,   )  = exclusive end,   [  = exclusive end (European)
intervalStartBracket
    : LBRACKET      // [ inclusive
    | LPAREN        // ( exclusive
    | RBRACKET      // ] exclusive (European notation)
    ;

intervalEndBracket
    : RBRACKET      // ] inclusive
    | RPAREN        // ) exclusive
    | LBRACKET      // [ exclusive (European notation)
    ;

// Endpoint for interval
endpoint
    : additiveExpression
    ;

// ===================== Expressions =====================

expression
    : textualExpression
    | boxedExpression
    ;

textualExpression
    : functionDefinition
    | forExpression
    | ifExpression
    | quantifiedExpression
    | ternaryExpression
    ;

// ===================== Boxed Expressions =====================

boxedExpression
    : list
    | context
    | functionDefinition
    ;

// ===================== Ternary Operator (C# compat) =====================

ternaryExpression
    : disjunction QUESTION expression COLON expression          # ternaryOp
    | disjunction                                               # ternaryPassthrough
    ;

// ===================== Logical Operators =====================

disjunction
    : conjunction ((OR | PIPEPIPE) conjunction)*
    ;

conjunction
    : comparison ((AND | AMPAMP) comparison)*
    ;

// ===================== Comparison =====================

comparison
    : additiveExpression compOp additiveExpression          # comparisonOp
    | additiveExpression BETWEEN additiveExpression AND additiveExpression  # comparisonBetween
    | additiveExpression IN LPAREN simpleUnaryTests RPAREN  # comparisonInList
    | additiveExpression IN simpleUnaryTest                 # comparisonInUnary
    | additiveExpression INSTANCE OF feelType               # comparisonInstanceOf
    | additiveExpression                                    # comparisonBase
    ;

compOp
    : LT | GT | LTE | GTE | EQ | EQEQ | NEQ
    ;

// ===================== Arithmetic =====================

additiveExpression
    : multiplicativeExpression ((PLUS | MINUS) multiplicativeExpression)*
    ;

multiplicativeExpression
    : exponentiationExpression ((STAR | FSLASH | PERCENT) exponentiationExpression)*
    ;

exponentiationExpression
    : unaryExpression (STARSTAR unaryExpression)*
    ;

unaryExpression
    : MINUS unaryExpression                                 # unaryMinus
    | EXCL unaryExpression                                  # unaryNot
    | postfixExpression                                     # unaryPostfix
    ;

// ===================== Postfix (member access, filter, invocation) =====================

postfixExpression
    : postfixExpression DOT simpleName                      # postfixMemberAccess
    | postfixExpression LBRACKET expression RBRACKET        # postfixFilter
    | postfixExpression LPAREN namedOrPositionalArgs RPAREN # postfixInvocation
    | postfixExpression LPAREN RPAREN                       # postfixEmptyInvocation
    | primary                                               # postfixPrimary
    ;

namedOrPositionalArgs
    : namedArg (COMMA namedArg)*                            # namedArgList
    | expression (COMMA expression)*                        # positionalArgList
    ;

namedArg
    : NAME COLON expression
    ;

// ===================== Primary =====================

primary
    : literal                                               # primaryLiteral
    | AT StringLiteral                                      # primaryAtLiteral
    | AtLiteral                                             # primaryAtLiteralToken
    | simpleName                                            # primaryName
    | LPAREN expression RPAREN                              # primaryParen
    | list                                                  # primaryList
    | context                                               # primaryContext
    ;

literal
    : IntegerLiteral                                        # literalInteger
    | FloatLiteral                                          # literalFloat
    | StringLiteral                                         # literalString
    | TRUE                                                  # literalTrue
    | FALSE                                                 # literalFalse
    | NULL                                                  # literalNull
    ;

// ===================== Names =====================

// Simple name can be a single NAME token or a keyword used as a name
simpleName
    : NAME
    | NOT      // 'not' can be used as a function name
    ;

qualifiedName
    : simpleName (DOT simpleName)*
    ;

// ===================== Control Flow =====================

ifExpression
    : IF expression THEN expression ELSE expression
    ;

forExpression
    : FOR iterationContext (COMMA iterationContext)* RETURN expression
    ;

iterationContext
    : NAME IN expression (DOTDOT expression)?
    ;

quantifiedExpression
    : (SOME | EVERY) iterationContext (COMMA iterationContext)* SATISFIES expression
    ;

functionDefinition
    : FUNCTION LPAREN formalParameterList? RPAREN (EXTERNAL)? expression
    ;

formalParameterList
    : formalParameter (COMMA formalParameter)*
    ;

formalParameter
    : NAME (COLON feelType)?
    ;

// ===================== Collections =====================

list
    : LBRACKET (expression (COMMA expression)*)? RBRACKET
    ;

context
    : LBRACE (contextEntry (COMMA contextEntry)*)? RBRACE
    ;

contextEntry
    : key COLON expression
    ;

key
    : simpleName
    | StringLiteral
    ;

// ===================== Type =====================

feelType
    : qualifiedName                                         # typeNamed
    | NULL                                                  # typeNull
    | NAME LT feelType GT                                   # typeList
    | FUNCTION LT (feelType (COMMA feelType)*)? GT ARROW feelType  # typeFunction
    | LBRACKET feelType RBRACKET                            # typeContext
    ;

