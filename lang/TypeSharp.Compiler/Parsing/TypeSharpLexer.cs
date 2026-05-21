using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.Compiler.Parsing;

public sealed class TypeSharpLexer
{
    private static readonly Dictionary<string, SyntaxKind> Keywords = new(StringComparer.Ordinal)
    {
        ["namespace"] = SyntaxKind.NamespaceKeyword,
        ["module"] = SyntaxKind.ModuleKeyword,
        ["import"] = SyntaxKind.ImportKeyword,
        ["open"] = SyntaxKind.OpenKeyword,
        ["static"] = SyntaxKind.StaticKeyword,
        ["export"] = SyntaxKind.ExportKeyword,
        ["fun"] = SyntaxKind.FunKeyword,
        ["let"] = SyntaxKind.LetKeyword,
        ["literal"] = SyntaxKind.LiteralKeyword,
        ["type"] = SyntaxKind.TypeKeyword,
        ["record"] = SyntaxKind.RecordKeyword,
        ["union"] = SyntaxKind.UnionKeyword,
        ["enum"] = SyntaxKind.EnumKeyword,
        ["match"] = SyntaxKind.MatchKeyword,
        ["as"] = SyntaxKind.AsKeyword,
        ["from"] = SyntaxKind.FromKeyword,
        ["with"] = SyntaxKind.WithKeyword,
        ["where"] = SyntaxKind.WhereKeyword,
        ["when"] = SyntaxKind.WhenKeyword,
        ["async"] = SyntaxKind.AsyncKeyword,
        ["await"] = SyntaxKind.AwaitKeyword,
        ["try"] = SyntaxKind.TryKeyword,
        ["catch"] = SyntaxKind.CatchKeyword,
        ["using"] = SyntaxKind.UsingKeyword,
        ["public"] = SyntaxKind.PublicKeyword,
        ["private"] = SyntaxKind.PrivateKeyword,
        ["partial"] = SyntaxKind.PartialKeyword,
        ["ambient"] = SyntaxKind.AmbientKeyword,
        ["elif"] = SyntaxKind.ElifKeyword,
        ["class"] = SyntaxKind.ClassKeyword,
        ["interface"] = SyntaxKind.InterfaceKeyword,
        ["delegate"] = SyntaxKind.DelegateKeyword,
        ["event"] = SyntaxKind.EventKeyword,
        ["extension"] = SyntaxKind.ExtensionKeyword,
        ["mut"] = SyntaxKind.MutKeyword,
        ["get"] = SyntaxKind.GetKeyword,
        ["set"] = SyntaxKind.SetKeyword,
        ["for"] = SyntaxKind.ForKeyword,
        ["in"] = SyntaxKind.InKeyword,
        ["out"] = SyntaxKind.OutKeyword,
        ["ref"] = SyntaxKind.RefKeyword,
        ["if"] = SyntaxKind.IfKeyword,
        ["else"] = SyntaxKind.ElseKeyword,
        ["satisfies"] = SyntaxKind.SatisfiesKeyword,
        ["yield"] = SyntaxKind.YieldKeyword,
        ["lock"] = SyntaxKind.LockKeyword,
        ["nameof"] = SyntaxKind.NameofKeyword,
        ["checked"] = SyntaxKind.CheckedKeyword,
        ["unchecked"] = SyntaxKind.UncheckedKeyword,
        ["keyof"] = SyntaxKind.KeyofKeyword,
        ["null"] = SyntaxKind.NullKeyword,
        ["true"] = SyntaxKind.TrueKeyword,
        ["false"] = SyntaxKind.FalseKeyword
    };

    private readonly string _text;
    private readonly string _file;
    private readonly List<Diagnostic> _diagnostics = [];
    private int _index;
    private int _line = 1;
    private int _column = 1;

    public TypeSharpLexer(string text, string file)
    {
        _text = text;
        _file = file;
    }

    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

    public IReadOnlyList<SyntaxToken> Lex()
    {
        var tokens = new List<SyntaxToken>();

        while (!IsAtEnd)
        {
            var leadingTrivia = ReadLeadingTriviaSummary();
            if (IsAtEnd)
            {
                break;
            }

            tokens.Add(ReadToken(leadingTrivia));
        }

        var eof = CurrentPosition;
        tokens.Add(new SyntaxToken(SyntaxKind.EndOfFileToken, string.Empty, new SourceSpan(eof, eof)));
        return tokens;
    }

    private SyntaxToken ReadToken(string leadingTriviaSummary)
    {
        var start = CurrentPosition;

        if (IsIdentifierStart(Current))
        {
            return ReadIdentifierOrKeyword(leadingTriviaSummary);
        }

        if (char.IsDigit(Current))
        {
            return ReadNumber(leadingTriviaSummary);
        }

        if (Current == '$' && Peek(1) == '"')
        {
            return ReadString(interpolated: true, leadingTriviaSummary);
        }

        if (Current == '"')
        {
            return ReadString(interpolated: false, leadingTriviaSummary);
        }

        foreach (var (text, kind) in MultiCharacterTokens)
        {
            if (StartsWith(text))
            {
                Advance(text.Length);
                return new SyntaxToken(kind, text, new SourceSpan(start, CurrentPosition), LeadingTriviaSummary: leadingTriviaSummary);
            }
        }

        var character = Current;
        Advance();

        var singleKind = character switch
        {
            '(' => SyntaxKind.OpenParenToken,
            ')' => SyntaxKind.CloseParenToken,
            '{' => SyntaxKind.OpenBraceToken,
            '}' => SyntaxKind.CloseBraceToken,
            '[' => SyntaxKind.OpenBracketToken,
            ']' => SyntaxKind.CloseBracketToken,
            '.' => SyntaxKind.DotToken,
            ',' => SyntaxKind.CommaToken,
            ':' => SyntaxKind.ColonToken,
            '?' => SyntaxKind.QuestionToken,
            '=' => SyntaxKind.EqualsToken,
            '>' => SyntaxKind.GreaterToken,
            '<' => SyntaxKind.LessToken,
            '+' => SyntaxKind.PlusToken,
            '-' => SyntaxKind.MinusToken,
            '*' => SyntaxKind.StarToken,
            '/' => SyntaxKind.SlashToken,
            '%' => SyntaxKind.PercentToken,
            '!' => SyntaxKind.BangToken,
            '|' => SyntaxKind.PipeToken,
            '&' => SyntaxKind.AmpersandToken,
            _ => SyntaxKind.UnknownToken
        };

        if (singleKind == SyntaxKind.UnknownToken)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.UnexpectedCharacter.Code,
                DiagnosticDescriptors.UnexpectedCharacter.DefaultSeverity,
                $"Unexpected character '{character}'.",
                _file,
                new SourceSpan(start, CurrentPosition)));
        }

        return new SyntaxToken(singleKind, character.ToString(), new SourceSpan(start, CurrentPosition), LeadingTriviaSummary: leadingTriviaSummary);
    }

    private SyntaxToken ReadIdentifierOrKeyword(string leadingTriviaSummary)
    {
        var start = CurrentPosition;
        var startIndex = _index;

        Advance();
        while (!IsAtEnd && IsIdentifierPart(Current))
        {
            Advance();
        }

        var text = _text[startIndex.._index];
        var kind = Keywords.TryGetValue(text, out var keywordKind)
            ? keywordKind
            : SyntaxKind.IdentifierToken;

        return new SyntaxToken(kind, text, new SourceSpan(start, CurrentPosition), LeadingTriviaSummary: leadingTriviaSummary);
    }

    private SyntaxToken ReadNumber(string leadingTriviaSummary)
    {
        var start = CurrentPosition;
        var startIndex = _index;

        while (!IsAtEnd && char.IsDigit(Current))
        {
            Advance();
        }

        if (!IsAtEnd && Current == '.' && char.IsDigit(Peek(1)))
        {
            Advance();
            while (!IsAtEnd && char.IsDigit(Current))
            {
                Advance();
            }
        }

        if (!IsAtEnd && Current is 'm' or 'M')
        {
            Advance();
        }

        return new SyntaxToken(SyntaxKind.NumericLiteralToken, _text[startIndex.._index], new SourceSpan(start, CurrentPosition), LeadingTriviaSummary: leadingTriviaSummary);
    }

    private SyntaxToken ReadString(bool interpolated, string leadingTriviaSummary)
    {
        var start = CurrentPosition;
        var startIndex = _index;

        if (interpolated)
        {
            Advance();
        }

        Advance();
        var escaped = false;
        while (!IsAtEnd)
        {
            if (escaped)
            {
                escaped = false;
                Advance();
                continue;
            }

            if (Current == '\\')
            {
                escaped = true;
                Advance();
                continue;
            }

            if (Current == '"')
            {
                Advance();
                var kind = interpolated ? SyntaxKind.InterpolatedStringLiteralToken : SyntaxKind.StringLiteralToken;
                return new SyntaxToken(kind, _text[startIndex.._index], new SourceSpan(start, CurrentPosition), LeadingTriviaSummary: leadingTriviaSummary);
            }

            Advance();
        }

        _diagnostics.Add(new Diagnostic(
            DiagnosticDescriptors.UnterminatedStringLiteral.Code,
            DiagnosticDescriptors.UnterminatedStringLiteral.DefaultSeverity,
            DiagnosticDescriptors.UnterminatedStringLiteral.MessageTemplate,
            _file,
            new SourceSpan(start, CurrentPosition)));

        var fallbackKind = interpolated ? SyntaxKind.InterpolatedStringLiteralToken : SyntaxKind.StringLiteralToken;
        return new SyntaxToken(fallbackKind, _text[startIndex.._index], new SourceSpan(start, CurrentPosition), LeadingTriviaSummary: leadingTriviaSummary);
    }

    private string ReadLeadingTriviaSummary()
    {
        var spaces = 0;
        var newlines = 0;
        var lineComments = 0;
        var blockComments = 0;

        while (!IsAtEnd)
        {
            if (Current is ' ' or '\t' or '\v' or '\f')
            {
                spaces++;
                Advance();
                continue;
            }

            if (Current is '\r' or '\n')
            {
                newlines++;
                Advance();
                continue;
            }

            if (Current == '/' && Peek(1) == '/')
            {
                lineComments++;
                while (!IsAtEnd && Current is not '\r' and not '\n')
                {
                    Advance();
                }

                continue;
            }

            if (Current == '/' && Peek(1) == '*')
            {
                blockComments++;
                Advance(2);
                while (!IsAtEnd && !(Current == '*' && Peek(1) == '/'))
                {
                    Advance();
                }

                if (!IsAtEnd)
                {
                    Advance(2);
                }

                continue;
            }

            break;
        }

        if (spaces == 0 && newlines == 0 && lineComments == 0 && blockComments == 0)
        {
            return string.Empty;
        }

        return $"spaces={spaces};newlines={newlines};lineComments={lineComments};blockComments={blockComments}";
    }

    private bool StartsWith(string value)
    {
        if (_index + value.Length > _text.Length)
        {
            return false;
        }

        return string.CompareOrdinal(_text, _index, value, 0, value.Length) == 0;
    }

    private void Advance(int count)
    {
        for (var index = 0; index < count; index++)
        {
            Advance();
        }
    }

    private void Advance()
    {
        if (IsAtEnd)
        {
            return;
        }

        if (Current == '\r')
        {
            _index++;
            if (!IsAtEnd && Current == '\n')
            {
                _index++;
            }

            _line++;
            _column = 1;
            return;
        }

        if (Current == '\n')
        {
            _index++;
            _line++;
            _column = 1;
            return;
        }

        _index++;
        _column++;
    }

    private SourcePosition CurrentPosition => new(_line, _column);
    private bool IsAtEnd => _index >= _text.Length;
    private char Current => IsAtEnd ? '\0' : _text[_index];

    private char Peek(int offset)
    {
        var position = _index + offset;
        return position >= _text.Length ? '\0' : _text[position];
    }

    private static bool IsIdentifierStart(char character) =>
        character == '_' || char.IsLetter(character);

    private static bool IsIdentifierPart(char character) =>
        character == '_' || char.IsLetterOrDigit(character);

    private static readonly (string Text, SyntaxKind Kind)[] MultiCharacterTokens =
    [
        ("...", SyntaxKind.DotDotDotToken),
        ("??", SyntaxKind.NullCoalescingToken),
        ("=>", SyntaxKind.EqualsGreaterToken),
        ("->", SyntaxKind.ArrowToken),
        ("|>", SyntaxKind.PipeGreaterToken),
        ("+=", SyntaxKind.PlusEqualsToken),
        ("-=", SyntaxKind.MinusEqualsToken),
        ("==", SyntaxKind.EqualsEqualsToken),
        ("!=", SyntaxKind.BangEqualsToken),
        ("<=", SyntaxKind.LessOrEqualsToken),
        (">=", SyntaxKind.GreaterOrEqualsToken),
        ("&&", SyntaxKind.AmpersandAmpersandToken),
        ("||", SyntaxKind.PipePipeToken)
    ];
}
