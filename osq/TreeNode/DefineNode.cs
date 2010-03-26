﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace osq.TreeNode {
    [DirectiveAttribute("def(ine)?")]
    public class DefineNode : DirectiveNode {
        public string Variable {
            get;
            private set;
        }

        public IList<string> FunctionParameters {
            get;
            private set;
        }

        public DefineNode(ITokenReader tokenReader, INodeReader nodeReader, string directiveName = null, Location location = null) :
            base(directiveName, location) {
            var startLocation = tokenReader.CurrentLocation;

            Token variable = tokenReader.ReadToken();

            if(variable == null) {
                throw new MissingDataException("Variable name", startLocation);
            }

            if(variable.TokenType != TokenType.Identifier) {
                throw new MissingDataException("Variable name", variable.Location);
            }

            Variable = variable.ToString();

            FunctionParameters = new List<string>();

            Token token = tokenReader.PeekToken();

            if(token != null && token.IsSymbol("(")) {
                token = tokenReader.ReadToken();

                while(token != null && !token.IsSymbol(")")) {
                    token = tokenReader.ReadToken();

                    if(token.TokenType == TokenType.Identifier) {
                        FunctionParameters.Add(token.Value.ToString());
                    }
                }

                if(token == null) {
                    throw new MissingDataException("Closing parentheses");
                }
            }

            var shorthand = ReadShorthandNode(tokenReader);

            if(shorthand != null) {
                ChildrenNodes.Add(shorthand);
            } else {
                foreach(var node in ReadNodes(nodeReader)) {
                    ChildrenNodes.Add(node);
                }
            }
        }

        private NodeBase ReadShorthandNode(ITokenReader tokenReader) {
            ICollection<Token> tokens = new List<Token>();
            Token curToken;

            while((curToken = tokenReader.ReadToken()) != null) {
                tokens.Add(curToken);
            }

            if(tokens.Count((token) => token.TokenType != TokenType.WhiteSpace) == 0) {
                return null;
            }

            return ExpressionRewriter.Rewrite(tokens);
        }

        private IEnumerable<NodeBase> ReadNodes(INodeReader nodeReader) {
            NodeBase curNode;

            while((curNode = nodeReader.ReadNode()) != null) {
                var endDirective = curNode as EndDirectiveNode;

                if(endDirective != null && endDirective.TargetDirectiveName == DirectiveName) {
                    yield break;
                }

                yield return curNode;
            }
        }

        protected override bool EndsWith(NodeBase node) {
            if(ChildrenNodes.Count != 0 && node == this) {
                return true;
            }

            var endDirective = node as EndDirectiveNode;

            return endDirective != null && endDirective.TargetDirectiveName == DirectiveName;
        }

        public override string Execute(ExecutionContext context) {
            context.SetVariable(Variable, new ExecutionContext.OsqFunction((token, subContext) => {
                subContext.PushScope();

                try {
                    var parameters = token.GetChildrenTokens();

                    if(parameters.Count == 1 && parameters[0].Token.IsSymbol(",")) {
                        parameters = parameters[0].GetChildrenTokens();
                    }

                    using(var paramNameEnumerator = FunctionParameters.GetEnumerator()) {
                        foreach(var child in parameters) {
                            if(!paramNameEnumerator.MoveNext()) {
                                break;
                            }

                            object value = child.Evaluate(context);

                            subContext.SetLocalVariable(paramNameEnumerator.Current, value);
                        }
                    }

                    string output = ExecuteChildren(context);

                    return output.TrimEnd(Environment.NewLine.ToCharArray());
                } finally {
                    subContext.PopScope();
                }
            }));

            return "";
        }
    }
}