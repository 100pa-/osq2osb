﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osq2osb.Parser;

namespace osq2osb {
    public class ExecutionException : Exception {
        public virtual Location Location {
            get {
                return location;
            }
        }

        private Location location;

        public ExecutionException() {
        }

        public ExecutionException(string message) :
            base(message) {
        }

        public ExecutionException(string message, Exception inner) :
            base(message, inner) {
        }

        public ExecutionException(string message, Location location) :
            this(message) {
            this.location = location;
        }

        public ExecutionException(string message, Location location, Exception inner) :
            this(message, inner) {
            this.location = location;
        }

        protected ExecutionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) :
            base(info, context) {
            throw new NotImplementedException();
        }

        public override string ToString() {
            if(Location == null) {
                return GetType().Name + ": " + Message + Environment.NewLine + StackTrace;
            } else {
                return GetType().Name + ": " + Message + " at " + Location.ToString() + Environment.NewLine + StackTrace;
            }
        }
    }
}
