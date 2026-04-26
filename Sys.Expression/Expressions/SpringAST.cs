using System;
using System.Collections;
using System.Runtime.Serialization;

namespace Spring.Expressions
{
    /// <summary>
    /// For internal purposes only. Use <see cref="BaseNode"/> for expression node implementations.
    /// </summary>
    [Serializable]
    public class SpringAST : AstNode, ISerializable, ICloneable
    {
        /// <summary>
        /// Create an instance
        /// </summary>
        public SpringAST()
        { }

        #region AstNode Overrides

        /// <summary>
        /// initialize this instance from an AstNode
        /// </summary>
        public override void initialize(AstNode t)
        {
            this.setText(t.getText());
            this.Type = t.Type;
        }

        /// <summary>
        /// initialize this instance from a token type number and a text
        /// </summary>
        public override void initialize(int t, string txt)
        {
            this.Type = t;
            this.setText(txt);
        }

        #endregion

        #region ICloneable

        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }

        #endregion

        #region ISerializable Implementation

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected SpringAST(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// populate SerializationInfo from this instance
        /// </summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #endregion
    }
}
