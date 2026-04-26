grammar Expression;

options {
    language = CSharp;
}

@parser::namespace { Spring.Expressions.Parser }
@lexer::namespace { Spring.Expressions.Parser }

// ========== Parser Rules ==========

expr
    :   expression EOF
    ;

exprList
    :   LPAREN expression (SEMI expression)+ RPAREN
    ;

expression
    :   logicalOrExpression ASSIGN logicalOrExpression
    |   logicalOrExpression DEFAULT logicalOrExpression
    |   logicalOrExpression QMARK expression COLON expression
    |   logicalOrExpression
    ;

parenExpr
    :   LPAREN expression RPAREN
    ;

logicalOrExpression
    :   logicalXorExpression (OR logicalXorExpression)*
    ;

logicalXorExpression
    :   logicalAndExpression (XOR logicalAndExpression)*
    ;

logicalAndExpression
    :   relationalExpression (AND relationalExpression)*
    ;

relationalExpression
    :   sumExpr relationalOperator sumExpr
    |   sumExpr
    ;

sumExpr
    :   prodExpr ((PLUS | MINUS) prodExpr)*
    ;

prodExpr
    :   powExpr ((STAR | DIV | MOD) powExpr)*
    ;

powExpr
    :   unaryExpression POWER unaryExpression
    |   unaryExpression
    ;

unaryExpression
    :   PLUS unaryExpression
    |   MINUS unaryExpression
    |   BANG unaryExpression
    |   primaryExpression
    ;

primaryExpression
    :   startNode node*
    ;

startNode
    :   exprList
    |   parenExpr
    |   methodOrProperty
    |   functionOrVar
    |   localFunctionOrVar
    |   reference
    |   indexer
    |   literal
    |   type
    |   constructor
    |   projection
    |   selection
    |   firstSelection
    |   lastSelection
    |   listInitializer
    |   mapInitializer
    |   lambda
    |   attribute
    ;

node
    :   methodOrProperty
    |   indexer
    |   projection
    |   selection
    |   firstSelection
    |   lastSelection
    |   exprList
    |   DOT
    ;

functionOrVar
    :   function
    |   var
    ;

function
    :   POUND ID methodArgs
    ;

var
    :   POUND ID
    ;

localFunctionOrVar
    :   localFunction
    |   localVar
    ;

localFunction
    :   DOLLAR ID methodArgs
    ;

localVar
    :   DOLLAR ID
    ;

methodOrProperty
    :   ID methodArgs
    |   property
    ;

methodArgs
    :   LPAREN (argument (COMMA argument)*)? RPAREN
    ;

property
    :   ID
    ;

reference
    :   AT LPAREN quotableName COLON quotableName RPAREN
    |   AT LPAREN quotableName RPAREN
    ;

indexer
    :   LBRACKET argument (COMMA argument)* RBRACKET
    ;

projection
    :   PROJECT expression RCURLY
    ;

selection
    :   SELECT expression (COMMA expression)* RCURLY
    ;

firstSelection
    :   SELECT_FIRST expression RCURLY
    ;

lastSelection
    :   SELECT_LAST expression RCURLY
    ;

type
    :   TYPE qualifiedId RPAREN
    ;

quotableName
    :   STRING_LITERAL
    |   qualifiedId
    ;

attribute
    :   AT LBRACKET qualifiedId ctorArgs? RBRACKET
    ;

lambda
    :   LAMBDA argList? PIPE expression RCURLY
    ;

argList
    :   ID (COMMA ID)*
    ;

constructor
    :   NEW qualifiedId ctorArgs
    |   NEW qualifiedId arrayRank listInitializer?
    ;

arrayRank
    :   LBRACKET (expression (COMMA expression)*)? RBRACKET
    ;

listInitializer
    :   LCURLY expression (COMMA expression)* RCURLY
    ;

mapInitializer
    :   POUND LCURLY mapEntry (COMMA mapEntry)* RCURLY
    ;

mapEntry
    :   expression COLON expression
    ;

ctorArgs
    :   LPAREN (namedArgument (COMMA namedArgument)*)? RPAREN
    ;

argument
    :   expression
    ;

namedArgument
    :   ID ASSIGN expression
    |   argument
    ;

qualifiedId
    :   ID (DOT ID)*
    ;

literal
    :   NULL_LITERAL
    |   INTEGER_LITERAL
    |   HEXADECIMAL_INTEGER_LITERAL
    |   REAL_LITERAL
    |   STRING_LITERAL
    |   boolLiteral
    ;

boolLiteral
    :   TRUE
    |   FALSE
    ;

relationalOperator
    :   EQUAL
    |   NOT_EQUAL
    |   LESS_THAN
    |   LESS_THAN_OR_EQUAL
    |   GREATER_THAN
    |   GREATER_THAN_OR_EQUAL
    |   IN
    |   IS
    |   BETWEEN
    |   LIKE
    |   MATCHES
    ;

// ========== Lexer Rules ==========

WS
    :   [ \t\n\r]+ -> skip
    ;

// Multi-char operators must come before single-char
TYPE
    :   'T('
    ;

LAMBDA
    :   '{|'
    ;

PROJECT
    :   '!{'
    ;

SELECT
    :   '?{'
    ;

SELECT_FIRST
    :   '^{'
    ;

SELECT_LAST
    :   '${'
    ;

DEFAULT
    :   '??'
    ;

EQUAL
    :   '=='
    ;

NOT_EQUAL
    :   '!='
    ;

LESS_THAN_OR_EQUAL
    :   '<='
    ;

GREATER_THAN_OR_EQUAL
    :   '>='
    ;

// Keywords (must come before ID)
NEW          : 'new' ;
NULL_LITERAL : 'null' ;
TRUE         : 'true' ;
FALSE        : 'false' ;
AND          : 'and' ;
OR           : 'or' ;
XOR          : 'xor' ;
IN           : 'in' ;
IS           : 'is' ;
BETWEEN      : 'between' ;
LIKE         : 'like' ;
MATCHES      : 'matches' ;

// Numeric literals (REAL before HEX/INT, before DOT)
REAL_LITERAL
    :   '.' [0-9]+ EXPONENT_PART? REAL_TYPE_SUFFIX?
    |   [0-9]+ '.' [0-9]+ EXPONENT_PART? REAL_TYPE_SUFFIX?
    |   [0-9]+ EXPONENT_PART REAL_TYPE_SUFFIX?
    |   [0-9]+ REAL_TYPE_SUFFIX
    ;

HEXADECIMAL_INTEGER_LITERAL
    :   '0x' [0-9a-fA-F]+ INTEGER_TYPE_SUFFIX?
    ;

INTEGER_LITERAL
    :   [0-9]+ INTEGER_TYPE_SUFFIX?
    ;

// String literal
STRING_LITERAL
    :   '\'' (APOS | ~'\'')* '\''
    ;

fragment APOS
    :   '\'' '\''
    ;

// Identifiers (after keywords)
// CJK ranges: \u3400-\u4DBF (Ext A), \u4E00-\u9FFF (Unified), \uF900-\uFAFF (Compat)
ID
    :   [a-zA-Z_\u3400-\u4DBF\u4E00-\u9FFF\uF900-\uFAFF] [a-zA-Z_0-9\u3400-\u4DBF\u4E00-\u9FFF\uF900-\uFAFF]*
    ;

// Single-char operators
DOT
    :   '.'
    ;

AT       : '@' ;
BACKTICK : '`' ;
BACKSLASH: '\\' ;
PIPE     : '|' ;
BANG     : '!' ;
QMARK    : '?' ;
DOLLAR   : '$' ;
POUND    : '#' ;
LPAREN   : '(' ;
RPAREN   : ')' ;
LBRACKET : '[' ;
RBRACKET : ']' ;
LCURLY   : '{' ;
RCURLY   : '}' ;
COMMA    : ',' ;
SEMI     : ';' ;
COLON    : ':' ;
ASSIGN   : '=' ;
PLUS     : '+' ;
MINUS    : '-' ;
DIV      : '/' ;
STAR     : '*' ;
MOD      : '%' ;
POWER    : '^' ;
QUOTE    : '\'' ;
DOT_ESCAPED : '\\.' ;

LESS_THAN    : '<' ;
GREATER_THAN : '>' ;

fragment DECIMAL_DIGIT
    :   [0-9]
    ;

fragment INTEGER_TYPE_SUFFIX
    :   [Uu][Ll] | [Ll][Uu] | [Uu] | [Ll]
    ;

fragment EXPONENT_PART
    :   [eE] SIGN? [0-9]+
    ;

fragment SIGN
    :   [+-]
    ;

fragment REAL_TYPE_SUFFIX
    :   [FfDdMm]
    ;
