namespace FulcrumLabs.Conductor.Jinja.Lexing;

/// <summary>
///     Represents the type of token in a Jinja2 template.
/// </summary>
public enum TokenType
{
    // Special tokens
    /// <summary>End of file/template.</summary>
    Eof,

    // Delimiters
    /// <summary>Variable output start: {{</summary>
    VariableStart,

    /// <summary>Variable output end: }}</summary>
    VariableEnd,

    /// <summary>Block/statement start: {%</summary>
    BlockStart,

    /// <summary>Block/statement end: %}</summary>
    BlockEnd,

    /// <summary>Comment start: {#</summary>
    CommentStart,

    /// <summary>Comment end: #}</summary>
    CommentEnd,

    // Literals
    /// <summary>Plain text outside Jinja2 syntax.</summary>
    Text,

    /// <summary>Identifier/variable name.</summary>
    Identifier,

    /// <summary>Numeric literal.</summary>
    Number,

    /// <summary>String literal.</summary>
    String,

    // Operators and punctuation
    /// <summary>Dot for member access: .</summary>
    Dot,

    /// <summary>Pipe for filters: |</summary>
    Pipe,

    /// <summary>Comma: ,</summary>
    Comma,

    /// <summary>Colon: :</summary>
    Colon,

    /// <summary>Left parenthesis: (</summary>
    LeftParen,

    /// <summary>Right parenthesis: )</summary>
    RightParen,

    /// <summary>Left bracket: [</summary>
    LeftBracket,

    /// <summary>Right bracket: ]</summary>
    RightBracket,

    /// <summary>Left brace: {</summary>
    LeftBrace,

    /// <summary>Right brace: }</summary>
    RightBrace,

    /// <summary>Assignment: =</summary>
    Assign,

    // Arithmetic operators
    /// <summary>Plus: +</summary>
    Plus,

    /// <summary>Minus: -</summary>
    Minus,

    /// <summary>Multiply: *</summary>
    Multiply,

    /// <summary>Divide: /</summary>
    Divide,

    /// <summary>Floor divide: //</summary>
    FloorDivide,

    /// <summary>Modulo: %</summary>
    Modulo,

    /// <summary>Power: **</summary>
    Power,

    // Comparison operators
    /// <summary>Equal: ==</summary>
    Equal,

    /// <summary>Not equal: !=</summary>
    NotEqual,

    /// <summary>Less than: &lt;</summary>
    LessThan,

    /// <summary>Less than or equal: &lt;=</summary>
    LessThanOrEqual,

    /// <summary>Greater than: &gt;</summary>
    GreaterThan,

    /// <summary>Greater than or equal: &gt;=</summary>
    GreaterThanOrEqual,

    // Keywords
    /// <summary>Keyword: for</summary>
    For,

    /// <summary>Keyword: endfor</summary>
    Endfor,

    /// <summary>Keyword: if</summary>
    If,

    /// <summary>Keyword: elif</summary>
    Elif,

    /// <summary>Keyword: else</summary>
    Else,

    /// <summary>Keyword: endif</summary>
    Endif,

    /// <summary>Keyword: set</summary>
    Set,

    /// <summary>Keyword: endset</summary>
    Endset,

    /// <summary>Keyword: in</summary>
    In,

    /// <summary>Keyword: is</summary>
    Is,

    /// <summary>Keyword: and</summary>
    And,

    /// <summary>Keyword: or</summary>
    Or,

    /// <summary>Keyword: not</summary>
    Not,

    /// <summary>Keyword: true</summary>
    True,

    /// <summary>Keyword: false</summary>
    False,

    /// <summary>Keyword: none/null</summary>
    None
}