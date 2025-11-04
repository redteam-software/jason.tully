


namespace RedTeam.Extensions.Console.CodeGenerators.Models
{
    public static class Tree
    {
        public static IEnumerable<T> DescendFirstMatches<T>(this TreeNode<T> self, Func<T, bool> predicate) where T : TreeNode<T>
        {

            return self.Evolve<T>((T current) => predicate(current)
            ? Array.Empty<T>()
            : current.Children, (T current, IEnumerable<T?> clds, IEnumerable<T?> seeds) => (!predicate(current)) ? seeds.Concat(clds) : seeds.Prepend(current));
        }
        public static IEnumerable<T> Evolve<T>(this TreeNode<T> startNode, Func<T, IEnumerable<T?>> getNodes, Func<T, IEnumerable<T?>, IEnumerable<T?>, IEnumerable<T?>> updatePendingNodes) where T : TreeNode<T>
        {
            ISet<T> exphistory = new HashSet<T>();
            ISet<T> rtnhistory = new HashSet<T>();
            IEnumerable<T?> seeds = new T[1] { (T)startNode };
            T? cur;
            while (expand(ref exphistory, out cur, ref seeds, getNodes, updatePendingNodes))
            {
                if (cur != null && rtnhistory.Add(cur))
                {
                    yield return cur;
                }
            }
        }

        private static bool expand<T>(ref ISet<T> history, out T? cur, ref IEnumerable<T?> seeds, Func<T, IEnumerable<T?>> getnewseeds, Func<T, IEnumerable<T?>, IEnumerable<T?>, IEnumerable<T?>> updateseeds)
        {
            if (!seeds.Any())
            {
                cur = default(T);
                return false;
            }

            cur = seeds.First();
            while (cur != null && history.Add(cur))
            {
                IEnumerable<T?> arg = getnewseeds(cur);
                seeds = updateseeds(cur, arg, seeds.Skip(1));
                cur = seeds.FirstOrDefault();
            }

            seeds = seeds.Skip(1);
            return true;
        }
    }
    public abstract class TreeNode<TNode> where TNode : TreeNode<TNode>
    {
        private readonly List<TNode> _children = new List<TNode>();
        protected TreeNode(IEnumerable<TNode> nodes)
        {
            _children = new List<TNode>(nodes);
        }
        public TNode? Parent { get; protected set; }

        //
        // Summary:
        //     Gets the child nodes.
        public IEnumerable<TNode> Children
        {
            get
            {
                return _children;
            }
        }


        public virtual void AddChild(TNode node)
        {
            _children.Add(node);
        }

    }

}
