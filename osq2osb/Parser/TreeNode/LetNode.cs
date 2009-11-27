﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    class LetNode : DirectiveNode {
        public override string Parameters {
            set {
                var re = new Regex(@"^(?<variable>\w+)\b\s*(?<value>.*)\s*$", RegexOptions.ExplicitCapture);
                var match = re.Match(value);

                if(!match.Success) {
                    throw new ParserException("Bad form for #" + DirectiveName + " directive", Parser, Location);
                }

                Variable = match.Groups["variable"].Value;
                Content = match.Groups["value"].Value;

                base.Parameters = value;
            }
        }

        public string Variable {
            get;
            private set;
        }

        public LetNode(Parser parser, Location location) :
            base(parser, location) {
        }

        protected override bool EndsWith(NodeBase node) {
            if(!string.IsNullOrEmpty(Content.Trim()) && node == this) {
                return true;
            }

            var endDirective = node as EndDirectiveNode;

            if(endDirective != null && endDirective.TargetDirectiveName == this.DirectiveName) {
                return true;
            }

            return false;
        }

        public override void Execute(TextWriter output) {
            using(var varWriter = new StringWriter()) {
                ExecuteChildren(varWriter);

                Parser.SetVariable(Variable, varWriter.ToString().Trim(Environment.NewLine.ToCharArray()));
            }
        }
    }
}
