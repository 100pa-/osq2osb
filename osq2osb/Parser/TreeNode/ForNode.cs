﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    class ForNode : DirectiveNode {
        public override string Parameters {
            set {
                var re = new Regex(@"^(?<variable>\w+)\b\s*(?<values>.*)$", RegexOptions.ExplicitCapture);
                var match = re.Match(value);

                if(!match.Success) {
                    throw new ArgumentException("Bad form for #" + DirectiveName + " directive");
                }

                Variable = match.Groups["variable"].Value;
                Values = match.Groups["values"].Value;

                base.Parameters = value;
            }
        }

        public string Variable {
            get;
            private set;
        }

        public string Values {
            get;
            private set;
        }

        protected override bool IsMultiline {
            get {
                return true;
            }
        }

        public override void Execute(Parser parser, TextWriter output) {
            double counter = double.NaN;

            while(true) {
                // Syntax: min max [step]
                string str = parser.ReplaceExpressions(Values);
                var parts = str.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

                if(parts.Length < 2 || parts.Length > 3) {
                    throw new InvalidDataException("Bad #for form: " + str);
                }

                double min = double.Parse(parts[0]);
                double max = double.Parse(parts[1]);
                double step = parts.Length >= 3 ? double.Parse(parts[2]) : 1;

                if(double.IsNaN(counter)) {
                    counter = min;
                }

                parser.SetVariable(Variable, counter);

                foreach(var child in ChildrenNodes) {
                    child.Execute(parser, output);
                }

                if(Content != null) {
                    var contentNode = new RawTextNode(Content);
                    contentNode.Execute(parser, output);
                }

                counter = System.Convert.ToDouble(parser.GetVariable(Variable));
                counter += step;

                if(counter >= max) {
                    break;
                }
            }
        }
    }
}