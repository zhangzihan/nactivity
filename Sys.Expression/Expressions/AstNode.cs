using System;
using System.Runtime.Serialization;

namespace Spring.Expressions
{
    /// <summary>
    /// Independent AST node base class that replaces the dependency on antlr.BaseAST.
    /// Provides the same child-sibling tree structure and navigation interface.
    /// </summary>
    [Serializable]
    public abstract class AstNode
    {
        /// <summary>First child node (down pointer in child-sibling tree)</summary>
        protected AstNode down;

        /// <summary>Next sibling node (right pointer in child-sibling tree)</summary>
        protected AstNode right;

        private string text;
        private int nodeType;

        /// <summary>Creates a new uninitialized AstNode instance.</summary>
        protected AstNode() { }

        /// <summary>Deserialization constructor.</summary>
        protected AstNode(SerializationInfo info, StreamingContext context)
        {
            down = (AstNode)info.GetValue("down", typeof(AstNode));
            right = (AstNode)info.GetValue("right", typeof(AstNode));
            nodeType = info.GetInt32("nodeType");
            text = info.GetString("text");
        }

        /// <summary>Populates SerializationInfo from this instance.</summary>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("down", down, typeof(AstNode));
            info.AddValue("right", right, typeof(AstNode));
            info.AddValue("nodeType", nodeType, typeof(int));
            info.AddValue("text", text, typeof(string));
        }

        #region Tree Navigation

        /// <summary>Gets the first child of this node; null if no children.</summary>
        public virtual AstNode getFirstChild()
        {
            return down;
        }

        /// <summary>Gets the next sibling in line after this one.</summary>
        public virtual AstNode getNextSibling()
        {
            return right;
        }

        /// <summary>Gets the number of children of this node; if leaf, returns 0.</summary>
        public int getNumberOfChildren()
        {
            AstNode t = this.down;
            int n = 0;
            if (t is not null)
            {
                n = 1;
                while (t.right is not null)
                {
                    t = t.right;
                    n++;
                }
            }
            return n;
        }

        /// <summary>Adds a node to the end of the child list for this node.</summary>
        public virtual void addChild(AstNode node)
        {
            if (node is null)
                return;
            AstNode t = this.down;
            if (t is not null)
            {
                while (t.right is not null)
                {
                    t = t.right;
                }
                t.right = node;
            }
            else
            {
                this.down = node;
            }
        }

        /// <summary>Sets the first child of this node.</summary>
        public virtual void setFirstChild(AstNode c)
        {
            down = c;
        }

        /// <summary>Sets the next sibling after this one.</summary>
        public virtual void setNextSibling(AstNode n)
        {
            right = n;
        }

        /// <summary>Removes all children of this node.</summary>
        public virtual void removeChildren()
        {
            down = null;
        }

        #endregion

        #region Text and Type

        /// <summary>Gets the token text for this node.</summary>
        public virtual string getText()
        {
            return text;
        }

        /// <summary>Sets the token text for this node.</summary>
        public virtual void setText(string txt)
        {
            text = txt;
        }

        /// <summary>Gets or sets the token type for this node.</summary>
        public virtual int Type
        {
            get { return nodeType; }
            set { nodeType = value; }
        }

        /// <summary>Sets the token type for this node.</summary>
        public virtual void setType(int ttype)
        {
            Type = ttype;
        }

        /// <summary>Gets or sets the text of this node (property accessor).</summary>
        public string Text
        {
            get { return getText(); }
            set { setText(value); }
        }

        #endregion

        #region Initialization

        /// <summary>Initializes this instance from a token type number and a text.</summary>
        public abstract void initialize(int t, string txt);

        /// <summary>Initializes this instance from another AstNode.</summary>
        public abstract void initialize(AstNode t);

        #endregion

        #region Object Overrides

        /// <summary>Returns a string representation of this node.</summary>
        public override string ToString()
        {
            return getText() ?? string.Empty;
        }

        /// <summary>Returns a string representation of the node's type and hash code.</summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
