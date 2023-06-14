#Грамматика

    line ::= number statement CR | statement CR
 
    statement ::= PRINT expr-list
                  IF logical-expression (THEN | ELSE) statement
                  GOTO expression
                  INPUT var-list
                  LET var = expression
                  GOSUB expression
                  REM string
                  RETURN
                  CLEAR
                  LIST (number | ε)
                  RUN
                  END
 
    expr-list ::= (string|expression) (, (string|expression) )*
 
    var-list ::= var (, var)*
 
    expression ::= (+|-|ε) term ((+|-) term)*
    
    logical-expression ::= expression relop expression
 
    term ::= factor ((*|/) factor)*
 
    factor ::= var | number | (expression)
 
    var ::= A | B | C ... | Y | Z
 
    number ::= digit digit*
 
    digit ::= 0 | 1 | 2 | 3 | ... | 8 | 9
 
    relop ::= < (>|=|ε) | > (<|=|ε) | =

    string ::= " ( |!|#|$ ... -|.|/|digit|: ... @|A|B|C ... |X|Y|Z)* "