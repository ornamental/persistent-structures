using System;
using System.Collections.Generic;
using System.Text;

namespace PersistentCollections.Avl
{
    public interface AvlTreeTraversal<N, P> where N : AvlNode<N, P>
    {
        Descent Descend();

        N CurrentNode
        {
            get;
        }
    }
}
