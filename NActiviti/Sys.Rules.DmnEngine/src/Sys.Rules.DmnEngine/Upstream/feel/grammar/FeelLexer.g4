lexer grammar FeelLexer;

// ===================== Keywords =====================

TRUE        : 'true' ;
FALSE       : 'false' ;
NULL        : 'null' ;
NOT         : 'not' ;
AND         : 'and' ;
OR          : 'or' ;
IF          : 'if' ;
THEN        : 'then' ;
ELSE        : 'else' ;
FOR         : 'for' ;
IN          : 'in' ;
RETURN      : 'return' ;
SOME        : 'some' ;
EVERY       : 'every' ;
SATISFIES   : 'satisfies' ;
BETWEEN     : 'between' ;
INSTANCE    : 'instance' ;
OF          : 'of' ;
FUNCTION    : 'function' ;
EXTERNAL    : 'external' ;

// ===================== Operators =====================

EQEQ        : '==' ;
EQ          : '=' ;
NEQ         : '!=' ;
LT          : '<' ;
GT          : '>' ;
LTE         : '<=' ;
GTE         : '>=' ;
PLUS        : '+' ;
MINUS       : '-' ;
STAR        : '*' ;
FSLASH      : '/' ;
PERCENT     : '%' ;
STARSTAR    : '**' ;
DOTDOT      : '..' ;
DOT         : '.' ;
AMPAMP      : '&&' ;
PIPEPIPE    : '||' ;
QUESTION    : '?' ;
EXCL        : '!' ;

// ===================== Delimiters =====================

LPAREN      : '(' ;
RPAREN      : ')' ;
LBRACKET    : '[' ;
RBRACKET    : ']' ;
LBRACE      : '{' ;
RBRACE      : '}' ;
COMMA       : ',' ;
COLON       : ':' ;
ARROW       : '->' ;
AT          : '@' ;

// ===================== Literals =====================

IntegerLiteral
    : Digits
    ;

FloatLiteral
    : Digits '.' Digits
    ;

StringLiteral
    : '"' (~["\\\r\n] | EscapeSequence)* '"'
    ;

fragment EscapeSequence
    : '\\' ["\\/bfnrtu]
    ;

fragment Digits
    : [0-9]+
    ;

// @"..." date/time literal syntax (DMN 1.4+)
AtLiteral
    : '@' '"' (~["\\\r\n] | EscapeSequence)* '"'
    ;

// ===================== Identifiers =====================

// A single name segment (no spaces); multi-word names are resolved post-lexing
NAME
    : NameStartChar NameChar*
    ;

fragment NameStartChar
    : [a-zA-Z_]
    | '\u00C0'..'\u00D6'
    | '\u00D8'..'\u00F6'
    | '\u00F8'..'\u02FF'
    | '\u0370'..'\u037D'
    | '\u037F'..'\u1FFF'
    | '\u200C'..'\u200D'
    | '\u2070'..'\u218F'
    | '\u2C00'..'\u2FEF'
    | '\u3001'..'\uD7FF'
    | '\uF900'..'\uFDCF'
    | '\uFDF0'..'\uFFFD'
    ;

fragment NameChar
    : NameStartChar
    | [0-9]
    | '\u00B7'
    | '\u0300'..'\u036F'
    | '\u203F'..'\u2040'
    | '\''
    ;

// ===================== Whitespace and Comments =====================

WS
    : [ \t\r\n]+ -> channel(HIDDEN)
    ;

BlockComment
    : '/*' .*? '*/' -> skip
    ;

LineComment
    : '//' ~[\r\n]* -> skip
    ;

