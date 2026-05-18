using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.Compiler.Parsing;

public sealed class TypeSharpParser
{
    private readonly string _file;
    private readonly IReadOnlyList<SyntaxToken> _tokens;
    private readonly List<Diagnostic> _diagnostics;
    private int _position;

    private TypeSharpParser(string file, IReadOnlyList<SyntaxToken> tokens, IReadOnlyList<Diagnostic> lexerDiagnostics)
    {
        _file = file;
        _tokens = tokens;
        _diagnostics = [..lexerDiagnostics];
    }

    public static ParseResult ParseText(string text, string file = "input.tysh")
    {
        var lexer = new TypeSharpLexer(text, file);
        var tokens = lexer.Lex();
        var parser = new TypeSharpParser(file, tokens, lexer.Diagnostics);
        var root = parser.ParseSourceFile();
        return new ParseResult(parser._diagnostics)
        {
            Root = root
        };
    }

    private SyntaxNode ParseSourceFile()
    {
        var children = new List<SyntaxNode>();

        while (Current.Kind != SyntaxKind.EndOfFileToken)
        {
            children.Add(Current.Kind switch
            {
                SyntaxKind.NamespaceKeyword => ParseNamespaceDeclaration(),
                SyntaxKind.ImportKeyword => ParseImportDeclaration(),
                SyntaxKind.ExportKeyword => ParseExportedDeclaration(),
                SyntaxKind.OpenBracketToken => ParseDeclarationWithPrefix(),
                SyntaxKind.PublicKeyword => ParseDeclarationWithPrefix(),
                SyntaxKind.PrivateKeyword => ParseDeclarationWithPrefix(),
                _ when IsFunctionDeclarationStart(Current) => ParseFunctionDeclaration(),
                SyntaxKind.TypeKeyword => ParseTypeAliasDeclaration(),
                SyntaxKind.RecordKeyword => ParseRecordDeclaration(),
                SyntaxKind.UnionKeyword => ParseUnionDeclaration(),
                SyntaxKind.ClassKeyword => ParseClassDeclaration(),
                SyntaxKind.DelegateKeyword => ParseDelegateDeclaration(),
                SyntaxKind.LiteralKeyword => ParseLiteralDeclaration(),
                SyntaxKind.LetKeyword => ParseValueDeclaration(),
                _ => ParseSkippedToken()
            });
        }

        return Node(SyntaxKind.SourceFile, children);
    }

    private SyntaxNode ParseNamespaceDeclaration()
    {
        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.NamespaceKeyword))
        };

        children.Add(ParseQualifiedName());
        return Node(SyntaxKind.NamespaceDeclaration, children);
    }

    private SyntaxNode ParseImportDeclaration()
    {
        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.ImportKeyword))
        };

        if (Current.Kind == SyntaxKind.StaticKeyword)
        {
            children.Add(TokenNode(NextToken()));
            children.Add(ParseQualifiedName());
            return Node(SyntaxKind.ImportStaticDeclaration, children);
        }

        if (Current.Kind == SyntaxKind.TypeKeyword)
        {
            children.Add(TokenNode(NextToken()));
            ParseNamedImportClause(children);
            return Node(SyntaxKind.ImportTypeDeclaration, children);
        }

        if (Current.Kind == SyntaxKind.OpenBraceToken)
        {
            ParseNamedImportClause(children);
            return Node(SyntaxKind.ImportNamedDeclaration, children);
        }

        children.Add(ParseSkippedToken());
        return Node(SyntaxKind.ImportNamedDeclaration, children);
    }

    private SyntaxNode ParseDeclarationWithPrefix()
    {
        var children = ParseDeclarationPrefix();

        return Current.Kind switch
        {
            _ when IsFunctionDeclarationStart(Current) => ParseFunctionDeclaration(children),
            SyntaxKind.TypeKeyword => ParseTypeAliasDeclaration(children),
            SyntaxKind.RecordKeyword => ParseRecordDeclaration(children),
            SyntaxKind.UnionKeyword => ParseUnionDeclaration(children),
            SyntaxKind.ClassKeyword => ParseClassDeclaration(children),
            SyntaxKind.DelegateKeyword => ParseDelegateDeclaration(children),
            SyntaxKind.EventKeyword => ParseEventDeclaration(children),
            SyntaxKind.LiteralKeyword => ParseLiteralDeclaration(children),
            SyntaxKind.LetKeyword => ParseValueDeclaration(children),
            _ => Node(SyntaxKind.SkippedToken, [..children, ParseSkippedToken()])
        };
    }

    private List<SyntaxNode> ParseDeclarationPrefix()
    {
        var children = new List<SyntaxNode>();

        while (Current.Kind == SyntaxKind.OpenBracketToken)
        {
            children.Add(ParseAttributeList());
        }

        while (Current.Kind is SyntaxKind.PublicKeyword or SyntaxKind.PrivateKeyword)
        {
            children.Add(ParseAccessModifier());
        }

        return children;
    }

    private SyntaxNode ParseAttributeList()
    {
        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.OpenBracketToken))
        };

        while (Current.Kind != SyntaxKind.CloseBracketToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            var attributeChildren = new List<SyntaxNode>
            {
                ParseQualifiedName()
            };

            if (Current.Kind == SyntaxKind.OpenParenToken)
            {
                attributeChildren.Add(ParseCallExpression(attributeChildren[0]));
            }

            children.Add(Node(SyntaxKind.Attribute, attributeChildren));

            if (Current.Kind == SyntaxKind.CommaToken)
            {
                children.Add(TokenNode(NextToken()));
            }
            else
            {
                break;
            }
        }

        children.Add(TokenNode(Expect(SyntaxKind.CloseBracketToken)));
        return Node(SyntaxKind.AttributeList, children);
    }

    private SyntaxNode ParseAccessModifier()
    {
        var token = TokenNode(NextToken());
        return token.Kind switch
        {
            SyntaxKind.PublicKeyword => new SyntaxNode(SyntaxKind.PublicModifier, token.Span, children: [token]),
            SyntaxKind.PrivateKeyword => new SyntaxNode(SyntaxKind.PrivateModifier, token.Span, children: [token]),
            _ => token
        };
    }

    private void ParseNamedImportClause(List<SyntaxNode> children)
    {
        children.Add(TokenNode(Expect(SyntaxKind.OpenBraceToken)));
        while (Current.Kind != SyntaxKind.CloseBraceToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            children.Add(TokenNode(Expect(SyntaxKind.IdentifierToken)));
            if (Current.Kind == SyntaxKind.CommaToken)
            {
                children.Add(TokenNode(NextToken()));
            }
            else
            {
                break;
            }
        }

        children.Add(TokenNode(Expect(SyntaxKind.CloseBraceToken)));
        children.Add(TokenNode(Expect(SyntaxKind.FromKeyword)));
        children.Add(TokenNode(Expect(SyntaxKind.StringLiteralToken)));
    }

    private SyntaxNode ParseExportedDeclaration()
    {
        var children = new List<SyntaxNode>();
        var export = TokenNode(Expect(SyntaxKind.ExportKeyword));
        children.Add(new SyntaxNode(SyntaxKind.ExportModifier, export.Span, children: [export]));
        children.AddRange(ParseDeclarationPrefix());

        return Current.Kind switch
        {
            _ when IsFunctionDeclarationStart(Current) => ParseFunctionDeclaration(children),
            SyntaxKind.TypeKeyword => ParseTypeAliasDeclaration(children),
            SyntaxKind.RecordKeyword => ParseRecordDeclaration(children),
            SyntaxKind.UnionKeyword => ParseUnionDeclaration(children),
            SyntaxKind.ClassKeyword => ParseClassDeclaration(children),
            SyntaxKind.DelegateKeyword => ParseDelegateDeclaration(children),
            SyntaxKind.LiteralKeyword => ParseLiteralDeclaration(children),
            SyntaxKind.LetKeyword => ParseValueDeclaration(children),
            _ => Node(SyntaxKind.SkippedToken, [..children, ParseSkippedToken()])
        };
    }

    private SyntaxNode ParseFunctionDeclaration(List<SyntaxNode>? prefixChildren = null)
    {
        var children = prefixChildren ?? [];
        var isExtern = false;

        while (IsFunctionModifier(Current))
        {
            var modifier = ParseFunctionModifier();
            isExtern = isExtern || modifier.Kind == SyntaxKind.ExternModifier;
            children.Add(modifier);
        }

        children.Add(TokenNode(Expect(SyntaxKind.FunKeyword)));
        children.Add(TokenNode(Expect(SyntaxKind.IdentifierToken)));
        if (Current.Kind == SyntaxKind.LessToken)
        {
            children.Add(ParseTypeParameterList());
        }

        children.Add(ParseParameterList());

        if (Current.Kind == SyntaxKind.ColonToken)
        {
            children.Add(ParseTypeAnnotation());
        }

        if (Current.Kind == SyntaxKind.OpenBraceToken)
        {
            children.Add(ParseFunctionBody());
        }
        else if (Current.Kind == SyntaxKind.EqualsToken)
        {
            var bodyChildren = new List<SyntaxNode>
            {
                TokenNode(NextToken()),
                ParseExpression()
            };
            children.Add(Node(SyntaxKind.FunctionBody, bodyChildren));
        }
        else if (isExtern)
        {
            return Node(SyntaxKind.FunctionDeclaration, children);
        }
        else
        {
            var position = Current.Kind == SyntaxKind.EndOfFileToken && _position > 0
                ? Previous.Span.End
                : Current.Span.Start;
            _diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.MissingFunctionBody.Code,
                DiagnosticDescriptors.MissingFunctionBody.DefaultSeverity,
                DiagnosticDescriptors.MissingFunctionBody.MessageTemplate,
                _file,
                new SourceSpan(position, position)));
            children.Add(SyntaxNode.Missing(SyntaxKind.MissingFunctionBody, position));
        }

        return Node(SyntaxKind.FunctionDeclaration, children);
    }

    private SyntaxNode ParseFunctionModifier()
    {
        var token = NextToken();
        var tokenNode = TokenNode(token);
        var kind = token.Kind == SyntaxKind.AsyncKeyword
            ? SyntaxKind.AsyncModifier
            : token.Text switch
            {
                "dynamic" => SyntaxKind.DynamicModifier,
                "reflect" => SyntaxKind.ReflectModifier,
                "interop" => SyntaxKind.InteropModifier,
                "unsafe" => SyntaxKind.UnsafeModifier,
                "extern" => SyntaxKind.ExternModifier,
                _ => SyntaxKind.SkippedToken
            };

        return new SyntaxNode(kind, token.Span, children: [tokenNode]);
    }

    private SyntaxNode ParseTypeAliasDeclaration(List<SyntaxNode>? prefixChildren = null)
    {
        var children = prefixChildren ?? [];
        children.Add(TokenNode(Expect(SyntaxKind.TypeKeyword)));
        children.Add(TokenNode(Expect(SyntaxKind.IdentifierToken)));
        if (Current.Kind == SyntaxKind.LessToken)
        {
            children.Add(ParseTypeParameterList());
        }

        children.Add(TokenNode(Expect(SyntaxKind.EqualsToken)));
        children.Add(ParseType());
        return Node(SyntaxKind.TypeAliasDeclaration, children);
    }

    private SyntaxNode ParseLiteralDeclaration(List<SyntaxNode>? prefixChildren = null)
    {
        var children = prefixChildren ?? [];
        children.Add(TokenNode(Expect(SyntaxKind.LiteralKeyword)));
        children.Add(TokenNode(Expect(SyntaxKind.IdentifierToken)));

        if (Current.Kind == SyntaxKind.ColonToken)
        {
            children.Add(ParseTypeAnnotation());
        }

        if (Current.Kind == SyntaxKind.EqualsToken)
        {
            var initializerChildren = new List<SyntaxNode>
            {
                TokenNode(NextToken()),
                ParseExpression()
            };
            children.Add(Node(SyntaxKind.Initializer, initializerChildren));
        }

        return Node(SyntaxKind.LiteralDeclaration, children);
    }

    private SyntaxNode ParseRecordDeclaration(List<SyntaxNode>? prefixChildren = null)
    {
        var children = prefixChildren ?? [];
        children.Add(TokenNode(Expect(SyntaxKind.RecordKeyword)));
        children.Add(TokenNode(Expect(SyntaxKind.IdentifierToken)));
        if (Current.Kind == SyntaxKind.LessToken)
        {
            children.Add(ParseTypeParameterList());
        }

        if (Current.Kind == SyntaxKind.OpenParenToken)
        {
            children.Add(ParseParameterList());
        }

        return Node(SyntaxKind.RecordDeclaration, children);
    }

    private SyntaxNode ParseUnionDeclaration(List<SyntaxNode>? prefixChildren = null)
    {
        var children = prefixChildren ?? [];
        children.Add(TokenNode(Expect(SyntaxKind.UnionKeyword)));
        children.Add(TokenNode(Expect(SyntaxKind.IdentifierToken)));
        if (Current.Kind == SyntaxKind.LessToken)
        {
            children.Add(ParseTypeParameterList());
        }

        children.Add(TokenNode(Expect(SyntaxKind.OpenBraceToken)));
        while (Current.Kind != SyntaxKind.CloseBraceToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            var caseChildren = new List<SyntaxNode>
            {
                TokenNode(Expect(SyntaxKind.IdentifierToken))
            };

            if (Current.Kind == SyntaxKind.OpenParenToken)
            {
                caseChildren.Add(ParseParameterList());
            }

            children.Add(Node(SyntaxKind.UnionCase, caseChildren));
        }

        children.Add(TokenNode(Expect(SyntaxKind.CloseBraceToken)));
        return Node(SyntaxKind.UnionDeclaration, children);
    }

    private SyntaxNode ParseTypeParameterList()
    {
        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.LessToken))
        };

        while (Current.Kind != SyntaxKind.GreaterToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            children.Add(TokenNode(Expect(SyntaxKind.IdentifierToken)));
            if (Current.Kind == SyntaxKind.CommaToken)
            {
                children.Add(TokenNode(NextToken()));
            }
            else
            {
                break;
            }
        }

        children.Add(TokenNode(Expect(SyntaxKind.GreaterToken)));
        return Node(SyntaxKind.TypeParameterList, children);
    }

    private SyntaxNode ParseFunctionBody()
    {
        var children = new List<SyntaxNode>
        {
            ParseBlockExpression()
        };
        return Node(SyntaxKind.FunctionBody, children);
    }

    private SyntaxNode ParseBlockExpression()
    {
        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.OpenBraceToken))
        };

        while (Current.Kind != SyntaxKind.CloseBraceToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            if (Current.Kind == SyntaxKind.LetKeyword)
            {
                children.Add(ParseValueDeclaration());
            }
            else
            {
                var expression = ParseExpression();
                children.Add(new SyntaxNode(SyntaxKind.ExpressionStatement, expression.Span, children: [expression]));
            }
        }

        children.Add(TokenNode(Expect(SyntaxKind.CloseBraceToken)));
        return Node(SyntaxKind.BlockExpression, children);
    }

    private SyntaxNode? _lastExpression;

    private SyntaxNode ParseClassDeclaration(List<SyntaxNode>? prefixChildren = null)
    {
        var children = prefixChildren ?? [];
        children.Add(TokenNode(Expect(SyntaxKind.ClassKeyword)));
        children.Add(TokenNode(Expect(SyntaxKind.IdentifierToken)));

        if (Current.Kind == SyntaxKind.LessToken)
        {
            children.Add(ParseTypeParameterList());
        }

        if (Current.Kind == SyntaxKind.OpenParenToken)
        {
            children.Add(ParseParameterList());
        }

        children.Add(TokenNode(Expect(SyntaxKind.OpenBraceToken)));
        while (Current.Kind != SyntaxKind.CloseBraceToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            children.Add(ParseClassMember());
        }

        children.Add(TokenNode(Expect(SyntaxKind.CloseBraceToken)));
        return Node(SyntaxKind.ClassDeclaration, children);
    }

    private SyntaxNode ParseClassMember()
    {
        var children = ParseDeclarationPrefix();

        return Current.Kind switch
        {
            _ when IsFunctionDeclarationStart(Current) => ParseFunctionDeclaration(children),
            SyntaxKind.LetKeyword => ParseValueDeclaration(children),
            SyntaxKind.EventKeyword => ParseEventDeclaration(children),
            _ => Node(SyntaxKind.SkippedToken, [..children, ParseSkippedToken()])
        };
    }

    private SyntaxNode ParseDelegateDeclaration(List<SyntaxNode>? prefixChildren = null)
    {
        var children = prefixChildren ?? [];
        children.Add(TokenNode(Expect(SyntaxKind.DelegateKeyword)));
        children.Add(TokenNode(Expect(SyntaxKind.IdentifierToken)));

        if (Current.Kind == SyntaxKind.LessToken)
        {
            children.Add(ParseTypeParameterList());
        }

        children.Add(ParseParameterList());

        if (Current.Kind == SyntaxKind.ColonToken)
        {
            children.Add(ParseTypeAnnotation());
        }

        return Node(SyntaxKind.DelegateDeclaration, children);
    }

    private SyntaxNode ParseEventDeclaration(List<SyntaxNode>? prefixChildren = null)
    {
        var children = prefixChildren ?? [];
        children.Add(TokenNode(Expect(SyntaxKind.EventKeyword)));
        children.Add(TokenNode(Expect(SyntaxKind.IdentifierToken)));

        if (Current.Kind == SyntaxKind.ColonToken)
        {
            children.Add(ParseTypeAnnotation());
        }

        return Node(SyntaxKind.EventDeclaration, children);
    }

    private SyntaxNode ParseValueDeclaration(List<SyntaxNode>? prefixChildren = null)
    {
        var children = prefixChildren ?? [];
        children.Add(TokenNode(Expect(SyntaxKind.LetKeyword)));

        if (Current.Kind == SyntaxKind.MutKeyword)
        {
            children.Add(TokenNode(NextToken()));
        }

        children.Add(TokenNode(Expect(SyntaxKind.IdentifierToken)));

        if (Current.Kind == SyntaxKind.ColonToken)
        {
            children.Add(ParseTypeAnnotation());
        }

        if (Current.Kind == SyntaxKind.EqualsToken)
        {
            var initializerChildren = new List<SyntaxNode>
            {
                TokenNode(NextToken()),
                ParseExpression()
            };
            children.Add(Node(SyntaxKind.Initializer, initializerChildren));
        }

        if (Current.Kind == SyntaxKind.OpenBraceToken)
        {
            children.Add(ParseAccessorBlock());
        }

        return Node(SyntaxKind.ValueDeclaration, children);
    }

    private SyntaxNode ParseAccessorBlock()
    {
        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.OpenBraceToken))
        };

        while (Current.Kind != SyntaxKind.CloseBraceToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            var accessorChildren = ParseDeclarationPrefix();
            if (Current.Kind is SyntaxKind.GetKeyword or SyntaxKind.SetKeyword)
            {
                accessorChildren.Add(TokenNode(NextToken()));
            }
            else
            {
                accessorChildren.Add(ParseSkippedToken());
            }

            children.Add(Node(SyntaxKind.AccessorDeclaration, accessorChildren));
        }

        children.Add(TokenNode(Expect(SyntaxKind.CloseBraceToken)));
        return Node(SyntaxKind.AccessorBlock, children);
    }

    private SyntaxNode ParseParameterList()
    {
        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.OpenParenToken))
        };

        while (Current.Kind != SyntaxKind.CloseParenToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            var parameterChildren = new List<SyntaxNode>
            {
                TokenNode(Expect(SyntaxKind.IdentifierToken))
            };

            if (Current.Kind == SyntaxKind.ColonToken)
            {
                parameterChildren.Add(ParseTypeAnnotation());
            }

            children.Add(Node(SyntaxKind.Parameter, parameterChildren));

            if (Current.Kind == SyntaxKind.CommaToken)
            {
                children.Add(TokenNode(NextToken()));
            }
            else
            {
                break;
            }
        }

        children.Add(TokenNode(Expect(SyntaxKind.CloseParenToken)));
        return Node(SyntaxKind.ParameterList, children);
    }

    private SyntaxNode ParseTypeAnnotation()
    {
        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.ColonToken)),
            ParseType()
        };
        return Node(SyntaxKind.TypeAnnotation, children);
    }

    private SyntaxNode ParseType()
    {
        var result = ParsePrimaryType();

        if (Current.Kind == SyntaxKind.LessToken)
        {
            result = Node(SyntaxKind.TypeName, [result, ParseTypeArgumentList()]);
        }

        while (Current.Kind is SyntaxKind.OpenBracketToken or SyntaxKind.QuestionToken)
        {
            if (Current.Kind == SyntaxKind.OpenBracketToken && Peek(1).Kind == SyntaxKind.CloseBracketToken)
            {
                result = Node(SyntaxKind.ArrayType, [result, TokenNode(NextToken()), TokenNode(NextToken())]);
                continue;
            }

            if (Current.Kind == SyntaxKind.QuestionToken)
            {
                result = Node(SyntaxKind.NullableType, [result, TokenNode(NextToken())]);
                continue;
            }

            break;
        }

        if (Current.Kind == SyntaxKind.ArrowToken)
        {
            result = Node(SyntaxKind.FunctionType, [result, TokenNode(NextToken()), ParseType()]);
        }

        if (Current.Kind == SyntaxKind.PipeToken)
        {
            var unionChildren = new List<SyntaxNode> { result };
            while (Current.Kind == SyntaxKind.PipeToken)
            {
                unionChildren.Add(TokenNode(NextToken()));
                unionChildren.Add(ParsePrimaryType());
            }

            result = Node(SyntaxKind.UnionType, unionChildren);
        }

        return result;
    }

    private SyntaxNode ParsePrimaryType()
    {
        if (Current.Kind == SyntaxKind.OpenBraceToken)
        {
            return ParseRecordShapeType();
        }

        return ParseQualifiedName();
    }

    private SyntaxNode ParseRecordShapeType()
    {
        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.OpenBraceToken))
        };

        while (Current.Kind != SyntaxKind.CloseBraceToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            var memberChildren = new List<SyntaxNode>
            {
                TokenNode(Expect(SyntaxKind.IdentifierToken))
            };

            if (Current.Kind == SyntaxKind.QuestionToken)
            {
                memberChildren.Add(TokenNode(NextToken()));
            }

            memberChildren.Add(TokenNode(Expect(SyntaxKind.ColonToken)));
            memberChildren.Add(ParseType());
            children.Add(Node(SyntaxKind.ShapeMember, memberChildren));

            if (Current.Kind == SyntaxKind.CommaToken)
            {
                children.Add(TokenNode(NextToken()));
            }
            else
            {
                break;
            }
        }

        children.Add(TokenNode(Expect(SyntaxKind.CloseBraceToken)));
        return Node(SyntaxKind.RecordShapeType, children);
    }

    private SyntaxNode ParseTypeArgumentList()
    {
        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.LessToken))
        };

        while (Current.Kind != SyntaxKind.GreaterToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            children.Add(ParseType());
            if (Current.Kind == SyntaxKind.CommaToken)
            {
                children.Add(TokenNode(NextToken()));
            }
            else
            {
                break;
            }
        }

        children.Add(TokenNode(Expect(SyntaxKind.GreaterToken)));
        return Node(SyntaxKind.TypeArgumentList, children);
    }

    private SyntaxNode ParseExpression(int parentPrecedence = 0)
    {
        SyntaxNode left;

        if (Current.Kind == SyntaxKind.IdentifierToken && Peek(1).Kind == SyntaxKind.EqualsGreaterToken)
        {
            left = ParseLambdaExpression();
        }
        else if (Current.Kind == SyntaxKind.IfKeyword)
        {
            left = ParseIfExpression();
        }
        else if (Current.Kind == SyntaxKind.MatchKeyword)
        {
            left = ParseMatchExpression();
        }
        else if (Current.Kind == SyntaxKind.TryKeyword)
        {
            left = ParseTryExpression();
        }
        else if (Current.Kind == SyntaxKind.UsingKeyword)
        {
            left = ParseUsingExpression();
        }
        else if (Current.Kind == SyntaxKind.AwaitKeyword)
        {
            left = ParseAwaitExpression();
        }
        else if (Current.Kind == SyntaxKind.ForKeyword)
        {
            left = ParseForExpression();
        }
        else if (GetUnaryPrecedence(Current.Kind) is var unaryPrecedence && unaryPrecedence != 0 && unaryPrecedence >= parentPrecedence)
        {
            var operatorToken = TokenNode(NextToken());
            var operand = ParseExpression(unaryPrecedence);
            left = Node(SyntaxKind.BinaryExpression, [operatorToken, operand]);
        }
        else
        {
            left = ParsePostfixExpression();
        }

        while (true)
        {
            var precedence = GetBinaryPrecedence(Current.Kind);
            if (precedence == 0 || precedence <= parentPrecedence)
            {
                break;
            }

            var operatorToken = TokenNode(NextToken());
            var right = ParseExpression(precedence);
            left = Node(SyntaxKind.BinaryExpression, [left, operatorToken, right]);
        }

        if (Current.Kind == SyntaxKind.EqualsToken)
        {
            left = Node(SyntaxKind.AssignmentExpression, [left, TokenNode(NextToken()), ParseExpression()]);
        }

        if (Current.Kind == SyntaxKind.WithKeyword)
        {
            left = Node(SyntaxKind.RecordUpdateExpression, [left, TokenNode(NextToken()), ParseRecordExpression()]);
        }

        _lastExpression = left;
        return left;
    }

    private SyntaxNode ParseLambdaExpression()
    {
        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.IdentifierToken)),
            TokenNode(Expect(SyntaxKind.EqualsGreaterToken)),
            ParseExpression()
        };

        return Node(SyntaxKind.LambdaExpression, children);
    }

    private SyntaxNode ParseIfExpression()
    {
        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.IfKeyword)),
            ParseExpression(),
            ParseBlockExpression()
        };

        while (Current.Kind == SyntaxKind.ElifKeyword)
        {
            var elseChildren = new List<SyntaxNode>
            {
                TokenNode(NextToken()),
                ParseExpression(),
                ParseBlockExpression()
            };
            children.Add(Node(SyntaxKind.ElseClause, elseChildren));
        }

        if (Current.Kind == SyntaxKind.ElseKeyword)
        {
            var elseChildren = new List<SyntaxNode>
            {
                TokenNode(NextToken()),
                Current.Kind == SyntaxKind.IfKeyword ? ParseIfExpression() : ParseBlockExpression()
            };
            children.Add(Node(SyntaxKind.ElseClause, elseChildren));
        }

        return Node(SyntaxKind.IfExpression, children);
    }

    private SyntaxNode ParseForExpression()
    {
        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.ForKeyword)),
            ParsePattern(),
            TokenNode(Expect(SyntaxKind.InKeyword)),
            ParseExpression(),
            ParseBlockExpression()
        };

        return Node(SyntaxKind.ForExpression, children);
    }

    private SyntaxNode ParseMatchExpression()
    {
        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.MatchKeyword)),
            ParseExpression(),
            TokenNode(Expect(SyntaxKind.OpenBraceToken))
        };

        while (Current.Kind != SyntaxKind.CloseBraceToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            var armChildren = new List<SyntaxNode>
            {
                ParsePattern(),
                TokenNode(Expect(SyntaxKind.EqualsGreaterToken)),
                ParseExpression()
            };
            children.Add(Node(SyntaxKind.MatchArm, armChildren));
        }

        children.Add(TokenNode(Expect(SyntaxKind.CloseBraceToken)));
        return Node(SyntaxKind.MatchExpression, children);
    }

    private SyntaxNode ParseTryExpression()
    {
        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.TryKeyword)),
            ParseBlockExpression()
        };

        while (Current.Kind == SyntaxKind.CatchKeyword)
        {
            var catchChildren = new List<SyntaxNode>
            {
                TokenNode(NextToken()),
                ParsePattern(),
                ParseBlockExpression()
            };
            children.Add(Node(SyntaxKind.CatchClause, catchChildren));
        }

        return Node(SyntaxKind.TryExpression, children);
    }

    private SyntaxNode ParseUsingExpression()
    {
        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.UsingKeyword)),
            ParsePattern(),
            TokenNode(Expect(SyntaxKind.EqualsToken)),
            ParseExpression(),
            ParseBlockExpression()
        };

        return Node(SyntaxKind.UsingExpression, children);
    }

    private SyntaxNode ParseAwaitExpression()
    {
        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.AwaitKeyword)),
            ParseExpression(7)
        };

        return Node(SyntaxKind.AwaitExpression, children);
    }

    private SyntaxNode ParsePattern()
    {
        if (Current.Kind == SyntaxKind.OpenBraceToken)
        {
            return ParseRecordPattern();
        }

        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.IdentifierToken))
        };

        if (Current.Kind == SyntaxKind.ColonToken)
        {
            children.Add(ParseTypeAnnotation());
        }

        if (Current.Kind == SyntaxKind.OpenParenToken)
        {
            var argumentChildren = new List<SyntaxNode>
            {
                TokenNode(NextToken())
            };

            while (Current.Kind != SyntaxKind.CloseParenToken && Current.Kind != SyntaxKind.EndOfFileToken)
            {
                argumentChildren.Add(ParsePattern());
                if (Current.Kind == SyntaxKind.CommaToken)
                {
                    argumentChildren.Add(TokenNode(NextToken()));
                }
                else
                {
                    break;
                }
            }

            argumentChildren.Add(TokenNode(Expect(SyntaxKind.CloseParenToken)));
            children.Add(Node(SyntaxKind.PatternArgumentList, argumentChildren));
        }

        return Node(SyntaxKind.Pattern, children);
    }

    private SyntaxNode ParseRecordPattern()
    {
        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.OpenBraceToken))
        };

        while (Current.Kind != SyntaxKind.CloseBraceToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            var fieldChildren = new List<SyntaxNode>
            {
                TokenNode(Expect(SyntaxKind.IdentifierToken))
            };

            if (Current.Kind == SyntaxKind.ColonToken)
            {
                fieldChildren.Add(TokenNode(NextToken()));
                fieldChildren.Add(ParsePattern());
            }

            children.Add(Node(SyntaxKind.PatternField, fieldChildren));

            if (Current.Kind == SyntaxKind.CommaToken)
            {
                children.Add(TokenNode(NextToken()));
            }
            else
            {
                break;
            }
        }

        children.Add(TokenNode(Expect(SyntaxKind.CloseBraceToken)));
        return Node(SyntaxKind.RecordPattern, children);
    }

    private SyntaxNode ParsePostfixExpression()
    {
        var expression = ParsePrimaryExpression();

        while (true)
        {
            if (HasLeadingNewline(Current))
            {
                break;
            }

            if (Current.Kind == SyntaxKind.DotToken)
            {
                expression = Node(SyntaxKind.MemberAccessExpression, [expression, TokenNode(NextToken()), TokenNode(Expect(SyntaxKind.IdentifierToken))]);
                continue;
            }

            if (Current.Kind == SyntaxKind.OpenBracketToken)
            {
                expression = Node(SyntaxKind.IndexerExpression, [expression, TokenNode(NextToken()), ParseExpression(), TokenNode(Expect(SyntaxKind.CloseBracketToken))]);
                continue;
            }

            if (Current.Kind == SyntaxKind.OpenParenToken)
            {
                expression = ParseCallExpression(expression);
                continue;
            }

            break;
        }

        return expression;
    }

    private SyntaxNode ParseCallExpression(SyntaxNode callee)
    {
        var children = new List<SyntaxNode>
        {
            callee,
            TokenNode(Expect(SyntaxKind.OpenParenToken))
        };

        if (Current.Kind != SyntaxKind.CloseParenToken)
        {
            children.Add(ParseArgument());
            while (Current.Kind == SyntaxKind.CommaToken)
            {
                children.Add(TokenNode(NextToken()));
                children.Add(ParseArgument());
            }
        }

        children.Add(TokenNode(Expect(SyntaxKind.CloseParenToken)));
        return Node(SyntaxKind.CallExpression, children);
    }

    private SyntaxNode ParsePrimaryExpression()
    {
        return Current.Kind switch
        {
            SyntaxKind.IdentifierToken => Node(SyntaxKind.IdentifierExpression, [TokenNode(NextToken())]),
            SyntaxKind.NumericLiteralToken
                or SyntaxKind.StringLiteralToken
                or SyntaxKind.InterpolatedStringLiteralToken
                or SyntaxKind.NullKeyword
                or SyntaxKind.TrueKeyword
                or SyntaxKind.FalseKeyword => Node(SyntaxKind.LiteralExpression, [TokenNode(NextToken())]),
            SyntaxKind.OpenBraceToken => ParseBlockExpression(),
            SyntaxKind.OpenBracketToken => ParseCollectionExpression(),
            SyntaxKind.OpenParenToken => ParseParenthesizedExpression(),
            _ => ParseMissingExpression()
        };
    }

    private SyntaxNode ParseRecordExpression()
    {
        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.OpenBraceToken))
        };

        while (Current.Kind != SyntaxKind.CloseBraceToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            var fieldChildren = new List<SyntaxNode>
            {
                TokenNode(Expect(SyntaxKind.IdentifierToken))
            };

            if (Current.Kind == SyntaxKind.ColonToken)
            {
                fieldChildren.Add(TokenNode(NextToken()));
                fieldChildren.Add(ParseExpression());
            }

            children.Add(Node(SyntaxKind.RecordField, fieldChildren));

            if (Current.Kind == SyntaxKind.CommaToken)
            {
                children.Add(TokenNode(NextToken()));
            }
            else
            {
                break;
            }
        }

        children.Add(TokenNode(Expect(SyntaxKind.CloseBraceToken)));
        return Node(SyntaxKind.RecordExpression, children);
    }

    private SyntaxNode ParseArgument()
    {
        if (Current.Kind == SyntaxKind.OpenBraceToken)
        {
            return ParseRecordExpression();
        }

        if (Current.Kind == SyntaxKind.OutKeyword)
        {
            return Node(SyntaxKind.OutArgument, [TokenNode(NextToken()), ParseExpression()]);
        }

        if (Current.Kind == SyntaxKind.InKeyword)
        {
            return Node(SyntaxKind.InArgument, [TokenNode(NextToken()), ParseExpression()]);
        }

        if (Current.Kind == SyntaxKind.IdentifierToken && Peek(1).Kind == SyntaxKind.ColonToken)
        {
            return Node(SyntaxKind.NamedArgument, [TokenNode(NextToken()), TokenNode(NextToken()), ParseExpression()]);
        }

        return ParseExpression();
    }

    private SyntaxNode ParseCollectionExpression()
    {
        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.OpenBracketToken))
        };

        if (Current.Kind != SyntaxKind.CloseBracketToken)
        {
            children.Add(ParseExpression());
            while (Current.Kind == SyntaxKind.CommaToken)
            {
                children.Add(TokenNode(NextToken()));
                children.Add(ParseExpression());
            }
        }

        children.Add(TokenNode(Expect(SyntaxKind.CloseBracketToken)));
        return Node(SyntaxKind.CollectionExpression, children);
    }

    private SyntaxNode ParseParenthesizedExpression()
    {
        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.OpenParenToken)),
            ParseExpression(),
            TokenNode(Expect(SyntaxKind.CloseParenToken))
        };
        return Node(SyntaxKind.IdentifierExpression, children);
    }

    private SyntaxNode ParseMissingExpression()
    {
        var position = Current.Span.Start;
        _diagnostics.Add(new Diagnostic(
            DiagnosticDescriptors.MissingExpression.Code,
            DiagnosticDescriptors.MissingExpression.DefaultSeverity,
            $"Expected expression before '{Current.Text}'.",
            _file,
            new SourceSpan(position, position)));

        return SyntaxNode.Missing(SyntaxKind.MissingExpression, position);
    }

    private SyntaxNode ParseQualifiedName()
    {
        var children = new List<SyntaxNode>
        {
            TokenNode(Expect(SyntaxKind.IdentifierToken))
        };

        while (Current.Kind == SyntaxKind.DotToken)
        {
            children.Add(TokenNode(NextToken()));
            children.Add(TokenNode(Expect(SyntaxKind.IdentifierToken)));
        }

        return Node(SyntaxKind.TypeName, children);
    }

    private SyntaxNode ParseSkippedToken() =>
        new(SyntaxKind.SkippedToken, Current.Span, children: [TokenNode(NextToken())]);

    private SyntaxToken Expect(SyntaxKind kind)
    {
        if (Current.Kind == kind)
        {
            return NextToken();
        }

        var position = Current.Span.Start;
        _diagnostics.Add(new Diagnostic(
            DiagnosticDescriptors.UnexpectedToken.Code,
            DiagnosticDescriptors.UnexpectedToken.DefaultSeverity,
            $"Expected {kind} but found {Current.Kind}.",
            _file,
            new SourceSpan(position, position)));

        return new SyntaxToken(kind, string.Empty, new SourceSpan(position, position), IsMissing: true);
    }

    private SyntaxNode TokenNode(SyntaxToken token) =>
        token.IsMissing ? SyntaxNode.Missing(token.Kind, token.Span.Start) : SyntaxNode.Token(token);

    private SyntaxToken NextToken()
    {
        var current = Current;
        if (_position < _tokens.Count - 1)
        {
            _position++;
        }

        return current;
    }

    private SyntaxToken Current => _tokens[_position];
    private SyntaxToken Previous => _tokens[Math.Max(0, _position - 1)];
    private SyntaxToken Peek(int offset)
    {
        var index = Math.Min(_position + offset, _tokens.Count - 1);
        return _tokens[index];
    }

    private static bool HasLeadingNewline(SyntaxToken token)
    {
        const string marker = "newlines=";
        var start = token.LeadingTriviaSummary.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0)
        {
            return false;
        }

        start += marker.Length;
        var end = token.LeadingTriviaSummary.IndexOf(';', start);
        var text = end < 0
            ? token.LeadingTriviaSummary[start..]
            : token.LeadingTriviaSummary[start..end];

        return int.TryParse(text, out var count) && count > 0;
    }

    private static bool IsFunctionDeclarationStart(SyntaxToken token) =>
        token.Kind == SyntaxKind.FunKeyword || IsFunctionModifier(token);

    private static bool IsFunctionModifier(SyntaxToken token) =>
        token.Kind == SyntaxKind.AsyncKeyword ||
        (token.Kind == SyntaxKind.IdentifierToken &&
            token.Text is "dynamic" or "reflect" or "interop" or "unsafe" or "extern");

    private static SyntaxNode Node(SyntaxKind kind, IReadOnlyList<SyntaxNode> children) =>
        new(kind, GetSpan(children), children: children);

    private static SourceSpan GetSpan(IReadOnlyList<SyntaxNode> children)
    {
        if (children.Count == 0)
        {
            var position = new SourcePosition(1, 1);
            return new SourceSpan(position, position);
        }

        return new SourceSpan(children[0].Span.Start, children[^1].Span.End);
    }

    private static int GetUnaryPrecedence(SyntaxKind kind) =>
        kind is SyntaxKind.PlusToken or SyntaxKind.MinusToken ? 7 : 0;

    private static int GetBinaryPrecedence(SyntaxKind kind) =>
        kind switch
        {
            SyntaxKind.StarToken or SyntaxKind.SlashToken or SyntaxKind.PercentToken => 6,
            SyntaxKind.PlusToken or SyntaxKind.MinusToken => 5,
            SyntaxKind.LessToken or SyntaxKind.LessOrEqualsToken or SyntaxKind.GreaterToken or SyntaxKind.GreaterOrEqualsToken => 4,
            SyntaxKind.EqualsEqualsToken or SyntaxKind.BangEqualsToken => 3,
            SyntaxKind.AmpersandAmpersandToken => 2,
            SyntaxKind.PipePipeToken => 1,
            SyntaxKind.NullCoalescingToken => 1,
            SyntaxKind.PipeGreaterToken => 1,
            _ => 0
        };
}
