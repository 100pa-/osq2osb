﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using osq.Parser;

namespace osq.TreeNode {
    [DirectiveAttribute("for")]
    public class ForNode : DirectiveNode {
        public string Variable {
            get;
            private set;
        }

        public TokenNode Start {
            get;
            private set;
        }

        public TokenNode End {
            get;
            private set;
        }

        public TokenNode Step {
            get;
            private set;
        }

        public IList<NodeBase> ChildrenNodes {
            get;
            private set;
        }

        public ForNode(ITokenReader tokenReader, INodeReader nodeReader, string directiveName = null, Location location = null) :
            base(directiveName, location) {
            var parameters = ExpressionRewriter.Rewrite(tokenReader);

            if(!parameters.Token.IsSymbol(",")) {
                throw new DataTypeException("Expected comma-separated list", this);
            }

            var children = parameters.ChildrenTokenNodes;

            if(children.Count < 3 || children.Count > 4) {
                throw new MissingDataException("#for directive requires 3 to 4 parameters", location);
            }

            if(children[0].Token.TokenType != TokenType.Identifier) {
                throw new MissingDataException("Identifier", children[0].Location);
            }

            Variable = children[0].Token.ToString();
            Start = children[1];
            End = children[2];
            Step = children.Count > 3 ? children[3] : null;

            ChildrenNodes = new List<NodeBase>(nodeReader.TakeWhile((node) => {
                var endDirective = node as EndDirectiveNode;

                if(endDirective != null && endDirective.TargetDirectiveName == DirectiveName) {
                    return false;
                }

                return true;
            }));
        }

        public override string Execute(ExecutionContext context) {
            var output = new StringBuilder();

            double counter = context.GetDoubleFrom(Start.Evaluate(context));

            while(true) {
                context.SetVariable(Variable, counter);

                output.Append(ExecuteChildren(context));

                counter = context.GetDoubleFrom(context.GetVariable(Variable));
                counter += Step == null ? 1.0 : context.GetDoubleFrom(Step.Evaluate(context));

                if(counter >= context.GetDoubleFrom(End.Evaluate(context))) {
                    break;
                }
            }

            return output.ToString();
        }

        private string ExecuteChildren(ExecutionContext context) {
            var output = new StringBuilder();

            foreach(var child in ChildrenNodes) {
                output.Append(child.Execute(context));
            }

            return output.ToString();
        }
    }
}